using System.Globalization;
using Horacio.Application.Common.Interfaces;
using Horacio.Application.Features.Comprobantes.DTOs;
using Horacio.Domain.Entities;
using Horacio.Domain.Enums;
using Horacio.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Horacio.Application.Features.Comprobantes.Commands;

/// <summary>
/// Tubería completa de la app móvil: recibe la foto del comprobante y
/// (1) guarda la imagen en el servidor, (2) la registra en log, (3) OCR (Google Vision),
/// (4) IA (DeepSeek) → JSON, (5) validación de negocio, (6) detección de duplicados,
/// (7) registro automático del egreso. Devuelve el egreso creado.
/// </summary>
public record ProcesarComprobanteCommand(byte[] Imagen, string Extension)
    : IRequest<ComprobanteDto>;

public class ProcesarComprobanteCommandHandler : IRequestHandler<ProcesarComprobanteCommand, ComprobanteDto>
{
    private readonly IFileStorageService _storage;
    private readonly IOcrService _ocr;
    private readonly IComprobanteAnalyzer _analyzer;
    private readonly IUnitOfWork _uow;
    private readonly IDateTimeService _dateTime;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;
    private readonly ILogger<ProcesarComprobanteCommandHandler> _logger;

    public ProcesarComprobanteCommandHandler(
        IFileStorageService storage, IOcrService ocr, IComprobanteAnalyzer analyzer,
        IUnitOfWork uow, IDateTimeService dateTime, ICurrentUserService currentUser,
        IAuditService audit, ILogger<ProcesarComprobanteCommandHandler> logger)
    {
        _storage = storage;
        _ocr = ocr;
        _analyzer = analyzer;
        _uow = uow;
        _dateTime = dateTime;
        _currentUser = currentUser;
        _audit = audit;
        _logger = logger;
    }

