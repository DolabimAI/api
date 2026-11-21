using DolApp.Core.Errors;
using FluentValidation;

namespace DolApp.Application.UseCases.Clothes.AnalyzeClothing
{
    /// Yalnızca "fotoğraf gönderildi mi" kontrolü. Boyut (413) ve tür (400) ImageFileRules'ta:
    /// validator hataları her zaman 400 döner, boyut hatası 413 olmalı; ayrıca tür için dosyanın okunması gerekir.

    public class AnalyzeClothingValidator : AbstractValidator<AnalyzeClothingRequest>
    {
        public AnalyzeClothingValidator()
        {
            RuleFor(request => request.ImageStream)
                .NotNull().WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields);
        }
    }
}
