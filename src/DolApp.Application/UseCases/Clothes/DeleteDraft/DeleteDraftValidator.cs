using DolApp.Core.Errors;
using FluentValidation;

namespace DolApp.Application.UseCases.Clothes.DeleteDraft;

public class DeleteDraftValidator : AbstractValidator<DeleteDraftRequest>
{
    public DeleteDraftValidator()
    {
        RuleFor(request => request.ClothingId)
            .GreaterThan(0).WithErrorCode(ErrorCodes.InvalidId).WithMessage(ErrorMessages.InvalidId);
    }
}