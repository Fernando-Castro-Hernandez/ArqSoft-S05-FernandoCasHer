using Citas_App.Interfaces;
using Citas_App.Models;

namespace Citas_App.Observers
{
    public class SmsObserver : ICitaObserver
    {
        public void Notificar(Cita cita)
        {
            Console.WriteLine($"[SMS] Recordatorio enviado al paciente {cita.PacienteId} — cita el {cita.Fecha:dd/MM/yyyy} a las {cita.Hora:HH:mm} — estado: {cita.Estado}");
        }
    }
}