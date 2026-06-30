package pe.edu.hzg.tesoreria.data;

import android.content.Context;
import android.content.SharedPreferences;

/** Guarda el token JWT y el usuario en SharedPreferences. */
public class SessionManager {
    private static final String PREFS = "tesoreria_session";
    private static final String KEY_TOKEN = "token";
    private static final String KEY_USER = "usuario";

    private final SharedPreferences prefs;

    public SessionManager(Context context) {
        prefs = context.getApplicationContext().getSharedPreferences(PREFS, Context.MODE_PRIVATE);
    }

    public void guardar(String token, String usuario) {
        prefs.edit().putString(KEY_TOKEN, token).putString(KEY_USER, usuario).apply();
    }

    public String getToken() {
        return prefs.getString(KEY_TOKEN, null);
    }

    public String getUsuario() {
        return prefs.getString(KEY_USER, "");
    }

    public boolean estaLogueado() {
        return getToken() != null;
    }

    public void cerrarSesion() {
        prefs.edit().clear().apply();
    }
}
