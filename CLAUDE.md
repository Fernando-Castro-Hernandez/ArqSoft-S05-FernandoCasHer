# CLAUDE.md — CitasApp

Contexto para Claude Code. Este archivo describe qué es el repo, cómo está
organizado, las convenciones que debes respetar y cómo ha evolucionado a través
de las prácticas de la materia **Arquitectura de Software** (3er cuatrimestre,
TSU Desarrollo de Software). El proyecto es una maqueta didáctica: cada decisión
está puesta a propósito para *demostrar en código* un concepto de arquitectura.
Los mensajes de commit y comentarios en el código suelen indicar qué patrón
demuestra cada cambio — conserva ese enfoque al agregar código nuevo.

---

## Qué es

**CitasApp** — app de citas médicas en **ASP.NET Core (.NET 10)**. Tres
entidades: `Paciente`, `Medico`, `Cita`. Sin base de datos real: la persistencia
por defecto es en archivos (JSON/CSV). El valor del proyecto no es la app en sí,
sino servir de vehículo para practicar estilos y patrones arquitectónicos.

Sobre la rama `Hexagonal-Architecture` (3 proyectos: Domain/Infrastructure/Web)
se agregó después una capa `Application` y un quinto proyecto `CitasApp.Api`
(ver [README.md](README.md) y la sección de evolución más abajo), así que en
`GOF-Patterns` y ramas posteriores el repo tiene **cinco proyectos**, con dos
front-ends (navegador vía Web, móvil/Postman vía Api) sobre el mismo núcleo:

```
Navegador     ──► CitasApp.Web  ─┐
                                  ├─► Application ─► Domain ◄─ Infrastructure
Móvil/Postman ──► CitasApp.Api  ─┘
```

No hay proyectos de test en la solución.

---

## ⚠️ Convenciones críticas (lee esto antes de escribir código)

Estas son las reglas que más se rompen. Respétalas siempre:

1. **Namespaces con guion bajo: `Citas_App.*`** — NO `CitasApp.*`.
   - `Citas_App.Models`, `Citas_App.Interfaces`, `Citas_App.Repositories`,
     `Citas_App.Services`, `Citas_App.Observers`, `Citas_App.Controllers`,
     `Citas_App.Application.Services`, etc.
   - Los nombres de los **proyectos/carpetas** sí usan punto (`CitasApp.Domain`,
     `CitasApp.Infrastructure`, `CitasApp.Web`, `CitasApp.Application`,
     `CitasApp.Api`), pero los **namespaces internos** conservan el guion bajo
     por decisión histórica (para minimizar cambios al migrar). El código del
     maestro suele venir con `CitasApp.Domain.*` / `CitasApp.Application.*` —
     **siempre hay que traducirlo** a `Citas_App.*`. No asumas que el namespace
     te dice en qué proyecto físico vive un archivo: revisa la ruta.
2. **Archivo de solución: `CitasApp.slnx`** (formato XML nuevo, default de .NET 10),
   NO `.sln` clásico. Para compilar: `dotnet build CitasApp.slnx`.
3. **Los `Console.WriteLine` (logs de Decorator y Observer) NO se ven en IIS Express.**
   Para verlos hay que correr con `dotnet run` o el perfil `http`/`CitasApp.Web`
   (la "terminal moradita"). Nunca uses IIS Express para demostrar patrones que
   loggean a consola.
4. **`launchSettings.json` sobreescribe `ASPNETCORE_ENVIRONMENT`.** Para forzar un
   entorno de verdad usa el flag: `--no-launch-profile --environment Production`.
5. Al crear clases nuevas desde Visual Studio, VS pone el namespace del proyecto
   (`CitasApp.Infrastructure.Repositories`). **Cámbialo a mano** a
   `Citas_App.Repositories` (o el que corresponda) para mantener consistencia.
6. **Hay dos clases `CitaService` independientes** — no es un error, es una
   divergencia real entre Web y Api:
   - `CitasApp.Domain/Services/CitaService.cs` (`namespace Citas_App.Services`)
     — tiene `ConfirmarCita` y notifica vía `IEnumerable<ICitaObserver>`
     (patrón Observer). La usa `CitasApp.Web`.
   - `CitasApp.Application/CitaService.cs`
     (`namespace Citas_App.Application.Services`) — solo lectura
     (`ObtenerTodos`, `ObtenerPorPaciente`), sin observers. La usa `CitasApp.Api`.

   Al tocar confirmación de citas o notificaciones, revisa si el cambio debe ir
   en las dos o solo en una a propósito.

