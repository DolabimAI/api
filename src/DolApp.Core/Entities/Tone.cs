using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// 3 ton.
/// </summary>
public partial class Tone
{
    public long Id { get; set; }

    public string Value { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public virtual ICollection<Clothe> Clothes { get; set; } = new List<Clothe>();

    public virtual ICollection<ModelPrediction> ModelPredictions { get; set; } = new List<ModelPrediction>();
}
