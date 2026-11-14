using DolApp.Core.Errors;
using FluentValidation;

namespace DolApp.Api.Middleware;

/// Bütün istekleri sarar. İsteğin herhangi bir yerinde hata fırlarsa yakalar ve
/// { code, message } biçiminde cevaba çevirir. Program.cs'te en başa eklenir.

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);   // isteği normal yoluna gönder
        }
        catch (AppException ex)
        {
            // Beklenen hata: kodu ve durumu fırlatan belirledi.
            await WriteAsync(context, ex.StatusCode, ex.Code, ex.Message);
        }
        catch (ValidationException ex)
        {
            // Validator hatası: ilk hatayı döneriz.
            // Validator'da WithErrorCode(...) yazılmadıysa FluentValidation kendi adını koyar
            // ("NotEmptyValidator" gibi); o durumda genel kodu kullanırız.
            var first = ex.Errors.FirstOrDefault();
            var code = first is not null && !first.ErrorCode.EndsWith("Validator")
                ? first.ErrorCode
                : ErrorCodes.InvalidRequest;
            var message = first?.ErrorMessage ?? ErrorMessages.InvalidRequest;
            await WriteAsync(context, StatusCodes.Status400BadRequest, code, message);
        }
        catch (Exception ex)
        {
            // Beklenmeyen hata: ayrıntı yalnızca loga yazılır, kullanıcıya genel mesaj gider.
            _logger.LogError(ex, "Beklenmeyen hata: {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteAsync(context, StatusCodes.Status500InternalServerError,
                ErrorCodes.InternalError, ErrorMessages.InternalError);
        }
    }

    private static Task WriteAsync(HttpContext context, int statusCode, string code, string message)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new ErrorResponse(code, message));
    }
}