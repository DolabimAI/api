# DolApp API

DolabımAI'nin C# API'si. Mobil uygulama yalnızca bu API ile konuşur; veritabanına yalnızca bu API yazar. Python servisi (`ml` deposu) ve Open-Meteo'ya da bu API bağlanır.

## Klasör yapısı

```
api/
├── db/                         Veritabanı betikleri, şemanın tek kaynağı (bkz. db/README.md)
├── src/
│   ├── DolApp.Api/             Controller'lar, hata biçimi, token doğrulama, Program.cs
│   ├── DolApp.Application/     Use case'ler (Request, Validator, Handler, Response) ve interface'ler
│   ├── DolApp.Core/            Entity'ler (veritabanından üretilir), sabitler, hata kodları
│   └── DolApp.Infrastructure/  AppDbContext, repository'ler, dış servis istemcileri
├── tests/DolApp.Tests/
└── global.json                 Bu depo .NET 10 SDK ile derlenir
```

Bağımlılık yönü: **Api → Infrastructure → Application → Core**. Core hiçbir katmanı tanımaz.

## Gerekenler

- **.NET 10 SDK.** `dotnet --list-sdks` çıktısında `10.0.x` görünmeli. Başka sürümler de kurulu olabilir; `global.json` bu klasörde .NET 10'u seçer.
- **Ayar değerleri.** Supabase bağlantısı ve anahtarları Zehra'dan özel mesajla alınır.
- Yalnızca entity'ler yeniden üretilecekse: `dotnet tool install --global dotnet-ef --version 10.*`

## Kurulum ve çalıştırma

Komutlar deponun kök klasöründe, PowerShell'de çalıştırılır.

1. Depoyu klonla:
   ```
   git clone https://github.com/DolabimAI/api.git
   cd api
   ```
2. Örnek ayar dosyasını kopyala ve Zehra'dan aldığın değerlerle doldur:
   ```
   Copy-Item src/DolApp.Api/appsettings.Development.example.json src/DolApp.Api/appsettings.Development.json
   ```
   `appsettings.Development.json` Git'e girmez (`.gitignore`). **Bu dosyayı asla commit etme.** Bu depoda diğer depolardaki `.env.example`'ın karşılığı `appsettings.Development.example.json`'dır.
3. Derle ve çalıştır:
   ```
   dotnet build
   dotnet run --project src/DolApp.Api
   ```
4. Tarayıcıda Swagger'ı aç: http://localhost:5076/swagger

## Ayarlar

| Ayar | Nerede | Not |
|---|---|---|
| `ConnectionStrings:Db` | `appsettings.Development.json` | Supabase → Connect → .NET → **Session pooler** |
| `Supabase:Url`, `PublishableKey`, `SecretKey` | `appsettings.Development.json` | Gizli; depoya girmez |
| `Admin:Key` | `appsettings.Development.json` | Dışa aktarma ucunun ekip anahtarı |
| `Python`, `OpenMeteo`, `Model`, `Study` | `appsettings.json` | Herkeste aynı olmalı; 7 Aralık'ta dondurulur |
| `MediatR:LicenseKey` | `appsettings.Development.json` | İsteğe bağlı; boşsa yalnızca log uyarısı çıkar |

## Ortak kurallar

- **Hata biçimi:** Bütün hatalar `{ "code": "...", "message": "..." }` olarak döner. Kodlar `src/DolApp.Core/Errors/ErrorCodes.cs`, mesajlar `ErrorMessages.cs` içindedir. Yeni kod, onu kullanan uç yazılırken eklenir.
- **Yetki:** Bütün uçlar varsayılan olarak `Authorization: Bearer <token>` ister. Yalnızca kayıt ve giriş `[AllowAnonymous]`'dır. Token, Supabase'in açık anahtarıyla (ES256, JWKS) doğrulanır. Token süresi 1 haftadır; yenileme ucu yoktur.
- **Veritabanı:** Yalnızca bu API yazar. `Core/Entities` ve `Infrastructure/Data/AppDbContext.cs` elle düzenlenmez; şema değişince yeniden üretilir (bkz. `db/README.md`).
- **Görseller:** Supabase Storage'da `gorseller` bucket'ı (herkese kapalı). Yol: `<kullanıcı-id>/<rastgele-ad>.webp`. Python yazar, C# süreli bağlantı üretir.

## Çalışma düzeni

- **Branch:** `api/<görev-kodu>-<kısa-ad>`, ör. `api/C2-account`
- **Commit:** Görev koduyla başlar, ör. `C2: kayıt ucu eklendi`
- **PR:** `main` korumalıdır; değişiklik PR ile girer.

## Sık karşılaşılan sorunlar

| Belirti | Sebep ve çözüm |
|---|---|
| Açılışta `ConnectionStrings:Db boş` | `appsettings.Development.json` yok ya da boş. Adım 2'yi yap. |
| Scaffold veritabanına bağlanamıyor | Ortam Development değil. Önce `$env:ASPNETCORE_ENVIRONMENT = "Development"` çalıştır. |
| `NU1107` sürüm çakışması | Paketi .NET 10 serisiyle ekle: `--version 10.*` |
| Her istek `401 unauthorized` | Token yok, süresi dolmuş ya da başka bir Supabase projesine ait. |

## Telefondan erişim

Aynı Wi-Fi'daki telefondan bağlanma ayarları (T-43) C2/C4 sırasında bu bölüme eklenecek.


## Telefondan bağlanma (T-43)

Telefondaki uygulama, bilgisayarda çalışan API'ye yerel ağ üzerinden bağlanır. Telefon ve bilgisayar aynı Wi-Fi'da olmalı.

1. Bilgisayarın IP adresi: `ipconfig` → "Wireless LAN adapter Wi-Fi" (Türkçe: "Kablosuz LAN bağdaştırıcısı Wi-Fi") → **IPv4 Address** (örn. `192.168.1.13`). Adres ağ değişince değişir.
2. Ağ profili **Özel** olmalı: `Get-NetConnectionProfile` → `NetworkCategory: Private`.
   Değilse: Ayarlar → Ağ ve İnternet → Wi-Fi → ağın adı → Ağ profili türü → Özel ağ.
3. Güvenlik duvarı izni (bir kez). Başlat'ta `powershell` yaz → sağ tık → **Yönetici olarak çalıştır** (komut satırı `C:\WINDOWS\system32` olmalı):
   `New-NetFirewallRule -DisplayName "DolApp API 5076" -Direction Inbound -Protocol TCP -LocalPort 5076 -Action Allow -Profile Private`
4. API'yi ağa açık çalıştır:
   `dotnet run --project src/DolApp.Api --urls http://0.0.0.0:5076`
   Bu komutla `http://localhost:5076` de çalışmaya devam eder.
5. Telefonun tarayıcısında `http://<IP>:5076/api/lookups` → `{"code":"unauthorized",...}` görünüyorsa bağlantı tamam.

Uygulamadaki API adresi `http://<IP>:5076` olur. IP ağ değiştikçe değiştiği için adres kodda sabit yazılmamalı, ayardan okunmalı.

Kampüs ya da misafir ağları cihazların birbirini görmesini engelleyebilir. Bu durumda telefonun erişim noktası açılır, bilgisayar ona bağlanır ve 1. adımdaki IP yeniden alınır.

Telefon gerekmiyorsa `--urls` olmadan çalıştırılır; API yalnızca `localhost`'ta açık kalır.