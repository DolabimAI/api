using DolApp.Application.Interfaces;
using DolApp.Application.UseCases.Clothes.AnalyzeClothing;
using DolApp.Core.Constants;
using DolApp.Core.Errors;
using MediatR;

namespace DolApp.Application.UseCases.Clothes.GetClothingById;

/// Parça detayı (FR-09, S5): görsel, alt tür, kategori, ana renk, ton, desen, tarzlar, mevsimler, kalınlık.
/// Sıcaklık aralığı dönmez (plan kuralı; cevapta alan yok).
/// Yalnızca onaylı parça: taslak, silinmiş ya da başka kullanıcının parçası 404 (T-17). Yalnızca okur.

public class GetClothingByIdHandler : IRequestHandler<GetClothingByIdRequest, GetClothingByIdResponse>
{
    private readonly ICurrentUser _currentUser;
    private readonly IWardrobeRepository _wardrobeRepository;
    private readonly IClothingRepository _clothingRepository;
    private readonly ILookupRepository _lookupRepository;
    private readonly IImageStorage _imageStorage;

    public GetClothingByIdHandler(
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

    public async Task<GetClothingByIdResponse> Handle(GetClothingByIdRequest request, CancellationToken cancellationToken)
    {
        var wardrobeId = await _wardrobeRepository.GetWardrobeIdByUserIdAsync(_currentUser.UserId, cancellationToken)
            ?? throw new AppException(404, ErrorCodes.NotFound, ErrorMessages.NotFound);
        var clothe = await _clothingRepository.GetForUpdateAsync(request.ClothingId, wardrobeId, cancellationToken)
            ?? throw new AppException(404, ErrorCodes.NotFound, ErrorMessages.NotFound);

        var clothingStatuses = await _lookupRepository.GetClothingStatusesAsync(cancellationToken);
        if (clothe.StatusId != clothingStatuses.First(status => status.Value == ClothingStatusValues.Approved).Id)
            throw new AppException(404, ErrorCodes.NotFound, ErrorMessages.NotFound);

        var imageUrl = string.IsNullOrEmpty(clothe.ImagePath)
            ? null
            : await _imageStorage.CreateSignedUrlAsync(clothe.ImagePath, cancellationToken);

        return GetClothingByIdResponse.FromClothe(clothe, imageUrl);
    }
}