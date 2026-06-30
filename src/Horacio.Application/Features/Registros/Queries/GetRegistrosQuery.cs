using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Registros.DTOs;
using Horacio.Domain.Entities;
using MediatR;

namespace Horacio.Application.Features.Registros.Queries;

/// <summary>
/// Lista los Registros de Matrícula (aulas) de un período, con su conteo de
/// estudiantes. Incluye los registros VACÍOS creados manualmente.
/// </summary>
public record GetRegistrosQuery(int? PeriodoId = null) : IRequest<IReadOnlyList<RegistroDto>>;

public class GetRegistrosQueryHandler : IRequestHandler<GetRegistrosQuery, IReadOnlyList<RegistroDto>>
{
    private readonly IUnitOfWork _uow;
    public GetRegistrosQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<RegistroDto>> Handle(GetRegistrosQuery request, CancellationToken ct)
    {
        var programas = (await _uow.Repository<Programa>().ListAllAsync(ct)).ToDictionary(p => p.Id, p => p.Nombre);
        var turnos = (await _uow.Repository<Turno>().ListAllAsync(ct)).ToDictionary(t => t.Id, t => t.Nombre);
        var periodos = (await _uow.Repository<PeriodoAcademico>().ListAllAsync(ct)).ToDictionary(p => p.Id, p => p.Nombre);
        var alumnos = await _uow.Repository<Alumno>().ListAllAsync(ct);

        var registros = (await _uow.Repository<RegistroMatricula>().ListAllAsync(ct))
            .Where(r => !request.PeriodoId.HasValue || r.PeriodoAcademicoId == request.PeriodoId.Value);

        return registros
            .Select(r => new RegistroDto
            {
                Id = r.Id,
                PeriodoId = r.PeriodoAcademicoId,
                Periodo = periodos.GetValueOrDefault(r.PeriodoAcademicoId, "—"),
                ProgramaId = r.ProgramaId,
                Programa = programas.GetValueOrDefault(r.ProgramaId, "—"),
                TurnoId = r.TurnoId,
                Turno = turnos.GetValueOrDefault(r.TurnoId, "—"),
                Seccion = r.Seccion,
                FechaCreacion = r.FechaCreacion,
                Cantidad = alumnos.Count(a => a.PeriodoAcademicoId == r.PeriodoAcademicoId
                                              && a.ProgramaId == r.ProgramaId
                                              && a.TurnoId == r.TurnoId
                                              && a.Seccion == r.Seccion)
            })
            .OrderBy(o => o.Periodo).ThenBy(o => o.Programa).ThenBy(o => o.Turno).ThenBy(o => o.Seccion)
            .ToList();
    }
}
