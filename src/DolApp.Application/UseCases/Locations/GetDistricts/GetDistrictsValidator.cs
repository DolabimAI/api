using DolApp.Core.Errors;
using FluentValidation;

namespace DolApp.Application.UseCases.Locations.GetDistricts;

/// Id'ler 1'den başlar; 0 ve eksi değerler veritabanına sorulmadan reddedilir.

public class GetDistrictsValidator : AbstractValidator<GetDistrictsRequest>
{
    public GetDistrictsValidator()
    {
        RuleFor(request => request.CityId)
            .GreaterThan(0).WithErrorCode(ErrorCodes.InvalidId).WithMessage(ErrorMessages.InvalidId);
    }
}