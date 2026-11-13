using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// Karşılaştırmanın 14 olası sonucu. C# ve Python result_key ile arar.
/// </summary>
public partial class ResultStatus
{
    public long Id { get; set; }

    public string Value { get; set; } = null!;

    public string ResultKey { get; set; } = null!;

    public string? Selected { get; set; }

    public string? Acceptance { get; set; }

    public bool InPreferenceAnalysis { get; set; }

    public bool InAcceptanceAnalysis { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Comparison> Comparisons { get; set; } = new List<Comparison>();
}
