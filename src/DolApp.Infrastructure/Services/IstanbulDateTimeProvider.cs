using DolApp.Application.Interfaces;

namespace DolApp.Infrastructure.Services;

/// IDateTimeProvider'ın gerçek implementasyonu: sistem saatini Europe/Istanbul'a çevirir.

public class IstanbulDateTimeProvider : IDateTimeProvider
{
    private static readonly TimeZoneInfo IstanbulTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");

    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime NowInIstanbul => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IstanbulTimeZone);

    public DateOnly TodayInIstanbul => DateOnly.FromDateTime(NowInIstanbul);
}