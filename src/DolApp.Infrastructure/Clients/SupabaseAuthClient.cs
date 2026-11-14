using DolApp.Application.Interfaces;
using DolApp.Core.Errors;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DolApp.Infrastructure.Clients;

/// Supabase Auth'un REST uçlarıyla konuşur.
/// Hesap açma/silme yönetici yoluyla (secret key) yapılır; Supabase'in herkese açık kayıt kapısı kapalıdır.
/// Giriş, publishable key ile yapılır.

public class SupabaseAuthClient : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseOptions _supabaseOptions;

    public SupabaseAuthClient(HttpClient httpClient, SupabaseOptions supabaseOptions)
    {
        _httpClient = httpClient;
        _supabaseOptions = supabaseOptions;
    }

    public async Task<Guid> CreateUserAsync(string email, string password, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "auth/v1/admin/users")
        {
            // email_confirm = true: e-posta doğrulaması istenmez, hesap hemen kullanılabilir.
            Content = JsonContent.Create(new { email, password, email_confirm = true })
        };
        AddApiKey(request, _supabaseOptions.SecretKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var createdUser = await response.Content.ReadFromJsonAsync<SupabaseUser>(cancellationToken);
            return createdUser!.Id;
        }

        var supabaseError = await ReadErrorAsync(response, cancellationToken);
        if (supabaseError.ErrorCode is "email_exists" or "user_already_exists")
            throw new AppException(409, ErrorCodes.EmailTaken, ErrorMessages.EmailTaken);
        if (supabaseError.ErrorCode is "weak_password")
            throw new AppException(400, ErrorCodes.WeakPassword, ErrorMessages.WeakPassword);

        // Beklenmeyen hata: middleware 500'e çevirir, ayrıntı yalnızca loga gider.
        throw new InvalidOperationException(
            $"Supabase hesap açma hatası: {(int)response.StatusCode} {response.RequestMessage?.RequestUri} {supabaseError.RawBody}");
    }

    public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"auth/v1/admin/users/{userId}");
        AddApiKey(request, _supabaseOptions.SecretKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        // Hesap zaten yoksa (404) amaç gerçekleşmiş sayılır; tekrar çağrılabilir olması için hata vermiyoruz.
        if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotFound)
        {
            var supabaseError = await ReadErrorAsync(response, cancellationToken);
            throw new InvalidOperationException(
                $"Supabase hesap silme hatası: {(int)response.StatusCode} {supabaseError.RawBody}");
        }
    }

    public async Task<AuthSession> SignInAsync(string email, string password, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "auth/v1/token?grant_type=password")
        {
            Content = JsonContent.Create(new { email, password })
        };
        AddApiKey(request, _supabaseOptions.PublishableKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var tokenResponse = await response.Content.ReadFromJsonAsync<SupabaseTokenResponse>(cancellationToken);
            return new AuthSession(
                tokenResponse!.User.Id,
                tokenResponse.AccessToken,
                DateTimeOffset.FromUnixTimeSeconds(tokenResponse.ExpiresAt));
        }

        // Yanlış şifre, olmayan ya da silinmiş hesap: hepsi aynı cevap (T-05).
        if ((int)response.StatusCode is 400 or 401 or 403 or 422)
            throw new AppException(401, ErrorCodes.InvalidCredentials, ErrorMessages.InvalidCredentials);

        var supabaseError = await ReadErrorAsync(response, cancellationToken);
        throw new InvalidOperationException(
            $"Supabase giriş hatası: {(int)response.StatusCode} {supabaseError.RawBody}");
    }


    /// Anahtarı iki başlıkta aynı değerle gönderir; yeni ve eski tip anahtarlarla çalışır.
    private static void AddApiKey(HttpRequestMessage request, string apiKey)
    {
        request.Headers.Add("apikey", apiKey);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    /// Supabase hata cevabından "error_code" alanını okur (ör. email_exists).
    private static async Task<SupabaseError> ReadErrorAsync(
        HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
        try
        {
            using var jsonDocument = JsonDocument.Parse(rawBody);
            if (jsonDocument.RootElement.TryGetProperty("error_code", out var errorCodeElement))
                return new SupabaseError(errorCodeElement.GetString(), rawBody);
        }
        catch (JsonException)
        {
            // Cevap JSON değilse kodu bilmiyoruz; ham metni loga yazacağız.
        }
        return new SupabaseError(null, rawBody);
    }

    // ---- Supabase'in JSON cevaplarından yalnızca ihtiyacımız olan alanlar ----

    private sealed record SupabaseUser(
        [property: JsonPropertyName("id")] Guid Id);

    private sealed record SupabaseTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_at")] long ExpiresAt,
        [property: JsonPropertyName("user")] SupabaseUser User);

    private sealed record SupabaseError(string? ErrorCode, string RawBody);
}
