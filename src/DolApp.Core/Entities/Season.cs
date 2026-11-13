using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// 4 mevsim; yalnız etiket, öneride kullanılmaz.
/// </summary>
public partial class Season
{
    public long Id { get; set; }

    public string Value { get; set; } = null!;

    public string DisplayName { get; set; } = null!;
}
