using DolApp.Api.Middleware;
using DolApp.Application;
using DolApp.Core.Errors;
using DolApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// ---- Servisler ----
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Gövde okunamadığında (bozuk JSON, "sayı" alanına metin gibi) ASP.NET kendi biçiminde
        // cevap verir. Bunu da bizim { code, message } biçimimize çeviriyoruz.
        options.InvalidModelStateResponseFactory = _ =>
            new BadRequestObjectResult(new ErrorResponse(ErrorCodes.InvalidRequest, ErrorMessages.InvalidRequest));
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication(builder.Configuration["MediatR:LicenseKey"]);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// ---- İstek hattı (her istek bu sırayla geçer) ----
app.UseMiddleware<ExceptionHandlingMiddleware>();   // en başta: sonraki her adımın hatasını yakalar

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();