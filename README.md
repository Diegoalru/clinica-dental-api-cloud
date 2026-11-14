# Clínica Dental — API

API REST del sistema web para consultorios dentales: autenticación, catálogo de servicios, odontólogos y agenda de citas.

Proyecto del curso Desarrollo de Aplicaciones para la Computación en la Nube (III C 2026).

## Repositorios del sistema

| Repositorio | Contenido |
|---|---|
| [clinica-dental-cloud](https://github.com/durena-dev/clinica-dental-cloud) | Base de datos: migraciones, datos de prueba y documentación (DER, flujos, arquitectura) |
| **clinica-dental-api-cloud** (este) | API REST en .NET |
| [clinica-dental-web-cloud](https://github.com/Diegoalru/clinica-dental-web-cloud) | Frontend en React |

## Tecnologías

- .NET 10 (ASP.NET Core Web API)
- Entity Framework Core 10 con Npgsql (PostgreSQL)
- Autenticación con JWT
- Contraseñas con BCrypt (BCrypt.Net-Next)
- Documentación interactiva con Scalar
- Base de datos PostgreSQL en Supabase

El esquema de la base de datos **no** se define en este repositorio: vive en las migraciones de `clinica-dental-cloud`. El API solo lo mapea; no crea ni modifica tablas.

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- Acceso a la base de datos: la de Supabase o una local levantada con Supabase CLI desde el repositorio `clinica-dental-cloud`

## Configuración

El repositorio no incluye secretos. El API lee su configuración de variables de entorno (también puede usarse `appsettings.example.json` como guía de los valores esperados).

| Variable | Descripción | Ejemplo |
|---|---|---|
| `ConnectionStrings__ClinicaDental` | Cadena de conexión a PostgreSQL | Ver abajo |
| `Jwt__Key` | Clave para firmar los tokens (mínimo 32 caracteres, aleatoria) | — |
| `Cors__AllowedOrigins__0` | Origen del frontend autorizado | `http://localhost:5173` |

**Cadena de conexión:**

- Base local (Supabase CLI): `Host=127.0.0.1;Port=54322;Database=postgres;Username=postgres;Password=postgres`
- Base en Supabase: se utiliza el **Transaction pooler** (puerto 6543) para garantizar compatibilidad con IPv4 y entornos de contenedores (Docker/Azure): `Host=aws-0-us-east-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.<project-ref>;Password=<contraseña>;SSL Mode=Require;Trust Server Certificate=true;No Reset On Close=true`. La contraseña la comparte el equipo por un canal privado; nunca se sube al repositorio.

En Windows (PowerShell) se pueden guardar como variables de usuario:

```powershell
[Environment]::SetEnvironmentVariable("ConnectionStrings__ClinicaDental", '<cadena de conexión>', "User")
[Environment]::SetEnvironmentVariable("Jwt__Key", '<clave aleatoria>', "User")
[Environment]::SetEnvironmentVariable("Cors__AllowedOrigins__0", "http://localhost:5173", "User")
```

Después de configurarlas, cierre y vuelva a abrir la terminal para que el API las lea.

## Ejecución local

```powershell
git clone https://github.com/Diegoalru/clinica-dental-api-cloud.git
cd clinica-dental-api-cloud
dotnet run --project clinica-dental-api-cloud
```

El API queda disponible en `http://localhost:5125`. Todas las rutas comienzan con `/api/v1`.

La documentación interactiva con Scalar, con la lista completa de endpoints, está en `http://localhost:5125/api/scalar/v1`.

### Ejecución con Docker

El API cuenta con un Dockerfile optimizado basado en imágenes *chiseled* de .NET (< 140 MB).

Construcción de la imagen (desde la raíz de este repositorio):

```powershell
docker build -t clinica-dental-api -f clinica-dental-api-cloud/Dockerfile .
```

Ejecución del contenedor:

```powershell
docker run -d --name clinica-api -p 5125:8080 `
  -e ConnectionStrings__ClinicaDental="<cadena de conexión>" `
  -e Jwt__Key="<clave aleatoria de al menos 32 caracteres>" `
  -e Cors__AllowedOrigins__0="http://localhost:5173" `
  clinica-dental-api
```

También puede levantarse junto con todo el sistema usando Docker Compose desde la raíz del workspace:

```powershell
docker compose up -d api
```

## Pruebas

```powershell
dotnet test
```

## Autenticación

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/v1/auth/login` | Recibe correo y contraseña; devuelve un token JWT y los datos del usuario |
| GET | `/api/v1/auth/me` | Devuelve el usuario autenticado (requiere el token) |

Las demás rutas requieren el encabezado `Authorization: Bearer <token>`.

## Credenciales de prueba

Datos ficticios de demostración. Las cuentas existen en la base de datos en la nube y en la base local cargada con el seed.

**Contraseña de todas las cuentas:** `Clinica2026!`

| Rol | Nombre | Correo |
|---|---|---|
| Administrador | Carlos Vargas | admin@clinica.example |
| Recepcionista | Lucía Prado | recepcion@clinica.example |
| Odontóloga | Dra. Ana Mora | ana.mora@clinica.example |
| Odontólogo | Dr. Luis Rojas | luis.rojas@clinica.example |
| Odontóloga | Dra. Marta Vega | marta.vega@clinica.example |
| Paciente | Sofía Castro | sofia.castro@clinica.example |
| Paciente | Mateo Jiménez | mateo.jimenez@clinica.example |

## Estructura

```
clinica-dental-api-cloud/
├── clinica-dental-api-cloud.sln
├── clinica-dental-api-cloud.Tests/   # Pruebas unitarias (reglas de estado, disponibilidad)
└── clinica-dental-api-cloud/
    ├── Controllers/      # Endpoints
    ├── Services/         # Lógica de negocio
    ├── Data/             # DbContext y entidades (mapeo de las tablas)
    ├── DTOs/             # Objetos de entrada y salida
    ├── Program.cs        # Configuración: conexión, JWT, CORS, Scalar
    └── Dockerfile
```

## Problemas comunes

| Síntoma | Causa y solución |
|---|---|
| El API no arranca y menciona la cadena de conexión o la clave JWT | Falta `ConnectionStrings__ClinicaDental` o `Jwt__Key` |
| El frontend muestra "No se pudo conectar con el servidor" y la consola del navegador menciona CORS | Falta `Cors__AllowedOrigins__0` con el origen del frontend; configúrela y reinicie el API |
| No conecta a Supabase | Use el **Transaction pooler** (puerto 6543, no 5432) y verifique `SSL Mode=Require;Trust Server Certificate=true`. La conexión directa (puerto 5432) es solo IPv6 y falla en redes sin soporte IPv6 |
