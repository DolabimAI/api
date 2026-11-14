using FluentValidation;
using MediatR;

namespace DolApp.Application.Behaviors;

/// Bütün MediatR istekleri handler'a gitmeden önce buradan geçer.
/// İsteğin validator'u varsa çalıştırılır; hata varsa handler hiç çağrılmaz ve
/// ValidationException fırlatılır (ExceptionHandlingMiddleware bunu 400 { code, message }'a çevirir).
/// Validator'u olmayan istekler doğrudan handler'a geçer.

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> nextHandler, CancellationToken cancellationToken)
    {
        foreach (var validator in _validators)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
        }

        return await nextHandler();
    }
}
