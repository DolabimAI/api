using DolApp.Application.Interfaces;
using DolApp.Core.Errors;
using FluentValidation;

namespace DolApp.Application.UseCases.Account.Register;

/// Kayıt isteğinin biçim kontrolleri. Ekranda da kontrol edilse burada yeniden yapılır:
/// ekran atlanıp istek doğrudan gönderilirse hesap açılmaz (T-01).
/// Kurallar sırayla çalışır; ilk hata kullanıcıya döner.

public class RegisterValidator : AbstractValidator<RegisterRequest>
{
    private const int MinimumAge = 18;

    public RegisterValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(request => request.FirstName)
            .NotEmpty().WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields);

        RuleFor(request => request.LastName)
            .NotEmpty().WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields);

        RuleFor(request => request.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields)
            .EmailAddress().WithErrorCode(ErrorCodes.InvalidEmail).WithMessage(ErrorMessages.InvalidEmail);

        RuleFor(request => request.BirthDate)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields)
            .Must(birthDate => IsAtLeastMinimumAge(birthDate, dateTimeProvider.TodayInIstanbul))
                .WithErrorCode(ErrorCodes.Under18).WithMessage(ErrorMessages.Under18);

        RuleFor(request => request.Password)
            .NotEmpty().WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields);

        RuleFor(request => request.ConsentAccepted)
            .Equal(true).WithErrorCode(ErrorCodes.ConsentRequired).WithMessage(ErrorMessages.ConsentRequired);
    }

    /// Doğum tarihi, bugünden 18 yıl öncesi ya da daha eskiyse kişi 18 yaşındadır.
    /// Bugün 18. doğum günü olan kişi kayıt olabilir.
    private static bool IsAtLeastMinimumAge(DateOnly birthDate, DateOnly todayInIstanbul)
    {
        return birthDate <= todayInIstanbul.AddYears(-MinimumAge);
    }
}