    public async Task<ComprobanteDto> Handle(ProcesarComprobanteCommand request, CancellationToken ct)
    {
        if (request.Imagen is null || request.Imagen.Length == 0)
            throw new DomainException("Debe enviar la foto del comprobante.");

        var usuarioId = _currentUser.UsuarioId
            ?? throw new DomainException("No se pudo identificar al usuario.");

        // 1) Guardar imagen en el servidor (Droplet) + 2) log.
        var archivo = await _storage.GuardarComprobanteAsync(request.Imagen, request.Extension, ct);
        _logger.LogInformation("Comprobante recibido del usuario {Usuario} → {Ruta}", usuarioId, archivo.Ruta);

        // 3) OCR.
        var base64 = Convert.ToBase64String(request.Imagen);
        var texto = await _ocr.ExtraerTextoAsync(base64, ct);
        if (string.IsNullOrWhiteSpace(texto))
            throw new DomainException("No se pudo leer texto en la imagen. Tome la foto más nítida y bien iluminada.");

        // 4) IA → JSON estructurado.
        var d = await _analyzer.AnalizarTextoAsync(texto, ct);
        d.TextoOcr = texto;

        // 5) Validación de negocio.
        var total = d.Total ?? 0m;
        var observaciones = d.Observaciones;
        if (total <= 0)
            observaciones = Concatenar(observaciones, "⚠ Total no detectado por la IA: revisar manualmente.");

        // 6) Detección de duplicados.
        var ruc = Limpiar(d.Ruc);
        var numero = Limpiar(d.NumeroComprobante);
        var registrados = (await _uow.Repository<Comprobante>()
            .ListAsync(c => c.Estado == EstadoComprobante.Registrado, ct));

        // Duplicado exacto (mismo RUC + número) → se rechaza para no duplicar el egreso.
        if (ruc is not null && numero is not null)
        {
            var exacto = registrados.FirstOrDefault(c =>
                string.Equals(c.Ruc, ruc, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(c.NumeroComprobante, numero, StringComparison.OrdinalIgnoreCase));
            if (exacto is not null)
                throw new DomainException(
                    $"Comprobante duplicado: ya fue registrado (#{exacto.Id}, {exacto.Proveedor}, S/ {exacto.Total:0.00}).");
        }

        // Duplicado probable (mismo proveedor + total + fecha) → se registra pero se marca.
        var fechaEmision = ParsearFecha(d.FechaEmision);
        var esDuplicadoProbable = d.EsDuplicadoProbable;
        var proveedor = Limpiar(d.Proveedor);
        if (!esDuplicadoProbable && proveedor is not null && total > 0)
        {
            var probable = registrados.Any(c =>
                string.Equals(c.Proveedor, proveedor, StringComparison.OrdinalIgnoreCase) &&
                c.Total == total &&
                c.FechaEmision?.Date == fechaEmision?.Date);
            if (probable)
            {
                esDuplicadoProbable = true;
                observaciones = Concatenar(observaciones, "⚠ Posible duplicado (mismo proveedor, total y fecha).");
            }
        }

        // Período activo (opcional).
        var periodoActivo = await _uow.Repository<PeriodoAcademico>()
            .FirstOrDefaultAsync(p => p.Estado == EstadoPeriodo.Abierto, ct);

        // 7) Registro automático del egreso.
        var comprobante = new Comprobante
        {
            Proveedor = proveedor,
            Ruc = ruc,
            TipoDocumento = Limpiar(d.TipoDocumento),
            NumeroComprobante = numero,
            FechaEmision = fechaEmision,
            HoraEmision = Limpiar(d.HoraEmision),
            Moneda = string.IsNullOrWhiteSpace(d.Moneda) ? "PEN" : d.Moneda.Trim().ToUpperInvariant(),
            Subtotal = d.Subtotal,
            Igv = d.Igv,
            Total = total,
            Categoria = NormalizarCategoria(d.Categoria),
            Concepto = Limpiar(d.Concepto),
            MetodoPago = Limpiar(d.MetodoPago),
            Confianza = Math.Clamp(d.Confianza, 0, 100),
            EsDuplicadoProbable = esDuplicadoProbable,
            Observaciones = observaciones,
            ImagenRuta = archivo.Ruta,
            ImagenUrl = archivo.Url,
            RespuestaIaJson = d.RespuestaIaJson,
            FechaRegistro = _dateTime.UtcNow,
            UsuarioId = usuarioId,
            PeriodoAcademicoId = periodoActivo?.Id,
            Estado = EstadoComprobante.Registrado
        };

        foreach (var p in d.Productos ?? new())
        {
            if (string.IsNullOrWhiteSpace(p.Descripcion) && p.Importe is null && p.Cantidad is null) continue;
            comprobante.Productos.Add(new ComprobanteProducto
            {
                Descripcion = Limpiar(p.Descripcion),
                Cantidad = p.Cantidad,
                PrecioUnitario = p.PrecioUnitario,
                Importe = p.Importe
            });
        }

        await _uow.Repository<Comprobante>().AddAsync(comprobante, ct);
        await _uow.SaveChangesAsync(ct);

        await _audit.RegistrarAsync(
            "Registro automático de egreso",
            $"Egreso · {comprobante.Proveedor ?? "Proveedor"} · {comprobante.Categoria} · S/ {comprobante.Total:0.00} · conf. {comprobante.Confianza}%",
            ct);

        var username = (await _uow.Repository<Usuario>().GetByIdAsync(usuarioId, ct))?.Username
            ?? _currentUser.Username ?? string.Empty;

        var dto = RegistrarComprobanteCommandHandler.Mapear(comprobante, username);
        dto.FechaRegistro = _dateTime.ToLocal(comprobante.FechaRegistro);
        return dto;
    }

    private static readonly string[] Categorias =
    {
        "Combustible", "Alimentacion", "Papeleria", "Materiales", "Servicios", "Movilidad",
        "Mantenimiento", "Tecnologia", "Limpieza", "Seguridad", "Salud", "Capacitacion", "Otros"
    };

    private static string NormalizarCategoria(string? categoria)
    {
        if (string.IsNullOrWhiteSpace(categoria)) return "Otros";
        return Categorias.FirstOrDefault(c => string.Equals(c, categoria.Trim(), StringComparison.OrdinalIgnoreCase)) ?? "Otros";
    }

    private static string? Limpiar(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static string Concatenar(string? a, string b) => string.IsNullOrWhiteSpace(a) ? b : $"{a} {b}";

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
}
