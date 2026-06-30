package pe.edu.hzg.tesoreria.ui;

import android.Manifest;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.os.Bundle;
import android.view.View;
import android.widget.Toast;

import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.camera.core.CameraSelector;
import androidx.camera.core.ImageCapture;
import androidx.camera.core.ImageCaptureException;
import androidx.camera.core.Preview;
import androidx.camera.lifecycle.ProcessCameraProvider;
import androidx.core.content.ContextCompat;

import com.google.common.util.concurrent.ListenableFuture;
import com.google.gson.Gson;

import java.io.File;

import okhttp3.MediaType;
import okhttp3.MultipartBody;
import okhttp3.RequestBody;
import okhttp3.ResponseBody;
import pe.edu.hzg.tesoreria.data.ApiClient;
import pe.edu.hzg.tesoreria.data.model.ComprobanteResponse;
import pe.edu.hzg.tesoreria.data.model.ErrorResponse;
import pe.edu.hzg.tesoreria.databinding.ActivityCameraBinding;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class CameraActivity extends AppCompatActivity {

    private ActivityCameraBinding b;
    private ImageCapture imageCapture;

    private final ActivityResultLauncher<String> permiso =
            registerForActivityResult(new ActivityResultContracts.RequestPermission(), concedido -> {
                if (concedido) iniciarCamara();
                else {
                    Toast.makeText(this, "Se necesita permiso de cámara", Toast.LENGTH_LONG).show();
                    finish();
                }
            });

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        b = ActivityCameraBinding.inflate(getLayoutInflater());
        setContentView(b.getRoot());

        b.btnTomar.setOnClickListener(v -> tomarFoto());
        b.btnCerrar.setOnClickListener(v -> finish());

        if (ContextCompat.checkSelfPermission(this, Manifest.permission.CAMERA)
                == PackageManager.PERMISSION_GRANTED) {
            iniciarCamara();
        } else {
            permiso.launch(Manifest.permission.CAMERA);
        }
    }

    private void iniciarCamara() {
        ListenableFuture<ProcessCameraProvider> future = ProcessCameraProvider.getInstance(this);
        future.addListener(() -> {
            try {
                ProcessCameraProvider provider = future.get();
                Preview preview = new Preview.Builder().build();
                preview.setSurfaceProvider(b.preview.getSurfaceProvider());

                imageCapture = new ImageCapture.Builder()
                        .setCaptureMode(ImageCapture.CAPTURE_MODE_MINIMIZE_LATENCY)
                        .build();

                provider.unbindAll();
                provider.bindToLifecycle(this, CameraSelector.DEFAULT_BACK_CAMERA, preview, imageCapture);
            } catch (Exception e) {
                Toast.makeText(this, "Error al abrir la cámara: " + e.getMessage(), Toast.LENGTH_LONG).show();
            }
        }, ContextCompat.getMainExecutor(this));
    }

    private void tomarFoto() {
        if (imageCapture == null) return;
        File foto = new File(getCacheDir(), "comprobante_" + System.currentTimeMillis() + ".jpg");
        ImageCapture.OutputFileOptions opciones = new ImageCapture.OutputFileOptions.Builder(foto).build();

        cargando(true, "Capturando…");
        imageCapture.takePicture(opciones, ContextCompat.getMainExecutor(this),
                new ImageCapture.OnImageSavedCallback() {
                    @Override
                    public void onImageSaved(@NonNull ImageCapture.OutputFileResults results) {
                        subir(foto);
                    }

                    @Override
                    public void onError(@NonNull ImageCaptureException exc) {
                        cargando(false, null);
                        Toast.makeText(CameraActivity.this, "Error al capturar: " + exc.getMessage(), Toast.LENGTH_LONG).show();
                    }
                });
    }

    private void subir(File foto) {
        cargando(true, "Procesando comprobante…");

        RequestBody cuerpo = RequestBody.create(MediaType.parse("image/jpeg"), foto);
        MultipartBody.Part parte = MultipartBody.Part.createFormData("imagen", foto.getName(), cuerpo);

        ApiClient.getService(this).subirComprobante(parte).enqueue(new Callback<ComprobanteResponse>() {
            @Override
            public void onResponse(@NonNull Call<ComprobanteResponse> call, @NonNull Response<ComprobanteResponse> resp) {
                cargando(false, null);
                if (resp.isSuccessful() && resp.body() != null) {
                    String json = new Gson().toJson(resp.body());
                    Intent i = new Intent(CameraActivity.this, ResultActivity.class);
                    i.putExtra(ResultActivity.EXTRA_JSON, json);
                    startActivity(i);
                    finish();
                } else {
                    Toast.makeText(CameraActivity.this, leerError(resp.errorBody()), Toast.LENGTH_LONG).show();
                }
            }

            @Override
            public void onFailure(@NonNull Call<ComprobanteResponse> call, @NonNull Throwable t) {
                cargando(false, null);
                Toast.makeText(CameraActivity.this, "No se pudo enviar: " + t.getMessage(), Toast.LENGTH_LONG).show();
            }
        });
    }

    private String leerError(ResponseBody errorBody) {
        if (errorBody == null) return "No se pudo procesar el comprobante";
        try {
            ErrorResponse e = new Gson().fromJson(errorBody.string(), ErrorResponse.class);
            return (e != null && e.mensaje != null) ? e.mensaje : "No se pudo procesar el comprobante";
        } catch (Exception ex) {
            return "No se pudo procesar el comprobante";
        }
    }

    private void cargando(boolean v, String texto) {
        b.overlay.setVisibility(v ? View.VISIBLE : View.GONE);
        b.btnTomar.setEnabled(!v);
        if (texto != null) b.txtEstado.setText(texto);
    }
}
