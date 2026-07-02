package pe.edu.hzg.tesoreria.data;

import android.content.Context;

import java.util.concurrent.TimeUnit;

import okhttp3.OkHttpClient;
import okhttp3.logging.HttpLoggingInterceptor;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

/**
 * Construye el cliente Retrofit. El interceptor agrega el token JWT a cada
 * petición autenticada.
 *
 * CONFIGURA AQUÍ LA URL DEL BACKEND:
 *  - Emulador Android  → http://10.0.2.2:5080/   (apunta al localhost de tu PC)
 *  - Celular en la red → http://IP_DE_TU_PC:5080/
 *  - Producción Droplet→ https://tu-dominio-o-ip/
 */
public class ApiClient {

    public static final String BASE_URL = "https://refreshing-wisdom-production.up.railway.app/";

    private static Retrofit retrofit;

    public static ApiService getService(Context context) {
        if (retrofit == null) {
            SessionManager session = new SessionManager(context);

            HttpLoggingInterceptor logging = new HttpLoggingInterceptor();
            logging.setLevel(HttpLoggingInterceptor.Level.BASIC);

            OkHttpClient client = new OkHttpClient.Builder()
                    .connectTimeout(30, TimeUnit.SECONDS)
                    .readTimeout(90, TimeUnit.SECONDS)   // OCR + IA puede tardar
                    .writeTimeout(60, TimeUnit.SECONDS)
                    .addInterceptor(chain -> {
                        String token = session.getToken();
                        okhttp3.Request.Builder req = chain.request().newBuilder();
                        if (token != null) {
                            req.header("Authorization", "Bearer " + token);
                        }
                        return chain.proceed(req.build());
                    })
                    .addInterceptor(logging)
                    .build();

            retrofit = new Retrofit.Builder()
                    .baseUrl(BASE_URL)
                    .client(client)
                    .addConverterFactory(GsonConverterFactory.create())
                    .build();
        }
        return retrofit.create(ApiService.class);
    }

    /** Fuerza recrear el cliente (p. ej. tras logout). */
    public static void reset() {
        retrofit = null;
    }
}
