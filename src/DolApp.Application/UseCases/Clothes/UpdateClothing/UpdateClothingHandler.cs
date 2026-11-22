using DolApp.Application.Interfaces;
using DolApp.Core.Constants;
using DolApp.Core.Entities;
using DolApp.Core.Errors;
using MediatR;

namespace DolApp.Application.UseCases.Clothes.UpdateClothing;

/// Etiket düzenleme (FR-09, S5; T-18). Akış:
///   1) Parça kullanıcının ve onaylı mı (değilse 404: taslak, silinmiş ya da başkasının; T-17).
///   2) Gönderilmeyen alan mevcut değerinde kalır. Gönderilen id listede yoksa 400 invalid_id.
///   3) Değişen her alan için bir label_changes satırı, aşama "sonradan". Karşılaştırma parçanın mevcut değeriyle
///      (onaydaki gibi model tahminiyle değil). Değerler onaydaki gibi kodla yazılır.
///   4) Alt tür değiştiyse kategori ve sıcaklık aralığı yeni türden; tarz, mevsim, kalınlık kullanıcınınki kalır.
///   3 ve 4 tek SaveChanges ile yazılır = tek işlem. updated_at'i veritabanındaki tetikleyici yazar.
///   5) Cevap: güncel parça.

public class UpdateClothingHandler : IRequestHandler<UpdateClothingRequest, UpdateClothingResponse>
{
    private readonly ICurrentUser _currentUser;
    private readonly IWardrobeRepository _wardrobeRepository;
    private readonly IClothingRepository _clothingRepository;
    private readonly ILookupRepository _lookupRepository;
    private readonly IImageStorage _imageStorage;

    public UpdateClothingHandler(
        ICurrentUser currentUser,
        IWardrobeRepository wardrobeRepository,
        IClothingRepository clothingRepository,
        ILookupRepository lookupRepository,
        IImageStorage imageStorage)
    {
        _currentUser = currentUser;
        _wardrobeRepository = wardrobeRepository;
        _clothingRepository = clothingRepository;
        _lookupRepository = lookupRepository;
        _imageStorage = imageStorage;
    }

