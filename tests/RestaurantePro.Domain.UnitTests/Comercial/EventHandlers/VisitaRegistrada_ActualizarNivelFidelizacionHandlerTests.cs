namespace RestaurantePro.Domain.UnitTests.Comercial.EventHandlers
{
    public class VisitaRegistrada_ActualizarNivelFidelizacionHandlerTests
    {
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly Mock<ITarjetaFidelizacionRepository> _tarjetaRepositoryMock;
        private readonly Mock<IClientesFrecuentesPolicy> _clientesFrecuentesPolicyMock;
        private readonly VisitaRegistrada_ActualizarNivelFidelizacionHandler _handler;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;

        public VisitaRegistrada_ActualizarNivelFidelizacionHandlerTests()
        {
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _tarjetaRepositoryMock = new Mock<ITarjetaFidelizacionRepository>();
            _clientesFrecuentesPolicyMock = new Mock<IClientesFrecuentesPolicy>();

            _handler = new VisitaRegistrada_ActualizarNivelFidelizacionHandler(
                _clienteRepositoryMock.Object,
                _tarjetaRepositoryMock.Object,
                _clientesFrecuentesPolicyMock.Object);
        }

        [Theory]
        [InlineData(10)]  // Umbral Plata
        [InlineData(20)]  // Umbral Oro
        [InlineData(30)]  // Umbral Platino
        public async Task Handle_ClienteAlcanzaUmbral_DebeEjecutarPolicy(int cantidadVisitas)
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var evento = new VisitaRegistrada(clienteId, cantidadVisitas);
            
            _clientesFrecuentesPolicyMock
                .Setup(p => p.EjecutarPolicyParaCliente(clienteId, _cancellationToken))
                .ReturnsAsync(new ResultadoClientesFrecuentesPolicy());

            // Act
            await _handler.Handle(evento, _cancellationToken);

            // Assert
            _clientesFrecuentesPolicyMock.Verify(
                p => p.EjecutarPolicyParaCliente(clienteId, _cancellationToken),
                Times.Once);
        }

        [Theory]
        [InlineData(9)]   // Debajo de umbral Plata
        [InlineData(11)]  // Encima de umbral Plata, pero no en umbral
        [InlineData(19)]  // Debajo de umbral Oro
        [InlineData(21)]  // Encima de umbral Oro, pero no en umbral
        [InlineData(29)]  // Debajo de umbral Platino
        [InlineData(31)]  // Encima de umbral Platino, pero no en umbral
        public async Task Handle_ClienteNoAlcanzaUmbral_NoDebeEjecutarPolicy(int cantidadVisitas)
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var evento = new VisitaRegistrada(clienteId, cantidadVisitas);

            // Act
            await _handler.Handle(evento, _cancellationToken);

            // Assert
            _clientesFrecuentesPolicyMock.Verify(
                p => p.EjecutarPolicyParaCliente(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
} 