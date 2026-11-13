using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// Düzeltmenin yapıldığı ekran: onay ekranı ya da parça detayı.
/// </summary>
public partial class LabelChangeStage
{
    public long Id { get; set; }

    public string Value { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public virtual ICollection<LabelChange> LabelChanges { get; set; } = new List<LabelChange>();
}
