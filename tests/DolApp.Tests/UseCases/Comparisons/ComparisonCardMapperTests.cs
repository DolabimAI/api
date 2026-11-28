using DolApp.Application.UseCases.Comparisons.Common;
using DolApp.Core.Entities;

namespace DolApp.Tests.UseCases.Comparisons;

/// comparisons satırının uygulama kartına çevrilmesi. Yöntem id'leri: 1 kural, 2 öğrenen.

public class ComparisonCardMapperTests
{
    private const long RuleMethodId = 1;
    private const long LearnedMethodId = 2;

    // 18 tişört (görselli), 26 kot (görselli), 36 ayakkabı (görselsiz), 19 gömlek (görselli), 40 atkı (görselli)
    private static readonly Dictionary<long, Clothe> ClothesById = new()
    {
        [18] = new Clothe { Id = 18, CategoryId = 1, ImagePath = "u/18.webp" },
        [26] = new Clothe { Id = 26, CategoryId = 2, ImagePath = "u/26.webp" },
        [36] = new Clothe { Id = 36, CategoryId = 5, ImagePath = null },
        [19] = new Clothe { Id = 19, CategoryId = 1, ImagePath = "u/19.webp" },
        [40] = new Clothe { Id = 40, CategoryId = 6, ImagePath = "u/40.webp" },
    };

    private static readonly Dictionary<string, string> ImageUrls = new()
    {
        ["u/18.webp"] = "https://img/18",
        ["u/26.webp"] = "https://img/26",
        ["u/19.webp"] = "https://img/19",
        ["u/40.webp"] = "https://img/40",
    };

    [Fact]
    public void ToCard_WhenRuleIsLeft_PutsRuleOutfitLeft()
    {
        var comparison = CreateComparison(leftMethodId: RuleMethodId);

        var card = ComparisonCardMapper.ToCard(comparison, RuleMethodId, ClothesById, ImageUrls);

        Assert.Equal(5021, card.Id);
        Assert.Equal(1, card.SequenceNo);
        Assert.False(card.IsSingleCard);
        Assert.Equal(new long[] { 18, 26, 36 }, card.Left.Select(item => item.Id));
        Assert.Equal(new long[] { 19, 26, 36 }, card.Right.Select(item => item.Id));
    }

    [Fact]
    public void ToCard_WhenLearnedIsLeft_PutsLearnedOutfitLeft()
    {
        var comparison = CreateComparison(leftMethodId: LearnedMethodId);

        var card = ComparisonCardMapper.ToCard(comparison, RuleMethodId, ClothesById, ImageUrls);

        Assert.Equal(new long[] { 19, 26, 36 }, card.Left.Select(item => item.Id));
        Assert.Equal(new long[] { 18, 26, 36 }, card.Right.Select(item => item.Id));
    }

    [Fact]
    public void ToCard_WhenOutfitsAreTheSame_ReturnsSingleCardWithEmptyRight()
    {
        var comparison = CreateComparison(leftMethodId: RuleMethodId);
        comparison.LearnedOutfit = new List<long> { 36, 18, 26 };  // aynı küme, farklı sıra

        var card = ComparisonCardMapper.ToCard(comparison, RuleMethodId, ClothesById, ImageUrls);

        Assert.True(card.IsSingleCard);
        Assert.Equal(new long[] { 18, 26, 36 }, card.Left.Select(item => item.Id));
        Assert.Empty(card.Right);
    }

    [Fact]
    public void ToCard_SetsCategoryImageUrlAndAccessories()
    {
        var comparison = CreateComparison(leftMethodId: RuleMethodId);

        var card = ComparisonCardMapper.ToCard(comparison, RuleMethodId, ClothesById, ImageUrls);

        Assert.Equal(1, card.Left[0].CategoryId);
        Assert.Equal("https://img/18", card.Left[0].ImageUrl);
        Assert.Null(card.Left[2].ImageUrl);  // görseli olmayan parça
        var accessory = Assert.Single(card.Accessories);
        Assert.Equal(40, accessory.Id);
        Assert.Equal("https://img/40", accessory.ImageUrl);
    }

    [Fact]
    public void CollectImagePaths_ReturnsDistinctPathsWithoutEmpty()
    {
        var comparison = CreateComparison(leftMethodId: RuleMethodId);

        var imagePaths = ComparisonCardMapper.CollectImagePaths(comparison, ClothesById);

        Assert.Equal(new[] { "u/18.webp", "u/19.webp", "u/26.webp", "u/40.webp" }, imagePaths.OrderBy(path => path));
    }

    private static Comparison CreateComparison(long leftMethodId) =>
        new()
        {
            Id = 5021,
            SequenceNo = 1,
            LeftMethodId = leftMethodId,
            RuleOutfit = new List<long> { 18, 26, 36 },
            LearnedOutfit = new List<long> { 19, 26, 36 },
            Accessories = new List<long> { 40 },
        };
}