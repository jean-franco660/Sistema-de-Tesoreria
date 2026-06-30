using System.Net;
using System.Text.Json;
using FluentValidation;
using Horacio.Domain.Exceptions;

namespace Horacio.API.Middleware;

/// <summary>
/// Manejo global de excepciones: traduce excepciones de dominio/validación a
/// respuestas HTTP coherentes en JSON.
/// </summary>
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
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        HttpStatusCode status;
        object payload;

        switch (ex)
        {
            case ValidationException vex:
                status = HttpStatusCode.BadRequest;
                payload = new
                {
                    mensaje = "Errores de validación.",
                    errores = vex.Errors.Select(e => new { campo = e.PropertyName, error = e.ErrorMessage })
                };
                break;

            case NotFoundException nfe:
                status = HttpStatusCode.NotFound;
                payload = new { mensaje = nfe.Message };
                break;

            case DomainException dex:
                status = HttpStatusCode.BadRequest;
                payload = new { mensaje = dex.Message };
                break;

            default:
                _logger.LogError(ex, "Error no controlado");
                status = HttpStatusCode.InternalServerError;
                payload = new { mensaje = "Ocurrió un error interno en el servidor." };
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
