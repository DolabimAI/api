using DolApp.Api.Auth;
using DolApp.Api.Middleware;
using DolApp.Application;
using DolApp.Application.Interfaces;
using DolApp.Core.Errors;
using DolApp.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---- Servisler ----
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(apiBehaviorOptions =>
    {
        // Gövde okunamadığında (bozuk JSON, "sayı" alanına metin gibi) ASP.NET kendi biçiminde
        // cevap verir. Bunu da bizim { code, message } biçimimize çeviriyoruz.
        // "_" : bu parametreyi kullanmadığımızı belirtir.
        apiBehaviorOptions.InvalidModelStateResponseFactory = _ =>
            new BadRequestObjectResult(new ErrorResponse(ErrorCodes.InvalidRequest, ErrorMessages.InvalidRequest));
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication(builder.Configuration["MediatR:LicenseKey"]);
builder.Services.AddInfrastructure(builder.Configuration);

// Bu isteği yapan kullanıcı (token'daki "sub")
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// ---- Kimlik doğrulama: Supabase token'ı ----
var supabaseUrl = builder.Configuration["Supabase:Url"]?.TrimEnd('/');
if (string.IsNullOrWhiteSpace(supabaseUrl))
    throw new InvalidOperationException("Supabase:Url boş. appsettings.Development.json dolu mu?");

var issuer = $"{supabaseUrl}/auth/v1";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(jwtBearerOptions =>
    {
        // Token içindeki alan adlarını (ör. "sub" = kullanıcı id'si) olduğu gibi bırak.
        jwtBearerOptions.MapInboundClaims = false;

        // Açık anahtarları Supabase'in JWKS adresinden al; .NET bellekte tutar ve gerektiğinde yeniler.
        jwtBearerOptions.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            $"{issuer}/.well-known/jwks.json",
            new JwksRetriever(issuer),
            new HttpDocumentRetriever { RequireHttps = true });

        jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,              // token'ı bizim Supabase projemiz mi verdi?
            ValidateAudience = true,
            ValidAudience = "authenticated",   // Supabase giriş yapmış kullanıcı token'larına bunu yazar
            ValidateLifetime = true,           // süresi dolmuş mu?
            ClockSkew = TimeSpan.FromSeconds(30),
        };

        jwtBearerOptions.Events = new JwtBearerEvents
        {
            // Token geçerli; ama hesap users tablosunda var ve silinmemiş mi? (silinmiş hesabın isteği reddedilir)
            OnTokenValidated = async tokenValidatedContext =>
            {
                var userIdText = tokenValidatedContext.Principal?.FindFirst("sub")?.Value;
                var userRepository = tokenValidatedContext.HttpContext.RequestServices
                    .GetRequiredService<IUserRepository>();

                var isActiveUser = Guid.TryParse(userIdText, out var userId)
                    && await userRepository.IsActiveAsync(userId, tokenValidatedContext.HttpContext.RequestAborted);

                if (!isActiveUser)
                    tokenValidatedContext.Fail("Hesap bulunamadı ya da silinmiş.");
            },

            // Token yok, geçersiz ya da hesap silinmişse cevabı bizim { code, message } biçimimizde ver.
            OnChallenge = async challengeContext =>
            {
                challengeContext.HandleResponse();
                challengeContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await challengeContext.Response.WriteAsJsonAsync(
                    new ErrorResponse(ErrorCodes.Unauthorized, ErrorMessages.Unauthorized));
            }
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// ---- İstek hattı (her istek bu sırayla geçer) ----
app.UseMiddleware<ExceptionHandlingMiddleware>();   // en başta: sonraki her adımın hatasını yakalar

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();   // token'ı oku ve doğrula: "kimsin?"
app.UseAuthorization();    // uca girme izni var mı?: "girebilir misin?"

// Bütün uçlar varsayılan olarak token ister. Açık olması gerekenlere [AllowAnonymous] yazılır.
app.MapControllers().RequireAuthorization();

app.Run();