using DolApp.Application.UseCases.Account.Register;
using DolApp.Core.Errors;
using DolApp.Tests.Fakes;

namespace DolApp.Tests.UseCases.Account;

/// T-03: Kayıt ortasında bir adım başarısız olunca Supabase giriş hesabı da kalmamalı.
/// Gerçek Supabase'e ve veritabanına gidilmez; sahte servisler kullanılır.
/// xUnit her test için bu class'tan yeni bir nesne oluşturur; sahte servisler testler arasında paylaşılmaz.

public class RegisterHandlerTests
{
    private readonly FakeAuthService _fakeAuthService = new();
    private readonly FakeUserRepository _fakeUserRepository = new();
    private readonly FakeDateTimeProvider _fakeDateTimeProvider = new();

    [Fact]
    public async Task Handle_WhenSignInFails_DeletesCreatedAuthAccount()
    {
        // Hazırla: 2. adım (giriş) hata verecek.
        _fakeAuthService.SignInException = new InvalidOperationException("Supabase giriş hatası (sahte)");
        var registerHandler = CreateRegisterHandler();

        // Çalıştır ve hatanın yukarı iletildiğini doğrula.
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => registerHandler.Handle(CreateValidRegisterRequest(), CancellationToken.None));

        // Doğrula: 1. adımda açılan hesap silindi, kullanıcı satırı yazılmadı.
        Assert.Equal(new[] { _fakeAuthService.CreatedUserId }, _fakeAuthService.DeletedUserIds);
        Assert.Empty(_fakeUserRepository.AddedUsers);
    }

    [Fact]
    public async Task Handle_WhenDatabaseWriteFails_DeletesCreatedAuthAccount()
    {
        // Hazırla: 3. adım (users + wardrobes yazımı) hata verecek.
        _fakeUserRepository.AddWithWardrobeException = new InvalidOperationException("Veritabanı hatası (sahte)");
        var registerHandler = CreateRegisterHandler();

        // Çalıştır
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => registerHandler.Handle(CreateValidRegisterRequest(), CancellationToken.None));

        // Doğrula: 1. adımda açılan hesap silindi.
        Assert.Equal(new[] { _fakeAuthService.CreatedUserId }, _fakeAuthService.DeletedUserIds);
    }

    [Fact]
    public async Task Handle_WhenEmailIsTaken_DoesNotDeleteAnything()
    {
        // Hazırla: 1. adım (hesap açma) 409 verecek; geri alınacak bir hesap yok.
        _fakeAuthService.CreateUserException = new AppException(409, ErrorCodes.EmailTaken, ErrorMessages.EmailTaken);
        var registerHandler = CreateRegisterHandler();

        // Çalıştır
        var appException = await Assert.ThrowsAsync<AppException>(
            () => registerHandler.Handle(CreateValidRegisterRequest(), CancellationToken.None));

        // Doğrula: kullanıcıya 409 email_taken dönüyor, hiçbir hesap silinmedi, satır yazılmadı.
        Assert.Equal(409, appException.StatusCode);
        Assert.Equal(ErrorCodes.EmailTaken, appException.Code);
        Assert.Empty(_fakeAuthService.DeletedUserIds);
        Assert.Empty(_fakeUserRepository.AddedUsers);
    }

    [Fact]
    public async Task Handle_WhenAllStepsSucceed_SavesUserAndDoesNotDelete()
    {
        // Hazırla
        var registerHandler = CreateRegisterHandler();

        // Çalıştır
        var registerResponse = await registerHandler.Handle(CreateValidRegisterRequest(), CancellationToken.None);

        // Doğrula: hiçbir hesap silinmedi, tek kullanıcı doğru bilgilerle yazıldı, cevap doğru.
        Assert.Empty(_fakeAuthService.DeletedUserIds);

        var addedUser = Assert.Single(_fakeUserRepository.AddedUsers);
        Assert.Equal(_fakeAuthService.CreatedUserId, addedUser.Id);
        Assert.Equal("test1@dolapp.test", addedUser.Email);   // büyük harfler küçültüldü
        Assert.True(addedUser.IsConsentAccepted);
        Assert.Equal(_fakeDateTimeProvider.UtcNow, addedUser.ConsentAt);

        Assert.Equal("sahte-token", registerResponse.AccessToken);
        Assert.Equal("Test", registerResponse.FirstName);
        Assert.False(registerResponse.IsOnboardingAccepted);
    }

    private RegisterHandler CreateRegisterHandler()
    {
        return new RegisterHandler(_fakeAuthService, _fakeUserRepository, _fakeDateTimeProvider);
    }

    private static RegisterRequest CreateValidRegisterRequest()
    {
        return new RegisterRequest
        {
            FirstName = "Test",
            LastName = "Bir",
            Email = "Test1@DolApp.test",
            BirthDate = new DateOnly(2000, 1, 15),
            Password = "Deneme123!",
            ConsentAccepted = true,
        };
    }
}