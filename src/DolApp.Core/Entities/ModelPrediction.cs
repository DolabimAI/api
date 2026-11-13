using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// Modelin ve renk analizinin ilk cevabı. Güncellenmez.
/// </summary>
public partial class ModelPrediction
{
    public long ClothingId { get; set; }

    public long SubtypeId { get; set; }

    public decimal SubtypeConfidence { get; set; }

    public long MainColorId { get; set; }

    public long ToneId { get; set; }

    public bool IsPatterned { get; set; }

    public string RawColor { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Clothe Clothing { get; set; } = null!;

    public virtual Color MainColor { get; set; } = null!;

    public virtual Subtype Subtype { get; set; } = null!;

    public virtual Tone Tone { get; set; } = null!;
}
