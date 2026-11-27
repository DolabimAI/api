using DolApp.Application.Interfaces;
using DolApp.Application.Models;
using DolApp.Application.UseCases.Comparisons.Common;
using DolApp.Application.UseCases.Wardrobe.GetWardrobeStatus;
using DolApp.Application.UseCases.Weather.GetWeather;
using DolApp.Core.Constants;
using DolApp.Core.Entities;
using DolApp.Core.Errors;
using MediatR;

namespace DolApp.Application.UseCases.Comparisons.CreateSession;

/// Kombin isteği: formdan iki karta (FR-13, FR-16 – FR-19; S6). Akış:
///   1) Dolap şartı (C4'teki hesap); hazır değilse 409 wardrobe_not_ready.
///   2) Etkinlik listede mi (yoksa 400 invalid_id).
///   3) Hava GET /api/weather ile aynı hesapla, aynı istek gönderilerek alınır (gün aralığı, time_passed,
///      district_not_in_city ve 502 weather_unavailable oradan gelir).
///   4) Onaylı parçalar ve öğrenmeye giren geçmiş kararlar okunur; Python isteği RecommendationRequestBuilder ile kurulur.
///   5) Python /recommend. Aday yoksa 422 no_candidates; Python sorunu 502 recommendation_failed (istemciden).
///      Python'u beklerken açık veritabanı işlemi yoktur.
///   6) Gelen parçalar bu dolabın onaylı parçaları mı, seçilme biçimi biliniyor mu (değilse 502).
///   7) Kural kartı rastgele sola ya da sağa konur; tek kartta kombin solda ve left_method_id kuraldır.
///   8) Tek SaveChanges = tek işlem: kullanıcının açık (BEKLIYOR) kartları TEPKISIZ olur, oturum ve ilk kart
///      (sıra 1, BEKLIYOR) yazılır. Daha önceki bir adım başarısız olursa hiçbir şey yazılmaz.
///   9) Cevap: oturum id'si, hava ve kart (yöntem bilgisi yok).

public class CreateSessionHandler : IRequestHandler<CreateSessionRequest, CreateSessionResponse>
{
    private readonly ICurrentUser _currentUser;
    private readonly IMediator _mediator;
    private readonly IWardrobeRepository _wardrobeRepository;
    private readonly IClothingRepository _clothingRepository;
    private readonly IComparisonRepository _comparisonRepository;
    private readonly ILookupRepository _lookupRepository;
    private readonly IOutfitRecommender _outfitRecommender;
    private readonly IImageStorage _imageStorage;
    private readonly IRandomProvider _randomProvider;

    public CreateSessionHandler(
        ICurrentUser currentUser,
        IMediator mediator,
        IWardrobeRepository wardrobeRepository,
        IClothingRepository clothingRepository,
        IComparisonRepository comparisonRepository,
        ILookupRepository lookupRepository,
        IOutfitRecommender outfitRecommender,
        IImageStorage imageStorage,
        IRandomProvider randomProvider)
    {
        _currentUser = currentUser;
        _mediator = mediator;
        _wardrobeRepository = wardrobeRepository;
        _clothingRepository = clothingRepository;
        _comparisonRepository = comparisonRepository;
        _lookupRepository = lookupRepository;
        _outfitRecommender = outfitRecommender;
        _imageStorage = imageStorage;
        _randomProvider = randomProvider;
    }

