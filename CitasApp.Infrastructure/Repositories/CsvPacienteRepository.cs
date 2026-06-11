// CitasApp.Infrastructure/Repositories/CsvPacienteRepository.cs
// Adapter de salida — implementa IPacienteRepository leyendo un archivo CSV

using Citas_App.Interfaces;
using Citas_App.Models;

namespace CitasApp.Infrastructure.Repositories
{
    public class CsvPacienteRepository : IPacienteRepository
    {
        private readonly string _filePath;

        public CsvPacienteRepository(string filePath)
        {
            _filePath = filePath;

            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "Id,Nombre,Apellido,Email,Telefono\n");
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private List<Paciente> LeerTodos()
        {
            var lista = new List<Paciente>();

            foreach (var linea in File.ReadAllLines(_filePath).Skip(1))
            {
                if (string.IsNullOrWhiteSpace(linea)) continue;
                var p = linea.Split(',');
                if (p.Length < 5) continue;

                lista.Add(new Paciente
                {
                    Id       = int.Parse(p[0]),
                    Nombre   = p[1],
                    Apellido = p[2],
                    Email    = p[3],
                    Telefono = p[4]
                });
            }

            return lista;
        }

        // ── Port ────────────────────────────────────────────────────────────────

        public List<Paciente> ObtenerTodos() => LeerTodos();

        public Paciente? ObtenerPorId(int id) =>
            LeerTodos().FirstOrDefault(p => p.Id == id);
    }
}
