// CitasApp.Infrastructure/Repositories/SqliteCitaRepository.cs
// Adapter de salida — implementa ICitaRepository usando SQLite
//
// ANTES DE USAR:
//   En CitasApp.Infrastructure ejecuta:
//   dotnet add package Microsoft.Data.Sqlite

using Microsoft.Data.Sqlite;
using Citas_App.Interfaces;
using Citas_App.Models;

namespace CitasApp.Infrastructure.Repositories
{
    public class SqliteCitaRepository : ICitaRepository
    {
        private readonly string _connectionString;

        public SqliteCitaRepository(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
            InicializarTabla();
        }

        private void InicializarTabla()
        {
            using var conn = Conectar();
            Ejecutar(conn, @"
                CREATE TABLE IF NOT EXISTS Citas (
                    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
                    PacienteId INTEGER NOT NULL,
                    MedicoId   INTEGER NOT NULL,
                    Fecha      TEXT    NOT NULL,   -- yyyy-MM-dd
                    Hora       TEXT    NOT NULL,   -- HH:mm
                    Motivo     TEXT,
                    Estado     TEXT    NOT NULL DEFAULT 'Pendiente'
                );");
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private SqliteConnection Conectar()
        {
            var conn = new SqliteConnection(_connectionString);
            conn.Open();
            return conn;
        }

        private static void Ejecutar(SqliteConnection conn, string sql)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        private static Cita LeerFila(SqliteDataReader r) => new Cita
        {
            Id         = r.GetInt32(0),
            PacienteId = r.GetInt32(1),
            MedicoId   = r.GetInt32(2),
            Fecha      = DateOnly.ParseExact(r.GetString(3), "yyyy-MM-dd"),
            Hora       = TimeOnly.ParseExact(r.GetString(4), "HH:mm"),
            Motivo     = r.IsDBNull(5) ? string.Empty : r.GetString(5),
            Estado     = r.GetString(6)
        };

        // ── Port ────────────────────────────────────────────────────────────────

        public List<Cita> ObtenerTodos()
        {
            using var conn = Conectar();
            var cmd = conn.CreateCommand();
            cmd.CommandText =
                "SELECT Id, PacienteId, MedicoId, Fecha, Hora, Motivo, Estado FROM Citas;";

            var lista = new List<Cita>();
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(LeerFila(r));
            return lista;
        }

        public Cita? ObtenerPorId(int id)
        {
            using var conn = Conectar();
            var cmd = conn.CreateCommand();
            cmd.CommandText =
                "SELECT Id, PacienteId, MedicoId, Fecha, Hora, Motivo, Estado " +
                "FROM Citas WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id);

            using var r = cmd.ExecuteReader();
            return r.Read() ? LeerFila(r) : null;
        }

        public List<Cita> ObtenerPorPaciente(int pacienteId)
        {
            using var conn = Conectar();
            var cmd = conn.CreateCommand();
            cmd.CommandText =
                "SELECT Id, PacienteId, MedicoId, Fecha, Hora, Motivo, Estado " +
                "FROM Citas WHERE PacienteId = $pid;";
            cmd.Parameters.AddWithValue("$pid", pacienteId);

            var lista = new List<Cita>();
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(LeerFila(r));
            return lista;
        }

            }
}
