namespace Horacio.Domain.Exceptions;

/// <summary>
/// Excepción base para violaciones de reglas de negocio del dominio.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
