using DolApp.Application.Interfaces;
using DolApp.Application.UseCases.Account.GetMe;
using DolApp.Core.Errors;
using MediatR;

namespace DolApp.Application.UseCases.Account.UpdateMe;

/// Ad, soyad ve Şehrim'i günceller; cevap GET /api/me ile aynı biçimdedir.
/// Bir kontrol başarısız olursa SaveChanges'a hiç gelinmez, veritabanına hiçbir şey yazılmaz.

public class UpdateMeHandler : IRequestHandler<UpdateMeRequest, GetMeResponse>
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserRepository _userRepository;
    private readonly ILocationRepository _locationRepository;

    public UpdateMeHandler(
        ICurrentUser currentUser,
        IUserRepository userRepository,
        ILocationRepository locationRepository)
    {
        _currentUser = currentUser;
        _userRepository = userRepository;
        _locationRepository = locationRepository;
    }

    public async Task<GetMeResponse> Handle(UpdateMeRequest request, CancellationToken cancellationToken)
    {
        var userToUpdate = await _userRepository.GetActiveForUpdateAsync(_currentUser.UserId, cancellationToken);
        if (userToUpdate is null)
            throw new AppException(401, ErrorCodes.Unauthorized, ErrorMessages.Unauthorized);

        if (request.FirstName is not null)
            userToUpdate.FirstName = request.FirstName.Trim();

        if (request.LastName is not null)
            userToUpdate.LastName = request.LastName.Trim();

        if (request.IsDefaultLocationSent)
        {
            await EnsureDefaultLocationIsValidAsync(request.DefaultCityId, request.DefaultDistrictId, cancellationToken);
            userToUpdate.DefaultCityId = request.DefaultCityId;
            userToUpdate.DefaultDistrictId = request.DefaultDistrictId;
        }

        await _userRepository.SaveChangesAsync(cancellationToken);

        // İl ve ilçe adları için kullanıcıyı Include ile yeniden oku.
        var updatedUser = await _userRepository.GetActiveWithDefaultLocationAsync(_currentUser.UserId, cancellationToken);
        if (updatedUser is null)
            throw new AppException(401, ErrorCodes.Unauthorized, ErrorMessages.Unauthorized);

        return GetMeResponse.FromUser(updatedUser);
    }

    /// İl ve ilçe veritabanında var mı, ilçe o ile ait mi?
    /// İkisi de null ise Şehrim temizleniyordur; kontrol edilecek bir şey yoktur.
    /// (İlçe var ama il yok durumunu validator zaten yakaladı.)
    private async Task EnsureDefaultLocationIsValidAsync(
        long? defaultCityId, long? defaultDistrictId, CancellationToken cancellationToken)
    {
        if (defaultCityId is null)
            return;

        var cityExists = await _locationRepository.CityExistsAsync(defaultCityId.Value, cancellationToken);
        if (!cityExists)
            throw new AppException(400, ErrorCodes.InvalidId, ErrorMessages.InvalidId);

        if (defaultDistrictId is null)
            return;

        var districtBelongsToCity = await _locationRepository.DistrictBelongsToCityAsync(
            defaultDistrictId.Value, defaultCityId.Value, cancellationToken);
        if (!districtBelongsToCity)
            throw new AppException(400, ErrorCodes.DistrictNotInCity, ErrorMessages.DistrictNotInCity);
    }
}