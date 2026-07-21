using Citas_App.Interfaces;
using Citas_App.Models;
using Citas_App.Services;
using Xunit;

namespace Citas_App.Tests
{
    public class CitaServiceTests
    {
        // Repo falso en memoria: devuelve la lista que le pasemos, sin tocar archivos.
        private class RepoFake : ICitaRepository
        {
            private readonly List<Cita> _citas;
            public RepoFake(List<Cita> citas) => _citas = citas;
            public List<Cita> ObtenerTodos() => _citas;
            public List<Cita> ObtenerPorPaciente(int pacienteId) =>
                _citas.Where(c => c.PacienteId == pacienteId).ToList();
        }

        private class ObserverEspia : ICitaObserver
        {
            public int Llamadas { get; private set; }
            public void Notificar(Cita cita) => Llamadas++;
        }

        [Fact]
        public void ConfirmarCita_CuandoExiste_CambiaEstadoYNotifica()
        {
            // Arrange
            var cita = new Cita { Id = 2, Estado = "Pendiente" };
            var repo = new RepoFake(new List<Cita> { cita });
            var espia = new ObserverEspia();
            var notificador = new CitaNotificador(new ICitaObserver[] { espia });
            var service = new CitaService(repo, notificador);

            // Act
            var resultado = service.ConfirmarCita(2);

            // Assert
            Assert.True(resultado);
            Assert.Equal("Confirmada", cita.Estado);
            Assert.Equal(1, espia.Llamadas);
        }

        [Fact]
        public void ConfirmarCita_CuandoNoExiste_RegresaFalseYNoNotifica()
        {
            // Arrange
            var repo = new RepoFake(new List<Cita>());   // vacío
            var espia = new ObserverEspia();
            var notificador = new CitaNotificador(new ICitaObserver[] { espia });
            var service = new CitaService(repo, notificador);

            // Act
            var resultado = service.ConfirmarCita(99);

            // Assert
            Assert.False(resultado);
            Assert.Equal(0, espia.Llamadas);
        }
    }
}
