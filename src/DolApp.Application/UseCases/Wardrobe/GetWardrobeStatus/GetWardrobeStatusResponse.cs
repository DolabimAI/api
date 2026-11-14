using DolApp.Core.Constants;
using DolApp.Core.Entities;

namespace DolApp.Application.UseCases.Wardrobe.GetWardrobeStatus;

/// GET /api/wardrobe/status cevabı. Parça onaylama ve silme uçları (C3) da aynı cevabı döner;
/// bu yüzden hesap Calculate metodundadır ve veritabanına gitmez (birim testiyle denenir).

public class GetWardrobeStatusResponse
{
    /// Onaylı parçaların toplamı (aksesuar dahil).
    public int TotalCount { get; set; }

    /// Şablonlardan biri tamam mı? true ise Kombin öner açılır, eksik kartı gizlenir.
    public bool IsReady { get; set; }

    /// Tamamlanmaya en yakın şablon (ust_alt ya da tek_parca). Hazırsa tamamlanan şablon.
    public string ClosestTemplate { get; set; } = string.Empty;

    /// En yakın şablonun bütün kategorileri (tamam olanlar dahil). Hazırsa boş.
    public List<WardrobeRequirementItem> Requirements { get; set; } = new();

    /// Dolap sayılarını şablon gereksinimleriyle karşılaştırır.
    /// En yakın şablon: tamamlanma oranı en yüksek olan (karşılanan / gereken; bir kategoride fazla parça
    /// oranı şişirmesin diye her kategori en fazla gereken kadar sayılır). Eşitlikte şablon id sırası (ust_alt önce).
    /// Eksik sayısına göre seçilseydi, toplamda daha az parça isteyen tek_parca şablonu boş dolapta da öne geçerdi.
    public static GetWardrobeStatusResponse Calculate(
        WardrobeSummary wardrobeSummary, IReadOnlyCollection<TemplateRequirement> templateRequirements)
    {
        var templateStatuses = templateRequirements
            .GroupBy(templateRequirement => templateRequirement.TemplateId)
            .OrderBy(templateGroup => templateGroup.Key)
            .Select(templateGroup =>
            {
                var requirementItems = templateGroup
                    .OrderBy(templateRequirement => templateRequirement.Id)
                    .Select(templateRequirement => new WardrobeRequirementItem
                    {
                        Category = templateRequirement.Category.Value,
                        Have = GetApprovedCount(wardrobeSummary, templateRequirement.Category.Value),
                        Need = templateRequirement.MinRequired,
                    })
                    .ToList();

                var totalNeeded = requirementItems.Sum(requirementItem => requirementItem.Need);
                var totalCovered = requirementItems.Sum(requirementItem => Math.Min(requirementItem.Have, requirementItem.Need));

                return new
                {
                    TemplateValue = templateGroup.First().Template.Value,
                    RequirementItems = requirementItems,
                    IsComplete = totalCovered >= totalNeeded,
                    CompletionRatio = totalNeeded == 0 ? 1.0 : (double)totalCovered / totalNeeded,
                };
            })
            .ToList();

        // OrderByDescending sıralaması kararlıdır: oranlar eşitse önceki sıra (şablon id) korunur.
        var closestTemplate = templateStatuses
            .OrderByDescending(templateStatus => templateStatus.CompletionRatio)
            .First();

        return new GetWardrobeStatusResponse
        {
            TotalCount = wardrobeSummary.TotalCount ?? 0,
            IsReady = closestTemplate.IsComplete,
            ClosestTemplate = closestTemplate.TemplateValue,
            Requirements = closestTemplate.IsComplete ? new List<WardrobeRequirementItem>() : closestTemplate.RequirementItems,
        };
    }

    /// wardrobe_summary görünümünde her kategorinin ayrı bir sütunu var; kategori koduna göre doğru sütun seçilir.
    private static int GetApprovedCount(WardrobeSummary wardrobeSummary, string categoryValue)
    {
        return categoryValue switch
        {
            CategoryValues.Top => wardrobeSummary.TopCount ?? 0,
            CategoryValues.Bottom => wardrobeSummary.BottomCount ?? 0,
            CategoryValues.OnePiece => wardrobeSummary.OnePieceCount ?? 0,
            CategoryValues.Outer => wardrobeSummary.OuterCount ?? 0,
            CategoryValues.Shoes => wardrobeSummary.ShoesCount ?? 0,
            _ => throw new InvalidOperationException(
                $"wardrobe_summary görünümünde '{categoryValue}' kategorisi için sütun yok."),
        };
    }
}