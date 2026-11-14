using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// 81 il, plaka kodu ve il merkezi koordinatı.
/// </summary>
public partial class City
{
    public long Id { get; set; }

    public string Value { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public int PlateCode { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public virtual ICollection<ComparisonSession> ComparisonSessions { get; set; } = new List<ComparisonSession>();

    public virtual ICollection<District> Districts { get; set; } = new List<District>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
