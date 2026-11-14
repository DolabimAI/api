using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// Seçilen kartta her değiştir/kaldır. Güncellenmez, silinmez.
/// </summary>
public partial class OutfitChange
{
    public long Id { get; set; }

    public long ComparisonId { get; set; }

    public int OrderNo { get; set; }

    public long CategoryId { get; set; }

    public long MethodId { get; set; }

    public long OldItemId { get; set; }

    public long? NewItemId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual Comparison Comparison { get; set; } = null!;

    public virtual Method Method { get; set; } = null!;

    public virtual Clothe? NewItem { get; set; }

    public virtual Clothe OldItem { get; set; } = null!;
}
