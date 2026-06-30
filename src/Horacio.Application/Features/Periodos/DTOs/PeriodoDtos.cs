namespace Horacio.Application.Features.Periodos.DTOs;

/// <summary>Período académico con totales calculados (para listado/histórico).</summary>
public class PeriodoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? UsuarioApertura { get; set; }
    public DateTime FechaApertura { get; set; }
    public string? UsuarioCierre { get; set; }
    public DateTime? FechaCierre { get; set; }
    public string? Observaciones { get; set; }
    public int Tickets { get; set; }
    public decimal Ingresos { get; set; }
    public int? DiasRestantes { get; set; }
}

/// <summary>Resumen del período (para el cierre y el acta).</summary>
public class PeriodoResumenDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int Tickets { get; set; }
    public int Estudiantes { get; set; }
    public decimal Ingresos { get; set; }
    public int Servicios { get; set; }
    public string? UsuarioApertura { get; set; }
    public string? UsuarioCierre { get; set; }
    public DateTime? FechaCierre { get; set; }
}

/// <summary>Sección de gestión financiera por período del dashboard.</summary>
public class DashboardPeriodoDto
{
    public bool HayPeriodoActivo { get; set; }
    public string? Nombre { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public decimal IngresosAcumulados { get; set; }
    public int Tickets { get; set; }
    public int Estudiantes { get; set; }
    public int Servicios { get; set; }
    public int DiasRestantes { get; set; }
}
