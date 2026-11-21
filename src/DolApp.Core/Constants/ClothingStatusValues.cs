namespace DolApp.Core.Constants
{
    /// clothing_statuses tablosundaki value kodları (002_seed_lookups.sql). Kod içinde metin yerine bu sabitler kullanılır.
    /// Taslak: fotoğraf analiz edildi, Kaydet'e basılmadı. Onaylı: dolapta. Silindi: dolaptan çıktı, satır ve görsel kalır.

    public static class ClothingStatusValues
    {
        public const string Draft = "taslak";
        public const string Approved = "onayli";
        public const string Deleted = "silindi";
    }
}
