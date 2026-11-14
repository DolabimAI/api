using DolApp.Core.Errors;
using FluentValidation;

namespace DolApp.Application.UseCases.Account.SignIn;

/// Giriş isteğinin biçim kontrolü: yalnızca alanların dolu olması.
/// E-posta biçimi kontrol edilmez; hatalı biçim zaten hiçbir hesapla eşleşmez ve invalid_credentials döner.

public class SignInValidator : AbstractValidator<SignInRequest>
{
    public SignInValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty().WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields);

        RuleFor(request => request.Password)
            .NotEmpty().WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields);
    }
}