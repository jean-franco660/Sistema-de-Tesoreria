namespace Horacio.Application.Features.Servicios.DTOs;

public class ServicioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Estado { get; set; } = string.Empty;
}
