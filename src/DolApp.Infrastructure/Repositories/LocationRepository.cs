using DolApp.Application.Interfaces;
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
}