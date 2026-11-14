using DolApp.Core.Errors;
using FluentValidation;

namespace DolApp.Application.UseCases.Locations.GetNearestLocation;

/// Konum gönderilmiş mi ve Türkiye sınırları içinde mi?
/// Sınırlar districts tablosundaki kuralla aynı: enlem 35–43, boylam 25–45.

public class GetNearestLocationValidator : AbstractValidator<GetNearestLocationRequest>
{
    private const decimal MinimumLatitude = 35;
    private const decimal MaximumLatitude = 43;
    private const decimal MinimumLongitude = 25;
    private const decimal MaximumLongitude = 45;

    public GetNearestLocationValidator()
    {
        RuleFor(request => request.Latitude)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields)
            .InclusiveBetween(MinimumLatitude, MaximumLatitude)
                .WithErrorCode(ErrorCodes.LocationOutOfRange).WithMessage(ErrorMessages.LocationOutOfRange);

        RuleFor(request => request.Longitude)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields)
            .InclusiveBetween(MinimumLongitude, MaximumLongitude)
                .WithErrorCode(ErrorCodes.LocationOutOfRange).WithMessage(ErrorMessages.LocationOutOfRange);
    }
}