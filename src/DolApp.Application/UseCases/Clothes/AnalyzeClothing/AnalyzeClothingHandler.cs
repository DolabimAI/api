using DolApp.Application.Interfaces;
using DolApp.Application.Options;
using DolApp.Core.Constants;
using DolApp.Core.Entities;
using DolApp.Core.Errors;
using MediatR;

namespace DolApp.Application.UseCases.Clothes.AnalyzeClothing
{
    /// Fotoğraftan taslak parça (FR-05 – FR-07, S3). Akış:
    ///   1) Fotoğraf kontrolü: boyut (413) ve tür (400), Python'a gitmeden.
    ///   2) Kullanıcının dolabı.
    ///   3) Kalmış eski taslak varsa önce görseli, sonra satırı (tahmin cascade ile) silinir (T-13).
    ///   4) Fotoğraf Python'a gider. Bu sırada veritabanı işlemi açık değildir (Python 30 sn sürebilir).
    ///   5) Python'un kodları (gomlek, beyaz, acik) id'ye çevrilir.
    ///   6) Güven eşiğin (0,40) altındaysa tür ve türden gelen alanlar boş kalır; tahmin yine olduğu gibi yazılır.
    ///   7) clothes (taslak) ve model_predictions tek işlemde yazılır.
    ///   5–7'den biri başarısız olursa Python'un depoya yazdığı görseli C# siler (sözleşme, 5. bölüm).
    ///   8) Süreli görsel bağlantısıyla cevap döner.

    public class AnalyzeClothingHandler : IRequestHandler<AnalyzeClothingRequest, AnalyzeClothingResponse>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IWardrobeRepository _wardrobeRepository;
        private readonly IClothingRepository _clothingRepository;
        private readonly ILookupRepository _lookupRepository;
        private readonly IClothingAnalyzer _clothingAnalyzer;
        private readonly IImageStorage _imageStorage;
        private readonly ModelOptions _modelOptions;

        public AnalyzeClothingHandler(
            ICurrentUser currentUser,
            IWardrobeRepository wardrobeRepository,
            IClothingRepository clothingRepository,
            ILookupRepository lookupRepository,
            IClothingAnalyzer clothingAnalyzer,
            IImageStorage imageStorage,
            ModelOptions modelOptions)
        {
            _currentUser = currentUser;
            _wardrobeRepository = wardrobeRepository;
            _clothingRepository = clothingRepository;
            _lookupRepository = lookupRepository;
            _clothingAnalyzer = clothingAnalyzer;
            _imageStorage = imageStorage;
            _modelOptions = modelOptions;
        }

        public async Task<AnalyzeClothingResponse> Handle(AnalyzeClothingRequest request, CancellationToken cancellationToken)
        {
            // Validator ImageStream'in dolu olduğunu kontrol etti.
            var imageStream = request.ImageStream!;

            // 1) Fotoğraf kontrolü
            var contentType = await ImageFileRules.ValidateAsync(imageStream, request.FileLength, cancellationToken);

            // 2) Dolap (kayıtta her kullanıcıya bir dolap açılır; yoksa açık bir hata dönsün)
            var wardrobeId = await _wardrobeRepository.GetWardrobeIdByUserIdAsync(_currentUser.UserId, cancellationToken)
                ?? throw new AppException(404, ErrorCodes.NotFound, ErrorMessages.NotFound);
            var draftStatusId = await GetClothingStatusIdAsync(ClothingStatusValues.Draft, cancellationToken);

            // 3) Eski taslak
            await DeleteOldDraftsAsync(wardrobeId, draftStatusId, cancellationToken);

            // 4) Python (400 image_unreadable ya da 502 analysis_failed burada fırlayabilir; o durumda yazılmış bir şey yoktur)
            var clothingAnalysis = await _clothingAnalyzer.AnalyzeAsync(
                imageStream, request.FileName, contentType, _currentUser.UserId, cancellationToken);

            // 5–7) Bundan sonra bir adım başarısız olursa Python'un yazdığı görsel sahipsiz kalmasın.
            Clothe newDraft;
            try
            {
                ModelPrediction modelPrediction;
                (newDraft, modelPrediction) = await BuildDraftAsync(wardrobeId, draftStatusId, clothingAnalysis, cancellationToken);
                await _clothingRepository.AddWithPredictionAsync(newDraft, modelPrediction, cancellationToken);
            }
            catch (Exception writeException)
            {
                await DeleteImageAfterFailureAsync(clothingAnalysis.ImagePath, writeException);
                throw;
            }

            // 8) Cevap. Bağlantı üretilemezse imageUrl boş gelir; satır yine yazılmıştır.
            var imageUrl = await _imageStorage.CreateSignedUrlAsync(clothingAnalysis.ImagePath, cancellationToken);
            return AnalyzeClothingResponse.FromClothe(newDraft, imageUrl);
        }

