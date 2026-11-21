using DolApp.Core.Errors;
using FluentValidation;

namespace DolApp.Application.UseCases.Clothes.ApproveClothing;

/// Tür, ana renk ve ton zorunlu (ekranda bunlar dolmadan Kaydet pasif; burada yeniden kontrol edilir).
/// Id'lerin listede olup olmadığı handler'da kontrol edilir (veritabanı gerekir).

public class ApproveClothingValidator : AbstractValidator<ApproveClothingRequest>
{
    public ApproveClothingValidator()
    {
        RuleFor(request => request.ClothingId)
            .GreaterThan(0).WithErrorCode(ErrorCodes.InvalidId).WithMessage(ErrorMessages.InvalidId);

        RuleFor(request => request.SubtypeId)
            .NotNull().WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields);

        RuleFor(request => request.MainColorId)
            .NotNull().WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields);

        RuleFor(request => request.ToneId)
            .NotNull().WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields);
    }
}