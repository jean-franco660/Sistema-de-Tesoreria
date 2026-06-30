using System.Globalization;
using FluentValidation;
using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Comprobantes.DTOs;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using Horacio.Domain.Exceptions;
using MediatR;

namespace Horacio.Application.Features.Comprobantes.Commands;

/// <summary>
/// Registra (guarda) un comprobante de egreso a partir de los datos analizados,
/// posiblemente corregidos por el usuario en la app. Se asocia al período activo
/// y al usuario que lo registra.
/// </summary>
public record RegistrarComprobanteCommand(AnalisisComprobanteDto Datos, string? ImagenBase64)
    : IRequest<ComprobanteDto>;

public class RegistrarComprobanteCommandValidator : AbstractValidator<RegistrarComprobanteCommand>
{
    public RegistrarComprobanteCommandValidator()
    {
        RuleFor(x => x.Datos).NotNull();
        RuleFor(x => x.Datos.Total)
            .NotNull().WithMessage("El total del comprobante es obligatorio.")
            .GreaterThan(0).WithMessage("El total debe ser mayor a 0.");
    }
}

public class RegistrarComprobanteCommandHandler : IRequestHandler<RegistrarComprobanteCommand, ComprobanteDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeService _dateTime;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;

    public RegistrarComprobanteCommandHandler(
        IUnitOfWork uow, IDateTimeService dateTime, ICurrentUserService currentUser, IAuditService audit)
    {
        _uow = uow;
        _dateTime = dateTime;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<ComprobanteDto> Handle(RegistrarComprobanteCommand request, CancellationToken cancellationToken)
    {
        var d = request.Datos;

        var usuarioId = _currentUser.UsuarioId
            ?? throw new DomainException("No se pudo identificar al usuario.");

        // Período activo (si existe). El egreso no se bloquea si no hay período abierto.
        var periodoActivo = await _uow.Repository<PeriodoAcademico>()
            .FirstOrDefaultAsync(p => p.Estado == EstadoPeriodo.Abierto, cancellationToken);

        var comprobante = new Comprobante
        {
            Proveedor = Limpiar(d.Proveedor),
            Ruc = Limpiar(d.Ruc),
            TipoDocumento = Limpiar(d.TipoDocumento),
            NumeroComprobante = Limpiar(d.NumeroComprobante),
            FechaEmision = ParsearFecha(d.FechaEmision),
            HoraEmision = Limpiar(d.HoraEmision),
            Moneda = string.IsNullOrWhiteSpace(d.Moneda) ? "PEN" : d.Moneda.Trim().ToUpperInvariant(),
            Subtotal = d.Subtotal,
            Igv = d.Igv,
            Total = d.Total ?? 0m,
            Categoria = NormalizarCategoria(d.Categoria),
            Concepto = Limpiar(d.Concepto),
            MetodoPago = Limpiar(d.MetodoPago),
            Confianza = Math.Clamp(d.Confianza, 0, 100),
            EsDuplicadoProbable = d.EsDuplicadoProbable,
            Observaciones = Limpiar(d.Observaciones),
            ImagenBase64 = request.ImagenBase64,
            RespuestaIaJson = d.RespuestaIaJson,
            FechaRegistro = _dateTime.UtcNow,
            UsuarioId = usuarioId,
            PeriodoAcademicoId = periodoActivo?.Id,
            Estado = EstadoComprobante.Registrado
        };

        foreach (var p in d.Productos ?? new())
        {
            if (string.IsNullOrWhiteSpace(p.Descripcion) && p.Importe is null && p.Cantidad is null)
                continue;
            comprobante.Productos.Add(new ComprobanteProducto
            {
                Descripcion = Limpiar(p.Descripcion),
                Cantidad = p.Cantidad,
                PrecioUnitario = p.PrecioUnitario,
                Importe = p.Importe
            });
        }

        await _uow.Repository<Comprobante>().AddAsync(comprobante, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await _audit.RegistrarAsync(
            "Registro de egreso",
            $"Egreso · {comprobante.Proveedor ?? "Proveedor"} · {comprobante.Categoria ?? "Otros"} · S/ {comprobante.Total:0.00}",
            cancellationToken);

        var username = (await _uow.Repository<Usuario>().GetByIdAsync(usuarioId, cancellationToken))?.Username
            ?? _currentUser.Username ?? string.Empty;

        return Mapear(comprobante, username);
    }

    /// <summary>Categorías permitidas del prompt; cae a "Otros" si no coincide.</summary>
    private static readonly string[] Categorias =
    {
        "Combustible", "Alimentacion", "Papeleria", "Materiales", "Servicios", "Movilidad",
        "Mantenimiento", "Tecnologia", "Limpieza", "Seguridad", "Salud", "Capacitacion", "Otros"
    };

    private static string NormalizarCategoria(string? categoria)
    {
        if (string.IsNullOrWhiteSpace(categoria)) return "Otros";
        var match = Categorias.FirstOrDefault(c =>
            string.Equals(c, categoria.Trim(), StringComparison.OrdinalIgnoreCase));
        return match ?? "Otros";
    }

    private static string? Limpiar(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static DateTime? ParsearFecha(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        string[] formatos = { "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "yyyy/MM/dd" };
        if (DateTime.TryParseExact(raw.Trim(), formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out var f))
            return DateTime.SpecifyKind(f.Date, DateTimeKind.Utc);
        if (DateTime.TryParse(raw.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var g))
            return DateTime.SpecifyKind(g.Date, DateTimeKind.Utc);
        return null;
    }

    internal static ComprobanteDto Mapear(Comprobante c, string usuario) => new()
    {
        Id = c.Id,
        Proveedor = c.Proveedor,
        Ruc = c.Ruc,
        TipoDocumento = c.TipoDocumento,
        NumeroComprobante = c.NumeroComprobante,
        FechaEmision = c.FechaEmision,
        HoraEmision = c.HoraEmision,
        Moneda = c.Moneda,
        Subtotal = c.Subtotal,
        Igv = c.Igv,
        Total = c.Total,
        Categoria = c.Categoria,
        Concepto = c.Concepto,
        MetodoPago = c.MetodoPago,
        Confianza = c.Confianza,
        EsDuplicadoProbable = c.EsDuplicadoProbable,
        Observaciones = c.Observaciones,
        ImagenRuta = c.ImagenRuta,
        ImagenUrl = c.ImagenUrl,
        ImagenBase64 = c.ImagenBase64,
        FechaRegistro = c.FechaRegistro,
        Usuario = usuario,
        Estado = c.Estado.ToString(),
        Productos = c.Productos.Select(p => new ComprobanteProductoDto
        {
            Descripcion = p.Descripcion,
            Cantidad = p.Cantidad,
            PrecioUnitario = p.PrecioUnitario,
            Importe = p.Importe
        }).ToList()
    };
}
