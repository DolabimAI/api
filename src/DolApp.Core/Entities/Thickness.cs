using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// 3 kalınlık; yalnız etiket, öneride kullanılmaz.
/// </summary>
public partial class Thickness
{
    public long Id { get; set; }

    public string Value { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public virtual ICollection<Clothe> Clothes { get; set; } = new List<Clothe>();

    public virtual ICollection<Subtype> Subtypes { get; set; } = new List<Subtype>();
}
