using Horacio.Domain.Common;
using Horacio.Domain.Enums;

namespace Horacio.Domain.Entities;

/// <summary>
/// Usuario del sistema (autenticación JWT). El username es único.
/// </summary>
public class Usuario : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; } = RolUsuario.Finanzas;
    public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Relaciones
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
