package pe.edu.hzg.tesoreria.data.model;

/** Respuesta del login (AuthResponse del backend). */
public class LoginResponse {
    public String token;
    public String username;
    public String nombreCompleto;
    public String rol;
    public int expiraEnMinutos;
}
