using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Alumnos.DTOs;
using Horacio.Domain.Entities;
using MediatR;

namespace Horacio.Application.Features.Alumnos.Queries;

/// <summary>Lista alumnos, opcionalmente filtrados por DNI o nombre/apellido.</summary>
public record GetAlumnosQuery(string? Buscar = null) : IRequest<IReadOnlyList<AlumnoDto>>;

public class GetAlumnosQueryHandler : IRequestHandler<GetAlumnosQuery, IReadOnlyList<AlumnoDto>>
{
    private readonly IUnitOfWork _uow;

    public GetAlumnosQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<AlumnoDto>> Handle(GetAlumnosQuery request, CancellationToken cancellationToken)
    {
        var alumnos = await _uow.Repository<Alumno>().ListAllAsync(cancellationToken);
        var programas = (await _uow.Repository<Programa>().ListAllAsync(cancellationToken))
            .ToDictionary(p => p.Id, p => p.Nombre);
        var turnos = (await _uow.Repository<Turno>().ListAllAsync(cancellationToken))
            .ToDictionary(t => t.Id, t => t.Nombre);

        var filtro = request.Buscar?.Trim().ToLowerInvariant();

        return alumnos
            .Where(a => string.IsNullOrEmpty(filtro)
                        || a.Dni.Contains(filtro)
                        || a.Nombres.ToLowerInvariant().Contains(filtro)
                        || a.Apellidos.ToLowerInvariant().Contains(filtro))
            .OrderBy(a => a.Apellidos)
            .Select(a => new AlumnoDto
            {
                Id = a.Id,
                Dni = a.Dni,
                Nombres = a.Nombres,
                Apellidos = a.Apellidos,
                NombreCompleto = a.NombreCompleto,
                ProgramaId = a.ProgramaId,
                Programa = programas.GetValueOrDefault(a.ProgramaId, string.Empty),
                TurnoId = a.TurnoId,
                Turno = turnos.GetValueOrDefault(a.TurnoId, string.Empty),
                Seccion = a.Seccion,
                Sexo = a.Sexo,
                FechaNacimiento = a.FechaNacimiento,
                Edad = a.Edad,
                Celular = a.Celular,
                Estado = a.Estado.ToString(),
                FechaRegistro = a.FechaRegistro
            })
            .ToList();
    }
}
