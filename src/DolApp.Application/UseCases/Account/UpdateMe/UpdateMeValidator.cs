using DolApp.Core.Errors;
using FluentValidation;

namespace DolApp.Application.UseCases.Account.UpdateMe;

/// Yalnızca isteğe bakarak verilebilen kararlar. İl ve ilçenin veritabanında olup olmadığı handler'da kontrol edilir.

public class UpdateMeValidator : AbstractValidator<UpdateMeRequest>
{
    public UpdateMeValidator()
    {
        // Ad gönderildiyse boş olamaz (null = değiştirme, "" ya da "   " = hata).
        RuleFor(request => request.FirstName)
            .NotEmpty().WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields)
            .When(request => request.FirstName is not null);

        RuleFor(request => request.LastName)
            .NotEmpty().WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields)
            .When(request => request.LastName is not null);

        // İlçe varsa il de olmalı.
        RuleFor(request => request.DefaultCityId)
            .NotNull().WithErrorCode(ErrorCodes.CityRequired).WithMessage(ErrorMessages.CityRequired)
            .When(request => request.DefaultDistrictId is not null);
    }
}