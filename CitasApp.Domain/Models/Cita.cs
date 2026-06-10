namespace Citas_App.Models
{

    public class Cita
    {
        public int Id { get; set; }
        public int PacienteId { get; set; }
        public int MedicoId { get; set; }
        public DateOnly Fecha { get; set; }
        public TimeOnly Hora { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string Estado { get; set; } = "Pendiente";
    }


}
