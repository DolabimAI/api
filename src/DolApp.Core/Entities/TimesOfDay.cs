using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// 4 vakit ve hava hesabının saat aralığı (end_hour dahil).
/// </summary>
public partial class TimesOfDay
{
    public long Id { get; set; }

    public string Value { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public int StartHour { get; set; }

    public int EndHour { get; set; }

    public bool IsDaytime { get; set; }

    public virtual ICollection<ComparisonSession> ComparisonSessions { get; set; } = new List<ComparisonSession>();
}
