using DolApp.Application.Interfaces;
using DolApp.Core.Errors;
using MediatR;

namespace DolApp.Application.UseCases.Account.GetMe;

/// Token'daki kullanıcının bilgilerini Şehrim'le birlikte döner.
/// Ana sayfa karşılaması, Profil > Bilgilerim ve formun Şehrim'le dolması bunu kullanır.

public class GetMeHandler : IRequestHandler<GetMeRequest, GetMeResponse>
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserRepository _userRepository;

    public GetMeHandler(ICurrentUser currentUser, IUserRepository userRepository)
    {
        _currentUser = currentUser;
        _userRepository = userRepository;
    }

    public async Task<GetMeResponse> Handle(GetMeRequest request, CancellationToken cancellationToken)
    {
        var activeUser = await _userRepository.GetActiveWithDefaultLocationAsync(_currentUser.UserId, cancellationToken);

        // Token doğrulaması silinmiş hesabı zaten reddediyor (2.2). Bu kontrol, token doğrulandıktan hemen sonra hesap silinirse diye ikinci bir güvence.
        if (activeUser is null)
            throw new AppException(401, ErrorCodes.Unauthorized, ErrorMessages.Unauthorized);

        return new GetMeResponse
        {
            FirstName = activeUser.FirstName ?? string.Empty,
            LastName = activeUser.LastName ?? string.Empty,
            Email = activeUser.Email ?? string.Empty,
            BirthDate = activeUser.BirthDate,
            IsOnboardingAccepted = activeUser.IsOnboardingAccepted,
            DefaultCity = activeUser.DefaultCity is null ? null : new MeLocationResponse { Id = activeUser.DefaultCity.Id, Name = activeUser.DefaultCity.DisplayName },
            DefaultDistrict = activeUser.DefaultDistrict is null ? null : new MeLocationResponse { Id = activeUser.DefaultDistrict.Id, Name = activeUser.DefaultDistrict.DisplayName },
        };
    }
}