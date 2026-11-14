using MediatR;

namespace DolApp.Application.UseCases.Account.AcceptOnboarding;

/// POST /api/me/onboarding isteği. Gövdesi yoktur; kullanıcı token'dan bulunur.

public class AcceptOnboardingRequest : IRequest<AcceptOnboardingResponse>
{
}