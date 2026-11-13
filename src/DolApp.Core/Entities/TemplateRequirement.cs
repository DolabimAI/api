using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// Şablon başına gereken en az onaylı parça (dolap şartı).
/// </summary>
public partial class TemplateRequirement
{
    public long Id { get; set; }

    public long TemplateId { get; set; }

    public long CategoryId { get; set; }

    public int MinRequired { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual OutfitTemplate Template { get; set; } = null!;
}
