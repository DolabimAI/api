using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// 4 tarz; kombin formundaki etkinlik türü de buradan.
/// </summary>
public partial class Style
{
    public long Id { get; set; }

    public string Value { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public virtual ICollection<ComparisonSession> ComparisonSessions { get; set; } = new List<ComparisonSession>();
}
