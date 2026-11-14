namespace DolApp.Infrastructure.Clients;

/// appsettings.json > OpenMeteo bölümü. Anahtar gerekmez (ücretsiz, ticari olmayan kullanım).

public class OpenMeteoOptions
{
    public required string BaseUrl { get; init; }
}