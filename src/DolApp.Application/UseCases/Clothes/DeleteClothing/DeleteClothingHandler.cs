using DolApp.Application.Interfaces;
using DolApp.Application.UseCases.Wardrobe.GetWardrobeStatus;
using DolApp.Core.Constants;
using DolApp.Core.Errors;
using MediatR;

namespace DolApp.Application.UseCases.Clothes.DeleteClothing;

/// Parçayı siler (FR-09, S5). Akış:
///   1) Parça kullanıcının ve onaylı mı (değilse 404: taslak, zaten silinmiş ya da başkasının; T-17).
///   2) Durum "silindi", deleted_at yazılır. Satır ve görsel KALIR: parça dolaptan ve sonraki önerilerden çıkar,
///      geçmiş karşılaştırma kayıtlarında görünmeye devam eder. Tek SaveChanges.
///   3) Cevap: güncel dolap durumu (C4'teki hesap).

public class DeleteClothingHandler : IRequestHandler<DeleteClothingRequest, DeleteClothingResponse>
{
    private readonly ICurrentUser _currentUser;
    private readonly IWardrobeRepository _wardrobeRepository;
    private readonly IClothingRepository _clothingRepository;
    private readonly ILookupRepository _lookupRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DeleteClothingHandler(
        ICurrentUser currentUser,
        IWardrobeRepository wardrobeRepository,
        IClothingRepository clothingRepository,
        ILookupRepository lookupRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _currentUser = currentUser;
        _wardrobeRepository = wardrobeRepository;
        _clothingRepository = clothingRepository;
        _lookupRepository = lookupRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<DeleteClothingResponse> Handle(DeleteClothingRequest request, CancellationToken cancellationToken)
    {
        // 1) Parça ve durumu
        var wardrobeId = await _wardrobeRepository.GetWardrobeIdByUserIdAsync(_currentUser.UserId, cancellationToken)
            ?? throw new AppException(404, ErrorCodes.NotFound, ErrorMessages.NotFound);
        var clothe = await _clothingRepository.GetForUpdateAsync(request.ClothingId, wardrobeId, cancellationToken)
            ?? throw new AppException(404, ErrorCodes.NotFound, ErrorMessages.NotFound);

        var clothingStatuses = await _lookupRepository.GetClothingStatusesAsync(cancellationToken);
        if (clothe.StatusId != clothingStatuses.First(status => status.Value == ClothingStatusValues.Approved).Id)
            throw new AppException(404, ErrorCodes.NotFound, ErrorMessages.NotFound);

        // 2) Silindi olarak işaretle; satır ve görsel kalır
        clothe.StatusId = clothingStatuses.First(status => status.Value == ClothingStatusValues.Deleted).Id;
        clothe.DeletedAt = _dateTimeProvider.UtcNow;

        await _clothingRepository.SaveChangesAsync(cancellationToken);

        // 3) Güncel dolap durumu
        var wardrobeSummary = await _wardrobeRepository.GetSummaryByUserIdAsync(_currentUser.UserId, cancellationToken)
            ?? throw new AppException(404, ErrorCodes.NotFound, ErrorMessages.NotFound);
        var templateRequirements = await _lookupRepository.GetTemplateRequirementsAsync(cancellationToken);

        return new DeleteClothingResponse
        {
            WardrobeStatus = GetWardrobeStatusResponse.Calculate(wardrobeSummary, templateRequirements),
        };
    }
}