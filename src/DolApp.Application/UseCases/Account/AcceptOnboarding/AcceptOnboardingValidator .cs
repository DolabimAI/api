using FluentValidation;

namespace DolApp.Application.UseCases.Account.AcceptOnboarding;

/// İstekte kontrol edilecek alan yoktur; kuralsız validator her isteği geçirir.
/// Yapı bütünlüğü için durur; ileride istek bir alan alırsa kuralı buraya yazılır.

public class AcceptOnboardingValidator : AbstractValidator<AcceptOnboardingRequest>
{
}