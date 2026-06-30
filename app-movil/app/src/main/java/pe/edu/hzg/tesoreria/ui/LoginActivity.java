package pe.edu.hzg.tesoreria.ui;

import android.content.Intent;
import android.os.Bundle;
import android.text.SpannableString;
import android.text.Spanned;
import android.text.style.ForegroundColorSpan;
import android.view.View;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.content.ContextCompat;

import pe.edu.hzg.tesoreria.R;
import pe.edu.hzg.tesoreria.data.ApiClient;
import pe.edu.hzg.tesoreria.data.SessionManager;
import pe.edu.hzg.tesoreria.data.model.LoginRequest;
import pe.edu.hzg.tesoreria.data.model.LoginResponse;
import pe.edu.hzg.tesoreria.databinding.ActivityLoginBinding;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class LoginActivity extends AppCompatActivity {

    private ActivityLoginBinding b;
    private SessionManager session;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        session = new SessionManager(this);

        if (session.estaLogueado()) {
            irAMain();
            return;
        }

        b = ActivityLoginBinding.inflate(getLayoutInflater());
        setContentView(b.getRoot());

        pintarTitular();

        b.btnIngresar.setOnClickListener(v -> intentarLogin());
        b.btnOlvidaste.setOnClickListener(v -> proximamente());
        b.btnGoogle.setOnClickListener(v -> proximamente());
        b.btnMicrosoft.setOnClickListener(v -> proximamente());
    }

    /** Resalta la palabra "egresos" en color violeta dentro del titular. */
    private void pintarTitular() {
        String t = getString(R.string.hero_titulo);
        SpannableString s = new SpannableString(t);
        int i = t.toLowerCase().lastIndexOf("egresos");
        if (i >= 0) {
            s.setSpan(new ForegroundColorSpan(ContextCompat.getColor(this, R.color.violet)),
                    i, i + "egresos".length(), Spanned.SPAN_EXCLUSIVE_EXCLUSIVE);
        }
        b.txtHeroTitulo.setText(s);
    }

    private void proximamente() {
        Toast.makeText(this, getString(R.string.proximamente), Toast.LENGTH_SHORT).show();
    }

    private void intentarLogin() {
        String usuario = b.inputUsuario.getText() != null ? b.inputUsuario.getText().toString().trim() : "";
        String clave = b.inputClave.getText() != null ? b.inputClave.getText().toString().trim() : "";
        if (usuario.isEmpty() || clave.isEmpty()) {
            Toast.makeText(this, "Ingrese usuario y contraseña", Toast.LENGTH_SHORT).show();
            return;
        }

        cargando(true);
        ApiClient.getService(this).login(new LoginRequest(usuario, clave))
                .enqueue(new Callback<LoginResponse>() {
                    @Override
                    public void onResponse(@NonNull Call<LoginResponse> call, @NonNull Response<LoginResponse> resp) {
                        cargando(false);
                        if (resp.isSuccessful() && resp.body() != null && resp.body().token != null) {
                            LoginResponse r = resp.body();
                            session.guardar(r.token, r.nombreCompleto != null ? r.nombreCompleto : r.username);
                            irAMain();
                        } else {
                            Toast.makeText(LoginActivity.this, "Usuario o contraseña incorrectos", Toast.LENGTH_LONG).show();
                        }
                    }

                    @Override
                    public void onFailure(@NonNull Call<LoginResponse> call, @NonNull Throwable t) {
                        cargando(false);
                        Toast.makeText(LoginActivity.this, "No se pudo conectar: " + t.getMessage(), Toast.LENGTH_LONG).show();
                    }
                });
    }

    private void cargando(boolean v) {
        b.progress.setVisibility(v ? View.VISIBLE : View.GONE);
        b.btnIngresar.setEnabled(!v);
        b.btnIngresar.setAlpha(v ? 0.6f : 1f);
    }

    private void irAMain() {
        startActivity(new Intent(this, MainActivity.class));
        finish();
    }
}
