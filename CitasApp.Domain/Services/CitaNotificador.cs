using Citas_App.Interfaces;
using Citas_App.Models;

namespace Citas_App.Services
{
    // EXTRACT CLASS — la responsabilidad de notificar a los observers
    // se saca de CitaService y vive aquí, en su propia clase.
    public class CitaNotificador
    {
        private readonly IEnumerable<ICitaObserver> _observers;

        public CitaNotificador(IEnumerable<ICitaObserver> observers)
        {
            _observers = observers;
        }

        // Avisa a todos los suscriptores sin saber quiénes son.
        public void Notificar(Cita cita)
        {
            foreach (var observer in _observers)
                observer.Notificar(cita);
        }
    }
}

