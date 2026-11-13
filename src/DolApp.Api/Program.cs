using DolApp.Application;
using DolApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ---- Servisler ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication(builder.Configuration["MediatR:LicenseKey"]);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// ---- İstek hattı (her istek bu sırayla geçer) ----
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();