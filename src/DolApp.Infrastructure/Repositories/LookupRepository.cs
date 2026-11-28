using DolApp.Application.Interfaces;
using DolApp.Core.Entities;
using DolApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DolApp.Infrastructure.Repositories;

/// Liste tabloları uygulama çalışırken değişmez; her liste ilk istendiğinde veritabanından okunur
/// ve 1 saat bellekte tutulur. Seed yeniden çalıştırılırsa yeni değerler en geç 1 saat içinde,
/// uygulama yeniden başlatılırsa hemen görünür.

public class LookupRepository : ILookupRepository
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    private readonly AppDbContext _dbContext;
    private readonly IMemoryCache _memoryCache;

    public LookupRepository(AppDbContext dbContext, IMemoryCache memoryCache)
    {
        _dbContext = dbContext;
        _memoryCache = memoryCache;
    }

    public Task<List<Category>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        return GetFromCacheOrDatabaseAsync("lookups:categories",
            () => _dbContext.Categories.AsNoTracking().OrderBy(category => category.Id).ToListAsync(cancellationToken));
    }

    public Task<List<Subtype>> GetSubtypesAsync(CancellationToken cancellationToken)
    {
        return GetFromCacheOrDatabaseAsync("lookups:subtypes",
            () => _dbContext.Subtypes.AsNoTracking().OrderBy(subtype => subtype.Id).ToListAsync(cancellationToken));
    }

    public Task<List<Color>> GetColorsAsync(CancellationToken cancellationToken)
    {
        return GetFromCacheOrDatabaseAsync("lookups:colors",
            () => _dbContext.Colors.AsNoTracking().OrderBy(color => color.Id).ToListAsync(cancellationToken));
    }

    public Task<List<Tone>> GetTonesAsync(CancellationToken cancellationToken)
    {
        return GetFromCacheOrDatabaseAsync("lookups:tones",
            () => _dbContext.Tones.AsNoTracking().OrderBy(tone => tone.Id).ToListAsync(cancellationToken));
    }

    public Task<List<Style>> GetStylesAsync(CancellationToken cancellationToken)
    {
        return GetFromCacheOrDatabaseAsync("lookups:styles",
            () => _dbContext.Styles.AsNoTracking().OrderBy(style => style.Id).ToListAsync(cancellationToken));
    }

    public Task<List<Season>> GetSeasonsAsync(CancellationToken cancellationToken)
    {
        return GetFromCacheOrDatabaseAsync("lookups:seasons",
            () => _dbContext.Seasons.AsNoTracking().OrderBy(season => season.Id).ToListAsync(cancellationToken));
    }

    public Task<List<Thickness>> GetThicknessesAsync(CancellationToken cancellationToken)
    {
        return GetFromCacheOrDatabaseAsync("lookups:thicknesses",
            () => _dbContext.Thicknesses.AsNoTracking().OrderBy(thickness => thickness.Id).ToListAsync(cancellationToken));
    }

    public Task<List<TimesOfDay>> GetTimesOfDayAsync(CancellationToken cancellationToken)
    {
        return GetFromCacheOrDatabaseAsync("lookups:times_of_day",
            () => _dbContext.TimesOfDays.AsNoTracking().OrderBy(timeOfDay => timeOfDay.Id).ToListAsync(cancellationToken));
    }

    public Task<List<TemplateRequirement>> GetTemplateRequirementsAsync(CancellationToken cancellationToken)
    {
        return GetFromCacheOrDatabaseAsync("lookups:template_requirements",
            () => _dbContext.TemplateRequirements
                .AsNoTracking()
                .Include(templateRequirement => templateRequirement.Template)
                .Include(templateRequirement => templateRequirement.Category)
                .OrderBy(templateRequirement => templateRequirement.Id)
                .ToListAsync(cancellationToken));
    }

    public Task<List<WeatherCondition>> GetWeatherConditionsAsync(CancellationToken cancellationToken)
    {
        return GetFromCacheOrDatabaseAsync("lookups:weather_conditions",
            () => _dbContext.WeatherConditions.AsNoTracking().OrderBy(weatherCondition => weatherCondition.Id).ToListAsync(cancellationToken));
    }


    public Task<List<ClothingStatus>> GetClothingStatusesAsync(CancellationToken cancellationToken)
    {
        return GetFromCacheOrDatabaseAsync("lookups:clothing_statuses",
            () => _dbContext.ClothingStatuses.AsNoTracking().OrderBy(clothingStatus => clothingStatus.Id).ToListAsync(cancellationToken));
    }

    public Task<List<LabelField>> GetLabelFieldsAsync(CancellationToken cancellationToken)
    {
        return GetFromCacheOrDatabaseAsync("lookups:label_fields",
            () => _dbContext.LabelFields.AsNoTracking().OrderBy(labelField => labelField.Id).ToListAsync(cancellationToken));
    }

    public Task<List<LabelChangeStage>> GetLabelChangeStagesAsync(CancellationToken cancellationToken)
    {
        return GetFromCacheOrDatabaseAsync("lookups:label_change_stages",
            () => _dbContext.LabelChangeStages.AsNoTracking().OrderBy(labelChangeStage => labelChangeStage.Id).ToListAsync(cancellationToken));
    }

    public Task<List<Method>> GetMethodsAsync(CancellationToken cancellationToken)
    {
        return GetFromCacheOrDatabaseAsync("lookups:methods",
            () => _dbContext.Methods.AsNoTracking().OrderBy(method => method.Id).ToListAsync(cancellationToken));
    }

    public Task<List<LearnedPickMode>> GetLearnedPickModesAsync(CancellationToken cancellationToken)
    {
        return GetFromCacheOrDatabaseAsync("lookups:learned_pick_modes",
            () => _dbContext.LearnedPickModes.AsNoTracking().OrderBy(learnedPickMode => learnedPickMode.Id).ToListAsync(cancellationToken));
    }

    public Task<List<ResultStatus>> GetResultStatusesAsync(CancellationToken cancellationToken)
    {
        return GetFromCacheOrDatabaseAsync("lookups:result_statuses",
            () => _dbContext.ResultStatuses.AsNoTracking().OrderBy(resultStatus => resultStatus.Id).ToListAsync(cancellationToken));
    }


    /// Liste bellekte varsa oradan döner; yoksa veritabanından okur, belleğe koyar ve döner.
    private async Task<List<TEntity>> GetFromCacheOrDatabaseAsync<TEntity>(
        string cacheKey, Func<Task<List<TEntity>>> loadFromDatabase)
    {
        var cachedList = await _memoryCache.GetOrCreateAsync(cacheKey, async cacheEntry =>
        {
            cacheEntry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await loadFromDatabase();
        });

        return cachedList ?? new List<TEntity>();
    }
}