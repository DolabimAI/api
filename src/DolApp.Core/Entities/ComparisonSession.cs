using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// Bir kombin formu gönderimi ve o anki hava.
/// </summary>
public partial class ComparisonSession
{
    public long Id { get; set; }

    public Guid UserId { get; set; }

    public long CityId { get; set; }

    public long? DistrictId { get; set; }

    public DateOnly TargetDate { get; set; }

    public long TimeOfDayId { get; set; }

    public long EventStyleId { get; set; }

    public bool WantsAccessories { get; set; }

    public decimal FeelsLikeTemp { get; set; }

    public long WeatherConditionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual City City { get; set; } = null!;

    public virtual ICollection<Comparison> Comparisons { get; set; } = new List<Comparison>();

    public virtual District? District { get; set; }

    public virtual District? DistrictNavigation { get; set; }

    public virtual Style EventStyle { get; set; } = null!;

    public virtual TimesOfDay TimeOfDay { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual WeatherCondition WeatherCondition { get; set; } = null!;
}
