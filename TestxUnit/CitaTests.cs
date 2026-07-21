using Citas_App.Models;
using Xunit;

namespace Citas_App.Tests
{
    public class CitaTests
    {
        [Fact]
        public void Cita_Nueva_TieneEstadoPendientePorDefecto()
        {
            // Arrange & Act
            var cita = new Cita();

            // Assert
            Assert.Equal("Pendiente", cita.Estado);
        }

        [Fact]
        public void Cita_GuardaLosDatosAsignados()
        {
            // Arrange & Act
            var cita = new Cita
            {
                Id = 1,
                PacienteId = 5,
                MedicoId = 2,
                Fecha = new DateOnly(2026, 7, 20),
                Hora = new TimeOnly(10, 0),
                Motivo = "Consulta"
            };

            // Assert
            Assert.Equal(5, cita.PacienteId);
            Assert.Equal("Consulta", cita.Motivo);
        }
    }
}
