package pe.edu.hzg.tesoreria.data.model;

/** Cuerpo del login: { "username": "...", "password": "..." }. */
public class LoginRequest {
    public String username;
    public String password;

    public LoginRequest(String username, String password) {
        this.username = username;
        this.password = password;
    }
}
