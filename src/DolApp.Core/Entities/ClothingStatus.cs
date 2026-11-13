using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// Parça durumu: taslak, onayli, silindi. Dolap ve kombine yalnız onayli girer.
/// </summary>
public partial class ClothingStatus
{
    public long Id { get; set; }

    public string Value { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public virtual ICollection<Clothe> Clothes { get; set; } = new List<Clothe>();
}
