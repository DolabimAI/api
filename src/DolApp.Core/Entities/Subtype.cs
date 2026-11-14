using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// 35 alt tür ve varsayılanları. FashionCLIP etiketleri value ile birebir aynıdır.
/// </summary>
public partial class Subtype
{
    public long Id { get; set; }

    public string Value { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public long CategoryId { get; set; }

    public List<long> DefaultStyleIds { get; set; } = null!;

    public List<long> DefaultSeasonIds { get; set; } = null!;

    public long DefaultThicknessId { get; set; }

    public int MinTemp { get; set; }

    public int MaxTemp { get; set; }

    public bool AvoidWhenWet { get; set; }

    public bool PreferredWhenWet { get; set; }

    public bool PreferredWhenHot { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Clothe> Clothes { get; set; } = new List<Clothe>();

    public virtual Thickness DefaultThickness { get; set; } = null!;

    public virtual ICollection<ModelPrediction> ModelPredictions { get; set; } = new List<ModelPrediction>();
}
