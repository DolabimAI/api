using DolApp.Core.Errors;
using FluentValidation;

namespace DolApp.Application.UseCases.Clothes.GetClothingById;

public class GetClothingByIdValidator : AbstractValidator<GetClothingByIdRequest>
{
    public GetClothingByIdValidator()
    {
        RuleFor(request => request.ClothingId)
            .GreaterThan(0).WithErrorCode(ErrorCodes.InvalidId).WithMessage(ErrorMessages.InvalidId);
    }
}