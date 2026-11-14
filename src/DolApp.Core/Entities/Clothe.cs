using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// Dolaptaki her parça. Fotoğraf yüklenince taslak, Kaydet ile onayli.
/// </summary>
public partial class Clothe
{
    public long Id { get; set; }

    public long WardrobeId { get; set; }

    public string? ImagePath { get; set; }

    public long? SubtypeId { get; set; }

    public long? CategoryId { get; set; }

    public List<long>? StyleIds { get; set; }

    public List<long>? SeasonIds { get; set; }

    public long? ThicknessId { get; set; }

    public int? MinTemp { get; set; }

    public int? MaxTemp { get; set; }

    public long? MainColorId { get; set; }

    public long? ToneId { get; set; }

    public bool IsPatterned { get; set; }

    public string? RawColor { get; set; }

    public long StatusId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<LabelChange> LabelChanges { get; set; } = new List<LabelChange>();

    public virtual Color? MainColor { get; set; }

    public virtual ModelPrediction? ModelPrediction { get; set; }

    public virtual ICollection<OutfitChange> OutfitChangeNewItems { get; set; } = new List<OutfitChange>();

    public virtual ICollection<OutfitChange> OutfitChangeOldItems { get; set; } = new List<OutfitChange>();

    public virtual ClothingStatus Status { get; set; } = null!;

    public virtual Subtype? Subtype { get; set; }

    public virtual Thickness? Thickness { get; set; }

    public virtual Tone? Tone { get; set; }

    public virtual Wardrobe Wardrobe { get; set; } = null!;
}
