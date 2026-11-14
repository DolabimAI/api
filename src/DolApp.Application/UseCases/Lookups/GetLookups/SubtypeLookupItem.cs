namespace DolApp.Application.UseCases.Lookups.GetLookups;

/// Alt tür: liste öğesine ek olarak kategorisi ve varsayılan tarz, mevsim, kalınlığı.
/// Kullanıcı türü değiştirince uygulama bu varsayılanları forma sunucuya sormadan doldurur.
/// Sıcaklık aralığı ve ıslak/sıcak hava tercihleri bilerek yok: öneri kuralının iç bilgisidir,
/// API belgesine göre hiçbir cevapta dönmez.

public class SubtypeLookupItem : LookupItem
{
    public long CategoryId { get; set; }
    public List<long> DefaultStyleIds { get; set; } = new();
    public List<long> DefaultSeasonIds { get; set; } = new();
    public long DefaultThicknessId { get; set; }
}