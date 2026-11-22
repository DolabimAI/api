using DolApp.Core.Errors;
using FluentValidation;

namespace DolApp.Application.UseCases.Clothes.GetClothes;

/// Filtre verildiyse pozitif olmalı. Parçası olmayan bir tür 400 değil, boş liste döner (handler).

public class GetClothesValidator : AbstractValidator<GetClothesRequest>
{
    public GetClothesValidator()
    {
        RuleFor(request => request.SubtypeId)
            .GreaterThan(0).WithErrorCode(ErrorCodes.InvalidId).WithMessage(ErrorMessages.InvalidId)
            .When(request => request.SubtypeId.HasValue);
    }
}