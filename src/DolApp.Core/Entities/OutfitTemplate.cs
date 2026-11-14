using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// Kombin şablonları: ust_alt, tek_parca.
/// </summary>
public partial class OutfitTemplate
{
    public long Id { get; set; }

    public string Value { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public virtual ICollection<TemplateRequirement> TemplateRequirements { get; set; } = new List<TemplateRequirement>();
}
