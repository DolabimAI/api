namespace DolApp.Core.Errors;

public static class ErrorCodes
{
    // Genel: middleware metodu kullanılır
    public const string InvalidRequest = "invalid_request";
    public const string InternalError = "internal_error";
    public const string Unauthorized = "unauthorized";
    public const string InvalidId = "invalid_id";
    public const string NotFound = "not_found";


    // Hesap oluşumu için gerekli olan hata kodları
    public const string EmailTaken = "email_taken";
    public const string InvalidCredentials = "invalid_credentials";
    public const string WeakPassword = "weak_password";
    public const string RequiredFields = "required_fields";
    public const string InvalidEmail = "invalid_email";
    public const string Under18 = "under_18";
    public const string ConsentRequired = "consent_required";

    // Şehrim
    public const string CityRequired = "city_required";
    public const string DistrictNotInCity = "district_not_in_city";

    // Konum
    public const string LocationOutOfRange = "location_out_of_range";

    // Hava durumu
    public const string DateOutOfRange = "date_out_of_range";
    public const string TimePassed = "time_passed";
    public const string WeatherUnavailable = "weather_unavailable";
}


