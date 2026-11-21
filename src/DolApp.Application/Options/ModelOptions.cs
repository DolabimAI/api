namespace DolApp.Application.Options
{
    /// appsettings > Model bölümü. Handler'lar kullandığı için Application'da durur; değeri Infrastructure okuyup kaydeder.
    /// Analizde kullanılan eşik 7 Aralık'ta dondurulur.

    public class ModelOptions
    {
        /// Alt tür güveni bunun altındaysa uygulamaya tür boş gider, kullanıcı "Seç" ile seçer (plan kararı: 0,40).
        public decimal ConfidenceThreshold { get; init; } = 0.40m;
    }
}
