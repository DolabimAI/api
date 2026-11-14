using DolApp.Core.Errors;
using FluentValidation;

namespace DolApp.Api.Middleware;

/// Bütün istekleri sarar. İsteğin herhangi bir yerinde hata fırlarsa yakalar ve
/// { code, message } biçiminde cevaba çevirir. Program.cs'te en başa eklenir.

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _nextMiddleware;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate nextMiddleware, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _nextMiddleware = nextMiddleware;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _nextMiddleware(httpContext);   // isteği normal yoluna gönder
        }
        catch (AppException appException)
        {
            // Beklenen hata: kodu ve durumu fırlatan belirledi.
            await WriteErrorAsync(httpContext, appException.StatusCode, appException.Code, appException.Message);
        }
        catch (ValidationException validationException)
        {
            // Validator hatası: ilk hatayı döneriz.
            // Validator'da WithErrorCode(...) yazılmadıysa FluentValidation kendi adını koyar
            // ("NotEmptyValidator" gibi); o durumda genel kodu kullanırız.
            var firstError = validationException.Errors.FirstOrDefault();
            var errorCode = firstError is not null && !firstError.ErrorCode.EndsWith("Validator")
                ? firstError.ErrorCode
                : ErrorCodes.InvalidRequest;
            var errorMessage = firstError?.ErrorMessage ?? ErrorMessages.InvalidRequest;
            await WriteErrorAsync(httpContext, StatusCodes.Status400BadRequest, errorCode, errorMessage);
        }
        catch (Exception exception)
        {
            // Beklenmeyen hata: ayrıntı yalnızca loga yazılır, kullanıcıya genel mesaj gider.
            _logger.LogError(exception, "Beklenmeyen hata: {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
            await WriteErrorAsync(httpContext, StatusCodes.Status500InternalServerError,
                ErrorCodes.InternalError, ErrorMessages.InternalError);
        }
    }

    private static Task WriteErrorAsync(HttpContext httpContext, int statusCode, string errorCode, string errorMessage)
    {
        httpContext.Response.StatusCode = statusCode;
        return httpContext.Response.WriteAsJsonAsync(new ErrorResponse(errorCode, errorMessage));
    }
}