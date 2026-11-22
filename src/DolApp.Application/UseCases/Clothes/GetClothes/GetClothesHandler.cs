using DolApp.Application.Interfaces;
using DolApp.Core.Constants;
using DolApp.Core.Errors;
using MediatR;

namespace DolApp.Application.UseCases.Clothes.GetClothes;

/// Dolabım listesi (FR-09, S4). Akış:
///   1) Kullanıcının dolabındaki onaylı parçalar (taslak ve silinmişler gelmez).
///   2) Gösterilecek parçaların görselleri için tek istekte süreli bağlantı (filtre varsa yalnızca o türünkiler).
///   3) Gruplar ClothingGroupBuilder'da kurulur. totalCount filtreden bağımsız, bütün onaylı parçalar.
/// Yalnızca okur; hiçbir şey yazmaz.

public class GetClothesHandler : IRequestHandler<GetClothesRequest, GetClothesResponse>
{
    private readonly ICurrentUser _currentUser;
    private readonly IWardrobeRepository _wardrobeRepository;
    private readonly IClothingRepository _clothingRepository;
    private readonly ILookupRepository _lookupRepository;
    private readonly IImageStorage _imageStorage;

    public GetClothesHandler(
        ICurrentUser currentUser,
        IWardrobeRepository wardrobeRepository,
        IClothingRepository clothingRepository,
        ILookupRepository lookupRepository,
        IImageStorage imageStorage)
    {
        _currentUser = currentUser;
        _wardrobeRepository = wardrobeRepository;
        _clothingRepository = clothingRepository;
        _lookupRepository = lookupRepository;
        _imageStorage = imageStorage;
    }

    public async Task<GetClothesResponse> Handle(GetClothesRequest request, CancellationToken cancellationToken)
    {
        // 1) Onaylı parçalar
        var wardrobeId = await _wardrobeRepository.GetWardrobeIdByUserIdAsync(_currentUser.UserId, cancellationToken)
            ?? throw new AppException(404, ErrorCodes.NotFound, ErrorMessages.NotFound);

        var clothingStatuses = await _lookupRepository.GetClothingStatusesAsync(cancellationToken);
        var approvedStatusId = clothingStatuses.First(status => status.Value == ClothingStatusValues.Approved).Id;
        var approvedClothes = await _clothingRepository.GetApprovedAsync(wardrobeId, approvedStatusId, cancellationToken);

        // 2) Görsel bağlantıları: yalnızca gösterilecek parçalar için, tek istekte
        var visibleClothes = request.SubtypeId is null
            ? approvedClothes
            : approvedClothes.Where(clothe => clothe.SubtypeId == request.SubtypeId).ToList();

        var imagePaths = visibleClothes
            .Where(clothe => !string.IsNullOrEmpty(clothe.ImagePath))
            .Select(clothe => clothe.ImagePath!)
            .Distinct()
            .ToList();

        IReadOnlyDictionary<string, string> imageUrls = imagePaths.Count == 0
            ? new Dictionary<string, string>()
            : await _imageStorage.CreateSignedUrlsAsync(imagePaths, cancellationToken);

        // 3) Gruplar (tür adları lookups'tan, önbellekli)
        var subtypes = await _lookupRepository.GetSubtypesAsync(cancellationToken);
        var subtypeNames = subtypes.ToDictionary(subtype => subtype.Id, subtype => subtype.DisplayName);

        return new GetClothesResponse
        {
            TotalCount = approvedClothes.Count,
            Groups = ClothingGroupBuilder.Build(visibleClothes, subtypeNames, imageUrls, request.SubtypeId),
        };
    }
}