---

## Arquitectura — Hexagonal (Ports & Adapters)

Cinco proyectos, dependencias apuntando siempre hacia Domain:

```
CitasApp.slnx
├── CitasApp.Domain/            ← núcleo, no depende de nadie
│   ├── Models/                 (Paciente, Medico, Cita, CitaJson)
│   ├── Interfaces/             (ICitaRepository, IMedicoRepository, IPacienteRepository, ICitaObserver)  ← Ports
│   └── Services/                (CitaService — versión con Observer, la usa Web)
│
├── CitasApp.Application/       ← capa de aplicación (solo depende de Domain)
│   └── CitaService.cs, MedicoService.cs, PacienteService.cs   (versión sin Observer, la usa Api)
│
├── CitasApp.Infrastructure/    ← adapters de salida (solo depende de Domain)
│   ├── Repositories/           (Json*, Csv*, Sqlite*, Memoria*, Logging*, RepositoryFactory)
│   └── Observers/              (SmsObserver, EmailObserver)
│
├── CitasApp.Web/                ← adapter de entrada (navegador) + composition root
│   ├── Controllers/            (MVC: Paciente, Medico, Cita, Home + API: CitaApiController)
│   ├── Views/
│   ├── Models/                 (ErrorViewModel — es de presentación, NO va en Domain)
│   ├── data/ (JSON) y wwwroot/data/ (CSV)
│   └── Program.cs              ← aquí se conectan ports con adapters (Factory + Decorator + Observer)
│
└── CitasApp.Api/                ← adapter de entrada (REST) + su propio composition root
    ├── Controllers/            (PacientesController, MedicosController, CitasController, CalculadoraController)
    ├── data/                   (JSON, copia propia)
    └── Program.cs              ← DI directa a adapters Json*, sin Factory/Decorator/Observer
```

### Regla de las referencias (dirección de dependencias — inviolable)

```
CitasApp.Web ──> CitasApp.Domain + CitasApp.Application + CitasApp.Infrastructure
CitasApp.Api ──> CitasApp.Domain + CitasApp.Application + CitasApp.Infrastructure
CitasApp.Application ──> CitasApp.Domain
CitasApp.Infrastructure ──> CitasApp.Domain
CitasApp.Domain ──> (nadie)
```

- **Domain no referencia a nadie.** Es el núcleo puro. Nunca debe importar un
  namespace de Infrastructure, Application, Web ni Api.
- **Infrastructure → Domain** y **Application → Domain**, porque implementan u
  orquestan los ports de Domain.
- **Web y Api son composition roots independientes.** Cada `Program.cs` es el
  único lugar que conoce los adapters concretos y los enchufa a sus ports —
  y lo hacen de forma distinta entre sí (ver "Patrones GOF" abajo).
- `CitasApp.Infrastructure.csproj` incluye
  `<FrameworkReference Include="Microsoft.AspNetCore.App" />` porque algunos repos
  reciben `IWebHostEnvironment` para resolver rutas de archivos.

### Los Ports son minimalistas

`ICitaRepository` solo declara `ObtenerTodos()` y `ObtenerPorPaciente(int)`.
`IPacienteRepository` e `IMedicoRepository` declaran `ObtenerTodos()` y
`ObtenerPorId(int)`. Los adapters del maestro a veces traen métodos extra
(`Agregar`, `ConfirmarCita`) — hay que **recortarlos** para que calcen con el port,
o **ampliar el port** e implementarlo en TODOS los adapters (más invasivo).

### Patrones GOF aplicados (difieren entre Web y Api)

- **Factory** — `RepositoryFactory` (`CitasApp.Infrastructure/Repositories/RepositoryFactory.cs`,
  clase estática) decide qué repositorio crear según el entorno: `Production` →
  `MemoriaPacienteRepository`; cualquier otro → `JsonPacienteRepository`. (Medico
  y Cita retornan lo mismo en ambas ramas del switch; el foco de la práctica es
  Paciente.) **Solo está enchufado en `CitasApp.Web/Program.cs`.**
- **Decorator** — `LoggingPacienteRepository` implementa `IPacienteRepository`,
  recibe otro `IPacienteRepository` en el constructor, loggea antes/después de
  cada operación y delega al `_inner` sin modificar el repo original. Envuelve lo
  que devuelve el Factory. **También solo en `CitasApp.Web/Program.cs`:**

  ```csharp
  builder.Services.AddScoped<IPacienteRepository>(sp =>
  {
      var env  = sp.GetRequiredService<IWebHostEnvironment>();
      var repo = RepositoryFactory.CrearPacienteRepository(env.EnvironmentName, env); // Factory
      return new LoggingPacienteRepository(repo);                                     // Decorator
  });
  ```

