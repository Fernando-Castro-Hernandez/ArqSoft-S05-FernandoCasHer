using Citas_App.Interfaces;
using Citas_App.Models;
using Citas_App.Services;
using Xunit;

namespace Citas_App.Tests
{
    public class CitaNotificadorTests
    {
        // Test double (espía): un observer falso que cuenta cuántas veces lo llamaron.
        private class ObserverEspia : ICitaObserver
        {
            public int Llamadas { get; private set; }
            public Cita? UltimaCita { get; private set; }

            public void Notificar(Cita cita)
            {
                Llamadas++;
                UltimaCita = cita;
            }
        }

        [Fact]
        public void Notificar_AvisaATodosLosObservers()
        {
            // Arrange
            var obs1 = new ObserverEspia();
            var obs2 = new ObserverEspia();
            var notificador = new CitaNotificador(new ICitaObserver[] { obs1, obs2 });
            var cita = new Cita { Id = 1 };

            // Act
            notificador.Notificar(cita);

            // Assert — los dos observers recibieron la MISMA cita, una vez cada uno
            Assert.Equal(1, obs1.Llamadas);
            Assert.Equal(1, obs2.Llamadas);
            Assert.Same(cita, obs1.UltimaCita);
        }
    }
}
