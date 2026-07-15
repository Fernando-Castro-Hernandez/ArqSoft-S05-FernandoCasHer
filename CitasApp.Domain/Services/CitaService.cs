using Citas_App.Interfaces;

namespace Citas_App.Services
{
    public class CitaService : ICitaService
    {
        private readonly ICitaRepository _citaRepo;
        private readonly CitaNotificador _notificador;

        // Recibe el repo y el notificador (que a su vez conoce a los observers).
        // Solo conoce abstracciones de Domain — nunca importa Infrastructure.
        public CitaService(ICitaRepository citaRepo, CitaNotificador notificador)
        {
            _citaRepo = citaRepo;
            _notificador = notificador;
        }

        public bool ConfirmarCita(int citaId)
        {
            var cita = _citaRepo.ObtenerTodos().FirstOrDefault(c => c.Id == citaId);
            if (cita is null) return false;

            cita.Estado = "Confirmada";

            // Delega la notificación a la clase extraída
            _notificador.Notificar(cita);

            return true;
        }
    }
}

