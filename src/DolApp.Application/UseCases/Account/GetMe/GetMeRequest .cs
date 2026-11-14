using MediatR;

namespace DolApp.Application.UseCases.Account.GetMe;

/// GET /api/me isteği. Alanı yoktur: hangi kullanıcının istendiği token'dan bulunur,
/// uygulama kullanıcı id'si göndermez (API belgesi, Ortak kurallar > Kimlik).

public class GetMeRequest : IRequest<GetMeResponse>
{
}