// CitasApp.Infrastructure/Repositories/SqliteMedicoRepository.cs
// Adapter de salida — implementa IMedicoRepository usando SQLite

using Citas_App.Interfaces;
using Citas_App.Models;
using Microsoft.Data.Sqlite;

namespace CitasApp.Infrastructure.Repositories
{
    public class SqliteMedicoRepository : IMedicoRepository
    {
        private readonly string _connectionString;

        public SqliteMedicoRepository(string dbPath)
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
                CREATE TABLE IF NOT EXISTS Medicos (
                    Id             INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre         TEXT NOT NULL,
                    Apellido       TEXT NOT NULL,
                    Especialidad   TEXT,
                    NumeroLicencia TEXT
                );";
            cmd.ExecuteNonQuery();
        }

        private static Medico LeerFila(SqliteDataReader r) => new Medico
        {
            Id             = r.GetInt32(0),
            Nombre         = r.GetString(1),
            Apellido       = r.GetString(2),
            Especialidad   = r.IsDBNull(3) ? string.Empty : r.GetString(3),
            NumeroLicencia = r.IsDBNull(4) ? string.Empty : r.GetString(4)
        };

        public List<Medico> ObtenerTodos()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText =
                "SELECT Id, Nombre, Apellido, Especialidad, NumeroLicencia FROM Medicos;";

            var lista = new List<Medico>();
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(LeerFila(r));
            return lista;
        }

        public Medico? ObtenerPorId(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText =
                "SELECT Id, Nombre, Apellido, Especialidad, NumeroLicencia " +
                "FROM Medicos WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id);

            using var r = cmd.ExecuteReader();
            return r.Read() ? LeerFila(r) : null;
        }
    }
}
