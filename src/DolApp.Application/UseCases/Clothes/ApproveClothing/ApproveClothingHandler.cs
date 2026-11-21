using DolApp.Application.Interfaces;
using DolApp.Application.UseCases.Clothes.AnalyzeClothing;
using DolApp.Application.UseCases.Wardrobe.GetWardrobeStatus;
using DolApp.Core.Constants;
using DolApp.Core.Entities;
using DolApp.Core.Errors;
using MediatR;

namespace DolApp.Application.UseCases.Clothes.ApproveClothing;

/// Taslak parçayı onaylar (Kaydet; FR-08, S3). Akış:
///   1) Parça kullanıcının dolabında mı (değilse 404) ve taslak mı (değilse 409 not_a_draft).
///   2) Gönderilen id'ler listelerde var mı (yoksa 400 invalid_id). Tarz, mevsim, kalınlık gelmediyse türün varsayılanı.
///   3) Değişen her alan için bir label_changes satırı, aşama "onay". Karşılaştırma:
///        alt tür, ana renk, ton, desen → modelin tahmini (model_predictions)
///        tarz, mevsim, kalınlık        → seçilen türün varsayılanı
///      Yalnızca son değer karşılaştırılır; ekranda aynı alan iki kez değiştirilse de tek satır yazılır (T-11).
///   4) Parça onaylı olur; kategori ve sıcaklık aralığı seçilen türden yeniden alınır; approved_at yazılır.
///   3 ve 4 tek SaveChanges ile yazılır = tek işlem.
///   5) Cevap: parça (analyze biçimi) ve güncel dolap durumu.

