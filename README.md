# CitasApp 🏥
 
App de citas médicas construida con ASP.NET Core MVC (.NET 10).
 
Proyecto de la materia **Arquitectura de Software**. Más que una app funcional,
es una maqueta pensada para *ver en código* los conceptos de arquitectura en
capas, abstracción por interfaces e inyección de dependencias.
 
## Entidades
 
- **Paciente** — lista y detalle de pacientes registrados
- **Médico** — lista y detalle de médicos disponibles
- **Cita** — agenda completa y filtro por paciente
## Persistencia
 
Archivos JSON en `data/` — sin base de datos.
 
- `data/pacientes.json`
- `data/medicos.json`
- `data/citas.json`
## Arquitectura
 
Repositorios por interfaz con inyección de dependencias.
 
- `Interfaces/` — contratos (`IPacienteRepository`, `IMedicoRepository`, `ICitaRepository`)
- `Repositories/` — implementaciones JSON
- `Models/` — entidades + `CitaJson` como DTO de serialización
- `Controllers/` — reciben los repositorios por constructor y pasan datos a las vistas
- `Views/` — Razor + Bootstrap para la capa de presentación
## Navegación
 
- `/Paciente` — lista de pacientes
- `/Medico` — lista de médicos
- `/Cita` — agenda completa
- `/Cita/PorPaciente?pacienteId=1` — citas de un paciente específico

 
## Conceptos
 
Esta es la parte importante para la materia: cada decisión del proyecto está
puesta a propósito para que se note *por qué* la arquitectura se diseña así.
 
### Separación en capas (responsabilidad única)
 
Cada carpeta tiene **un solo trabajo**:
 
| Capa | Carpeta | Su única responsabilidad |
|---|---|---|
| Presentación | `Views/` | Mostrar datos al usuario |
| Coordinación | `Controllers/` | Recibir el request y decidir qué responder |
| Acceso a datos | `Repositories/` | Leer y escribir la información |
| Dominio | `Models/` | Representar las entidades del negocio |
 
La idea que se aprende: si un cambio toca solo una capa, las demás no se enteran.
Por ejemplo, cambiar de JSON a una base de datos se hace **solo** en `Repositories/`.
 
### Programar contra interfaces, no contra implementaciones
 
El `PacienteController` no conoce a `JsonPacienteRepository`; solo conoce el
contrato `IPacienteRepository`. Esto es **abstracción**: el controlador pide
"algo que sepa obtener pacientes" sin importarle de dónde salgan (JSON, SQL,
una API externa). Esa indiferencia es lo que hace al sistema flexible.
 
