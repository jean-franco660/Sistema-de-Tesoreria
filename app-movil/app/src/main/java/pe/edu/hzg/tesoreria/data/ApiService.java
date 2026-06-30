package pe.edu.hzg.tesoreria.data;

import java.util.List;

import okhttp3.MultipartBody;
import pe.edu.hzg.tesoreria.data.model.ComprobanteResponse;
import pe.edu.hzg.tesoreria.data.model.LoginRequest;
import pe.edu.hzg.tesoreria.data.model.LoginResponse;
import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.GET;
import retrofit2.http.Multipart;
import retrofit2.http.POST;
import retrofit2.http.Part;

/** Endpoints del backend de tesorería. */
public interface ApiService {

    @POST("api/auth/login")
    Call<LoginResponse> login(@Body LoginRequest body);

    /** Tubería completa: sube la foto y devuelve el egreso ya registrado. */
    @Multipart
    @POST("api/comprobantes")
    Call<ComprobanteResponse> subirComprobante(@Part MultipartBody.Part imagen);

    @GET("api/comprobantes")
    Call<List<ComprobanteResponse>> listarComprobantes();
}
