using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Horacio.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior que registra cada request, su duración y posibles fallos.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var nombre = typeof(TRequest).Name;
        var cronometro = Stopwatch.StartNew();

        _logger.LogInformation("Ejecutando {Request}", nombre);
        try
        {
            var respuesta = await next();
            cronometro.Stop();
            _logger.LogInformation("Completado {Request} en {Elapsed} ms", nombre, cronometro.ElapsedMilliseconds);
            return respuesta;
        }
        catch (Exception ex)
        {
            cronometro.Stop();
            _logger.LogError(ex, "Error en {Request} tras {Elapsed} ms", nombre, cronometro.ElapsedMilliseconds);
            throw;
        }
    }
}
