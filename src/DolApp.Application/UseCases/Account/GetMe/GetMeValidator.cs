using FluentValidation;

namespace DolApp.Application.UseCases.Account.GetMe;

/// İstekte kontrol edilecek alan yoktur; kuralsız validator her isteği geçirir.
/// Yapı bütünlüğü için durur.

public class GetMeValidator : AbstractValidator<GetMeRequest>
{
}