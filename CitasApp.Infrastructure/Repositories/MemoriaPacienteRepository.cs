using Citas_App.Interfaces;
using Citas_App.Models;

namespace Citas_App.Repositories
{
    public class MemoriaPacienteRepository : IPacienteRepository
    {
        private readonly List<Paciente> _pacientes = new()
        {
            new Paciente { Id = 1, Nombre = "Production", Apellido = "Uno",  Email = "prod1@mail.com", Telefono = "999-0001" },
            new Paciente { Id = 2, Nombre = "Production", Apellido = "Dos",  Email = "prod2@mail.com", Telefono = "999-0002" },
            new Paciente { Id = 3, Nombre = "Production", Apellido = "Tres", Email = "prod3@mail.com", Telefono = "999-0003" }
        };

        public List<Paciente> ObtenerTodos() => _pacientes;

        public Paciente? ObtenerPorId(int id) =>
            _pacientes.FirstOrDefault(p => p.Id == id);
    }
}