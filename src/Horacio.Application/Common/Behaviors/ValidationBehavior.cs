using FluentValidation;
using MediatR;

namespace Horacio.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior de MediatR que ejecuta los validadores de FluentValidation
/// antes de llegar al handler. Lanza ValidationException con los errores.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var resultados = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var fallos = resultados
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (fallos.Count != 0)
                throw new ValidationException(fallos);
        }

        return await next();
    }
}
