namespace Horacio.Application.Features.Reportes.DTOs;

/// <summary>Una línea del informe de ingresos (un servicio cobrado en un ticket).</summary>
public class ReporteIngresoItem
{
    public DateTime Fecha { get; set; }
    public string NumeroTicket { get; set; } = string.Empty;
    public string Contador { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string Alumno { get; set; } = string.Empty;
    public string Programa { get; set; } = string.Empty;
    public string Servicio { get; set; } = string.Empty;   // "por qué pagó"
    public decimal Importe { get; set; }
    public string Usuario { get; set; } = string.Empty;
}

/// <summary>Informe completo de ingresos por recursos propios (contabilidad).</summary>
public class ReporteIngresosDto
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public int CantidadTickets { get; set; }
    public int CantidadServicios { get; set; }
    public decimal Total { get; set; }
    public List<ReporteIngresoItem> Items { get; set; } = new();

    /// <summary>Subtotales por servicio (para la sección resumen del informe).</summary>
    public List<ReporteResumen> ResumenPorServicio { get; set; } = new();
    /// <summary>Subtotales por programa.</summary>
    public List<ReporteResumen> ResumenPorPrograma { get; set; } = new();
}

public class ReporteResumen
{
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Monto { get; set; }
}
