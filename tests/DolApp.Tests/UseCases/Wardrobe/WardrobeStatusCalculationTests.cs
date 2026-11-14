using DolApp.Application.UseCases.Wardrobe.GetWardrobeStatus;
using DolApp.Core.Entities;

namespace DolApp.Tests.UseCases.Wardrobe;

/// Dolap şartı hesabı (GetWardrobeStatusResponse.Calculate). Veritabanına gidilmez;
/// şablonlar 002_seed_lookups.sql'deki değerlerle bellekte kurulur:
/// ust_alt = 6 üst, 5 alt, 3 ayakkabı, 2 dış · tek_parca = 5 tek parça, 3 ayakkabı, 2 dış.

public class WardrobeStatusCalculationTests
{
    [Fact]
    public void Calculate_WhenWardrobeIsEmpty_ReturnsTopBottomTemplateWithAllMissing()
    {
        var wardrobeSummary = CreateWardrobeSummary(top: 0, bottom: 0, onePiece: 0, outer: 0, shoes: 0);

        var wardrobeStatus = GetWardrobeStatusResponse.Calculate(wardrobeSummary, CreateSeedTemplateRequirements());

        // Eşitlikte (%0 – %0) ust_alt seçilir; eksik sayısına göre seçilseydi tek_parca çıkardı.
        Assert.False(wardrobeStatus.IsReady);
        Assert.Equal("ust_alt", wardrobeStatus.ClosestTemplate);
        Assert.Equal(4, wardrobeStatus.Requirements.Count);
        Assert.All(wardrobeStatus.Requirements, requirementItem => Assert.Equal(0, requirementItem.Have));
    }

    [Fact]
    public void Calculate_WithApiDocumentExample_ReturnsTopBottomTemplateWithAllCategories()
    {
        // API belgesindeki örnek: 4 üst, 5 alt, 2 ayakkabı, 2 dış = 13.
        var wardrobeSummary = CreateWardrobeSummary(top: 4, bottom: 5, onePiece: 0, outer: 2, shoes: 2);

        var wardrobeStatus = GetWardrobeStatusResponse.Calculate(wardrobeSummary, CreateSeedTemplateRequirements());

        Assert.False(wardrobeStatus.IsReady);
        Assert.Equal(13, wardrobeStatus.TotalCount);
        Assert.Equal("ust_alt", wardrobeStatus.ClosestTemplate);
        Assert.Collection(wardrobeStatus.Requirements,
            requirementItem => AssertRequirement(requirementItem, "ust", have: 4, need: 6),
            requirementItem => AssertRequirement(requirementItem, "alt", have: 5, need: 5),
            requirementItem => AssertRequirement(requirementItem, "ayakkabi", have: 2, need: 3),
            requirementItem => AssertRequirement(requirementItem, "dis", have: 2, need: 2));
    }

    [Fact]
    public void Calculate_WhenOnePieceTemplateIsCloser_ReturnsOnePieceTemplate()
    {
        // 3 tek parça + 1 ayakkabı: ust_alt %6, tek_parca %40.
        var wardrobeSummary = CreateWardrobeSummary(top: 0, bottom: 0, onePiece: 3, outer: 0, shoes: 1);

        var wardrobeStatus = GetWardrobeStatusResponse.Calculate(wardrobeSummary, CreateSeedTemplateRequirements());

        Assert.False(wardrobeStatus.IsReady);
        Assert.Equal("tek_parca", wardrobeStatus.ClosestTemplate);
        Assert.Collection(wardrobeStatus.Requirements,
            requirementItem => AssertRequirement(requirementItem, "tek_parca", have: 3, need: 5),
            requirementItem => AssertRequirement(requirementItem, "ayakkabi", have: 1, need: 3),
            requirementItem => AssertRequirement(requirementItem, "dis", have: 0, need: 2));
    }

