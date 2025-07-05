namespace RestaurantePro.Domain.UnitTests.Comercial.Clientes.Events.Cliente
{
    public class PuntosFidelizacionCanjeadosTests
    {
        [Fact]
        public void Constructor_DatosValidos_DebeInicializarPropiedadesCorrectamente()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var cantidad = 50;
            var puntosRestantes = 100;
            var motivo = "Descuento en próxima visita";

            // Act
            var evento = new PuntosFidelizacionCanjeados(clienteId, cantidad, puntosRestantes, motivo);

            // Assert
            evento.Should().NotBeNull();
            evento.ClienteId.Should().Be(clienteId);
            evento.Cantidad.Should().Be(cantidad);
            evento.PuntosRestantes.Should().Be(puntosRestantes);
            evento.Motivo.Should().Be(motivo);
            evento.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
        
        [Fact]
        public void NombreEvento_DebeSerCorrectoYSinSufijoEvent()
        {
            // Arrange & Act
            var evento = new PuntosFidelizacionCanjeados(Guid.NewGuid(), 10, 90, "Test");
            var nombreEvento = evento.GetType().Name;
            
            // Assert
            nombreEvento.Should().Be("PuntosFidelizacionCanjeados");
            nombreEvento.Should().NotEndWith("Event");
        }
    }
} 