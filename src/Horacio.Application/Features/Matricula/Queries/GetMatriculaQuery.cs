using Horacio.Application.Common.Interfaces;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using MediatR;
using DomainConfig = Horacio.Domain.Entities.Configuracion;

namespace Horacio.Application.Features.Matricula.Queries;

public class MatriculaItem
{
    public int N { get; set; }
    public string CodigoMatricula { get; set; } = string.Empty; // DNI
    public string ApellidosNombres { get; set; } = string.Empty;
    public string? Sexo { get; set; }       // "H" / "M"
    public DateTime? FechaNacimiento { get; set; }
    public int? Edad { get; set; }
    public string? Celular { get; set; }
}

/// <summary>Padrón oficial de matrícula (encabezado institucional + lista de estudiantes).</summary>
public class MatriculaDto
{
    public string NombreInstitucion { get; set; } = string.Empty;
    public string CodigoModular { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty;
    public DateTime? PeriodoInicio { get; set; }
    public DateTime? PeriodoFin { get; set; }
    public string Programa { get; set; } = string.Empty;
    public string Turno { get; set; } = string.Empty;
    public string Seccion { get; set; } = string.Empty;

    // Datos fijos del formato oficial (desde Configuración).
    public string DreGre { get; set; } = string.Empty;
    public string Ugel { get; set; } = string.Empty;
    public string ResolucionCreacion { get; set; } = string.Empty;
    public string ResolucionAutorizacion { get; set; } = string.Empty;
    public string PeriodoLectivo { get; set; } = string.Empty;
    public string ModalidadServicio { get; set; } = string.Empty;
    public string NivelFormativo { get; set; } = string.Empty;
    public string TipoPlan { get; set; } = string.Empty;

    // Datos del registro (se ingresan al crearlo).
    public string? Profesor { get; set; }
    public string? ModuloFormativo { get; set; }

    public int Cantidad { get; set; }
    public List<MatriculaItem> Estudiantes { get; set; } = new();
}

/// <summary>Combinación matriculable existente (programa + turno + sección) con su conteo.</summary>
public class MatriculaOpcionDto
{
    public int? PeriodoId { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public int ProgramaId { get; set; }
    public string Programa { get; set; } = string.Empty;
    public int TurnoId { get; set; }
    public string Turno { get; set; } = string.Empty;
    public string Seccion { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public record GetMatriculaQuery(int ProgramaId, int TurnoId, string? Seccion = null, int? PeriodoId = null)
    : IRequest<MatriculaDto>;

public record GetMatriculaOpcionesQuery(int? PeriodoId = null) : IRequest<IReadOnlyList<MatriculaOpcionDto>>;

public class GetMatriculaQueryHandler : IRequestHandler<GetMatriculaQuery, MatriculaDto>
{
    private readonly IUnitOfWork _uow;
    public GetMatriculaQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<MatriculaDto> Handle(GetMatriculaQuery request, CancellationToken ct)
    {
        var config = (await _uow.Repository<DomainConfig>().ListAllAsync(ct)).FirstOrDefault();
        var periodo = request.PeriodoId.HasValue
            ? await _uow.Repository<PeriodoAcademico>().GetByIdAsync(request.PeriodoId.Value, ct)
            : await _uow.Repository<PeriodoAcademico>().FirstOrDefaultAsync(p => p.Estado == EstadoPeriodo.Abierto, ct);

        var programa = await _uow.Repository<Programa>().GetByIdAsync(request.ProgramaId, ct);
        var turno = await _uow.Repository<Turno>().GetByIdAsync(request.TurnoId, ct);
        var seccion = request.Seccion?.Trim().ToUpperInvariant();

        var alumnos = (await _uow.Repository<Alumno>().ListAllAsync(ct))
            .Where(a => a.ProgramaId == request.ProgramaId && a.TurnoId == request.TurnoId
                        && (string.IsNullOrEmpty(seccion) || a.Seccion.ToUpperInvariant() == seccion)
                        && (periodo == null || a.PeriodoAcademicoId == periodo.Id))
            .OrderBy(a => a.Apellidos).ThenBy(a => a.Nombres)
            .ToList();

        return new MatriculaDto
        {
            NombreInstitucion = config?.NombreInstitucion ?? string.Empty,
            CodigoModular = config?.CodigoModular ?? string.Empty,
            Direccion = config?.Direccion ?? string.Empty,
            Ciudad = config?.Ciudad ?? string.Empty,
            Periodo = periodo?.Nombre ?? "—",
            PeriodoInicio = periodo?.FechaInicio,
            PeriodoFin = periodo?.FechaFin,
            Programa = programa?.Nombre ?? string.Empty,
            Turno = turno?.Nombre ?? string.Empty,
            Seccion = string.IsNullOrEmpty(seccion) ? "U" : seccion,
            DreGre = config?.DreGre ?? string.Empty,
            Ugel = config?.Ugel ?? string.Empty,
            ResolucionCreacion = config?.ResolucionCreacion ?? string.Empty,
            ResolucionAutorizacion = config?.ResolucionAutorizacion ?? string.Empty,
            PeriodoLectivo = config?.PeriodoLectivo ?? string.Empty,
            ModalidadServicio = config?.ModalidadServicio ?? string.Empty,
            NivelFormativo = config?.NivelFormativo ?? string.Empty,
            TipoPlan = config?.TipoPlan ?? string.Empty,
            Cantidad = alumnos.Count,
            Estudiantes = alumnos.Select((a, i) => new MatriculaItem
            {
                N = i + 1,
                CodigoMatricula = a.Dni,
                ApellidosNombres = a.NombreCompleto,
                Sexo = a.Sexo,
                FechaNacimiento = a.FechaNacimiento,
                Edad = a.Edad,
                Celular = a.Celular
            }).ToList()
        };
    }
}

public class GetMatriculaOpcionesQueryHandler : IRequestHandler<GetMatriculaOpcionesQuery, IReadOnlyList<MatriculaOpcionDto>>
{
    private readonly IUnitOfWork _uow;
    public GetMatriculaOpcionesQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<MatriculaOpcionDto>> Handle(GetMatriculaOpcionesQuery request, CancellationToken ct)
    {
        var programas = (await _uow.Repository<Programa>().ListAllAsync(ct)).ToDictionary(p => p.Id, p => p.Nombre);
        var turnos = (await _uow.Repository<Turno>().ListAllAsync(ct)).ToDictionary(t => t.Id, t => t.Nombre);
        var periodos = (await _uow.Repository<PeriodoAcademico>().ListAllAsync(ct)).ToDictionary(p => p.Id, p => p.Nombre);
        var alumnos = (await _uow.Repository<Alumno>().ListAllAsync(ct))
            .Where(a => !request.PeriodoId.HasValue || a.PeriodoAcademicoId == request.PeriodoId.Value);

        return alumnos
            .GroupBy(a => new { a.PeriodoAcademicoId, a.ProgramaId, a.TurnoId, a.Seccion })
            .Select(g => new MatriculaOpcionDto
            {
                PeriodoId = g.Key.PeriodoAcademicoId,
                Periodo = g.Key.PeriodoAcademicoId.HasValue ? periodos.GetValueOrDefault(g.Key.PeriodoAcademicoId.Value, "—") : "—",
                ProgramaId = g.Key.ProgramaId,
                Programa = programas.GetValueOrDefault(g.Key.ProgramaId, "—"),
                TurnoId = g.Key.TurnoId,
                Turno = turnos.GetValueOrDefault(g.Key.TurnoId, "—"),
                Seccion = g.Key.Seccion,
                Cantidad = g.Count()
            })
            .OrderBy(o => o.Periodo).ThenBy(o => o.Programa).ThenBy(o => o.Turno).ThenBy(o => o.Seccion)
            .ToList();
    }
}
