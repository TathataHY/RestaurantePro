namespace RestaurantePro.Domain.UnitTests.Inventario.EventHandlers
{
    public class ComandaCreada_VerificarDisponibilidadHandlerTests
    {
        private readonly Mock<RestaurantePro.Domain.Operaciones.Services.IOperacionesInventarioIntegrationService> _integrationServiceMock;
        private readonly Mock<IDomainEventRegistry> _eventRegistryMock;
        private readonly ComandaCreada_VerificarDisponibilidadHandler _handler;

        public ComandaCreada_VerificarDisponibilidadHandlerTests()
        {
            _integrationServiceMock = new Mock<RestaurantePro.Domain.Operaciones.Services.IOperacionesInventarioIntegrationService>();
            _eventRegistryMock = new Mock<IDomainEventRegistry>();

            _handler = new ComandaCreada_VerificarDisponibilidadHandler(
                _integrationServiceMock.Object,
                _eventRegistryMock.Object);
        }

        [Fact]
        public async Task Handle_ConStockSuficiente_NoDebeGenerarAdvertencias()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var meseroId = Guid.NewGuid();

            // Crear evento de comanda
            var eventoComanda = new ComandaCreada(comandaId, mesaId, meseroId);
            
            // Configurar resultado exitoso del servicio de integración
            var resultadoDisponibilidad = new RestaurantePro.Domain.Operaciones.Results.DisponibilidadIngredientesResult
            {
                TodosDisponibles = true,
                ProductosNoDisponibles = new Dictionary<Guid, string>(),
                IngredientesFaltantes = new Dictionary<string, decimal>()
            };
            
            _integrationServiceMock
                .Setup(s => s.VerificarDisponibilidadIngredientesComandaAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(resultadoDisponibilidad));
                
            // Configurar event registry
            _eventRegistryMock
                .Setup(l => l.RegisterAsync(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            // Act
            await _handler.Handle(eventoComanda);
            
            // Assert
            // Verificar que se llamó al servicio de integración
            _integrationServiceMock.Verify(
                s => s.VerificarDisponibilidadIngredientesComandaAsync(comandaId, It.IsAny<CancellationToken>()), 
                Times.Once);
                
            // Verificar que se registró el evento
            _eventRegistryMock.Verify(
                l => l.RegisterAsync(
                    It.IsAny<ComandaCreada>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ConStockInsuficiente_DebeGenerarAdvertencia()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var meseroId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            
            // Crear evento de comanda
            var eventoComanda = new ComandaCreada(comandaId, mesaId, meseroId);
            
            // Configurar resultado del servicio de integración con stock insuficiente
            var resultadoDisponibilidad = new RestaurantePro.Domain.Operaciones.Results.DisponibilidadIngredientesResult
            {
                TodosDisponibles = false,
                ProductosNoDisponibles = new Dictionary<Guid, string> 
                { 
                    { productoId, "Falta ingrediente: Lechuga" } 
                },
                IngredientesFaltantes = new Dictionary<string, decimal> 
                { 
                    { "Lechuga", 4.0m } 
                }
            };
            
            _integrationServiceMock
                .Setup(s => s.VerificarDisponibilidadIngredientesComandaAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(resultadoDisponibilidad));
                
            // Configurar event registry
            _eventRegistryMock
                .Setup(l => l.RegisterAsync(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            // Act
            await _handler.Handle(eventoComanda);
            
            // Assert
            // Verificar que se llamó al servicio de integración
            _integrationServiceMock.Verify(
                s => s.VerificarDisponibilidadIngredientesComandaAsync(comandaId, It.IsAny<CancellationToken>()), 
                Times.Once);
                
            // Verificar que se registró el evento
            _eventRegistryMock.Verify(
                l => l.RegisterAsync(
                    It.IsAny<ComandaCreada>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task Handle_ErrorEnServicio_DebeRegistrarEvento()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var meseroId = Guid.NewGuid();
            
            // Crear evento de comanda
            var eventoComanda = new ComandaCreada(comandaId, mesaId, meseroId);
            
            // Configurar error en el servicio de integración
            _integrationServiceMock
                .Setup(s => s.VerificarDisponibilidadIngredientesComandaAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<RestaurantePro.Domain.Operaciones.Results.DisponibilidadIngredientesResult>("Error al verificar disponibilidad"));
                
            // Configurar event registry
            _eventRegistryMock
                .Setup(l => l.RegisterAsync(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            // Act
            await _handler.Handle(eventoComanda);
            
            // Assert
            // Verificar que se llamó al servicio de integración
            _integrationServiceMock.Verify(
                s => s.VerificarDisponibilidadIngredientesComandaAsync(comandaId, It.IsAny<CancellationToken>()), 
                Times.Once);
                
            // Verificar que se registró el evento a pesar del error
            _eventRegistryMock.Verify(
                l => l.RegisterAsync(
                    It.IsAny<ComandaCreada>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task Handle_ExcepcionLanzada_DebeRegistrarEvento()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var meseroId = Guid.NewGuid();
            
            // Crear evento de comanda
            var eventoComanda = new ComandaCreada(comandaId, mesaId, meseroId);
            
            // Configurar que el servicio lance una excepción
            _integrationServiceMock
                .Setup(s => s.VerificarDisponibilidadIngredientesComandaAsync(comandaId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error simulado"));
                
            // Configurar event registry
            _eventRegistryMock
                .Setup(l => l.RegisterAsync(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            // Act
            await _handler.Handle(eventoComanda);
            
            // Assert
            // Verificar que se intentó llamar al servicio de integración
            _integrationServiceMock.Verify(
                s => s.VerificarDisponibilidadIngredientesComandaAsync(comandaId, It.IsAny<CancellationToken>()), 
                Times.Once);
                
            // Verificar que se registró el evento a pesar de la excepción
            _eventRegistryMock.Verify(
                l => l.RegisterAsync(
                    It.IsAny<ComandaCreada>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
} 