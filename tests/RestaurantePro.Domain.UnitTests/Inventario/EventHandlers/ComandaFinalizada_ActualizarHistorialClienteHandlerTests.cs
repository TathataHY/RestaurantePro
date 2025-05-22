namespace RestaurantePro.Domain.UnitTests.Inventario.EventHandlers
{
    public class ComandaFinalizada_ActualizarHistorialClienteHandlerTests
    {
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly Mock<IComandaRepository> _comandaRepositoryMock;
        private readonly ComandaFinalizada_ActualizarHistorialClienteHandler _handler;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;

        public ComandaFinalizada_ActualizarHistorialClienteHandlerTests()
        {
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _comandaRepositoryMock = new Mock<IComandaRepository>();

            _handler = new ComandaFinalizada_ActualizarHistorialClienteHandler(
                _clienteRepositoryMock.Object,
                _comandaRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ComandaConCliente_DebeRegistrarVisita()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            
            // Crear evento ComandaFinalizada
            var evento = new ComandaFinalizada(comandaId, 100.0m);
            
            // Crear comanda con cliente asociado
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid());
            
            // Asignar el ID de la comanda manualmente para el test
            var idField = typeof(EntityBase).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance);
            idField?.SetValue(comanda, comandaId);
            
            // Configurar propiedad ClienteId en la comanda utilizando reflexión
            var clienteIdProperty = typeof(Comanda).GetProperty("ClienteId");
            clienteIdProperty?.SetValue(comanda, clienteId);
            
            // Configurar mock del repositorio de comandas
            _comandaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(comandaId, _cancellationToken))
                .ReturnsAsync(comanda);
                
            // Crear cliente para el test
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            
            // Asignar ID al cliente manualmente
            idField?.SetValue(cliente, clienteId);
            
            // Configurar mock del repositorio de clientes
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, _cancellationToken))
                .ReturnsAsync(cliente);
                
            _clienteRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Cliente>(), _cancellationToken))
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(evento, _cancellationToken);

            // Assert
            _clienteRepositoryMock.Verify(r => r.ActualizarAsync(
                It.IsAny<Cliente>(), 
                _cancellationToken), 
                Times.Once);
                
            // Verificar que se llamó a RegistrarVisita en el cliente
            cliente.CantidadVisitas.Should().Be(1);
            cliente.DomainEvents.Should().ContainSingle(e => e is VisitaRegistrada);
        }

        [Fact]
        public async Task Handle_ComandaSinCliente_NoDebeRegistrarVisita()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            
            // Crear evento ComandaFinalizada
            var evento = new ComandaFinalizada(comandaId, 100.0m);
            
            // Crear comanda sin cliente asociado (clienteId = null)
            var comanda = Comanda.Crear(Guid.NewGuid(), null, Guid.NewGuid());
            
            // Asignar el ID de la comanda manualmente para el test
            var idField = typeof(EntityBase).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance);
            idField?.SetValue(comanda, comandaId);
            
            // Configurar mock del repositorio de comandas
            _comandaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(comandaId, _cancellationToken))
                .ReturnsAsync(comanda);

            // Act
            await _handler.Handle(evento, _cancellationToken);

            // Assert
            _clienteRepositoryMock.Verify(r => r.ObtenerPorIdAsync(
                It.IsAny<Guid>(), 
                _cancellationToken), 
                Times.Never);
                
            _clienteRepositoryMock.Verify(r => r.ActualizarAsync(
                It.IsAny<Cliente>(), 
                _cancellationToken), 
                Times.Never);
        }

        [Fact]
        public async Task Handle_ClienteInactivo_NoDebeRegistrarVisita()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            
            // Crear evento ComandaFinalizada
            var evento = new ComandaFinalizada(comandaId, 100.0m);
            
            // Crear comanda con cliente asociado
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid());
            
            // Asignar el ID de la comanda manualmente para el test
            var idField = typeof(EntityBase).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance);
            idField?.SetValue(comanda, comandaId);
            
            // Asignar cliente a la comanda
            var clienteIdProperty = typeof(Comanda).GetProperty("ClienteId");
            clienteIdProperty?.SetValue(comanda, clienteId);
            
            // Configurar mock del repositorio de comandas
            _comandaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(comandaId, _cancellationToken))
                .ReturnsAsync(comanda);
                
            // Crear cliente inactivo para el test
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            cliente.Desactivar(); // Cliente inactivo
            
            // Asignar ID al cliente manualmente
            idField?.SetValue(cliente, clienteId);
            
            // Configurar mock del repositorio de clientes
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, _cancellationToken))
                .ReturnsAsync(cliente);

            // Act
            await _handler.Handle(evento, _cancellationToken);

            // Assert
            _clienteRepositoryMock.Verify(r => r.ActualizarAsync(
                It.IsAny<Cliente>(), 
                _cancellationToken), 
                Times.Never);
        }
    }
} 