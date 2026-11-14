using DolApp.Application.Interfaces;
using MediatR;

namespace DolApp.Application.UseCases.Lookups.GetLookups;

/// Bütün liste tablolarını okuyup tek cevapta döner.
/// Sorgular sırayla çalışır: aynı DbContext üzerinde aynı anda iki sorgu çalıştırılamaz.

public class GetLookupsHandler : IRequestHandler<GetLookupsRequest, GetLookupsResponse>
{
    private readonly ILookupRepository _lookupRepository;

    public GetLookupsHandler(ILookupRepository lookupRepository)
    {
        _lookupRepository = lookupRepository;
    }

    public async Task<GetLookupsResponse> Handle(GetLookupsRequest request, CancellationToken cancellationToken)
    {
        var categories = await _lookupRepository.GetCategoriesAsync(cancellationToken);
        var subtypes = await _lookupRepository.GetSubtypesAsync(cancellationToken);
        var colors = await _lookupRepository.GetColorsAsync(cancellationToken);
        var tones = await _lookupRepository.GetTonesAsync(cancellationToken);
        var styles = await _lookupRepository.GetStylesAsync(cancellationToken);
        var seasons = await _lookupRepository.GetSeasonsAsync(cancellationToken);
        var thicknesses = await _lookupRepository.GetThicknessesAsync(cancellationToken);
        var timesOfDay = await _lookupRepository.GetTimesOfDayAsync(cancellationToken);

        return new GetLookupsResponse
        {
            Categories = categories.Select(category => ToLookupItem(category.Id, category.Value, category.DisplayName)).ToList(),
            Subtypes = subtypes.Select(subtype => new SubtypeLookupItem
            {
                Id = subtype.Id,
                Value = subtype.Value,
                DisplayName = subtype.DisplayName,
                CategoryId = subtype.CategoryId,
                DefaultStyleIds = subtype.DefaultStyleIds.ToList(),   // kopya: bellekteki liste paylaşılıyor
                DefaultSeasonIds = subtype.DefaultSeasonIds.ToList(),
                DefaultThicknessId = subtype.DefaultThicknessId,
            }).ToList(),
            Colors = colors.Select(color => ToLookupItem(color.Id, color.Value, color.DisplayName)).ToList(),
            Tones = tones.Select(tone => ToLookupItem(tone.Id, tone.Value, tone.DisplayName)).ToList(),
            Styles = styles.Select(style => ToLookupItem(style.Id, style.Value, style.DisplayName)).ToList(),
            Seasons = seasons.Select(season => ToLookupItem(season.Id, season.Value, season.DisplayName)).ToList(),
            Thicknesses = thicknesses.Select(thickness => ToLookupItem(thickness.Id, thickness.Value, thickness.DisplayName)).ToList(),
            TimesOfDay = timesOfDay.Select(timeOfDay => new TimeOfDayLookupItem
            {
                Id = timeOfDay.Id,
                Value = timeOfDay.Value,
                DisplayName = timeOfDay.DisplayName,
                StartHour = timeOfDay.StartHour,
                EndHour = timeOfDay.EndHour,
            }).ToList(),
        };
    }

    private static LookupItem ToLookupItem(long id, string value, string displayName)
    {
        return new LookupItem { Id = id, Value = value, DisplayName = displayName };
    }
}