using MediatR;

namespace DolApp.Application.UseCases.Comparisons.CreateSession;

/// POST /api/comparisons/sessions gövdesi (kombin formu). districtId isteğe bağlı; diğerleri zorunlu (null = gönderilmemiş).
/// Örnek: { "cityId": 34, "districtId": 442, "date": "2026-11-28", "timeOfDayId": 3, "eventStyleId": 1, "wantsAccessories": true }

public class CreateSessionRequest : IRequest<CreateSessionResponse>
{
    public long? CityId { get; set; }
    public long? DistrictId { get; set; }
    public DateOnly? Date { get; set; }
    public long? TimeOfDayId { get; set; }
    public long? EventStyleId { get; set; }
    public bool? WantsAccessories { get; set; }
}