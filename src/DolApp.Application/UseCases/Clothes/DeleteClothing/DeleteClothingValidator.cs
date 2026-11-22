using DolApp.Core.Errors;
using FluentValidation;

namespace DolApp.Application.UseCases.Clothes.DeleteClothing;

public class DeleteClothingValidator : AbstractValidator<DeleteClothingRequest>
{
    public DeleteClothingValidator()
    {
        RuleFor(request => request.ClothingId)
            .GreaterThan(0).WithErrorCode(ErrorCodes.InvalidId).WithMessage(ErrorMessages.InvalidId);
    }
}