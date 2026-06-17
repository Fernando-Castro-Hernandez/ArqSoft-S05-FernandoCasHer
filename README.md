# CitasApp — Rama `Api`
 
Sistema de gestión de citas médicas para una clínica. Esta rama agrega una **API REST** sobre la arquitectura por capas existente, exponiendo la lógica de la aplicación por HTTP + JSON para que cualquier cliente (app móvil, Postman, otro servicio) pueda consumirla, no solo el navegador.
 
## ¿Qué agrega esta rama?
 
Hasta la rama anterior, el único cliente de la lógica de negocio era `CitasApp.Web` (MVC, para navegador). Esta rama añade un **quinto proyecto, `CitasApp.Api`**, que reutiliza exactamente las mismas capas de Domain, Application e Infrastructure y solo cambia la "puerta de entrada": en lugar de devolver vistas HTML, devuelve JSON a través de endpoints HTTP.
 
```
Navegador ─────► CitasApp.Web  ─┐
                                 ├─► Application ─► Domain ◄─ Infrastructure
Móvil/Postman ─► CitasApp.Api  ─┘
```
 
Mismo `Application`, mismo `Domain`, misma `Infrastructure`. Solo un cliente nuevo.
 
## Arquitectura
 
El proyecto sigue una separación en capas con dependencias dirigidas hacia el dominio:
 
| Capa | Proyecto | Responsabilidad |
|------|----------|-----------------|
| Dominio | `CitasApp.Domain` | Modelos (`Paciente`, `Medico`, `Cita`) e interfaces de repositorio (puertos). |
| Aplicación | `CitasApp.Application` | Servicios (`PacienteService`, `MedicoService`, `CitaService`) que orquestan la lógica. Dependen solo del dominio. |
| Infraestructura | `CitasApp.Infrastructure` | Implementaciones concretas de los repositorios (adapters JSON) que leen los datos. |
| Presentación (web) | `CitasApp.Web` | Cliente MVC para navegador. |
| Presentación (API) | `CitasApp.Api` | Cliente REST. Controllers que exponen los servicios como endpoints HTTP. |
 
**Flujo de una petición:**
 
```
GET /api/pacientes
  → PacientesController          (recibe la petición HTTP)
  → PacienteService              (lógica de aplicación)
  → IPacienteRepository          (puerto / interfaz)
  → JsonPacienteRepository       (adapter que lee pacientes.json)
  → JSON de respuesta
```
 
Los controllers **no tienen lógica de negocio**: solo reciben la petición, llaman al servicio correspondiente y traducen el resultado a un código HTTP (`200 OK`, `404 Not Found`, `400 Bad Request`). El cableado entre interfaz e implementación se hace por **inyección de dependencias** en `Program.cs`, así que cambiar el origen de datos (de JSON a una base de datos) no obliga a tocar los servicios ni los controllers — solo se registra otro adapter.
 
## Estructura del proyecto API
 
```
CitasApp.Api/
├── Controllers/
│   ├── PacientesController.cs
│   ├── MedicosController.cs
│   ├── CitasController.cs
│   └── CalculadoraController.cs
├── data/                  ← pacientes.json, medicos.json, citas.json (Copy Always)
└── Program.cs             ← registro de repositorios y servicios (DI)
```
 
## Endpoints
 
Pacientes, médicos y citas (sustituye `{puerto}` por el puerto http):
 
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/pacientes` | Lista de pacientes |
| GET | `/api/pacientes/{id}` | Un paciente por id (`404` si no existe) |
| GET | `/api/medicos` | Lista de médicos |
| GET | `/api/medicos/{id}` | Un médico por id (`404` si no existe) |
| GET | `/api/citas` | Agenda completa |
| GET | `/api/citas/porpaciente/{pacienteId}` | Citas de un paciente (`404` si no tiene) |
 
Calculadora (parámetros por query string `?a=&b=`):
 
| Método | Ruta | Ejemplo de respuesta |
|--------|------|----------------------|
| GET | `/api/calculadora/sumar?a=28&b=32` | `{"operacion":"suma","a":28,"b":32,"resultado":60}` |
| GET | `/api/calculadora/restar?a=28&b=32` | `{"operacion":"resta",...,"resultado":-4}` |
| GET | `/api/calculadora/multiplicar?a=28&b=32` | `{"operacion":"multiplicacion",...,"resultado":896}` |
| GET | `/api/calculadora/dividir?a=28&b=32` | `{"operacion":"division",...,"resultado":0.875}` |
 
La división entre cero responde `400 Bad Request` con un mensaje de error.
 
### Verificación rápida (PowerShell)
 
```powershell
(iwr "http://localhost:5183/api/pacientes").Content
(iwr "http://localhost:5183/api/citas/porpaciente/2").Content
(iwr "http://localhost:5183/api/calculadora/sumar?a=28&b=32").Content
```
 
## Persistencia
 
Actualmente los datos viven en archivos JSON dentro de `CitasApp.Api/data/`, leídos por los repositorios `Json*Repository`. Los archivos deben tener **Copy to Output Directory = Copy Always** para que se copien al directorio de salida; si la API responde `[]`, normalmente es porque esos archivos no se copiaron.
 
Esta decisión de persistencia es temporal. Al desplegar en producción, los archivos locales no escalan (varias instancias tendrían datos distintos), por lo que el siguiente paso es sustituir el adapter JSON por uno conectado a una base de datos — sin modificar los servicios, gracias a la separación por capas.
 
## Stack
 
- ASP.NET Core 10 (Web API)
- C# / .NET 10
- Persistencia en JSON (vía repositorios intercambiables)
- Solución en formato `.slnx`
