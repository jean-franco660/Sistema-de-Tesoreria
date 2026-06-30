using Horacio.Application.Common.Interfaces;
using Horacio.Domain.Entities;
using MediatR;

namespace Horacio.Application.Features.Usuarios.Queries;

public class UsuarioDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}

/// <summary>Lista los usuarios del sistema.</summary>
public record GetUsuariosQuery : IRequest<IReadOnlyList<UsuarioDto>>;

public class GetUsuariosQueryHandler : IRequestHandler<GetUsuariosQuery, IReadOnlyList<UsuarioDto>>
{
    private readonly IUnitOfWork _uow;

    public GetUsuariosQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<UsuarioDto>> Handle(GetUsuariosQuery request, CancellationToken cancellationToken)
    {
        var usuarios = await _uow.Repository<Usuario>().ListAllAsync(cancellationToken);
        return usuarios
            .OrderBy(u => u.Username)
            .Select(u => new UsuarioDto
            {
                Id = u.Id,
                Username = u.Username,
                NombreCompleto = u.NombreCompleto,
                Rol = u.Rol.ToString(),
                Estado = u.Estado.ToString(),
                FechaCreacion = u.FechaCreacion
            })
            .ToList();
    }
}
