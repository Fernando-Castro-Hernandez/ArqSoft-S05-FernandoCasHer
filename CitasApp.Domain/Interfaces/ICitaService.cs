using Citas_App.Models;

namespace Citas_App.Interfaces
{
    // Contrato del servicio de citas — el controller depende de esta abstracción,
    // no de la clase concreta CitaService (invierte la dependencia).
    public interface ICitaService
    {
        bool ConfirmarCita(int citaId);
    }
}

