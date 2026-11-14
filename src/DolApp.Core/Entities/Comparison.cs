using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// Her iki kartlı karşılaştırma. Sonucu yalnız C# yazar.
/// </summary>
public partial class Comparison
{
    public long Id { get; set; }

    public long SessionId { get; set; }

    public int SequenceNo { get; set; }

    public long LeftMethodId { get; set; }

    public List<long> RuleOutfit { get; set; } = null!;

    public List<long> LearnedOutfit { get; set; } = null!;

    public List<long>? Accessories { get; set; }

    public long ResultId { get; set; }

    public long LearnedPickModeId { get; set; }

    public List<long>? FinalOutfit { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? SelectedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual LearnedPickMode LearnedPickMode { get; set; } = null!;

    public virtual Method LeftMethod { get; set; } = null!;

    public virtual ICollection<OutfitChange> OutfitChanges { get; set; } = new List<OutfitChange>();

    public virtual ResultStatus Result { get; set; } = null!;

    public virtual ComparisonSession Session { get; set; } = null!;
}
