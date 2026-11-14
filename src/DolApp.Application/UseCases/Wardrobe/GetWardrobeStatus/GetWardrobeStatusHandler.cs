using DolApp.Application.Interfaces;
using DolApp.Core.Errors;
using MediatR;

namespace DolApp.Application.UseCases.Wardrobe.GetWardrobeStatus;

/// Kullanıcının dolap durumunu döner: dolap şartı tamam mı, değilse en yakın şablonun eksikleri.

public class GetWardrobeStatusHandler : IRequestHandler<GetWardrobeStatusRequest, GetWardrobeStatusResponse>
{
    private readonly ICurrentUser _currentUser;
    private readonly IWardrobeRepository _wardrobeRepository;
    private readonly ILookupRepository _lookupRepository;

    public GetWardrobeStatusHandler(
        ICurrentUser currentUser,
        IWardrobeRepository wardrobeRepository,
        ILookupRepository lookupRepository)
    {
        _currentUser = currentUser;
        _wardrobeRepository = wardrobeRepository;
        _lookupRepository = lookupRepository;
    }

    public async Task<GetWardrobeStatusResponse> Handle(
        GetWardrobeStatusRequest request, CancellationToken cancellationToken)
    {
        // Kayıtta her kullanıcıya bir dolap açılır; bulunamazsa açık bir hata dönsün.
        var wardrobeSummary = await _wardrobeRepository.GetSummaryByUserIdAsync(_currentUser.UserId, cancellationToken);
        if (wardrobeSummary is null)
            throw new AppException(404, ErrorCodes.NotFound, ErrorMessages.NotFound);

        var templateRequirements = await _lookupRepository.GetTemplateRequirementsAsync(cancellationToken);

        return GetWardrobeStatusResponse.Calculate(wardrobeSummary, templateRequirements);
    }
}