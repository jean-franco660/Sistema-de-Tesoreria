package pe.edu.hzg.tesoreria.ui;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;

import com.google.gson.Gson;

import java.util.List;
import java.util.Locale;

import pe.edu.hzg.tesoreria.R;
import pe.edu.hzg.tesoreria.data.ApiClient;
import pe.edu.hzg.tesoreria.data.SessionManager;
import pe.edu.hzg.tesoreria.data.model.ComprobanteResponse;
import pe.edu.hzg.tesoreria.databinding.ActivityMainBinding;
import pe.edu.hzg.tesoreria.databinding.ItemEgresoBinding;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class MainActivity extends AppCompatActivity {

    private ActivityMainBinding b;
    private SessionManager session;
    private final Gson gson = new Gson();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        session = new SessionManager(this);
        b = ActivityMainBinding.inflate(getLayoutInflater());
        setContentView(b.getRoot());

        b.txtHola.setText("Hola, " + primerNombre(session.getUsuario()));

        View.OnClickListener escanear = v -> startActivity(new Intent(this, CameraActivity.class));
        b.qaEscanear.setOnClickListener(escanear);
        b.fabScan.setOnClickListener(escanear);

        b.navInicio.setOnClickListener(v -> cargar());
        b.navPerfil.setOnClickListener(v -> confirmarSalir());
        View.OnClickListener prox = v -> Toast.makeText(this, R.string.proximamente, Toast.LENGTH_SHORT).show();
        b.qaHistorial.setOnClickListener(prox);
        b.qaCategorias.setOnClickListener(prox);
        b.qaReportes.setOnClickListener(prox);
        b.navHistorial.setOnClickListener(prox);
        b.navReportes.setOnClickListener(prox);
        b.txtVerTodo.setOnClickListener(prox);
        b.btnBell.setOnClickListener(prox);
        b.btnMenu.setOnClickListener(prox);
    }

    @Override
    protected void onResume() {
        super.onResume();
        cargar();
    }

    private void cargar() {
        ApiClient.getService(this).listarComprobantes().enqueue(new Callback<List<ComprobanteResponse>>() {
            @Override
            public void onResponse(@NonNull Call<List<ComprobanteResponse>> call, @NonNull Response<List<ComprobanteResponse>> resp) {
                if (resp.isSuccessful() && resp.body() != null) {
                    pintar(resp.body());
                }
            }

            @Override
            public void onFailure(@NonNull Call<List<ComprobanteResponse>> call, @NonNull Throwable t) {
                // Silencioso: la tarjeta queda en su estado por defecto.
            }
        });
    }

    private void pintar(List<ComprobanteResponse> lista) {
        b.containerReciente.removeAllViews();

        if (lista.isEmpty()) {
            b.txtTotalMonto.setText("S/ 0.00");
            b.txtTotalProveedor.setText("Sin egresos aún");
            b.txtTotalCategoria.setText("—");
            b.txtTotalConfianza.setText("Escanea tu primer comprobante");
            b.txtRecienteVacio.setVisibility(View.VISIBLE);
            return;
        }

        // Tarjeta total = último egreso
        ComprobanteResponse ultimo = lista.get(0);
        b.txtTotalMonto.setText(money(ultimo.moneda, ultimo.total));
        b.txtTotalProveedor.setText(nz(ultimo.proveedor, "Proveedor"));
        b.txtTotalCategoria.setText(nz(ultimo.categoria, "Otros"));
        b.txtTotalConfianza.setText("Confianza IA: " + ultimo.confianza + "%");
        b.cardTotal.setOnClickListener(v -> abrirDetalle(ultimo));

        // Actividad reciente (hasta 4)
        b.txtRecienteVacio.setVisibility(View.GONE);
        int n = Math.min(4, lista.size());
        for (int i = 0; i < n; i++) {
            ComprobanteResponse c = lista.get(i);
            ItemEgresoBinding row = ItemEgresoBinding.inflate(getLayoutInflater(), b.containerReciente, false);
            row.txtProv.setText(nz(c.proveedor, "Proveedor"));
            row.txtSub.setText(subtitulo(c));
            row.txtMonto.setText(money(c.moneda, c.total));
            row.txtEstado.setText(nz(c.estado, "Registrado"));
            row.getRoot().setOnClickListener(v -> abrirDetalle(c));
            b.containerReciente.addView(row.getRoot());
        }
    }

    private void abrirDetalle(ComprobanteResponse c) {
        Intent i = new Intent(this, ResultActivity.class);
        i.putExtra(ResultActivity.EXTRA_JSON, gson.toJson(c));
        i.putExtra(ResultActivity.EXTRA_SOLO_LECTURA, true);
        startActivity(i);
    }

    private void confirmarSalir() {
        new AlertDialog.Builder(this)
                .setTitle("Perfil")
                .setMessage("¿Deseas cerrar sesión?")
                .setNegativeButton("Cancelar", null)
                .setPositiveButton(R.string.cerrar_sesion, (d, w) -> {
                    session.cerrarSesion();
                    ApiClient.reset();
                    startActivity(new Intent(this, LoginActivity.class));
                    finish();
                })
                .show();
    }

    private String subtitulo(ComprobanteResponse c) {
        String doc = c.tipoDocumento != null ? c.tipoDocumento : "Comprobante";
        if (c.numeroComprobante != null && !c.numeroComprobante.isEmpty()) doc += " " + c.numeroComprobante;
        String fecha = c.fechaRegistro != null && c.fechaRegistro.length() >= 10 ? c.fechaRegistro.substring(0, 10) : "";
        return fecha.isEmpty() ? doc : doc + " · " + fecha;
    }

    private String money(String moneda, double total) {
        String s = "USD".equalsIgnoreCase(moneda) ? "$ " : "S/ ";
        return s + String.format(Locale.US, "%.2f", total);
    }

    private String primerNombre(String n) {
        if (n == null || n.trim().isEmpty()) return "Usuario";
        return n.trim().split("\\s+")[0];
    }

    private String nz(String v, String alt) {
        return (v == null || v.trim().isEmpty()) ? alt : v;
    }
}
