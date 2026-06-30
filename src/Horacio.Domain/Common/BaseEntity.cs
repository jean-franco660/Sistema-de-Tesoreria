namespace Horacio.Domain.Common;

/// <summary>
/// Clase base para todas las entidades del dominio. Expone la clave primaria.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
}
