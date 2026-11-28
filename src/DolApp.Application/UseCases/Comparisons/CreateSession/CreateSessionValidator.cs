using DolApp.Core.Errors;
using FluentValidation;

namespace DolApp.Application.UseCases.Comparisons.CreateSession;

/// Alanlar dolu mu, id'ler geçerli sayı mı. Gün aralığı, vaktin geçip geçmediği ve ilçenin ile ait olması
/// hava isteğinde (GetWeatherRequest) aynı kurallarla kontrol edilir; burada tekrar yazılmaz.
/// Etkinliğin listede olup olmadığı handler'da kontrol edilir (veritabanı gerekir).

public class CreateSessionValidator : AbstractValidator<CreateSessionRequest>
{
    public CreateSessionValidator()
    {
        RuleFor(request => request.CityId)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields)
            .GreaterThan(0).WithErrorCode(ErrorCodes.InvalidId).WithMessage(ErrorMessages.InvalidId);

        RuleFor(request => request.DistrictId)
            .GreaterThan(0).WithErrorCode(ErrorCodes.InvalidId).WithMessage(ErrorMessages.InvalidId)
            .When(request => request.DistrictId is not null);

        RuleFor(request => request.Date)
            .NotNull().WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields);

        RuleFor(request => request.TimeOfDayId)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields)
            .GreaterThan(0).WithErrorCode(ErrorCodes.InvalidId).WithMessage(ErrorMessages.InvalidId);

        RuleFor(request => request.EventStyleId)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields)
            .GreaterThan(0).WithErrorCode(ErrorCodes.InvalidId).WithMessage(ErrorMessages.InvalidId);

        RuleFor(request => request.WantsAccessories)
            .NotNull().WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields);
    }
}