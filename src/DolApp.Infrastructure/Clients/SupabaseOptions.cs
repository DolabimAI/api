namespace DolApp.Infrastructure.Clients;

/// Supabase bağlantı bilgileri. appsettings'teki "Supabase" bölümünden doldurulur.
public class SupabaseOptions
{
    public required string Url { get; init; }
    public required string PublishableKey { get; init; }
    public required string SecretKey { get; init; }
}