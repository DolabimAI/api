using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// 6 ana kategori. Kullanıcı seçmez; C# alt türden yazar.
/// </summary>
public partial class Category
{
    public long Id { get; set; }

    public string Value { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public virtual ICollection<Clothe> Clothes { get; set; } = new List<Clothe>();

    public virtual ICollection<OutfitChange> OutfitChanges { get; set; } = new List<OutfitChange>();

    public virtual ICollection<Subtype> Subtypes { get; set; } = new List<Subtype>();

    public virtual ICollection<TemplateRequirement> TemplateRequirements { get; set; } = new List<TemplateRequirement>();
}
