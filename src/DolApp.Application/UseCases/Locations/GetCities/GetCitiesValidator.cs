using FluentValidation;

namespace DolApp.Application.UseCases.Locations.GetCities;

/// İstekte kontrol edilecek alan yoktur; yapı bütünlüğü için durur.

public class GetCitiesValidator : AbstractValidator<GetCitiesRequest>
{
}