namespace Horacio.Application.Features.Registros.DTOs;

/// <summary>
/// Resumen de un Registro de Matrícula (aula) para el listado: identifica la
/// combinación período/programa/turno/sección y cuántos estudiantes contiene.
/// </summary>
public class RegistroDto
{
    public int Id { get; set; }
    public int PeriodoId { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public int ProgramaId { get; set; }
    public string Programa { get; set; } = string.Empty;
    public int TurnoId { get; set; }
    public string Turno { get; set; } = string.Empty;
    public string Seccion { get; set; } = "U";
    public int Cantidad { get; set; }
    public DateTime FechaCreacion { get; set; }
}
