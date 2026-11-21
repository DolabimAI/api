using DolApp.Application.Interfaces;
using DolApp.Core.Constants;
using DolApp.Core.Errors;
using MediatR;

namespace DolApp.Application.UseCases.Clothes.DeleteDraft;

/// Taslak parçayı tamamen siler (FR-08, S3: kaydetmeden çıkış ve Yeniden çek; T-12).
/// Yalnızca taslakta çalışır; onaylı ya da silinmiş parçada 409 not_a_draft.
/// Sıra analyze'daki eski taslak temizliğiyle aynı: önce görsel, sonra satır (tahmin cascade ile).
/// Görsel silinemezse satır da kalır; bir sonraki yükleme taslağı yeniden temizler.

public class DeleteDraftHandler : IRequestHandler<DeleteDraftRequest, DeleteDraftResponse>
{
    private readonly ICurrentUser _currentUser;
    private readonly IWardrobeRepository _wardrobeRepository;
    private readonly IClothingRepository _clothingRepository;
    private readonly ILookupRepository _lookupRepository;
    private readonly IImageStorage _imageStorage;

    public DeleteDraftHandler(
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

    public async Task<DeleteDraftResponse> Handle(DeleteDraftRequest request, CancellationToken cancellationToken)
    {
        var wardrobeId = await _wardrobeRepository.GetWardrobeIdByUserIdAsync(_currentUser.UserId, cancellationToken)
            ?? throw new AppException(404, ErrorCodes.NotFound, ErrorMessages.NotFound);
        var clothe = await _clothingRepository.GetForUpdateAsync(request.ClothingId, wardrobeId, cancellationToken)
            ?? throw new AppException(404, ErrorCodes.NotFound, ErrorMessages.NotFound);

        var clothingStatuses = await _lookupRepository.GetClothingStatusesAsync(cancellationToken);
        if (clothe.StatusId != clothingStatuses.First(status => status.Value == ClothingStatusValues.Draft).Id)
            throw new AppException(409, ErrorCodes.NotADraft, ErrorMessages.NotADraft);

        if (!string.IsNullOrEmpty(clothe.ImagePath))
            await _imageStorage.DeleteAsync(clothe.ImagePath, cancellationToken);

        await _clothingRepository.DeleteAsync(new[] { clothe }, cancellationToken);
        return new DeleteDraftResponse();
    }
}