using Citas_App.Interfaces;
using Citas_App.Models;

namespace Citas_App.Application.Services
{
    public class CitaService
    {
        private readonly ICitaRepository _repo;

        public CitaService(ICitaRepository repo)
        {
            _repo = repo;
        }

        public List<Cita> ObtenerTodos()
        {
            return _repo.ObtenerTodos();
        }

        public List<Cita> ObtenerPorPaciente(int pacienteId)
        {
            return _repo.ObtenerPorPaciente(pacienteId);
        }
    }
}
