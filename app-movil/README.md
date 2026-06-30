# Tesorería Egresos — App Android (Java + CameraX)

App móvil del CETPRO "Horacio Zeballos Gámez" para **registrar egresos por foto**.
Toma una foto del comprobante (factura/boleta/ticket) y la envía al backend, que la
procesa con **OCR (Google Vision) + IA (DeepSeek)** y registra el egreso automáticamente
en la **misma base de datos PostgreSQL** del sistema de tesorería.

```
Cámara (CameraX) → POST /api/comprobantes → [guardar imagen → OCR → DeepSeek →
validar → detectar duplicados → registrar egreso] → PostgreSQL → Web "Comparativa"
```

## Requisitos
- Android Studio (Koala o superior) con Android SDK 34.
- El backend `sistema-empresarial` corriendo y accesible desde el teléfono/emulador.

## Cómo abrir y ejecutar
1. Android Studio → **Open** → carpeta `D:\app-movil`.
2. Espera el *Gradle sync* (descarga dependencias y genera el wrapper).
3. Configura la URL del backend en
   [`ApiClient.java`](app/src/main/java/pe/edu/hzg/tesoreria/data/ApiClient.java):
   - **Emulador**: `http://10.0.2.2:5080/` (apunta al `localhost` de tu PC).
   - **Teléfono físico** (misma red Wi-Fi): `http://IP_DE_TU_PC:5080/`
     y agrega esa IP en
     [`network_security_config.xml`](app/src/main/res/xml/network_security_config.xml).
   - **Producción (Droplet)**: `https://tu-dominio/`.
4. Run ▶ en un emulador o dispositivo.
5. Inicia sesión con un usuario del sistema (ej. `admin` / `Admin123*`) y pulsa
   **Capturar comprobante**.

## Estructura
```
app/src/main/java/pe/edu/hzg/tesoreria/
├── data/
│   ├── ApiClient.java        ← Retrofit + interceptor JWT (CONFIGURA LA URL AQUÍ)
│   ├── ApiService.java       ← endpoints (login, subir comprobante)
│   ├── SessionManager.java   ← guarda el token JWT
│   └── model/                ← LoginRequest/Response, ComprobanteResponse…
└── ui/
    ├── LoginActivity.java     ← inicio de sesión
    ├── MainActivity.java      ← menú: capturar / cerrar sesión
    ├── CameraActivity.java    ← CameraX: preview + captura + subida
    └── ResultActivity.java    ← muestra el egreso ya registrado
```

## Notas
- La app **no** procesa la imagen localmente: solo la captura y la sube. Todo el
  OCR/IA/validación ocurre en el backend (las API keys viven en el servidor).
- El endpoint devuelve el egreso ya creado, con el aviso de **posible duplicado** si
  corresponde.
