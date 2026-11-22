using DolApp.Application.UseCases.Clothes.GetClothes;
using DolApp.Core.Entities;

namespace DolApp.Tests.UseCases.Clothes;

/// Dolabım listesinin grupları: Türkçe sıralama, sayılar, grup içi sıra, filtre ve eksik görsel.

public class ClothingGroupBuilderTests
{
    private static readonly Dictionary<long, string> SubtypeNames = new()
    {
        [1] = "Şapka",
        [2] = "Bot",
        [3] = "Çanta",
        [4] = "Ceket",
        [5] = "Sweatshirt",
    };

    private static readonly Dictionary<string, string> ImageUrls = new()
    {
        ["u/1.webp"] = "https://depo/1",
        ["u/2.webp"] = "https://depo/2",
        ["u/3.webp"] = "https://depo/3",
    };

    [Fact]
    public void Build_SortsGroupsInTurkishAlphabeticalOrder()
    {
        var clothes = new[]
        {
            Clothe(1, subtypeId: 1), Clothe(2, subtypeId: 2), Clothe(3, subtypeId: 3),
            Clothe(4, subtypeId: 4), Clothe(5, subtypeId: 5),
        };

        var groups = ClothingGroupBuilder.Build(clothes, SubtypeNames, ImageUrls, subtypeId: null);

        Assert.Equal(new[] { "Bot", "Ceket", "Çanta", "Sweatshirt", "Şapka" }, groups.Select(group => group.Name));
    }

    [Fact]
    public void Build_CountsItemsAndPutsLatestApprovedFirst()
    {
        var clothes = new[]
        {
            Clothe(10, subtypeId: 2, approvedAt: new DateTime(2026, 11, 20)),
            Clothe(11, subtypeId: 2, approvedAt: new DateTime(2026, 11, 22)),
            Clothe(12, subtypeId: 4, approvedAt: new DateTime(2026, 11, 21)),
        };

        var groups = ClothingGroupBuilder.Build(clothes, SubtypeNames, ImageUrls, subtypeId: null);

        var bootGroup = Assert.Single(groups, group => group.SubtypeId == 2);
        Assert.Equal(2, bootGroup.Count);
        Assert.Equal(new long[] { 11, 10 }, bootGroup.Items.Select(item => item.Id));
    }

    [Fact]
    public void Build_WhenSubtypeFilterGiven_ReturnsOnlyThatGroup()
    {
        var clothes = new[] { Clothe(1, subtypeId: 2), Clothe(2, subtypeId: 4), Clothe(3, subtypeId: 4) };

        var groups = ClothingGroupBuilder.Build(clothes, SubtypeNames, ImageUrls, subtypeId: 4);

        var group = Assert.Single(groups);
        Assert.Equal("Ceket", group.Name);
        Assert.Equal(2, group.Count);
    }

    [Fact]
    public void Build_WhenFilterHasNoItems_ReturnsEmpty()
    {
        var clothes = new[] { Clothe(1, subtypeId: 2) };

        var groups = ClothingGroupBuilder.Build(clothes, SubtypeNames, ImageUrls, subtypeId: 5);

        Assert.Empty(groups);
    }

    [Fact]
    public void Build_WhenImageMissingInStorage_ReturnsNullImageUrl()
    {
        var clothes = new[] { Clothe(1, subtypeId: 2), Clothe(9, subtypeId: 2) };   // u/9.webp depoda yok

        var groups = ClothingGroupBuilder.Build(clothes, SubtypeNames, ImageUrls, subtypeId: null);

        var items = Assert.Single(groups).Items;
        Assert.Equal("https://depo/1", items.Single(item => item.Id == 1).ImageUrl);
        Assert.Null(items.Single(item => item.Id == 9).ImageUrl);
    }

    private static Clothe Clothe(long id, long subtypeId, DateTime? approvedAt = null)
    {
        return new Clothe
        {
            Id = id,
            SubtypeId = subtypeId,
            ImagePath = $"u/{id}.webp",
            ApprovedAt = approvedAt ?? new DateTime(2026, 11, 22),
        };
    }
}