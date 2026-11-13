using System;
using System.Collections.Generic;

namespace DolApp.Core.Entities;

/// <summary>
/// Profil, onam ve silme bilgisi. id Supabase giriş hesabından gelir; hesap silinince satır kimliksiz kalır.
/// </summary>
public partial class User
{
    public Guid Id { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public DateOnly? BirthDate { get; set; }

    public long? DefaultCityId { get; set; }

    public long? DefaultDistrictId { get; set; }

    public bool IsConsentAccepted { get; set; }

    public DateTime ConsentAt { get; set; }

    public bool IsOnboardingAccepted { get; set; }

    public DateTime? OnboardingAcceptedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<ComparisonSession> ComparisonSessions { get; set; } = new List<ComparisonSession>();

    public virtual City? DefaultCity { get; set; }

    public virtual District? DefaultDistrict { get; set; }

    public virtual District? District { get; set; }

    public virtual Wardrobe? Wardrobe { get; set; }
}