        /// Önce görsel, sonra satır silinir. Görsel silinemezse satır da kalır ve bir sonraki yüklemede yeniden denenir;
        /// tersi olsaydı (önce satır) görselin yolu kaybolur, depoda sahipsiz görsel kalırdı.
        private async Task DeleteOldDraftsAsync(long wardrobeId, long draftStatusId, CancellationToken cancellationToken)
        {
            var oldDrafts = await _clothingRepository.GetDraftsAsync(wardrobeId, draftStatusId, cancellationToken);
            foreach (var oldDraft in oldDrafts.Where(oldDraft => !string.IsNullOrEmpty(oldDraft.ImagePath)))
                await _imageStorage.DeleteAsync(oldDraft.ImagePath!, cancellationToken);

            await _clothingRepository.DeleteAsync(oldDrafts, cancellationToken);
        }

        /// Python'un kodlarını id'ye çevirir ve taslak satırını ile tahmin satırını hazırlar (henüz yazmaz).
        private async Task<(Clothe NewDraft, ModelPrediction ModelPrediction)> BuildDraftAsync(
            long wardrobeId, long draftStatusId, ClothingAnalysis clothingAnalysis, CancellationToken cancellationToken)
        {
            var subtypes = await _lookupRepository.GetSubtypesAsync(cancellationToken);
            var colors = await _lookupRepository.GetColorsAsync(cancellationToken);
            var tones = await _lookupRepository.GetTonesAsync(cancellationToken);

            // Python ortak kod listesindeki kodları döner (A3). Listede olmayan kod sözleşme hatasıdır: 500 ve logda ayrıntı.
            var predictedSubtype = subtypes.FirstOrDefault(subtype => subtype.Value == clothingAnalysis.SubtypeValue)
                ?? throw UnknownCode("alt tür", clothingAnalysis.SubtypeValue);
            var predictedColor = colors.FirstOrDefault(color => color.Value == clothingAnalysis.ColorValue)
                ?? throw UnknownCode("renk", clothingAnalysis.ColorValue);
            var predictedTone = tones.FirstOrDefault(tone => tone.Value == clothingAnalysis.ToneValue)
                ?? throw UnknownCode("ton", clothingAnalysis.ToneValue);

            // Eşik ve üstü: tür ve türden gelen alanlar doldurulur. Altı: hepsi boş, kullanıcı türü kendisi seçer.
            var isConfident = clothingAnalysis.SubtypeConfidence >= _modelOptions.ConfidenceThreshold;
            var draftSubtype = isConfident ? predictedSubtype : null;

            var newDraft = new Clothe
            {
                WardrobeId = wardrobeId,
                StatusId = draftStatusId,
                ImagePath = clothingAnalysis.ImagePath,
                SubtypeId = draftSubtype?.Id,
                CategoryId = draftSubtype?.CategoryId,
                // Listeler bellekteki (önbellek) alt türle paylaşılmasın diye kopyalanır.
                StyleIds = draftSubtype?.DefaultStyleIds.ToList(),
                SeasonIds = draftSubtype?.DefaultSeasonIds.ToList(),
                ThicknessId = draftSubtype?.DefaultThicknessId,
                MinTemp = draftSubtype?.MinTemp,
                MaxTemp = draftSubtype?.MaxTemp,
                MainColorId = predictedColor.Id,
                ToneId = predictedTone.Id,
                IsPatterned = clothingAnalysis.IsPatterned,
                RawColor = clothingAnalysis.RawColor,
            };

            // Tahmin, güven ne olursa olsun olduğu gibi yazılır; modelin doğruluğu buradan ölçülür (T-10).
            var modelPrediction = new ModelPrediction
            {
                SubtypeId = predictedSubtype.Id,
                SubtypeConfidence = clothingAnalysis.SubtypeConfidence,
                MainColorId = predictedColor.Id,
                ToneId = predictedTone.Id,
                IsPatterned = clothingAnalysis.IsPatterned,
                RawColor = clothingAnalysis.RawColor,
            };

            return (newDraft, modelPrediction);
        }

        /// Yazma başarısız olduysa Python'un yazdığı görseli siler. CancellationToken.None: istek iptal edilmiş olsa bile
        /// silme yarıda kesilmesin. Silme de başarısız olursa iki hata birlikte loga gider (görsel yolu mesajda).
        private async Task DeleteImageAfterFailureAsync(string imagePath, Exception writeException)
        {
            try
            {
                await _imageStorage.DeleteAsync(imagePath, CancellationToken.None);
            }
            catch (Exception deleteException)
            {
                throw new AggregateException(
                    $"Taslak yazılamadı ve görsel silinemedi; depoda sahipsiz görsel kaldı: {imagePath}",
                    writeException, deleteException);
            }
        }

        private async Task<long> GetClothingStatusIdAsync(string statusValue, CancellationToken cancellationToken)
        {
            var clothingStatuses = await _lookupRepository.GetClothingStatusesAsync(cancellationToken);
            return clothingStatuses.FirstOrDefault(clothingStatus => clothingStatus.Value == statusValue)?.Id
                ?? throw new InvalidOperationException($"clothing_statuses tablosunda '{statusValue}' yok. Seed çalıştırıldı mı?");
        }

        private static InvalidOperationException UnknownCode(string listName, string codeValue) =>
            new($"Python'un döndüğü {listName} kodu listede yok: '{codeValue}'. Ortak kod listesi ile ml deposu aynı mı?");
    }
}
