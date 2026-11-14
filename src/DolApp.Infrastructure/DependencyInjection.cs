using DolApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DolApp.Infrastructure;

/// Infrastructure katmanının servislerini kaydeder: veritabanı, ileride Supabase ve Open-Meteo istemcileri, repository'ler.
/// Program.cs bunu tek satırla çağırır: builder.Services.AddInfrastructure(builder.Configuration)

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Db");

        // Bağlantı bilgisi boşsa uygulama açılırken açık bir mesajla dur.
        // (Genelde ortam "Development" olmadığı için appsettings.Development.json okunmamıştır.)
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "ConnectionStrings:Db boş. appsettings.Development.json dolu mu ve ortam Development mı?");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }
}