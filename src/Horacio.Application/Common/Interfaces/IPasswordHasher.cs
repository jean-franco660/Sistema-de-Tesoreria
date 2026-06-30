namespace Horacio.Application.Common.Interfaces;

/// <summary>
/// Hashing y verificación de contraseñas.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
