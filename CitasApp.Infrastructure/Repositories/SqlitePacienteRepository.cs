// CitasApp.Infrastructure/Repositories/SqlitePacienteRepository.cs
// Adapter de salida — implementa IPacienteRepository usando SQLite
//
// Comparte el mismo archivo .db que SqliteCitaRepository.
// Pasa la misma ruta dbPath desde Program.cs.

using Citas_App.Interfaces;
using Citas_App.Models;
using Microsoft.Data.Sqlite;

namespace CitasApp.Infrastructure.Repositories
{
    public class SqlitePacienteRepository : IPacienteRepository
    {
        private readonly string _connectionString;

        public SqlitePacienteRepository(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
            InicializarTabla();
        }

        private void InicializarTabla()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Pacientes (
                    Id       INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre   TEXT NOT NULL,
                    Apellido TEXT NOT NULL,
                    Email    TEXT,
                    Telefono TEXT
                );";
            cmd.ExecuteNonQuery();
        }

        private static Paciente LeerFila(SqliteDataReader r) => new Paciente
        {
            Id       = r.GetInt32(0),
            Nombre   = r.GetString(1),
            Apellido = r.GetString(2),
            Email    = r.IsDBNull(3) ? string.Empty : r.GetString(3),
            Telefono = r.IsDBNull(4) ? string.Empty : r.GetString(4)
        };

        public List<Paciente> ObtenerTodos()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText =
                "SELECT Id, Nombre, Apellido, Email, Telefono FROM Pacientes;";

            var lista = new List<Paciente>();
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(LeerFila(r));
            return lista;
        }

        public Paciente? ObtenerPorId(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText =
                "SELECT Id, Nombre, Apellido, Email, Telefono " +
                "FROM Pacientes WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id);

            using var r = cmd.ExecuteReader();
            return r.Read() ? LeerFila(r) : null;
        }
    }
}
