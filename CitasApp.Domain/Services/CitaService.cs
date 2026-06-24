using Citas_App.Interfaces;
using Citas_App.Models;

namespace Citas_App.Services
{
    public class CitaService
    {
        private readonly ICitaRepository _citaRepo;
        private readonly IEnumerable<ICitaObserver> _observers;

        // Recibe el repo y TODOS los observers registrados.
        // Solo conoce abstracciones de Domain — nunca importa Infrastructure.
        public CitaService(ICitaRepository citaRepo, IEnumerable<ICitaObserver> observers)
        {
            _citaRepo = citaRepo;
            _observers = observers;
        }

        public bool ConfirmarCita(int citaId)
        {
            var cita = _citaRepo.ObtenerTodos().FirstOrDefault(c => c.Id == citaId);
            if (cita is null) return false;

            cita.Estado = "Confirmada";

            // Avisa a todos los suscriptores sin saber quiénes son
            foreach (var observer in _observers)
                observer.Notificar(cita);

            return true;
        }
    }
}