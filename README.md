# Refactorización: Detección de Code Smells

Rama: `CodeSmells`

Esta entrega identifica **code smells** en CitasApp y los corrige aplicando técnicas de
refactorización, sin cambiar el comportamiento observable del sistema. La refactorización
consiste en mejorar la **estructura interna** del código manteniendo intacto lo que hace:
el endpoint de confirmación de citas sigue funcionando igual y los observers SMS/EMAIL
siguen notificando en consola, antes y después.

---

## Code smells identificados

Se identificaron **2 code smells**, y se refactorizaron **ambos**:

| # | Code smell | Dónde | Técnica aplicada |
|---|---|---|---|
| 1 | **Tight Coupling** (acoplamiento fuerte) | `CitaApiController` dependía de la clase concreta `CitaService` | **Dependency Injection** (interfaz `ICitaService`) |
| 2 | **Responsabilidad mezclada** (God Class en pequeño) | `CitaService.ConfirmarCita` confirmaba la cita **y** notificaba a los observers | **Extract Class** (`CitaNotificador`) |

---

## Refactor 1 — Tight Coupling → Dependency Injection

### El smell

`CitaApiController` recibía por constructor la **clase concreta** `CitaService`:

```csharp
private readonly CitaService _citaService;          // ← depende de una clase concreta

public CitaApiController(CitaService citaService)
{
    _citaService = citaService;
}
```

Aunque ya usaba inyección por constructor, dependía del **tipo concreto**, no de una
abstracción. Eso amarra el controller a esa implementación específica: no se puede
sustituir por otra implementación ni mockear para pruebas sin tocar el controller.

### La solución

Se creó la interfaz `ICitaService` (en `CitasApp.Domain/Interfaces/`) y el controller
pasó a depender de ella:

```csharp
// ICitaService.cs — nuevo contrato en Domain
public interface ICitaService
{
    bool ConfirmarCita(int citaId);
}
```

```csharp
// CitaApiController.cs — ahora depende de la abstracción
private readonly ICitaService _citaService;

public CitaApiController(ICitaService citaService)
{
    _citaService = citaService;
}
```

`CitaService` implementa esa interfaz (`public class CitaService : ICitaService`), y en
`Program.cs` se registra bajo el contrato:

```csharp
builder.Services.AddScoped<ICitaService, CitaService>();
```

### Por qué

Esto invierte la dependencia: el controller ya no conoce la clase concreta, solo el
contrato. Mañana se puede cambiar la implementación de `CitaService` (o inyectar un mock
en pruebas) sin modificar el controller. Es exactamente la cura al Tight Coupling que
plantea la teoría de la semana: *depender de una interfaz, no de una clase concreta*.

---

## Refactor 2 — Responsabilidad mezclada → Extract Class

### El smell

El método `CitaService.ConfirmarCita` hacía **dos cosas distintas**: (1) buscaba la cita
y cambiaba su estado, y (2) recorría los observers y los notificaba uno por uno.

```csharp
public bool ConfirmarCita(int citaId)
{
    var cita = _citaRepo.ObtenerTodos().FirstOrDefault(c => c.Id == citaId);
    if (cita is null) return false;

    cita.Estado = "Confirmada";

    // Responsabilidad 2: recorrer y notificar observers (mezclada aquí)
    foreach (var observer in _observers)
        observer.Notificar(cita);

    return true;
}
```

`CitaService` cargaba con la responsabilidad de negocio (confirmar) **y** con la de
orquestar la notificación.

### La solución

Se extrajo la responsabilidad de notificar a una clase propia, `CitaNotificador`
(en `CitasApp.Domain/Services/`):

```csharp
// CitaNotificador.cs — nueva clase, una sola responsabilidad
public class CitaNotificador
{
    private readonly IEnumerable<ICitaObserver> _observers;

    public CitaNotificador(IEnumerable<ICitaObserver> observers)
    {
        _observers = observers;
    }

    public void Notificar(Cita cita)
    {
        foreach (var observer in _observers)
            observer.Notificar(cita);
    }
}
```

`CitaService` ahora recibe el `CitaNotificador` y le **delega** la notificación:

```csharp
private readonly ICitaRepository _citaRepo;
private readonly CitaNotificador _notificador;

public bool ConfirmarCita(int citaId)
{
    var cita = _citaRepo.ObtenerTodos().FirstOrDefault(c => c.Id == citaId);
    if (cita is null) return false;

    cita.Estado = "Confirmada";
    _notificador.Notificar(cita);   // delega la notificación

    return true;
}
```

En `Program.cs` se registra la clase extraída:

```csharp
builder.Services.AddScoped<CitaNotificador>();
```

### Por qué

Cada clase queda con **una sola responsabilidad**: `CitaService` confirma la cita,
`CitaNotificador` notifica. El código es más fácil de leer, probar y modificar: si mañana
cambia *cómo* se notifica, se toca `CitaNotificador` sin arriesgar la lógica de
confirmación. El patrón Observer de la práctica anterior se conserva intacto (los
observers siguen inyectándose como `IEnumerable<ICitaObserver>`), solo se movió a una
clase dedicada.

---

## Archivos afectados

| Acción | Archivo |
|---|---|
| ➕ Nuevo | `CitasApp.Domain/Interfaces/ICitaService.cs` |
| ➕ Nuevo | `CitasApp.Domain/Services/CitaNotificador.cs` |
| ✏️ Modificado | `CitasApp.Domain/Services/CitaService.cs` |
| ✏️ Modificado | `CitasApp.Web/Controllers/CitaApiController.cs` |
| ✏️ Modificado | `CitasApp.Web/Program.cs` |

---

## Verificación — el comportamiento no cambió

- El proyecto **compila sin errores**: `dotnet build CitasApp.slnx`.
- El endpoint de confirmación sigue respondiendo igual:
  `POST /api/citas/confirmar/{citaId}` devuelve `"Cita {id} confirmada"`.
- Los observers **SMS** y **EMAIL** siguen imprimiendo en consola al confirmar una cita.

El diff entre el commit *antes* y el commit *después* muestra únicamente la
refactorización descrita, sin cambios de comportamiento — cumpliendo el requisito de la
actividad: *el comportamiento del sistema debe ser idéntico al de antes*.

---

## Técnicas de refactorización aplicadas (referencia de la semana)

- **Dependency Injection** — depender de una interfaz (`ICitaService`) en vez de una
  clase concreta, resolviendo el Tight Coupling.
- **Extract Class** — sacar una responsabilidad completa (`CitaNotificador`) de una clase
  que hacía de más, dejándola enfocada en una sola tarea.
