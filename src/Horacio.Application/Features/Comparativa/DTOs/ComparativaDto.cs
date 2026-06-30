namespace Horacio.Application.Features.Comparativa.DTOs;

/// <summary>
/// Comparativa de INGRESOS (tickets de tesorería) vs EGRESOS (comprobantes IA)
/// para un rango de fechas. Alimenta la sección "Comparativa de Ingresos y Egresos".
/// </summary>
public class ComparativaDto
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }

    public decimal TotalIngresos { get; set; }
    public decimal TotalEgresos { get; set; }
    /// <summary>Ingresos − Egresos.</summary>
    public decimal Balance { get; set; }

    public int CantidadIngresos { get; set; }
    public int CantidadEgresos { get; set; }

    /// <summary>Serie diaria combinada para el gráfico.</summary>
    public List<ComparativaDiaDto> Serie { get; set; } = new();

    /// <summary>Egresos agrupados por categoría.</summary>
    public List<ComparativaGrupoDto> EgresosPorCategoria { get; set; } = new();

    /// <summary>Ingresos agrupados por servicio/concepto.</summary>
    public List<ComparativaGrupoDto> IngresosPorConcepto { get; set; } = new();

    /// <summary>Últimos movimientos (ingresos y egresos) ordenados por fecha desc.</summary>
    public List<MovimientoDto> Movimientos { get; set; } = new();
}

public class ComparativaDiaDto
{
    public string Fecha { get; set; } = string.Empty;   // YYYY-MM-DD
    public decimal Ingresos { get; set; }
    public decimal Egresos { get; set; }
}

public class ComparativaGrupoDto
{
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Monto { get; set; }
}

/// <summary>Un movimiento unificado: ingreso o egreso.</summary>
public class MovimientoDto
{
    /// <summary>"Ingreso" | "Egreso".</summary>
    public string Tipo { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    /// <summary>N° de ticket (ingreso) o proveedor (egreso).</summary>
    public string Descripcion { get; set; } = string.Empty;
    /// <summary>Concepto/servicio (ingreso) o categoría (egreso).</summary>
    public string Detalle { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Usuario { get; set; } = string.Empty;
}
