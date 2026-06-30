package pe.edu.hzg.tesoreria.ui;

import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.view.View;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.google.gson.Gson;

import pe.edu.hzg.tesoreria.R;
import pe.edu.hzg.tesoreria.data.ApiClient;
import pe.edu.hzg.tesoreria.data.model.ComprobanteResponse;
import pe.edu.hzg.tesoreria.databinding.ActivityResultBinding;

/** Muestra el egreso registrado (o el detalle de uno existente, en modo lectura). */
public class ResultActivity extends AppCompatActivity {

    public static final String EXTRA_JSON = "comprobante_json";
    public static final String EXTRA_SOLO_LECTURA = "solo_lectura";

    private ActivityResultBinding b;
    private ComprobanteResponse comprobante;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        b = ActivityResultBinding.inflate(getLayoutInflater());
        setContentView(b.getRoot());

        boolean soloLectura = getIntent().getBooleanExtra(EXTRA_SOLO_LECTURA, false);
        String json = getIntent().getStringExtra(EXTRA_JSON);
        comprobante = new Gson().fromJson(json, ComprobanteResponse.class);

        if (comprobante != null) pintar(comprobante);

        // En modo lectura (visto desde el historial) no mostramos el banner de "registrado".
        b.layoutExito.setVisibility(soloLectura ? View.GONE : View.VISIBLE);

        b.btnBack.setOnClickListener(v -> finish());
        b.btnMore.setOnClickListener(v -> Toast.makeText(this, R.string.proximamente, Toast.LENGTH_SHORT).show());
        b.btnVer.setOnClickListener(v -> verImagen());
        b.btnOtro.setOnClickListener(v -> {
            startActivity(new Intent(this, CameraActivity.class));
            finish();
        });
    }

    private void pintar(ComprobanteResponse c) {
        b.txtTotal.setText(money(c.moneda, c.total));
        b.txtProveedor.setText(nz(c.proveedor, "Proveedor no detectado"));
        b.txtCategoria.setText(nz(c.categoria, "Otros"));
        b.txtConfianza.setText("Confianza IA: " + c.confianza + "%");

        fila(b.lblRuc, b.valRuc, c.ruc);
        fila(b.lblDoc, b.valDoc, join(c.tipoDocumento, c.numeroComprobante));
        fila(b.lblFecha, b.valFecha, c.fechaEmision);
        fila(b.lblMetodo, b.valMetodo, c.metodoPago);
        fila(b.lblConcepto, b.valConcepto, c.concepto);
        fila(b.lblObs, b.valObs, c.observaciones);

        b.bannerDuplicado.setVisibility(c.esDuplicadoProbable ? View.VISIBLE : View.GONE);
    }

    private void verImagen() {
        if (comprobante == null || comprobante.imagenUrl == null || comprobante.imagenUrl.isEmpty()) {
            Toast.makeText(this, "Imagen no disponible", Toast.LENGTH_SHORT).show();
            return;
        }
        String base = ApiClient.BASE_URL.endsWith("/") ? ApiClient.BASE_URL.substring(0, ApiClient.BASE_URL.length() - 1) : ApiClient.BASE_URL;
        String url = comprobante.imagenUrl.startsWith("http") ? comprobante.imagenUrl : base + comprobante.imagenUrl;
        try {
            startActivity(new Intent(Intent.ACTION_VIEW, Uri.parse(url)));
        } catch (Exception e) {
            Toast.makeText(this, "No se pudo abrir la imagen", Toast.LENGTH_SHORT).show();
        }
    }

    /** Rellena una fila etiqueta/valor; oculta toda la fila si el valor está vacío. */
    private void fila(TextView label, TextView valor, String dato) {
        View fila = (View) label.getParent();
        if (dato == null || dato.trim().isEmpty()) {
            fila.setVisibility(View.GONE);
        } else {
            fila.setVisibility(View.VISIBLE);
            valor.setText(dato);
        }
    }

    private String money(String moneda, double total) {
        String simbolo = "USD".equalsIgnoreCase(moneda) ? "$ " : "S/ ";
        return simbolo + String.format(java.util.Locale.US, "%.2f", total);
    }

    private String join(String a, String b) {
        if (a == null && b == null) return null;
        if (a == null) return b;
        if (b == null) return a;
        return a + " " + b;
    }

    private String nz(String v, String alt) {
        return (v == null || v.trim().isEmpty()) ? alt : v;
    }
}
