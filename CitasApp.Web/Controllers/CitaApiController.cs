using Citas_App.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Citas_App.Controllers
{
    [ApiController]
    [Route("api/citas")]
    public class CitaApiController : ControllerBase
    {
        private readonly ICitaService _citaService;

        public CitaApiController(ICitaService citaService)
        {
            _citaService = citaService;
        }

        [HttpPost("confirmar/{citaId}")]
        public IActionResult Confirmar(int citaId)
        {
            var ok = _citaService.ConfirmarCita(citaId);
            if (!ok)
                return NotFound(new { mensaje = $"Cita {citaId} no encontrada" });

            return Ok(new { mensaje = $"Cita {citaId} confirmada" });
        }
    }
}