    public async Task<CreateSessionResponse> Handle(CreateSessionRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        // 1) Dolap şartı
        var wardrobeSummary = await _wardrobeRepository.GetSummaryByUserIdAsync(userId, cancellationToken);
        var wardrobeId = wardrobeSummary?.WardrobeId
            ?? throw new AppException(404, ErrorCodes.NotFound, ErrorMessages.NotFound);
        var templateRequirements = await _lookupRepository.GetTemplateRequirementsAsync(cancellationToken);
        if (!GetWardrobeStatusResponse.Calculate(wardrobeSummary, templateRequirements).IsReady)
            throw new AppException(409, ErrorCodes.WardrobeNotReady, ErrorMessages.WardrobeNotReady);

        // 2) Etkinlik (validator alanların dolu olduğunu garanti eder)
        var styles = await _lookupRepository.GetStylesAsync(cancellationToken);
        var eventStyle = styles.FirstOrDefault(style => style.Id == request.EventStyleId!.Value)
            ?? throw new AppException(400, ErrorCodes.InvalidId, ErrorMessages.InvalidId);

        // 3) Hava: GET /api/weather ile aynı istek (aynı validator ve hata kodları)
        var weather = await _mediator.Send(new GetWeatherRequest
        {
            CityId = request.CityId,
            DistrictId = request.DistrictId,
            Date = request.Date,
            TimeOfDayId = request.TimeOfDayId,
        }, cancellationToken);

        var weatherConditions = await _lookupRepository.GetWeatherConditionsAsync(cancellationToken);
        var weatherCondition = weatherConditions.First(condition => condition.Value == weather.Condition.Value);
        var timesOfDay = await _lookupRepository.GetTimesOfDayAsync(cancellationToken);
        var timeOfDay = timesOfDay.First(timeOfDayItem => timeOfDayItem.Id == request.TimeOfDayId!.Value);

        // 4) Parçalar, geçmiş ve Python isteği
        var clothingStatuses = await _lookupRepository.GetClothingStatusesAsync(cancellationToken);
        var approvedStatusId = clothingStatuses.First(status => status.Value == ClothingStatusValues.Approved).Id;
        var approvedClothes = await _clothingRepository.GetApprovedAsync(wardrobeId, approvedStatusId, cancellationToken);

        var resultStatuses = await _lookupRepository.GetResultStatusesAsync(cancellationToken);
        var learningResultIds = resultStatuses
            .Where(resultStatus => ResultStatusValues.LearningResultKeys.Contains(resultStatus.ResultKey))
            .Select(resultStatus => resultStatus.Id)
            .ToList();
        var learningHistory = await _comparisonRepository.GetLearningHistoryAsync(userId, learningResultIds, cancellationToken);
        var historyItemIds = RecommendationRequestBuilder.CollectHistoryItemIds(learningHistory);
        var historyClothes = historyItemIds.Count == 0
            ? new List<Clothe>()
            : await _clothingRepository.GetByIdsAsync(wardrobeId, historyItemIds, cancellationToken);

        var lookups = RecommendationLookups.Create(
            await _lookupRepository.GetSubtypesAsync(cancellationToken),
            await _lookupRepository.GetCategoriesAsync(cancellationToken),
            await _lookupRepository.GetColorsAsync(cancellationToken),
            await _lookupRepository.GetTonesAsync(cancellationToken),
            styles,
            clothingStatuses,
            weatherConditions,
            resultStatuses);

        var context = new RecommendationContext
        {
            FeelsLikeTemp = weather.FeelsLikeTemperature,
            WeatherCondition = weatherCondition.Value,
            TimeOfDay = timeOfDay.Value,
            IsDaytime = timeOfDay.IsDaytime,
            EventStyle = eventStyle.Value,
            WantsAccessories = request.WantsAccessories!.Value,
        };
        var recommendationRequest = RecommendationRequestBuilder.Build(
            userId, context, approvedClothes, learningHistory, historyClothes, lookups);

        // 5) Python
        var recommendation = await _outfitRecommender.RecommendAsync(recommendationRequest, cancellationToken);
        if (!recommendation.HasCandidates)
            throw new AppException(422, ErrorCodes.NoCandidates, ErrorMessages.NoCandidates);

        // 6) Cevabın kontrolü: Python yalnızca gönderilen dolaptan seçebilir
        var approvedClothesById = approvedClothes.ToDictionary(clothe => clothe.Id);
        var returnedItemIds = recommendation.RuleItemIds
            .Concat(recommendation.LearnedItemIds)
            .Concat(recommendation.AccessoryIds);
        var learnedPickModes = await _lookupRepository.GetLearnedPickModesAsync(cancellationToken);
        var learnedPickMode = learnedPickModes.FirstOrDefault(mode => mode.Value == recommendation.LearnedPickMode);
        if (returnedItemIds.Any(itemId => !approvedClothesById.ContainsKey(itemId)) || learnedPickMode is null)
            throw new AppException(502, ErrorCodes.RecommendationFailed, ErrorMessages.RecommendationFailed);

        // 7) Sol / sağ
        var methods = await _lookupRepository.GetMethodsAsync(cancellationToken);
        var ruleMethodId = methods.First(method => method.Value == MethodValues.Rule).Id;
        var learnedMethodId = methods.First(method => method.Value == MethodValues.Learned).Id;
        var leftMethodId = recommendation.IsSameOutfit || _randomProvider.NextBool() ? ruleMethodId : learnedMethodId;

        // 8) Tek işlemde yazma
        var pendingResultId = resultStatuses.First(resultStatus => resultStatus.Value == ResultStatusValues.Pending).Id;
        var unansweredResultId = resultStatuses.First(resultStatus => resultStatus.Value == ResultStatusValues.Unanswered).Id;

        var pendingComparisons = await _comparisonRepository.GetPendingAsync(userId, pendingResultId, cancellationToken);
        foreach (var pendingComparison in pendingComparisons)
            pendingComparison.ResultId = unansweredResultId;

        var comparison = new Comparison
        {
            SequenceNo = 1,
            LeftMethodId = leftMethodId,
            RuleOutfit = recommendation.RuleItemIds,
            LearnedOutfit = recommendation.LearnedItemIds,
            Accessories = recommendation.AccessoryIds,
            ResultId = pendingResultId,
            LearnedPickModeId = learnedPickMode.Id,
        };
        var session = new ComparisonSession
        {
            UserId = userId,
            CityId = request.CityId!.Value,
            DistrictId = request.DistrictId,
            TargetDate = request.Date!.Value,
            TimeOfDayId = timeOfDay.Id,
            EventStyleId = eventStyle.Id,
            WantsAccessories = request.WantsAccessories!.Value,
            FeelsLikeTemp = weather.FeelsLikeTemperature,
            WeatherConditionId = weatherCondition.Id,
        };
        session.Comparisons.Add(comparison);

        _comparisonRepository.AddSession(session);
        await _comparisonRepository.SaveChangesAsync(cancellationToken);

        // 9) Cevap: görseller için tek istekte süreli bağlantı
        var imagePaths = ComparisonCardMapper.CollectImagePaths(comparison, approvedClothesById);
        IReadOnlyDictionary<string, string> imageUrls = imagePaths.Count == 0
            ? new Dictionary<string, string>()
            : await _imageStorage.CreateSignedUrlsAsync(imagePaths, cancellationToken);

        return new CreateSessionResponse
        {
            SessionId = session.Id,
            Weather = weather,
            Comparison = ComparisonCardMapper.ToCard(comparison, ruleMethodId, approvedClothesById, imageUrls),
        };
    }
}