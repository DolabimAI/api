using DolApp.Application.Interfaces;
using DolApp.Core.Errors;
using MediatR;

namespace DolApp.Application.UseCases.Account.AcceptOnboarding;

/// Bilgilendirme onayını kaydeder. Tek yönlüdür: bir kez true olur, geri alınmaz.
/// Tekrar çağrılırsa hiçbir şey değişmez; onboarding_accepted_at ilk onayın zamanı olarak kalır.

public class AcceptOnboardingHandler : IRequestHandler<AcceptOnboardingRequest, AcceptOnboardingResponse>
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserRepository _userRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AcceptOnboardingHandler(
        ICurrentUser currentUser,
        IUserRepository userRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _currentUser = currentUser;
        _userRepository = userRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<AcceptOnboardingResponse> Handle(AcceptOnboardingRequest request, CancellationToken cancellationToken)
    {
        var userToUpdate = await _userRepository.GetActiveForUpdateAsync(_currentUser.UserId, cancellationToken);
        if (userToUpdate is null)
            throw new AppException(401, ErrorCodes.Unauthorized, ErrorMessages.Unauthorized);

        // Zaten onaylıysa dokunma: ilk onayın zamanı korunur.
        if (!userToUpdate.IsOnboardingAccepted)
        {
            userToUpdate.IsOnboardingAccepted = true;
            userToUpdate.OnboardingAcceptedAt = _dateTimeProvider.UtcNow;
            await _userRepository.SaveChangesAsync(cancellationToken);
        }

        return new AcceptOnboardingResponse();
    }
}