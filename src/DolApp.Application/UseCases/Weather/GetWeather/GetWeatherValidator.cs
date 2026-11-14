using DolApp.Application.Interfaces;
using DolApp.Core.Errors;
using FluentValidation;

namespace DolApp.Application.UseCases.Weather.GetWeather;

/// İsteğe bakarak verilebilen kararlar: alanlar dolu mu, id'ler geçerli sayı mı, gün bugün ile 6 gün sonrası arasında mı.
/// Vaktin geçip geçmediği, il ve ilçenin varlığı veritabanı gerektirdiği için handler'da kontrol edilir.

public class GetWeatherValidator : AbstractValidator<GetWeatherRequest>
{
    private const int MaximumDaysAhead = 6;

    public GetWeatherValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(request => request.CityId)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields)
            .GreaterThan(0).WithErrorCode(ErrorCodes.InvalidId).WithMessage(ErrorMessages.InvalidId);

        RuleFor(request => request.DistrictId)
            .GreaterThan(0).WithErrorCode(ErrorCodes.InvalidId).WithMessage(ErrorMessages.InvalidId)
            .When(request => request.DistrictId is not null);

        RuleFor(request => request.Date)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields)
            .Must(date => IsWithinForecastRange(date!.Value, dateTimeProvider.TodayInIstanbul))
                .WithErrorCode(ErrorCodes.DateOutOfRange).WithMessage(ErrorMessages.DateOutOfRange);

        RuleFor(request => request.TimeOfDayId)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithErrorCode(ErrorCodes.RequiredFields).WithMessage(ErrorMessages.RequiredFields)
            .GreaterThan(0).WithErrorCode(ErrorCodes.InvalidId).WithMessage(ErrorMessages.InvalidId);
    }

    /// Bugün (İstanbul saatiyle) ve sonraki 6 gün: toplam 7 gün.
    private static bool IsWithinForecastRange(DateOnly date, DateOnly todayInIstanbul)
    {
        return date >= todayInIstanbul && date <= todayInIstanbul.AddDays(MaximumDaysAhead);
    }
}