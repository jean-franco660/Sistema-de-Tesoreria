namespace Horacio.Application.Features.Alumnos.DTOs;

public class AlumnoDto
{
    public int Id { get; set; }
    public string Dni { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public int ProgramaId { get; set; }
    public string Programa { get; set; } = string.Empty;
    public int TurnoId { get; set; }
    public string Turno { get; set; } = string.Empty;
    public string Seccion { get; set; } = string.Empty;
    public string? Sexo { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public int? Edad { get; set; }
    public string? Celular { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
}
