# CitasApp — Rama `UML`
 
Sistema de gestión de citas médicas para una clínica. Esta rama agrega una **API REST** sobre la arquitectura por capas existente, exponiendo la lógica de la aplicación por HTTP + JSON para que cualquier cliente (app móvil, Postman, otro servicio) pueda consumirla, no solo el navegador.

## Documentación de arquitectura — Diagramas C4 (rama `UML`)

La rama `UML` se creó a partir de `GOF-Patterns` (la rama más actualizada del
proyecto).  Su único propósito es
agregar documentación de arquitectura **versionada como código**: tres
diagramas del modelo C4 (Contexto, Contenedores, Componentes) escritos en
Mermaid dentro de `/docs`, que reflejan el estado real de CitasApp — no un
diagrama genérico.

**Por qué:** un diagrama dibujado en Paint hace un mes deja de coincidir con
el código en cuanto el proyecto crece — vive en el escritorio de alguien, no
en el repositorio, y nadie sabe si sigue vigente. Estos diagramas están en
texto plano dentro del repo: se versionan en el mismo commit que el código,
se revisan en Code Review como cualquier otro archivo.

**Para quién:** cada nivel tiene una audiencia distinta —

- **C1 — Contexto**: cualquier persona, técnica o no (cliente, maestro, un
  compañero nuevo el primer día).
- **C2 — Contenedores**: el equipo técnico, para entender de qué piezas
  grandes está hecho el sistema.
- **C3 — Componentes**: quien va a modificar código dentro de
  `CitasApp.Web`, donde viven los patrones GOF (Factory, Decorator, Observer)
  de la práctica #26.

| Nivel | Archivo | Responde |
|---|---|---|
| C1 — Contexto | [docs/C1-Contexto.md](docs/C1-Contexto.md) | ¿Qué es el sistema y quién lo usa? |
| C2 — Contenedores | [docs/C2-Contenedores.md](docs/C2-Contenedores.md) | ¿De qué piezas técnicas grandes se compone? |
| C3 — Componentes | [docs/C3-Componentes.md](docs/C3-Componentes.md) | ¿Qué hay dentro de `CitasApp.Web`? |


## Arquitectura
 
El proyecto sigue una separación en capas con dependencias dirigidas hacia el dominio:
 
| Capa | Proyecto | Responsabilidad |
|------|----------|-----------------|
| Dominio | `CitasApp.Domain` | Modelos (`Paciente`, `Medico`, `Cita`) e interfaces de repositorio (puertos). |
| Aplicación | `CitasApp.Application` | Servicios (`PacienteService`, `MedicoService`, `CitaService`) que orquestan la lógica. Dependen solo del dominio. |
| Infraestructura | `CitasApp.Infrastructure` | Implementaciones concretas de los repositorios (adapters JSON) que leen los datos. |
| Presentación (web) | `CitasApp.Web` | Cliente MVC para navegador. |
| Presentación (API) | `CitasApp.Api` | Cliente REST. Controllers que exponen los servicios como endpoints HTTP. |
 

## Stack
 
- ASP.NET Core 10 (Web API)
- C# / .NET 10
- Persistencia en JSON (vía repositorios intercambiables)
- Solución en formato `.slnx`
