using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// Her kullanıcının tek dolabı.
/// </summary>
public partial class Wardrobe
{
    public long Id { get; set; }

    public Guid UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Clothe> Clothes { get; set; } = new List<Clothe>();

    public virtual User User { get; set; } = null!;
}