    [Fact]
    public void Calculate_WhenTopsAreExtraButOthersMissing_DoesNotCountExtraTops()
    {
        // 20 üst, başka hiçbir şey yok: fazla üstler eksikleri kapatmamalı (her kategori en fazla gereken kadar sayılır).
        var wardrobeSummary = CreateWardrobeSummary(top: 20, bottom: 0, onePiece: 0, outer: 0, shoes: 0);

        var wardrobeStatus = GetWardrobeStatusResponse.Calculate(wardrobeSummary, CreateSeedTemplateRequirements());

        Assert.False(wardrobeStatus.IsReady);
        Assert.Equal("ust_alt", wardrobeStatus.ClosestTemplate);
    }

    [Fact]
    public void Calculate_WhenTopBottomTemplateIsComplete_ReturnsReadyWithNoRequirements()
    {
        var wardrobeSummary = CreateWardrobeSummary(top: 6, bottom: 5, onePiece: 0, outer: 2, shoes: 3);

        var wardrobeStatus = GetWardrobeStatusResponse.Calculate(wardrobeSummary, CreateSeedTemplateRequirements());

        Assert.True(wardrobeStatus.IsReady);
        Assert.Equal("ust_alt", wardrobeStatus.ClosestTemplate);
        Assert.Empty(wardrobeStatus.Requirements);
    }

    private static void AssertRequirement(WardrobeRequirementItem requirementItem, string category, int have, int need)
    {
        Assert.Equal(category, requirementItem.Category);
        Assert.Equal(have, requirementItem.Have);
        Assert.Equal(need, requirementItem.Need);
    }

    private static WardrobeSummary CreateWardrobeSummary(int top, int bottom, int onePiece, int outer, int shoes)
    {
        return new WardrobeSummary
        {
            WardrobeId = 1,
            TopCount = top,
            BottomCount = bottom,
            OnePieceCount = onePiece,
            OuterCount = outer,
            ShoesCount = shoes,
            AccessoryCount = 0,
            TotalCount = top + bottom + onePiece + outer + shoes,
        };
    }

    /// 002_seed_lookups.sql'deki şablon gereksinimlerinin bellekteki kopyası.
    private static List<TemplateRequirement> CreateSeedTemplateRequirements()
    {
        var topBottomTemplate = new OutfitTemplate { Id = 1, Value = "ust_alt", DisplayName = "Üst ve alt" };
        var onePieceTemplate = new OutfitTemplate { Id = 2, Value = "tek_parca", DisplayName = "Tek parça" };

        var topCategory = new Category { Id = 1, Value = "ust", DisplayName = "Üst" };
        var bottomCategory = new Category { Id = 2, Value = "alt", DisplayName = "Alt" };
        var onePieceCategory = new Category { Id = 3, Value = "tek_parca", DisplayName = "Tek parça" };
        var outerCategory = new Category { Id = 4, Value = "dis", DisplayName = "Dış giyim" };
        var shoesCategory = new Category { Id = 5, Value = "ayakkabi", DisplayName = "Ayakkabı" };

        return new List<TemplateRequirement>
        {
            CreateTemplateRequirement(1, topBottomTemplate, topCategory, 6),
            CreateTemplateRequirement(2, topBottomTemplate, bottomCategory, 5),
            CreateTemplateRequirement(3, topBottomTemplate, shoesCategory, 3),
            CreateTemplateRequirement(4, topBottomTemplate, outerCategory, 2),
            CreateTemplateRequirement(5, onePieceTemplate, onePieceCategory, 5),
            CreateTemplateRequirement(6, onePieceTemplate, shoesCategory, 3),
            CreateTemplateRequirement(7, onePieceTemplate, outerCategory, 2),
        };
    }

    private static TemplateRequirement CreateTemplateRequirement(
        long id, OutfitTemplate outfitTemplate, Category category, int minimumRequired)
    {
        return new TemplateRequirement
        {
            Id = id,
            TemplateId = outfitTemplate.Id,
            Template = outfitTemplate,
            CategoryId = category.Id,
            Category = category,
            MinRequired = minimumRequired,
        };
    }
}