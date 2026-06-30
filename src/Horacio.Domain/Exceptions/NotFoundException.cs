namespace Horacio.Domain.Exceptions;

/// <summary>
/// Se lanza cuando una entidad solicitada no existe.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entidad, object clave)
        : base($"No se encontró '{entidad}' con clave '{clave}'.") { }

    public NotFoundException(string message) : base(message) { }
}
