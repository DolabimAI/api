using DolApp.Core.Entities;

namespace DolApp.Application.UseCases.Comparisons.Common;

/// Python isteği kurulurken id → kod çevirisinde kullanılan liste tabloları (handler önbellekten okuyup buraya koyar).
/// Sözlük olmasının sebebi her parça için listeyi baştan taramamak.

public class RecommendationLookups
{
    public IReadOnlyDictionary<long, Subtype> Subtypes { get; init; } = new Dictionary<long, Subtype>();
    public IReadOnlyDictionary<long, string> Categories { get; init; } = new Dictionary<long, string>();
    public IReadOnlyDictionary<long, string> Colors { get; init; } = new Dictionary<long, string>();
    public IReadOnlyDictionary<long, string> Tones { get; init; } = new Dictionary<long, string>();
    public IReadOnlyDictionary<long, string> Styles { get; init; } = new Dictionary<long, string>();
    public IReadOnlyDictionary<long, string> ClothingStatuses { get; init; } = new Dictionary<long, string>();
    public IReadOnlyDictionary<long, string> WeatherConditions { get; init; } = new Dictionary<long, string>();
    public IReadOnlyDictionary<long, ResultStatus> ResultStatuses { get; init; } = new Dictionary<long, ResultStatus>();

    public static RecommendationLookups Create(
        IEnumerable<Subtype> subtypes,
        IEnumerable<Category> categories,
        IEnumerable<Color> colors,
        IEnumerable<Tone> tones,
        IEnumerable<Style> styles,
        IEnumerable<ClothingStatus> clothingStatuses,
        IEnumerable<WeatherCondition> weatherConditions,
        IEnumerable<ResultStatus> resultStatuses)
    {
        return new RecommendationLookups
        {
            Subtypes = subtypes.ToDictionary(subtype => subtype.Id),
            Categories = categories.ToDictionary(category => category.Id, category => category.Value),
            Colors = colors.ToDictionary(color => color.Id, color => color.Value),
            Tones = tones.ToDictionary(tone => tone.Id, tone => tone.Value),
            Styles = styles.ToDictionary(style => style.Id, style => style.Value),
            ClothingStatuses = clothingStatuses.ToDictionary(status => status.Id, status => status.Value),
            WeatherConditions = weatherConditions.ToDictionary(condition => condition.Id, condition => condition.Value),
            ResultStatuses = resultStatuses.ToDictionary(resultStatus => resultStatus.Id),
        };
    }
}