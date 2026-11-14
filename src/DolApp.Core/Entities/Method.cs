using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// Kombin ya da parça seçen yöntemler. Kullanıcıya gösterilmez.
/// </summary>
public partial class Method
{
    public long Id { get; set; }

    public string Value { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Comparison> Comparisons { get; set; } = new List<Comparison>();

    public virtual ICollection<OutfitChange> OutfitChanges { get; set; } = new List<OutfitChange>();
}
