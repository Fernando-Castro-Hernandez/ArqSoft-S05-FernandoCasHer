using Citas_App.Interfaces;
using Citas_App.Models;

namespace Citas_App.Observers
{
    public class EmailObserver : ICitaObserver
    {
        public void Notificar(Cita cita)
        {
            Console.WriteLine($"[EMAIL] Confirmación enviada al paciente {cita.PacienteId} — motivo: {cita.Motivo} — estado: {cita.Estado}");
        }
    }
}