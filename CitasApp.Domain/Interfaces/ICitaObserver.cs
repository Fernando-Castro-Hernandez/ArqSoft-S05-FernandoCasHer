using Citas_App.Models;

namespace Citas_App.Interfaces
{
    // OBSERVER — el contrato que cumple cualquier notificador de citas
    public interface ICitaObserver
    {
        void Notificar(Cita cita);
    }
}