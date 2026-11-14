using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DolApp.Application;

/// Application katmanının servislerini kaydeder: MediatR handler'ları ve FluentValidation validator'ları.
/// Program.cs bunu tek satırla çağırır: builder.Services.AddApplication(...)

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, string? mediatRLicenseKey)
    {
        var applicationAssembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(mediatRConfiguration =>
        {
            // Bu projedeki bütün IRequestHandler sınıflarını otomatik bulur ve kaydeder.
            mediatRConfiguration.RegisterServicesFromAssembly(applicationAssembly);

            // Lisans anahtarı varsa ver; yoksa MediatR yine çalışır, yalnızca logda uyarı yazar.
            if (!string.IsNullOrWhiteSpace(mediatRLicenseKey))
                mediatRConfiguration.LicenseKey = mediatRLicenseKey;
        });

        // Bu projedeki bütün AbstractValidator sınıflarını otomatik bulur ve kaydeder.
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }
}