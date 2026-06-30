using FluentValidation;
using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Tickets.DTOs;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using Horacio.Domain.Exceptions;
using MediatR;

namespace Horacio.Application.Features.Tickets.Commands;

/// <summary>
/// Emite un comprobante interno (ticket) para un alumno, con uno o más servicios.
/// Genera numeración automática (NumeroTicket "001/001" y Contador "000000001"),
/// calcula el total y el importe en letras.
/// </summary>
public record EmitirTicketCommand(int AlumnoId, List<EmitirTicketDetalleInput> Detalles) : IRequest<TicketDto>;

public class EmitirTicketCommandValidator : AbstractValidator<EmitirTicketCommand>
{
    public EmitirTicketCommandValidator()
    {
        RuleFor(x => x.AlumnoId).GreaterThan(0);
        RuleFor(x => x.Detalles).NotEmpty().WithMessage("El ticket debe tener al menos un servicio.");
        RuleForEach(x => x.Detalles).ChildRules(d =>
        {
            d.RuleFor(i => i.ServicioId).GreaterThan(0);
            d.RuleFor(i => i.Importe).GreaterThan(0).WithMessage("El importe debe ser mayor a 0.");
        });
    }
}

public class EmitirTicketCommandHandler : IRequestHandler<EmitirTicketCommand, TicketDto>
{
    private readonly IUnitOfWork _uow;
    private readonly INumberToWordsService _numberToWords;
    private readonly IDateTimeService _dateTime;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;

    public EmitirTicketCommandHandler(
        IUnitOfWork uow,
        INumberToWordsService numberToWords,
        IDateTimeService dateTime,
        ICurrentUserService currentUser,
        IAuditService audit)
    {
        _uow = uow;
        _numberToWords = numberToWords;
        _dateTime = dateTime;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<TicketDto> Handle(EmitirTicketCommand request, CancellationToken cancellationToken)
    {
        // Debe existir un período académico ABIERTO para poder emitir.
        var periodoActivo = await _uow.Repository<PeriodoAcademico>()
            .FirstOrDefaultAsync(p => p.Estado == EstadoPeriodo.Abierto, cancellationToken)
            ?? throw new DomainException("No existe un período académico activo. Comuníquese con el administrador.");

        var alumno = await _uow.Repository<Alumno>()
            .FirstOrDefaultAsync(a => a.Id == request.AlumnoId, cancellationToken)
            ?? throw new NotFoundException("Alumno", request.AlumnoId);

        var serviciosDict = (await _uow.Repository<Servicio>().ListAllAsync(cancellationToken))
            .ToDictionary(s => s.Id);

        foreach (var d in request.Detalles)
        {
            if (!serviciosDict.TryGetValue(d.ServicioId, out var servicio))
                throw new NotFoundException("Servicio", d.ServicioId);
            if (servicio.Estado != EstadoRegistro.Activo)
                throw new DomainException($"El servicio '{servicio.Nombre}' no está activo.");
        }

        var usuarioId = _currentUser.UsuarioId
            ?? throw new DomainException("No se pudo identificar al usuario emisor.");

        var programa = await _uow.Repository<Programa>().GetByIdAsync(alumno.ProgramaId, cancellationToken);
        var turno = await _uow.Repository<Turno>().GetByIdAsync(alumno.TurnoId, cancellationToken);

        // Toda la numeración + inserción ocurre dentro de una transacción.
        var resultado = await _uow.ExecuteInTransactionAsync(async token =>
        {
            var (numeroTicket, contador) = await GenerarNumeracionAsync(token);

            var ticket = new Ticket
            {
                NumeroTicket = numeroTicket,
                Contador = contador,
                FechaEmision = _dateTime.UtcNow,
                AlumnoId = alumno.Id,
                UsuarioId = usuarioId,
                PeriodoAcademicoId = periodoActivo.Id,
                Estado = EstadoTicket.Emitido
            };

            foreach (var d in request.Detalles)
                ticket.Detalles.Add(new DetalleTicket { ServicioId = d.ServicioId, Importe = d.Importe });

            ticket.RecalcularTotal();

            await _uow.Repository<Ticket>().AddAsync(ticket, token);
            await _uow.SaveChangesAsync(token);

            return new TicketDto
            {
                Id = ticket.Id,
                NumeroTicket = ticket.NumeroTicket,
                Contador = ticket.Contador,
                FechaEmision = _dateTime.ToLocal(ticket.FechaEmision),
                Dni = alumno.Dni,
                AlumnoNombre = alumno.NombreCompleto,
                Programa = programa?.Nombre ?? string.Empty,
                Turno = turno?.Nombre ?? string.Empty,
                Total = ticket.Total,
                TotalEnLetras = _numberToWords.ConvertirImporte(ticket.Total),
                Usuario = _currentUser.Username ?? "TESORERIA",
                Estado = ticket.Estado.ToString(),
                Detalles = ticket.Detalles
                    .Select(d => new TicketDetalleDto
                    {
                        ServicioId = d.ServicioId,
                        Servicio = serviciosDict[d.ServicioId].Nombre,
                        Importe = d.Importe
                    })
                    .ToList()
            };
        }, cancellationToken);

        // Auditoría (fuera de la transacción del ticket)
        await _audit.RegistrarAsync(
            "Emisión de ticket",
            $"Ticket {resultado.NumeroTicket} · {resultado.AlumnoNombre} · Total S/ {resultado.Total:0.00}",
            cancellationToken);

        return resultado;
    }

    /// <summary>
    /// Incrementa los contadores persistentes y devuelve (NumeroTicket, Contador).
    /// NumeroTicket: "{serie}/{correlativo:000}" con rotación de serie al pasar 999.
    /// Contador: correlativo global de 9 dígitos.
    /// </summary>
    private async Task<(string numeroTicket, string contador)> GenerarNumeracionAsync(CancellationToken ct)
    {
        var repo = _uow.Repository<Contador>();

        var general = await repo.FirstOrDefaultAsync(c => c.Nombre == "GENERAL", ct)
            ?? throw new DomainException("Contador 'GENERAL' no inicializado.");
        general.UltimoValor += 1;
        repo.Update(general);

        // Un único correlativo: el N° de ticket y el contador comparten el mismo número.
        long numero = general.UltimoValor;
        var contadorStr = numero.ToString("D9");     // ej. 000000125

        var ticketCont = await repo.FirstOrDefaultAsync(c => c.Nombre == "TICKET", ct);
        var serie = ticketCont?.Serie ?? "001";
        var numeroTicket = $"{serie}/{numero:D5}";   // ej. 001/00125

        return (numeroTicket, contadorStr);
    }
}
