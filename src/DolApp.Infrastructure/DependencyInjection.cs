using DolApp.Application.Interfaces;
using DolApp.Infrastructure.Clients;
using DolApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DolApp.Infrastructure.Repositories;
using DolApp.Infrastructure.Services;

namespace DolApp.Infrastructure;

/// Infrastructure katmanının servislerini kaydeder: veritabanı, Supabase giriş ve depo istemcileri, Python, Open-Meteo ve repository'ler.
/// Program.cs bunu tek satırla çağırır: builder.Services.AddInfrastructure(builder.Configuration)
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = Required(configuration, "ConnectionStrings:Db");
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        var supabaseOptions = new SupabaseOptions
        {
            Url = Required(configuration, "Supabase:Url").TrimEnd('/'),
            PublishableKey = Required(configuration, "Supabase:PublishableKey"),
            SecretKey = Required(configuration, "Supabase:SecretKey"),
        };
        services.AddSingleton(supabaseOptions);

        // Biri IAuthService isterse SupabaseAuthClient verilir; içine Supabase adresine ayarlı bir HttpClient konur.
        services.AddHttpClient<IAuthService, SupabaseAuthClient>(httpClient =>
        {
            httpClient.BaseAddress = new Uri(supabaseOptions.Url + "/");
            httpClient.Timeout = TimeSpan.FromSeconds(15);
        });

        // Open-Meteo: anahtar gerekmez. Zaman aşımı 10 sn; aşılırsa kullanıcıya 502 weather_unavailable döner.
        var openMeteoOptions = new OpenMeteoOptions
        {
            BaseUrl = Required(configuration, "OpenMeteo:BaseUrl"),
        };
        services.AddSingleton(openMeteoOptions);
        services.AddHttpClient<IWeatherService, OpenMeteoWeatherService>(httpClient =>
        {
            httpClient.Timeout = TimeSpan.FromSeconds(10);
        });


        // Supabase depo (görseller): süreli bağlantı ve silme. Somut sınıf da kayıtlı, çünkü taklit (FakeClothingAnalyzer)
        // yer tutucu görseli yüklemek için arayüzde olmayan UploadPlaceholderAsync'i kullanıyor.
        var storageOptions = new StorageOptions
        {
            Bucket = Optional(configuration, "Storage:Bucket") ?? "gorseller",
            SignedUrlSeconds = OptionalInt(configuration, "Storage:SignedUrlSeconds") ?? 3600,
        };
        services.AddSingleton(storageOptions);
        services.AddHttpClient<SupabaseImageStorage>(httpClient =>
        {
            httpClient.BaseAddress = new Uri(supabaseOptions.Url + "/storage/v1/");
            httpClient.Timeout = TimeSpan.FromSeconds(15);
        });
        services.AddScoped<IImageStorage>(serviceProvider => serviceProvider.GetRequiredService<SupabaseImageStorage>());

        // Python servisi (/analyze, /health). UseFake = true ise Python'a gidilmez, taklit kullanılır (yalnızca geliştirme).
        var pythonOptions = new PythonOptions
        {
            BaseUrl = Required(configuration, "Python:BaseUrl").TrimEnd('/'),
            TimeoutSeconds = OptionalInt(configuration, "Python:TimeoutSeconds") ?? 30,
            UseFake = OptionalBool(configuration, "Python:UseFake") ?? false,
            FakeConfidence = OptionalDecimal(configuration, "Python:FakeConfidence") ?? 0.873m,
        };
        services.AddSingleton(pythonOptions);
        if (pythonOptions.UseFake)
        {
            services.AddScoped<IClothingAnalyzer, FakeClothingAnalyzer>();
        }
        else
        {
            services.AddHttpClient<IClothingAnalyzer, PythonClothingAnalyzer>(httpClient =>
            {
                httpClient.BaseAddress = new Uri(pythonOptions.BaseUrl + "/");
                httpClient.Timeout = TimeSpan.FromSeconds(pythonOptions.TimeoutSeconds);
            });
        }

        // Bellek önbelleği: liste tabloları (lookups) burada tutulur. Tek örnek, bütün isteklerle paylaşılır.
        services.AddMemoryCache();

        // ---- Repository'ler ----
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IDateTimeProvider, IstanbulDateTimeProvider>();  // saat sağlayıcı tek örnek: sistem saatini Europe/Istanbul'a çevirir.
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<ILookupRepository, LookupRepository>();
        services.AddScoped<IWardrobeRepository, WardrobeRepository>();

        return services;
    }


    /// Ayar boşsa uygulama açılırken açık bir mesajla durur.
    /// (Genelde appsettings.Development.json eksiktir ya da ortam Development değildir.)

    private static string Required(IConfiguration configuration, string settingKey)
    {
        var settingValue = configuration[settingKey];
        if (string.IsNullOrWhiteSpace(settingValue))
            throw new InvalidOperationException(
                $"{settingKey} boş. appsettings.Development.json dolu mu ve ortam Development mı?");
        return settingValue;
    }


    /// İsteğe bağlı ayarlar: yoksa null döner, çağıran varsayılanı verir.
    /// Sayılar InvariantCulture ile okunur (Türkçe ortamda "0.873" virgüllü beklenmesin).

    private static string? Optional(IConfiguration configuration, string settingKey)
    {
        var settingValue = configuration[settingKey];
        return string.IsNullOrWhiteSpace(settingValue) ? null : settingValue;
    }

    private static int? OptionalInt(IConfiguration configuration, string settingKey) =>
        Optional(configuration, settingKey) is { } settingValue
            ? int.Parse(settingValue, System.Globalization.CultureInfo.InvariantCulture)
            : null;

    private static bool? OptionalBool(IConfiguration configuration, string settingKey) =>
        Optional(configuration, settingKey) is { } settingValue ? bool.Parse(settingValue) : null;

    private static decimal? OptionalDecimal(IConfiguration configuration, string settingKey) =>
        Optional(configuration, settingKey) is { } settingValue
            ? decimal.Parse(settingValue, System.Globalization.CultureInfo.InvariantCulture)
            : null;
}