    public async Task<UpdateClothingResponse> Handle(UpdateClothingRequest request, CancellationToken cancellationToken)
    {
        // 1) Parça ve durumu
        var wardrobeId = await _wardrobeRepository.GetWardrobeIdByUserIdAsync(_currentUser.UserId, cancellationToken)
            ?? throw new AppException(404, ErrorCodes.NotFound, ErrorMessages.NotFound);
        var clothe = await _clothingRepository.GetForUpdateAsync(request.ClothingId, wardrobeId, cancellationToken)
            ?? throw new AppException(404, ErrorCodes.NotFound, ErrorMessages.NotFound);

        var clothingStatuses = await _lookupRepository.GetClothingStatusesAsync(cancellationToken);
        if (clothe.StatusId != clothingStatuses.First(status => status.Value == ClothingStatusValues.Approved).Id)
            throw new AppException(404, ErrorCodes.NotFound, ErrorMessages.NotFound);

        // 2) Yeni değerler: gönderilmeyen alan mevcut değerinde kalır (onaylı parçada bu alanlar hep dolu)
        var subtypes = await _lookupRepository.GetSubtypesAsync(cancellationToken);
        var colors = await _lookupRepository.GetColorsAsync(cancellationToken);
        var tones = await _lookupRepository.GetTonesAsync(cancellationToken);
        var styles = await _lookupRepository.GetStylesAsync(cancellationToken);
        var seasons = await _lookupRepository.GetSeasonsAsync(cancellationToken);
        var thicknesses = await _lookupRepository.GetThicknessesAsync(cancellationToken);

        var subtypeId = request.SubtypeId ?? clothe.SubtypeId!.Value;
        var mainColorId = request.MainColorId ?? clothe.MainColorId!.Value;
        var toneId = request.ToneId ?? clothe.ToneId!.Value;
        var isPatterned = request.IsPatterned ?? clothe.IsPatterned;
        var styleIds = Normalize(request.StyleIds ?? clothe.StyleIds ?? new List<long>());
        var seasonIds = Normalize(request.SeasonIds ?? clothe.SeasonIds ?? new List<long>());
        var thicknessId = request.ThicknessId ?? clothe.ThicknessId!.Value;

        var subtype = subtypes.FirstOrDefault(subtypeItem => subtypeItem.Id == subtypeId) ?? throw InvalidId();
        if (colors.All(color => color.Id != mainColorId)
            || tones.All(tone => tone.Id != toneId)
            || styleIds.Any(styleId => styles.All(style => style.Id != styleId))
            || seasonIds.Any(seasonId => seasons.All(season => season.Id != seasonId))
            || thicknesses.All(thickness => thickness.Id != thicknessId))
            throw InvalidId();

        // 3) Değişen alanlar → label_changes (aşama "sonradan"). Eski değer parçanın şu anki değeri.
        var labelFields = await _lookupRepository.GetLabelFieldsAsync(cancellationToken);
        var labelChangeStages = await _lookupRepository.GetLabelChangeStagesAsync(cancellationToken);
        var laterStageId = labelChangeStages.First(stage => stage.Value == LabelChangeStageValues.Later).Id;

        void AddIfChanged(string fieldValue, string? oldValue, string? newValue)
        {
            if (oldValue == newValue)
                return;

            clothe.LabelChanges.Add(new LabelChange
            {
                FieldId = labelFields.First(field => field.Value == fieldValue).Id,
                OldValue = oldValue,
                NewValue = newValue,
                StageId = laterStageId,
            });
        }

        var subtypeCodes = subtypes.ToDictionary(item => item.Id, item => item.Value);
        var colorCodes = colors.ToDictionary(item => item.Id, item => item.Value);
        var toneCodes = tones.ToDictionary(item => item.Id, item => item.Value);
        var styleCodes = styles.ToDictionary(item => item.Id, item => item.Value);
        var seasonCodes = seasons.ToDictionary(item => item.Id, item => item.Value);
        var thicknessCodes = thicknesses.ToDictionary(item => item.Id, item => item.Value);

        AddIfChanged(LabelFieldValues.Subtype, subtypeCodes[clothe.SubtypeId!.Value], subtype.Value);
        AddIfChanged(LabelFieldValues.MainColor, colorCodes[clothe.MainColorId!.Value], colorCodes[mainColorId]);
        AddIfChanged(LabelFieldValues.Tone, toneCodes[clothe.ToneId!.Value], toneCodes[toneId]);
        AddIfChanged(LabelFieldValues.Pattern, PatternText(clothe.IsPatterned), PatternText(isPatterned));
        AddIfChanged(LabelFieldValues.Style, CodesOf(styleCodes, clothe.StyleIds ?? new List<long>()), CodesOf(styleCodes, styleIds));
        AddIfChanged(LabelFieldValues.Season, CodesOf(seasonCodes, clothe.SeasonIds ?? new List<long>()), CodesOf(seasonCodes, seasonIds));
        AddIfChanged(LabelFieldValues.Thickness, thicknessCodes[clothe.ThicknessId!.Value], thicknessCodes[thicknessId]);

        // 4) Yeni değerler. Alt tür değiştiyse kategori ve sıcaklık aralığı yeni türden (T-18).
        if (subtype.Id != clothe.SubtypeId)
        {
            clothe.CategoryId = subtype.CategoryId;
            clothe.MinTemp = subtype.MinTemp;
            clothe.MaxTemp = subtype.MaxTemp;
        }

        clothe.SubtypeId = subtype.Id;
        clothe.MainColorId = mainColorId;
        clothe.ToneId = toneId;
        clothe.IsPatterned = isPatterned;
        clothe.StyleIds = styleIds;
        clothe.SeasonIds = seasonIds;
        clothe.ThicknessId = thicknessId;

        await _clothingRepository.SaveChangesAsync(cancellationToken);

        // 5) Cevap
        var imageUrl = string.IsNullOrEmpty(clothe.ImagePath)
            ? null
            : await _imageStorage.CreateSignedUrlAsync(clothe.ImagePath, cancellationToken);

        return UpdateClothingResponse.FromClothe(clothe, imageUrl);
    }

    private static List<long> Normalize(IEnumerable<long> ids) => ids.Distinct().Order().ToList();

    private static string PatternText(bool isPatterned) => isPatterned ? "desenli" : "desensiz";

    /// Liste değerini kodlarla, id sırasıyla ve virgülle yazar: [4, 2] → "gunluk,spor". Sıra farkı değişiklik sayılmaz.
    private static string CodesOf(Dictionary<long, string> codesById, IEnumerable<long> ids) =>
        string.Join(",", Normalize(ids).Select(id => codesById[id]));

    private static AppException InvalidId() => new(400, ErrorCodes.InvalidId, ErrorMessages.InvalidId);
}