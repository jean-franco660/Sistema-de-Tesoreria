using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Horacio.API.Controllers;

/// <summary>Controlador base con acceso a MediatR.</summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}
