using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// Düzeltilebilen etiket alanları (label_changes).
/// </summary>
public partial class LabelField
{
    public long Id { get; set; }

    public string Value { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public virtual ICollection<LabelChange> LabelChanges { get; set; } = new List<LabelChange>();
}
