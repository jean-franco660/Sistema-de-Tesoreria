using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Alumnos.DTOs;
using Horacio.Domain.Entities;
using MediatR;

namespace Horacio.Application.Features.Alumnos.Queries;

/// <summary>
/// Consulta automática por DNI: primero busca en la BD local; si no existe,
/// consulta automáticamente RENIEC. No requiere ningún botón en el cliente.
/// </summary>
public record BuscarAlumnoPorDniQuery(string Dni) : IRequest<ConsultaDniResult>;

public class BuscarAlumnoPorDniQueryHandler : IRequestHandler<BuscarAlumnoPorDniQuery, ConsultaDniResult>
{
    private readonly IUnitOfWork _uow;
    private readonly IReniecService _reniec;

    public BuscarAlumnoPorDniQueryHandler(IUnitOfWork uow, IReniecService reniec)
    {
        _uow = uow;
        _reniec = reniec;
    }

    public async Task<ConsultaDniResult> Handle(BuscarAlumnoPorDniQuery request, CancellationToken cancellationToken)
    {
        var dni = (request.Dni ?? string.Empty).Trim();
        var resultado = new ConsultaDniResult { Dni = dni };

        if (dni.Length != 8 || !dni.All(char.IsDigit))
            return resultado;

        // 1) Buscar en la base de datos local.
        var alumno = await _uow.Repository<Alumno>()
            .FirstOrDefaultAsync(a => a.Dni == dni, cancellationToken);

        if (alumno is not null)
        {
            var programa = await _uow.Repository<Programa>().GetByIdAsync(alumno.ProgramaId, cancellationToken);
            var turno = await _uow.Repository<Turno>().GetByIdAsync(alumno.TurnoId, cancellationToken);

            resultado.Existe = true;
            resultado.AlumnoId = alumno.Id;
            resultado.Nombres = alumno.Nombres;
            resultado.Apellidos = alumno.Apellidos;
            resultado.NombreCompleto = alumno.NombreCompleto;
            resultado.ProgramaId = alumno.ProgramaId;
            resultado.Programa = programa?.Nombre;
            resultado.TurnoId = alumno.TurnoId;
            resultado.Turno = turno?.Nombre;
            resultado.Sexo = alumno.Sexo;
            resultado.FechaNacimiento = alumno.FechaNacimiento;
            resultado.Edad = alumno.Edad;
            resultado.Celular = alumno.Celular;
            resultado.Seccion = alumno.Seccion;
            return resultado;
        }

        // 2) No existe localmente -> consultar RENIEC automáticamente.
        var persona = await _reniec.ConsultarDniAsync(dni, cancellationToken);
        if (persona is not null)
        {
            resultado.EncontradoReniec = true;
            resultado.Nombres = persona.Nombres;
            resultado.Apellidos = persona.Apellidos;
            resultado.NombreCompleto = $"{persona.Apellidos} {persona.Nombres}".Trim();
            resultado.Sexo = persona.Sexo;                       // null si el proveedor no lo entrega
            resultado.FechaNacimiento = persona.FechaNacimiento; // null en el plan gratuito
            if (persona.FechaNacimiento is { } fn)
            {
                var hoy = DateTime.UtcNow.Date;
                var edad = hoy.Year - fn.Year;
                if (fn.Date > hoy.AddYears(-edad)) edad--;
                resultado.Edad = edad < 0 ? null : edad;
            }
        }

        return resultado;
    }
}
