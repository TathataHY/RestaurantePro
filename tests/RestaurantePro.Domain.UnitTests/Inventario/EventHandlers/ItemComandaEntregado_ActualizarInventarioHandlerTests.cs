namespace RestaurantePro.Domain.UnitTests.Inventario.EventHandlers
{
    public class ItemComandaEntregado_ActualizarInventarioHandlerTests
    {
        private readonly Mock<RestaurantePro.Domain.Operaciones.Services.IOperacionesInventarioIntegrationService> _integrationServiceMock;
        private readonly Mock<IDomainEventRegistry> _eventRegistryMock;
        private readonly ItemComandaEntregado_ActualizarInventarioHandler _handler;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;
        
        public ItemComandaEntregado_ActualizarInventarioHandlerTests()
        {
            _integrationServiceMock = new Mock<RestaurantePro.Domain.Operaciones.Services.IOperacionesInventarioIntegrationService>();
            _eventRegistryMock = new Mock<IDomainEventRegistry>();
            
            _handler = new ItemComandaEntregado_ActualizarInventarioHandler(
                _integrationServiceMock.Object,
                _eventRegistryMock.Object);
        }
        
        [Fact]
        public async Task Handle_ItemEntregado_DebeConfirmarConsumoIngredientes()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var comandaId = Guid.NewGuid();
            var cantidad = 2;
            
            // Crear evento
            var evento = new ItemComandaEntregado(itemId, productoId, comandaId, cantidad);
            
            // Configurar resultado exitoso del servicio de integración
            _integrationServiceMock
                .Setup(s => s.ConfirmarConsumoIngredientesAsync(comandaId, _cancellationToken))
                .ReturnsAsync(Result.Success(true));
                
            // Configurar el mock del event registry
            _eventRegistryMock
                .Setup(l => l.RegisterAsync(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            // Act
            await _handler.Handle(evento, _cancellationToken);
            
            // Assert
            // Verificar que se llamó al servicio de integración
            _integrationServiceMock.Verify(
                s => s.ConfirmarConsumoIngredientesAsync(comandaId, _cancellationToken), 
                Times.Once);
                
            // Verificar que se registró el evento
            _eventRegistryMock.Verify(
                l => l.RegisterAsync(
                    It.IsAny<DomainEvent>(),
                    It.Is<CancellationToken>(c => c == _cancellationToken)), 
                Times.Once);
        }
        
        [Fact]
        public async Task Handle_ErrorEnIntegracion_DebeManejarErrorYRegistrarEvento()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var comandaId = Guid.NewGuid();
            var cantidad = 2;
            
            // Crear evento
            var evento = new ItemComandaEntregado(itemId, productoId, comandaId, cantidad);
            
            // Configurar resultado con error del servicio de integración
            var errors = new List<Error> { new Error("Error al confirmar consumo") };
            _integrationServiceMock
                .Setup(s => s.ConfirmarConsumoIngredientesAsync(comandaId, _cancellationToken))
                .ReturnsAsync(Result.Failure<bool>(errors));
                
            // Configurar el mock del event registry
            _eventRegistryMock
                .Setup(l => l.RegisterAsync(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            // Act
            await _handler.Handle(evento, _cancellationToken);
            
            // Assert
            // Verificar que se llamó al servicio de integración
            _integrationServiceMock.Verify(
                s => s.ConfirmarConsumoIngredientesAsync(comandaId, _cancellationToken), 
                Times.Once);
                
            // Verificar que se registró el evento a pesar del error
            _eventRegistryMock.Verify(
                l => l.RegisterAsync(
                    It.IsAny<DomainEvent>(),
                    It.Is<CancellationToken>(c => c == _cancellationToken)), 
                Times.Once);
        }
        
        [Fact]
        public async Task Handle_ExcepcionLanzada_DebeManejarExcepcionYRegistrarEvento()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var comandaId = Guid.NewGuid();
            var cantidad = 2;
            
            // Crear evento
            var evento = new ItemComandaEntregado(itemId, productoId, comandaId, cantidad);
            
            // Configurar que el servicio lance una excepción
            _integrationServiceMock
                .Setup(s => s.ConfirmarConsumoIngredientesAsync(comandaId, _cancellationToken))
                .ThrowsAsync(new Exception("Error simulado"));
                
            // Configurar el mock del event registry
            _eventRegistryMock
                .Setup(l => l.RegisterAsync(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            // Act
            await _handler.Handle(evento, _cancellationToken);
            
            // Assert
            // Verificar que se intentó llamar al servicio de integración
            _integrationServiceMock.Verify(
                s => s.ConfirmarConsumoIngredientesAsync(comandaId, _cancellationToken), 
                Times.Once);
                
            // Verificar que se registró el evento a pesar de la excepción
            _eventRegistryMock.Verify(
                l => l.RegisterAsync(
                    It.IsAny<DomainEvent>(),
                    It.Is<CancellationToken>(c => c == _cancellationToken)), 
                Times.Once);
        }
    }
} 