# CitasApp 🏥

App de citas médicas construida con **ASP.NET Core MVC (.NET 10)**.

Proyecto de la materia **Arquitectura de Software**. Más que una app funcional,
es una maqueta pensada para *ver en código* cómo se organiza un sistema bajo el
estilo **hexagonal (Ports & Adapters)**.

---

## ¿Por qué se refactorizó?

La versión anterior era una **arquitectura por capas** dentro de un solo proyecto:
todo (controllers, vistas, repositorios y modelos) vivía junto, y la lógica de
negocio estaba mezclada con la tecnología web. El problema concreto: si mañana la
clínica pide una **app móvil** o una **API REST**, habría que duplicar la lógica,
porque la capa de presentación estaba amarrada al resto.

La refactorización separa el sistema en **tres proyectos independientes** para que
el núcleo de negocio quede aislado y la tecnología (web, JSON, SQL, móvil) sea
*intercambiable* sin tocar ese núcleo.

---

## La estructura

```
CitasApp.sln
├── CitasApp.Domain/            ← el núcleo: no depende de nadie
│   ├── Models/                 (Paciente, Medico, Cita, CitaJson)
│   └── Interfaces/             (ICitaRepository, IMedicoRepository, IPacienteRepository)  ← Ports
│
├── CitasApp.Infrastructure/    ← los Adapters de salida
│   └── Repositories/           (JsonCitaRepository, JsonMedicoRepository, JsonPacienteRepository)
│
└── CitasApp.Web/               ← el Adapter de entrada + composition root
    ├── Controllers/
    ├── Views/
    ├── data/                   (pacientes.json, medicos.json, citas.json)
    └── Program.cs              ← aquí se "enchufan" los adapters a los ports
```

---

## ¿Qué es la arquitectura hexagonal aquí?

La idea central es: **el negocio al centro, todo lo demás es intercambiable**.

El núcleo (`CitasApp.Domain`) define *qué* hace el sistema mediante entidades e
interfaces, sin saber *con qué tecnología* se hace. Todo lo externo —la web, el
almacenamiento— se conecta a ese núcleo a través de **Ports** (interfaces) que se
implementan con **Adapters** (clases concretas).

- **Port** = una interfaz que define cómo se comunica algo con el núcleo.
  En este repo, `ICitaRepository` es un *port de salida*: el núcleo dice
  "necesito algo que sepa obtener citas", sin importarle de dónde salgan.
- **Adapter** = la implementación concreta de ese port.
  `JsonCitaRepository` es el adapter actual: lee las citas de archivos JSON.
  El día que se use una base de datos real, se crea un `SqlCitaRepository`
  —mismo port, nuevo adapter— y el núcleo **no cambia ni una línea**.

---

## ¿Cómo están conectadas las partes? (las referencias)

La regla de oro de hexagonal es que **las dependencias apuntan hacia adentro**,
hacia el núcleo. Eso se traduce a esta dirección exacta entre los tres proyectos:

```
   CitasApp.Web ──────────────┐
      │  │                     │
      │  └──> CitasApp.Infrastructure
      │              │
      └──────────────┴──> CitasApp.Domain
                               │
                             (nadie)
```

- **`CitasApp.Domain`** no referencia a nadie. Es el núcleo puro.
- **`CitasApp.Infrastructure` → `Domain`**, porque sus adapters *implementan* los
  ports (interfaces) que viven en Domain.
- **`CitasApp.Web` → `Domain` + `Infrastructure`**. Domain para usar los modelos y
  las interfaces en los controllers; Infrastructure porque `Program.cs` es el
  **composition root**: el único lugar que conoce a los adapters concretos y los
  conecta a sus ports.

Esa conexión final ocurre en `Program.cs`:

```csharp
builder.Services.AddScoped<IPacienteRepository, JsonPacienteRepository>();
builder.Services.AddScoped<IMedicoRepository, JsonMedicoRepository>();
builder.Services.AddScoped<ICitaRepository, JsonCitaRepository>();
```

Cada línea dice: *"cuando alguien pida este Port, entrégale este Adapter"*. Es el
único punto del sistema donde el negocio y la tecnología se tocan.

### ¿Por qué Domain no debe referenciar a Infrastructure?

Si lo hiciera, el núcleo quedaría amarrado a la tecnología de persistencia y se
perdería justo lo que se busca: poder cambiar de JSON a SQL tocando solo
Infrastructure. La dependencia se invertiría y volveríamos al problema original.

---

## Diferencia con la arquitectura por capas anterior

Antes y ahora **comparten una idea** (separar responsabilidades), pero la diferencia
está en *qué tan estricta y física* es esa separación.

| | Arquitectura por capas (antes) | Arquitectura hexagonal (ahora) |
|---|---|---|
| **Separación** | Por carpetas dentro de **un solo proyecto** | Por **tres proyectos** independientes (.csproj) |
| **Qué impone los límites** | Disciplina del programador (nada impide saltarse capas) | El **compilador**: si Domain intentara usar algo de Infrastructure, no compila |
| **Dirección de dependencias** | De arriba hacia abajo (Presentation → … → Infrastructure) | Hacia el centro (todo apunta a Domain) |
| **El núcleo** | Conoce indirectamente la infraestructura | Aislado por completo; no sabe que existe la web ni el JSON |
| **Agregar un cliente (móvil/API)** | La capa de presentación se vuelve un cuello de botella | Se agrega un nuevo **adapter de entrada**; el núcleo no se toca |

En capas, "no saltarse capas" era una **convención** que dependía de uno. En
hexagonal, las referencias entre proyectos hacen que esa regla sea **imposible de
violar**: Domain literalmente no tiene forma de llamar a Infrastructure porque no
la referencia. Esa es la ganancia real del cambio.

---

## Persistencia

Archivos JSON en `CitasApp.Web/data/` — sin base de datos.

- `data/pacientes.json`
- `data/medicos.json`
- `data/citas.json`

Son el detalle de implementación que esconde el adapter JSON. Cambiar a una base
de datos no afectaría a Domain ni a los Controllers.

---

## Cómo correr el proyecto

```bash
dotnet build CitasApp.slnx
dotnet run --project CitasApp.Web
```

> En Visual Studio, asegúrate de que **CitasApp.Web** esté marcado como *proyecto
> de inicio* (aparece en negrita). Domain e Infrastructure son librerías y no se
> pueden ejecutar.

### Navegación

- `/Paciente` — lista de pacientes
- `/Medico` — lista de médicos
- `/Cita` — agenda completa
- `/Cita/PorPaciente?pacienteId=1` — citas de un paciente específico

---
