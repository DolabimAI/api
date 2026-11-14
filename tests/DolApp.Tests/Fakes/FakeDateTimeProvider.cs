using DolApp.Application.Interfaces;

namespace DolApp.Tests.Fakes;

/// Sabit saat: testler her çalıştığında aynı tarihi görür.
/// UtcNow test içinde değiştirilebilir (ör. "bugün 18. doğum günü" denemesi için).

public class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; set; } = new DateTime(2026, 11, 14, 12, 0, 0, DateTimeKind.Utc);

    public DateTime NowInIstanbul => UtcNow.AddHours(3);

    public DateOnly TodayInIstanbul => DateOnly.FromDateTime(NowInIstanbul);
}