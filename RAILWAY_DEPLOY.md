# Guía de Despliegue en Railway — Sistema de Tesorería

Este repositorio ha sido configurado para poder desplegarse de manera independiente y sumamente sencilla en **Railway** como un entorno multiproyecto (monorepo).

El sistema consta de 3 servicios en Railway:
1. **Base de Datos:** PostgreSQL (Nativo de Railway).
2. **Backend (API):** Aplicación .NET 8 Web API.
3. **Frontend (Web):** Aplicación React 19 + Vite (Servido con Nginx).

---

## 🗄️ Paso 1: Crear la Base de Datos PostgreSQL
1. En tu panel de Railway, haz clic en **New** -> **Database** -> **Add PostgreSQL**.
2. Railway creará el servicio de base de datos de manera inmediata. No requiere configuración adicional.

---

## ⚙️ Paso 2: Desplegar el Backend (API .NET 8)
1. Haz clic en **New** -> **Github Repo** y selecciona este repositorio.
2. Una vez creado el servicio del Backend, ve a su pestaña **Settings**:
   * **Service Name:** Cámbialo a `sistema-tesoreria-backend` (o el nombre que prefieras).
   * **Root Directory:** Déjalo en `/` (raíz del proyecto).
   * **Dockerfile Path:** Escribe `src/Horacio.API/Dockerfile`.
3. Ve a la pestaña **Variables** y agrega las siguientes variables de entorno:

| Variable | Valor / Referencia | Descripción |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | `${{Postgres.DATABASE_URL}}` | Enlace automático con la BD PostgreSQL de Railway |
| `Jwt__Key` | `TU_CLAVE_SUPER_SECRETA_MINIMO_32_CARACTERES` | Clave para firmar los tokens de sesión JWT |
| `Jwt__Issuer` | `HoracioTesoreria` | Emisor del token JWT |
| `Jwt__Audience` | `HoracioTesoreriaClient` | Audiencia del token JWT |
| `Jwt__ExpirationMinutes` | `480` | Duración del token de sesión (8 horas) |
| `Reniec__Provider` | `Api` | `Api` para producción o `Fake` para simulación |
| `Reniec__Token` | `TU_TOKEN_DE_APIS_NET_PE` | Token de consulta DNI (opcional si es Fake) |
| `Ocr__Provider` | `Mock` o `GoogleVision` | Proveedor para la extracción de texto en fotos |
| `Ocr__ApiKey` | `TU_GOOGLE_VISION_API_KEY` | Clave API para Google Cloud Vision (opcional si es Mock) |
| `DeepSeek__Provider` | `Mock` o `DeepSeek` | Proveedor para la estructuración de comprobantes |
| `DeepSeek__ApiKey` | `TU_DEEPSEEK_API_KEY` | Clave API de DeepSeek (opcional si es Mock) |
| `Storage__RootPath` | `wwwroot/uploads` | Directorio de almacenamiento de imágenes subidas |
| `Storage__PublicBaseUrl` | `/uploads` | Ruta pública para consumir las imágenes |

4. En la pestaña **Settings**, bajo **Public Networking**, haz clic en **Generate Domain** para obtener la URL pública de tu API (ej: `https://sistema-tesoreria-backend.up.railway.app`). **Copia esta URL**, la necesitarás para configurar el frontend.

> ℹ️ *Nota: Al iniciar por primera vez, el backend aplicará de forma automática las migraciones EF Core y sembrará los datos iniciales (incluyendo el usuario `admin` con contraseña `Admin123*`).*

---

## 💻 Paso 3: Desplegar el Frontend (React + Vite)
1. Haz clic en **New** -> **Github Repo** y selecciona **el mismo repositorio** (creará un segundo servicio).
2. Ve a la pestaña **Settings** del nuevo servicio:
   * **Service Name:** Cámbialo a `sistema-tesoreria-frontend`.
   * **Root Directory:** Escribe `/frontend`.
   * *(Railway detectará automáticamente el archivo `/frontend/Dockerfile` y compilará la aplicación).*
3. Ve a la pestaña **Variables** y agrega la siguiente variable de entorno:

| Variable | Valor | Descripción |
|---|---|---|
| `VITE_API_URL` | `https://sistema-tesoreria-backend.up.railway.app/api` | La URL pública de tu Backend generado en el Paso 2 (debe terminar en `/api`) |

> ⚠️ **IMPORTANTE:** En las aplicaciones creadas con Vite, las variables de entorno se inyectan en **tiempo de compilación** (build-time). Asegúrate de agregar esta variable **antes** de que termine el primer build, o de lo contrario la aplicación web intentará conectarse a `http://localhost:5080/api` por defecto. Si esto ocurre, simplemente guarda la variable y haz clic en **Redeploy**.

4. En la pestaña **Settings** del frontend, ve a **Public Networking** y haz clic en **Generate Domain** para obtener la URL pública del sistema web.

¡Listo! Ya tienes el sistema completamente operativo en la nube.
