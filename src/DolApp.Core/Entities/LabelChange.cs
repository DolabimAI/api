using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// Etiket düzeltme logu. Güncellenmez, silinmez.
/// </summary>
public partial class LabelChange
{
    public long Id { get; set; }

    public long ClothingId { get; set; }

    public long FieldId { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public long StageId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Clothe Clothing { get; set; } = null!;

    public virtual LabelField Field { get; set; } = null!;

    public virtual LabelChangeStage Stage { get; set; } = null!;
}