public class ApproveClothingHandler : IRequestHandler<ApproveClothingRequest, ApproveClothingResponse>
{
    private readonly ICurrentUser _currentUser;
    private readonly IWardrobeRepository _wardrobeRepository;
    private readonly IClothingRepository _clothingRepository;
    private readonly ILookupRepository _lookupRepository;
    private readonly IImageStorage _imageStorage;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ApproveClothingHandler(
        ICurrentUser currentUser,
        IWardrobeRepository wardrobeRepository,
        IClothingRepository clothingRepository,
        ILookupRepository lookupRepository,
        IImageStorage imageStorage,
        IDateTimeProvider dateTimeProvider)
    {
        _currentUser = currentUser;
        _wardrobeRepository = wardrobeRepository;
        _clothingRepository = clothingRepository;
        _lookupRepository = lookupRepository;
        _imageStorage = imageStorage;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<ApproveClothingResponse> Handle(ApproveClothingRequest request, CancellationToken cancellationToken)
    {
        // 1) Parça ve durumu
        var wardrobeId = await _wardrobeRepository.GetWardrobeIdByUserIdAsync(_currentUser.UserId, cancellationToken)
            ?? throw new AppException(404, ErrorCodes.NotFound, ErrorMessages.NotFound);
        var clothe = await _clothingRepository.GetForUpdateAsync(request.ClothingId, wardrobeId, cancellationToken)
            ?? throw new AppException(404, ErrorCodes.NotFound, ErrorMessages.NotFound);

        var clothingStatuses = await _lookupRepository.GetClothingStatusesAsync(cancellationToken);
        if (clothe.StatusId != clothingStatuses.First(status => status.Value == ClothingStatusValues.Draft).Id)
            throw new AppException(409, ErrorCodes.NotADraft, ErrorMessages.NotADraft);

        var modelPrediction = clothe.ModelPrediction
            ?? throw new InvalidOperationException($"Taslak parça {clothe.Id} için model_predictions satırı yok.");

        // 2) Id kontrolleri ve varsayılanlar
        var subtypes = await _lookupRepository.GetSubtypesAsync(cancellationToken);
        var colors = await _lookupRepository.GetColorsAsync(cancellationToken);
        var tones = await _lookupRepository.GetTonesAsync(cancellationToken);
        var styles = await _lookupRepository.GetStylesAsync(cancellationToken);
        var seasons = await _lookupRepository.GetSeasonsAsync(cancellationToken);
        var thicknesses = await _lookupRepository.GetThicknessesAsync(cancellationToken);

        var subtype = subtypes.FirstOrDefault(subtypeItem => subtypeItem.Id == request.SubtypeId) ?? throw InvalidId();
        var mainColor = colors.FirstOrDefault(color => color.Id == request.MainColorId) ?? throw InvalidId();
        var tone = tones.FirstOrDefault(toneItem => toneItem.Id == request.ToneId) ?? throw InvalidId();

        // Liste gönderilmediyse ya da boşsa türün varsayılanı. Sıralı ve tekrarsız saklanır.
        var styleIds = Normalize(request.StyleIds is { Count: > 0 } ? request.StyleIds : subtype.DefaultStyleIds);
        var seasonIds = Normalize(request.SeasonIds is { Count: > 0 } ? request.SeasonIds : subtype.DefaultSeasonIds);
        var thicknessId = request.ThicknessId ?? subtype.DefaultThicknessId;

        if (styleIds.Any(styleId => styles.All(style => style.Id != styleId))
            || seasonIds.Any(seasonId => seasons.All(season => season.Id != seasonId))
            || thicknesses.All(thickness => thickness.Id != thicknessId))
            throw InvalidId();

        // 3) Değişen alanlar → label_changes (aşama "onay")
        var labelFields = await _lookupRepository.GetLabelFieldsAsync(cancellationToken);
        var labelChangeStages = await _lookupRepository.GetLabelChangeStagesAsync(cancellationToken);
        var approvalStageId = labelChangeStages.First(stage => stage.Value == LabelChangeStageValues.Approval).Id;

        void AddIfChanged(string fieldValue, string? oldValue, string? newValue)
        {
            if (oldValue == newValue)
                return;

            clothe.LabelChanges.Add(new LabelChange
            {
                FieldId = labelFields.First(field => field.Value == fieldValue).Id,
                OldValue = oldValue,
                NewValue = newValue,
                StageId = approvalStageId,
            });
        }

        // Değerler kodla yazılır (gomlek, beyaz; listeler "gunluk,spor"); analizde okunur, id değişse de anlamı kalır.
        var subtypeCodes = subtypes.ToDictionary(item => item.Id, item => item.Value);
        var colorCodes = colors.ToDictionary(item => item.Id, item => item.Value);
        var toneCodes = tones.ToDictionary(item => item.Id, item => item.Value);
        var styleCodes = styles.ToDictionary(item => item.Id, item => item.Value);
        var seasonCodes = seasons.ToDictionary(item => item.Id, item => item.Value);
        var thicknessCodes = thicknesses.ToDictionary(item => item.Id, item => item.Value);

        AddIfChanged(LabelFieldValues.Subtype, subtypeCodes[modelPrediction.SubtypeId], subtype.Value);
        AddIfChanged(LabelFieldValues.MainColor, colorCodes[modelPrediction.MainColorId], mainColor.Value);
        AddIfChanged(LabelFieldValues.Tone, toneCodes[modelPrediction.ToneId], tone.Value);
        AddIfChanged(LabelFieldValues.Pattern, PatternText(modelPrediction.IsPatterned), PatternText(request.IsPatterned));
        AddIfChanged(LabelFieldValues.Style, CodesOf(styleCodes, subtype.DefaultStyleIds), CodesOf(styleCodes, styleIds));
        AddIfChanged(LabelFieldValues.Season, CodesOf(seasonCodes, subtype.DefaultSeasonIds), CodesOf(seasonCodes, seasonIds));
        AddIfChanged(LabelFieldValues.Thickness, thicknessCodes[subtype.DefaultThicknessId], thicknessCodes[thicknessId]);

        // 4) Parça onaylı. Kategori ve sıcaklık aralığı son seçilen türden.
        clothe.SubtypeId = subtype.Id;
        clothe.CategoryId = subtype.CategoryId;
        clothe.MinTemp = subtype.MinTemp;
        clothe.MaxTemp = subtype.MaxTemp;
        clothe.MainColorId = mainColor.Id;
        clothe.ToneId = tone.Id;
        clothe.IsPatterned = request.IsPatterned;
        clothe.StyleIds = styleIds;
        clothe.SeasonIds = seasonIds;
        clothe.ThicknessId = thicknessId;
        clothe.StatusId = clothingStatuses.First(status => status.Value == ClothingStatusValues.Approved).Id;
        clothe.ApprovedAt = _dateTimeProvider.UtcNow;

        await _clothingRepository.SaveChangesAsync(cancellationToken);

        // 5) Güncel dolap durumu (C4'teki hesap) ve parça
        var wardrobeSummary = await _wardrobeRepository.GetSummaryByUserIdAsync(_currentUser.UserId, cancellationToken)
            ?? throw new AppException(404, ErrorCodes.NotFound, ErrorMessages.NotFound);
        var templateRequirements = await _lookupRepository.GetTemplateRequirementsAsync(cancellationToken);

        var imageUrl = string.IsNullOrEmpty(clothe.ImagePath)
            ? null
            : await _imageStorage.CreateSignedUrlAsync(clothe.ImagePath, cancellationToken);

        return new ApproveClothingResponse
        {
            Clothing = AnalyzeClothingResponse.FromClothe(clothe, imageUrl),
            WardrobeStatus = GetWardrobeStatusResponse.Calculate(wardrobeSummary, templateRequirements),
        };
    }

    private static List<long> Normalize(IEnumerable<long> ids) => ids.Distinct().Order().ToList();

    private static string PatternText(bool isPatterned) => isPatterned ? "desenli" : "desensiz";

    /// Liste değerini kodlarla, id sırasıyla ve virgülle yazar: [4, 2] → "gunluk,spor". Sıra farkı değişiklik sayılmaz.
    private static string CodesOf(Dictionary<long, string> codesById, IEnumerable<long> ids) =>
        string.Join(",", Normalize(ids).Select(id => codesById[id]));

    private static AppException InvalidId() => new(400, ErrorCodes.InvalidId, ErrorMessages.InvalidId);
}