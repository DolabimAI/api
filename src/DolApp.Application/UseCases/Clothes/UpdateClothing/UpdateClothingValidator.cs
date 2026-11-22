using DolApp.Core.Errors;
using FluentValidation;

namespace DolApp.Application.UseCases.Clothes.UpdateClothing;

/// Id adresten pozitif gelmeli. Tarz ve mevsim gönderildiyse boş olamaz.
/// Id'lerin listede olup olmadığı handler'da kontrol edilir (veritabanı gerekir).

public class UpdateClothingValidator : AbstractValidator<UpdateClothingRequest>
{
    public UpdateClothingValidator()
    {
        RuleFor(request => request.ClothingId)
            .GreaterThan(0).WithErrorCode(ErrorCodes.InvalidId).WithMessage(ErrorMessages.InvalidId);

        RuleFor(request => request.StyleIds)
            .Must(styleIds => styleIds == null || styleIds.Count > 0)
            .WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields);

        RuleFor(request => request.SeasonIds)
            .Must(seasonIds => seasonIds == null || seasonIds.Count > 0)
            .WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields);
    }
}