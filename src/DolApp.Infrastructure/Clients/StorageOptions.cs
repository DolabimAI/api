namespace DolApp.Infrastructure.Clients
{
    /// appsettings > Storage bölümü. Adres ve anahtar SupabaseOptions'tan gelir.

    public class StorageOptions
    {
        /// Görsellerin durduğu bucket (private, 1 MB, image/webp ve image/png).
        public string Bucket { get; init; } = "gorseller";

        /// Süreli bağlantının geçerlilik süresi. Uygulama görseli parça id'siyle önbelleğe aldığı için 1 saat yeterli.
        public int SignedUrlSeconds { get; init; } = 3600;
    }
}
