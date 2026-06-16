using Citas_App.Interfaces;
using Citas_App.Models;

namespace Citas_App.Application.Services
{
    public class PacienteService
    {
        private readonly IPacienteRepository _repo;

        public PacienteService(IPacienteRepository repo)
        {
            _repo = repo;
        }

        public List<Paciente> ObtenerTodos()
        {
            return _repo.ObtenerTodos();
        }

        public Paciente? ObtenerPorId(int id)
        {
            return _repo.ObtenerPorId(id);
        }
    }
}