- **Observer** — `ICitaObserver` (Domain) + `SmsObserver`/`EmailObserver`
  (Infrastructure) + `CitaService` (Domain) que guarda `IEnumerable<ICitaObserver>`
  y notifica a todos al confirmar una cita. `CitaService` solo depende de la
  abstracción (inversión de dependencias); los observers concretos se inyectan
  desde `Program.cs` con dos registros `AddScoped<ICitaObserver, ...>()` que .NET
  entrega juntos como `IEnumerable<ICitaObserver>`. Requiere `app.MapControllers();`
  en el pipeline para habilitar las rutas de atributo. Punto de entrada:
  `CitaApiController` (dentro de **Web**, no del proyecto Api) con
  `POST /api/citas/confirmar/{citaId}`.

  > Nota: la confirmación cambia el estado en memoria solo para construir el
  > mensaje; NO se persiste (el archivo se relee en cada llamada). Es
  > intencional para la práctica.

`CitasApp.Api/Program.cs` registra los repos **directamente** contra `Json*Repository`
para las tres entidades, sin Factory, sin Decorator y sin Observer — no asumas que
el comportamiento de Paciente/Cita es el mismo entre Web y Api.

### Adapters de repositorio disponibles vs. realmente conectados

Existen implementaciones Json, Csv, Sqlite y en memoria (`MemoriaPacienteRepository`)
por entidad en Infrastructure, pero no todas están en uso:

- **Web**: `Medico`/`Cita` → Csv (`wwwroot/data/*.csv`); `Paciente` → Factory
  (Json o Memoria según entorno) + Decorator de logging.
- **Api**: las tres entidades → Json (`data/*.json`, copia propia del proyecto).
- **Sqlite** (`Sqlite*Repository`, requiere `Microsoft.Data.Sqlite` en
  Infrastructure) existe en el código pero **no está registrado en ningún
  `Program.cs` actualmente** — verifica el DI antes de asumir que un adapter
  está vivo. La BD `citasapp.db` se autocrea vacía si se llega a usar.

### Persistencia de datos

`CitasApp.Web` y `CitasApp.Api` leen/escriben JSON en su propia carpeta `data/`
(o `wwwroot/data/` para CSV) — son copias independientes, no compartidas. Los
archivos deben tener `CopyToOutputDirectory = Always` en el `.csproj`; si un
endpoint devuelve `[]` o vacío, primero revisa que el archivo se haya copiado al
directorio de salida antes de asumir que el repositorio está mal.

CORS está habilitado en `CitasApp.Api` (`AllowAnyOrigin/Method/Header`) — es
permisividad de desarrollo, no lo restrinjas sin comentarlo antes.

---

## Evolución por prácticas (historia del repo)

El repo ha avanzado en ramas conforme el maestro (Dr. Jorge Pedrozo) da cada tema.
`main`/`master` conserva el estado evaluable base; el trabajo nuevo va en ramas.

### 1. Base — Arquitectura por capas (un solo proyecto)
Estado inicial: proyecto único MVC con carpetas `Controllers/`, `Views/`,
`Models/`, `Interfaces/`, `Repositories/`. Persistencia JSON en `data/`.
Repositorios por interfaz + inyección de dependencias. Namespace `Citas_App.*`.

### 2. Rama `Hexagonal-Architecture` — refactor a hexagonal multi-proyecto
Se separó el proyecto único en tres (`Domain`, `Infrastructure`, `Web`) con
`git mv`, se configuraron las referencias entre proyectos y se rehízo la solución
como `.slnx`. `ErrorViewModel` se quedó en Web. Se agregó
`MemoriaPacienteRepository` como segundo adapter para demostrar que cambiar el
Adapter en `Program.cs` no toca el núcleo. Commit tipo:
`refactor: migración a arquitectura hexagonal multi-proyecto`.

### 3. Test de intercambio de adapters (JSON → CSV → SQLite)
Ejercicio para demostrar que se puede cambiar la fuente de datos sin tocar Domain
ni Controllers. Se crearon adapters `Csv*Repository` (datos en `wwwroot/data/*.csv`)
y `Sqlite*Repository` (requiere `Microsoft.Data.Sqlite`, instalado EN
Infrastructure; la BD `citasapp.db` se autocrea vacía). Se alterna cuál adapter se
registra en `Program.cs`. Nota: los adapters de Cita del maestro traían métodos de
más que hubo que recortar al port.

