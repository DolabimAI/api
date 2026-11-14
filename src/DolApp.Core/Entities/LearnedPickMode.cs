using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// Öğrenen kartın nasıl seçildiği. is_random analizde model seçimlerini rastgele kartlardan ayırır.
/// </summary>
public partial class LearnedPickMode
{
    public long Id { get; set; }

    public string Value { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public bool IsRandom { get; set; }

    public virtual ICollection<Comparison> Comparisons { get; set; } = new List<Comparison>();
}
