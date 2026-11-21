using DolApp.Core.Entities;

namespace DolApp.Application.Interfaces;

/// Kıyafet ve bağlam liste tablolarına erişim (kategori, alt tür, renk, ton, tarz, mevsim, kalınlık, vakit).
/// lookups ucu (C4) ve parça kaydetmedeki id kontrolleri (C3) kullanır. Hepsi id sırasıyla döner.

public interface ILookupRepository
{
    Task<List<Category>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<List<Subtype>> GetSubtypesAsync(CancellationToken cancellationToken);
    Task<List<Color>> GetColorsAsync(CancellationToken cancellationToken);
    Task<List<Tone>> GetTonesAsync(CancellationToken cancellationToken);
    Task<List<Style>> GetStylesAsync(CancellationToken cancellationToken);
    Task<List<Season>> GetSeasonsAsync(CancellationToken cancellationToken);
    Task<List<Thickness>> GetThicknessesAsync(CancellationToken cancellationToken);
    Task<List<TimesOfDay>> GetTimesOfDayAsync(CancellationToken cancellationToken);
    Task<List<TemplateRequirement>> GetTemplateRequirementsAsync(CancellationToken cancellationToken);     /// Dolap şablonlarının gereksinimleri; şablon ve kategori bilgisiyle birlikte, id sırasıyla.                                                                                                      
    Task<List<WeatherCondition>> GetWeatherConditionsAsync(CancellationToken cancellationToken); /// Hava durumu türleri ve her birinin Open-Meteo (WMO) kodları.
    Task<List<ClothingStatus>> GetClothingStatusesAsync(CancellationToken cancellationToken);     /// Parça durumları: taslak, onayli, silindi (C3).
    Task<List<LabelField>> GetLabelFieldsAsync(CancellationToken cancellationToken);                 /// Etiket düzeltme logunun alanları: alt_tur, ana_renk, ton, desen, tarz, kalinlik, mevsim (C3).
    Task<List<LabelChangeStage>> GetLabelChangeStagesAsync(CancellationToken cancellationToken);     /// Düzeltmenin yapıldığı ekran: onay, sonradan (C3).
}