### 4. Práctica #26 — Patrones GOF: Factory + Decorator (+ Observer como extra)
Aplicados sobre **Paciente** (ver detalle completo en "Patrones GOF aplicados"
arriba). Resumen: Factory decide Json vs. Memoria según entorno, Decorator agrega
logging envolviendo lo que arma el Factory, y Observer (extra) notifica por
SMS/Email al confirmar una cita vía `POST /api/citas/confirmar/{citaId}` en
`CitaApiController` (dentro de Web).

### 5. Rama `Api` — quinto proyecto `CitasApp.Api`
Se agregó `CitasApp.Application` (capa de servicios de solo lectura, sin
Observer) y un quinto proyecto, `CitasApp.Api`, que reutiliza Domain,
Application e Infrastructure pero cambia la "puerta de entrada": en vez de
vistas HTML devuelve JSON. Endpoints: `/api/pacientes`, `/api/medicos`,
`/api/citas`, `/api/citas/porpaciente/{id}`, y una `CalculadoraController` con
operaciones básicas (`sumar`, `restar`, `multiplicar`, `dividir`, con `400` en
división entre cero). CORS abierto para permitir clientes externos. El registro
de repos en `CitasApp.Api/Program.cs` es directo a Json — **no reutiliza el
Factory/Decorator/Observer de la práctica #26**, que sigue viviendo solo en Web.
Ver [README.md](README.md) para el detalle de endpoints.

> VERIFICAR con Fernando: si la práctica #26 (Factory/Decorator/Observer) se
> hizo en su propia rama (`GOF-Patterns`) o continuó sobre `Hexagonal-Architecture`,
> y si el plan es eventualmente llevar Factory/Decorator/Observer también a
> `CitasApp.Api`. Este archivo asume la evolución descrita arriba; ajústalo si
> el historial real difiere.

---

## Comandos frecuentes

```bash
# Compilar todo
dotnet build CitasApp.slnx

# Correr Web (Development, ve logs de Decorator/Observer en consola — NO IIS Express)
dotnet run --project CitasApp.Web          # http://localhost:5259

# Forzar Production en Web (prueba del Factory: cambia Json → Memoria)
dotnet run --project CitasApp.Web --no-launch-profile --environment Production   # http://localhost:5000

# Correr Api (REST/JSON)
dotnet run --project CitasApp.Api          # http://localhost:5183

# Instalar un paquete SOLO en un proyecto (p. ej. Infrastructure)
dotnet add CitasApp.Infrastructure package Microsoft.Data.Sqlite
```

Rutas MVC (Web): `/Paciente`, `/Medico`, `/Cita`, `/Cita/PorPaciente?pacienteId=1`.
API de confirmación (Observer, dentro de Web):
`POST /api/citas/confirmar/{citaId}` — probar con
`Invoke-RestMethod -Method POST http://localhost:5259/api/citas/confirmar/2`.

Smoke test rápido de la Api (PowerShell), ver [README.md](README.md):
```powershell
(iwr "http://localhost:5183/api/pacientes").Content
(iwr "http://localhost:5183/api/citas/porpaciente/2").Content
(iwr "http://localhost:5183/api/calculadora/sumar?a=28&b=32").Content
```

---

## Flujo de trabajo Git

- Trabajo nuevo en ramas, no directo en `main`/`master`.
- Commits atómicos por unidad lógica, en español, con prefijo tipo `refactor:`,
  `feat:`, `docs:`, `fix:`.
- Se usa la GUI de Git de Visual Studio (no terminal) para commits/push cuando es
  posible; en terminal preferir `Invoke-WebRequest`/`Invoke-RestMethod` sobre `curl`.

---

## Notas de verificación (qué prueba cada cosa)

- **Hexagonal**: cambiar el adapter en `Program.cs` sin tocar Domain/Controllers y
  que la app siga respondiendo igual.
- **Factory**: al cambiar el entorno, cambian los datos (Development=JSON,
  Production=Memoria).
- **Decorator**: aparecen logs `ObtenerTodos — inicio / N registros` en consola.
- **Observer**: un solo POST dispara varias líneas `[SMS]` y `[EMAIL]` en consola.
- **Api vs. Web**: mismo dato (p. ej. un paciente) debería verse igual desde
  `/api/pacientes/{id}` (Api) y desde las vistas de Web — si difiere, el primer
  sospechoso es que cada proyecto tiene su propia copia de `data/*.json`.
