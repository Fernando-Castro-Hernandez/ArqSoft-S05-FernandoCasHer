# Configuración de Base de Datos (PostgreSQL) + Identity Core

Este documento describe la integración de una base de datos **PostgreSQL** mediante
**Entity Framework Core** y la instalación de **ASP.NET Core Identity** en el proyecto
`CitasApp.Web`, realizada en la rama `BaseDeDatosAndIdentity`.

---

## Qué se hizo 

Hasta este punto, CitasApp persistía todos sus datos (pacientes, médicos, citas) en
archivos **JSON/CSV**, sin base de datos real. En esta etapa se agregó la infraestructura
de base de datos y de autenticación, sin reemplazar todavía la persistencia en archivos:

1. Se conectó el proyecto a **PostgreSQL** usando Entity Framework Core.
2. Se instaló **ASP.NET Core Identity** (gestión de usuarios y roles).
3. Se generó y aplicó la **migración inicial**, creando las tablas de Identity en la base de datos.

> **Importante:** las entidades de negocio (`Paciente`, `Medico`, `Cita`) **siguen en JSON/CSV**.
> Esta etapa solo construye la infraestructura de autenticación; migrar el negocio a la BD
> queda para una etapa posterior.

---

## Paquetes instalados (NuGet)

Instalados en el proyecto `CitasApp.Web`:

| Paquete | Para qué sirve |
|---|---|
| `Npgsql.EntityFrameworkCore.PostgreSQL` | Driver que permite a EF Core comunicarse con PostgreSQL. |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | ASP.NET Core Identity respaldado por EF Core (trae `IdentityDbContext`, `IdentityUser`, `IdentityRole`). |
| `Microsoft.EntityFrameworkCore.Design` | Herramientas de diseño necesarias para generar migraciones con `dotnet ef`. |

Comandos usados:

```bash
dotnet add CitasApp.Web package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add CitasApp.Web package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add CitasApp.Web package Microsoft.EntityFrameworkCore.Design

# Herramientas de línea de comandos de EF Core (una vez por máquina)
dotnet tool install --global dotnet-ef
```

---

## El DbContext

Se creó la clase `AppDbContext` en `CitasApp.Web/data/AppDbContext.cs`. Hereda de
`IdentityDbContext<IdentityUser>`, lo que hace que EF Core "conozca" automáticamente
todas las tablas que Identity necesita, sin escribirlas a mano.

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Citas_App.Data
{
    // Hereda de IdentityDbContext: eso trae AspNetUsers, AspNetRoles, etc.
    // No definimos DbSet<Paciente/Medico/Cita> todavía — siguen en JSON.
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    }
}
```

---

## Registro en `Program.cs`

Se agregó el registro del `DbContext` (apuntando a PostgreSQL) y de Identity en el
composition root, junto a los patrones ya existentes (Factory, Decorator, Observer):

```csharp
using Citas_App.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// ── Base de datos (PostgreSQL) + Identity Core ───────────────────────────────
// DbContext apunta a PostgreSQL usando la cadena de appsettings.json.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Identity: usuarios + roles, respaldados por AppDbContext (las tablas AspNet*).
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
```

Y en el pipeline HTTP se agregó el middleware de autenticación, **antes** de la
autorización (el orden importa: primero se identifica *quién eres*, luego *qué puedes hacer*):

```csharp
app.UseAuthentication();   // ← debe ir ANTES de UseAuthorization
app.UseAuthorization();
```

---

## Cadena de conexión y manejo seguro de la contraseña

La cadena de conexión vive en `appsettings.json`, pero **sin la contraseña real**. En su
lugar se usa un placeholder, y la contraseña verdadera se guarda en **User Secrets**
(fuera del repositorio, en la carpeta de usuario), para no exponerla en Git.

`appsettings.json` (lo que sí se commitea):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=citasapp;Username=postgres;Password=__SET_IN_USER_SECRETS__"
  }
}
```

La contraseña real se guarda con User Secrets (no se sube al repo):

```bash
dotnet user-secrets init --project CitasApp.Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=citasapp;Username=postgres;Password=TU_PASSWORD" --project CitasApp.Web
```

En desarrollo, .NET mezcla automáticamente los User Secrets con `appsettings.json` y
**sobrescribe** el placeholder con la cadena real. El código
(`GetConnectionString("DefaultConnection")`) no cambia.

> **Por qué:** si la contraseña estuviera en `appsettings.json`, quedaría en el historial
> de Git para siempre. Con User Secrets, cada desarrollador guarda su propia contraseña
> localmente y el repositorio nunca la ve.

---
## Qué se construyó en la base de datos

Al aplicar la migración, se crearon en PostgreSQL las tablas estándar de Identity:

| Tabla | Contenido |
|---|---|
| `AspNetUsers` | Usuarios (email, contraseña hasheada, etc.) |
| `AspNetRoles` | Roles (ej. Admin, Paciente, Medico) |
| `AspNetUserRoles` | Relación usuarios ↔ roles |
| `AspNetUserClaims` | Claims por usuario |
| `AspNetUserLogins` | Logins externos (Google, etc.) |
| `AspNetUserTokens` | Tokens por usuario |
| `AspNetRoleClaims` | Claims por rol |
| `__EFMigrationsHistory` | Registro de qué migraciones ya se aplicaron |

> Las tablas se crean **vacías**. Identity solo construye la estructura; los usuarios de la
> aplicación se registran después, cuando se implemente la funcionalidad de registro/login.

---

## Cómo reproducir en otra máquina

1. Tener **PostgreSQL** instalado y corriendo (por defecto en `localhost:5432`).
2. Guardar la contraseña de PostgreSQL en User Secrets (ver sección de cadena de conexión).
3. Aplicar las migraciones para crear las tablas:

   ```bash
   dotnet ef database update --project CitasApp.Web
   ```

4. Verificar que las tablas se crearon (opcional, con `psql`):

   ```bash
   psql -U postgres -h localhost -d citasapp -c "\dt"
   ```

---

## Datos de conexión usados

| Parámetro | Valor |
|---|---|
| Motor | PostgreSQL |
| Host | `localhost` |
| Puerto | `5432` |
| Base de datos | `citasapp` |
| Usuario | `postgres` |
| Contraseña | Guardada en **User Secrets** (no en el repositorio) |
