namespace DolApp.Application.UseCases.Lookups.GetLookups;

/// Vakit: liste öğesine ek olarak başlangıç ve bitiş saati (bitiş dahil).
/// Uygulama bugün için geçmiş vakitleri bu saatlere bakarak pasif yapar.

public class TimeOfDayLookupItem : LookupItem
{
    public int StartHour { get; set; }
    public int EndHour { get; set; }
}