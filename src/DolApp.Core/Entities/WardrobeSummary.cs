using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

public partial class WardrobeSummary
{
    public long? WardrobeId { get; set; }

    public int? TotalCount { get; set; }

    public int? TopCount { get; set; }

    public int? BottomCount { get; set; }

    public int? OnePieceCount { get; set; }

    public int? OuterCount { get; set; }

    public int? ShoesCount { get; set; }

    public int? AccessoryCount { get; set; }
}
