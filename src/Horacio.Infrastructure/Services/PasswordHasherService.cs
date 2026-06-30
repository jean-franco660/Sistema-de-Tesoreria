using Horacio.Application.Common.Interfaces;

namespace Horacio.Infrastructure.Services;

/// <summary>Hashing de contraseñas con BCrypt.</summary>
public class PasswordHasherService : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
