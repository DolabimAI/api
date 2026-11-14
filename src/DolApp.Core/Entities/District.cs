using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// İlçeler ve ilçe merkezi koordinatı. value il içinde tekildir.
/// </summary>
public partial class District
{
    public long Id { get; set; }

    public long CityId { get; set; }

    public string Value { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public virtual City City { get; set; } = null!;

    public virtual ICollection<ComparisonSession> ComparisonSessionDistrictNavigations { get; set; } = new List<ComparisonSession>();

    public virtual ICollection<ComparisonSession> ComparisonSessionDistricts { get; set; } = new List<ComparisonSession>();

    public virtual ICollection<User> UserDefaultDistricts { get; set; } = new List<User>();

    public virtual ICollection<User> UserDistricts { get; set; } = new List<User>();
}
