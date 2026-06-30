# Sistema de Ingresos por Recursos Propios y Actividades Productivas
### IEST "Horacio Zeballos Gámez" — Juliaca, Perú

Sistema **interno** de tesorería para la emisión de **comprobantes internos** (tickets) por
ingresos de recursos propios y actividades productivas (D.S. N° 028-2007-ED).

> ⚠️ **No es facturación electrónica ni un sistema SUNAT.** No genera facturas, boletas
> electrónicas, XML, CDR, RUC, IGV ni QR tributario. Es un comprobante interno institucional.

---

## 🧱 Arquitectura

**Clean Architecture** estricta con CQRS + MediatR, Repository + Unit of Work, FluentValidation y DI.

```
src/
 ├─ Horacio.Domain         Entidades, enums, excepciones (núcleo, sin dependencias)
 ├─ Horacio.Application     CQRS (commands/queries), DTOs, validators, interfaces de servicios
 ├─ Horacio.Infrastructure  Servicios técnicos: RENIEC, JWT, número→letras, BCrypt, fecha
 ├─ Horacio.Persistence     EF Core, DbContext, configuraciones, repositorios, UoW, seeders
 └─ Horacio.API             Controllers, middleware, Program.cs, Swagger, Serilog, JWT
tests/
 ├─ Horacio.UnitTests
 └─ Horacio.IntegrationTests
frontend/                   React 19 + Vite + TypeScript + Tailwind + Shadcn (FASE 8)
```

### Stack
- **Backend:** .NET 8 Web API · EF Core 8 · PostgreSQL 16 · MediatR · FluentValidation · Serilog · JWT.
- **Frontend:** React 19 · TypeScript · Vite · TailwindCSS · Shadcn UI · React Hook Form · Zod · TanStack Query · Axios.
- **Infra:** Docker + Docker Compose · Swagger/OpenAPI · Health Checks.

---

## 🚀 Puesta en marcha

### Opción A — Docker Compose (recomendado)
Requisitos: **Docker Desktop**.

```bash
docker compose up -d --build
```
- API:      http://localhost:5080
- Swagger:  http://localhost:5080/swagger
- Health:   http://localhost:5080/health
- PostgreSQL: localhost:5432 (db `horacio_tesoreria`, user/pass `postgres`)

La API **aplica migraciones y siembra los datos** automáticamente al iniciar.

### Opción B — Local (.NET SDK 8)
Requisitos: **.NET 8 SDK**, **Docker** (solo para PostgreSQL) o un PostgreSQL local.

```powershell
# 1) PostgreSQL
docker run -d --name horacio-postgres -e POSTGRES_PASSWORD=postgres `
  -e POSTGRES_DB=horacio_tesoreria -p 5432:5432 postgres:16

# 2) API
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:ASPNETCORE_URLS="http://localhost:5080"
dotnet run --project src/Horacio.API/Horacio.API.csproj
```

> Si el SDK se instaló en el perfil del usuario (sin admin):
> ```powershell
> $env:DOTNET_ROOT="$env:LOCALAPPDATA\Microsoft\dotnet"
> $env:PATH="$env:LOCALAPPDATA\Microsoft\dotnet;$env:USERPROFILE\.dotnet\tools;$env:PATH"
> ```

---

## 🔐 Acceso inicial
| Usuario | Contraseña  | Rol           |
|---------|-------------|---------------|
| `admin` | `Admin123*` | Administrador |

> Cambie la contraseña y `Jwt:Key` antes de producción.

**Roles:** `Administrador` (todo) y `Finanzas` (tesorería y consultas).

---

## 🧩 Funcionalidades del backend (implementadas)
- **Autenticación JWT** con roles y permisos.
- **Catálogos sembrados:** 11 programas de estudio, 3 turnos, 11 servicios.
- **Alumnos + consulta automática RENIEC**: al completar 8 dígitos del DNI se busca en BD y,
  si no existe, se consulta RENIEC automáticamente (**sin botón**). Proveedor intercambiable
  (`IReniecService`): `Fake` (desarrollo) o `Api` (HTTP real), configurable en `appsettings`.
- **Tesorería / Tickets:** emisión con múltiples servicios, **numeración automática**
  (`001/001` + contador global `000000001`), total e **importe en letras**
  ("QUINCE CON 00/100 SOLES").
- **Dashboard:** recaudación de hoy/mes, conteos, servicios y programas top, últimos tickets.
- **Calidad:** manejo global de excepciones, validación con FluentValidation, logging Serilog,
  health checks, Swagger.

### Endpoints principales
| Método | Ruta                                | Descripción                          |
|--------|-------------------------------------|--------------------------------------|
| POST   | `/api/auth/login`                   | Login (devuelve JWT)                 |
| GET    | `/api/programas`                    | Lista de programas                   |
| GET    | `/api/turnos`                       | Catálogo de turnos                   |
| GET    | `/api/servicios`                    | Lista de servicios                   |
| GET    | `/api/alumnos/consulta-dni/{dni}`   | Consulta automática (BD → RENIEC)    |
| POST   | `/api/alumnos`                      | Registrar alumno                     |
| POST   | `/api/tickets`                      | Emitir ticket                        |
| GET    | `/api/tickets/{id}`                 | Ticket completo (reimpresión)        |
| GET    | `/api/dashboard`                    | Métricas del panel                   |
| GET    | `/health`                           | Estado del servicio                  |

---

## 🗄️ Migraciones EF Core
```powershell
dotnet ef migrations add <Nombre> -p src/Horacio.Persistence -s src/Horacio.API -o Migrations
dotnet ef database update           -p src/Horacio.Persistence -s src/Horacio.API
```
(La API también migra automáticamente al iniciar.)

---

## ⚙️ Configuración (`src/Horacio.API/appsettings.json`)
- `ConnectionStrings:DefaultConnection` — cadena PostgreSQL.
- `Jwt:Key/Issuer/Audience/ExpirationMinutes` — token.
- `Reniec:Provider` — `Fake` | `Api`; `Reniec:BaseUrl` (usar `{dni}` o se anexa) y `Reniec:Token`.

En Docker se sobreescriben con variables `ConnectionStrings__DefaultConnection`, `Jwt__Key`, etc.

---

## 📌 Estado y hoja de ruta
El avance detallado por fases está en **`SISTEMA_MASTER.md`** (memoria del proyecto).
Pendiente: Reportes + exportación Excel/PDF, auditoría enganchada, impresión térmica (QZ Tray),
frontend React y suite de tests.
