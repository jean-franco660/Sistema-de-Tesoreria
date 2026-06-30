namespace Horacio.Application.Features.Programas.DTOs;

public class ProgramaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}
