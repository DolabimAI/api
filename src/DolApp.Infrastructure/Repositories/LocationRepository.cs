using DolApp.Application.Interfaces;
using DolApp.Core.Entities;
using DolApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DolApp.Infrastructure.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly AppDbContext _dbContext;

    public LocationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> CityExistsAsync(long cityId, CancellationToken cancellationToken)
    {
        return await _dbContext.Cities.AnyAsync(city => city.Id == cityId, cancellationToken);
    }

    public async Task<bool> DistrictBelongsToCityAsync(long districtId, long cityId, CancellationToken cancellationToken)
    {
        return await _dbContext.Districts
            .AnyAsync(district => district.Id == districtId && district.CityId == cityId, cancellationToken);
    }

    public async Task<List<City>> GetCitiesAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Cities
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<District>> GetDistrictsByCityIdAsync(long cityId, CancellationToken cancellationToken)
    {
        return await _dbContext.Districts
            .AsNoTracking()
            .Where(district => district.CityId == cityId)
            .ToListAsync(cancellationToken);
    }

    public async Task<District?> GetNearestDistrictAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken)
    {
        // Türkiye'nin enleminde 1° boylam, 1° enlemin yaklaşık %75'i kadardır; boylam farkı bu oranla küçültülür.
        var longitudeScale = (decimal)Math.Cos((double)latitude * Math.PI / 180);

        return await _dbContext.Districts
            .AsNoTracking()
            .Include(district => district.City)
            .OrderBy(district =>
                (district.Latitude - latitude) * (district.Latitude - latitude)
                + (district.Longitude - longitude) * longitudeScale * (district.Longitude - longitude) * longitudeScale)
            .FirstOrDefaultAsync(cancellationToken);
    }
}