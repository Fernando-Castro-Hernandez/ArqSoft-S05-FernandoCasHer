using Citas_App.Interfaces;
using Citas_App.Models;

namespace Citas_App.Application.Services
{
    public class MedicoService
    {
        private readonly IMedicoRepository _repo;

        public MedicoService(IMedicoRepository repo)
        {
            _repo = repo;
        }

        public List<Medico> ObtenerTodos()
        {
            return _repo.ObtenerTodos();
        }

        public Medico? ObtenerPorId(int id)
        {
            return _repo.ObtenerPorId(id);
        }
    }
}
