namespace Horacio.Application.Features.Auth.DTOs;

/// <summary>Respuesta de autenticación con el token JWT y datos del usuario.</summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public int ExpiraEnMinutos { get; set; }
}
