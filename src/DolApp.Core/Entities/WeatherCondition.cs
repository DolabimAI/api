using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// Open-Meteo kodlarının Türkçe hava durumuna tek çeviri yeri.
/// </summary>
public partial class WeatherCondition
{
    public long Id { get; set; }

    public string Value { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public int Severity { get; set; }

    public bool IsSunny { get; set; }

    public bool IsWet { get; set; }

    public List<int> ApiCodes { get; set; } = null!;

    public virtual ICollection<ComparisonSession> ComparisonSessions { get; set; } = new List<ComparisonSession>();
}
