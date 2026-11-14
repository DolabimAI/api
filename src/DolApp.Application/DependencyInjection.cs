using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DolApp.Application;

/// Application katmanının servislerini kaydeder: MediatR handler'ları ve FluentValidation validator'ları.
/// Program.cs bunu tek satırla çağırır: builder.Services.AddApplication(...)

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, string? mediatRLicenseKey)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            // Bu projedeki bütün IRequestHandler sınıflarını otomatik bulur ve kaydeder.
            cfg.RegisterServicesFromAssembly(assembly);

            // Lisans anahtarı varsa ver; yoksa MediatR yine çalışır, yalnızca logda uyarı yazar.
            if (!string.IsNullOrWhiteSpace(mediatRLicenseKey))
                cfg.LicenseKey = mediatRLicenseKey;
        });

        // Bu projedeki bütün AbstractValidator sınıflarını otomatik bulur ve kaydeder.
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}