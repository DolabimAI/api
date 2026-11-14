using DolApp.Application.Interfaces;
using DolApp.Infrastructure.Clients;
using DolApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DolApp.Infrastructure.Repositories;
using DolApp.Infrastructure.Services;

namespace DolApp.Infrastructure;

/// Infrastructure katmanının servislerini kaydeder: veritabanı, Supabase giriş istemcisi; ileride Open-Meteo ve repository'ler.
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

        // ---- Repository'ler ----
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IDateTimeProvider, IstanbulDateTimeProvider>();  // saat sağlayıcı tek örnek: sistem saatini Europe/Istanbul'a çevirir.

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
}