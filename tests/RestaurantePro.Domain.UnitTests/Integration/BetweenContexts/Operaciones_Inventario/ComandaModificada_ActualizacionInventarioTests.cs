namespace RestaurantePro.Domain.UnitTests.Integration.BetweenContexts.Operaciones_Inventario
{
    /// <summary>
    /// Tests de integración para verificar la actualización del inventario cuando se modifica una comanda.
    /// Demuestra la interacción entre los contextos de Operaciones (Comandas) e Inventario.
    /// </summary>
    public class ComandaModificada_ActualizacionInventarioTests
    {
        private readonly Mock<RestaurantePro.Domain.Operaciones.Services.IOperacionesInventarioIntegrationService> _integrationServiceMock = new();
        private readonly Mock<IDomainEventRegistry> _eventRegistryMock = new();
        private readonly Mock<IDateTimeService> _dateTimeServiceMock = new();
        
        private readonly ComandaModificada_ActualizarInventarioHandler _handler;
        private readonly DateTime _fechaActual = new DateTime(2023, 5, 15, 10, 0, 0);
        
        public ComandaModificada_ActualizacionInventarioTests()
        {
            // Configurar fecha actual
            _dateTimeServiceMock.Setup(s => s.Now).Returns(_fechaActual);
            
            // Inicializar handler
            _handler = new ComandaModificada_ActualizarInventarioHandler(
                _integrationServiceMock.Object,
                _eventRegistryMock.Object,
                _dateTimeServiceMock.Object);
        }
        
        [Fact]
        public async Task ProductoAgregadoAComanda_DebeReservarIngredientes()
        {
            // Arrange
            // 1. Crear IDs para el test
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var itemComandaId = Guid.NewGuid();
            
            // 2. Configurar el servicio de integración para simular una reserva exitosa
            _integrationServiceMock
                .Setup(s => s.ReservarIngredientesComandaAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(true));
                
            // 3. Configurar el event registry
            _eventRegistryMock
                .Setup(e => e.RegisterAsync(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // 4. Crear evento ProductoAgregadoAComanda
            var evento = new RestaurantePro.Domain.Operaciones.Comandas.Events.ItemComanda.ItemComandaCreado(
                comandaId, 
                itemComandaId,
                productoId, 
                "Ensalada mixta", 
                2); // Cantidad
            
            // Act
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            // 1. Verificar que se llamó al servicio de integración para reservar ingredientes
            _integrationServiceMock.Verify(
                s => s.ReservarIngredientesComandaAsync(comandaId, It.IsAny<CancellationToken>()),
                Times.Once);
                
            // 2. Verificar que se registró el evento
            _eventRegistryMock.Verify(
                e => e.RegisterAsync(
                    It.IsAny<RestaurantePro.Domain.Operaciones.Comandas.Events.ItemComanda.ItemComandaCreado>(), 
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task ProductoEliminadoDeComanda_DebeLiberarReservaIngredientes()
        {
            // Arrange
            // 1. Crear IDs para el test
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var itemComandaId = Guid.NewGuid();
            
            // 2. Configurar el servicio de integración para simular una liberación exitosa
            _integrationServiceMock
                .Setup(s => s.LiberarReservaIngredientesAsync(comandaId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(true));
                
            // 3. Configurar el event registry
            _eventRegistryMock
                .Setup(e => e.RegisterAsync(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // 4. Crear evento ProductoEliminadoDeComanda
            var evento = new RestaurantePro.Domain.Operaciones.Comandas.Events.ItemComanda.ProductoEliminadoDeComanda(
                comandaId,
                itemComandaId,
                productoId,
                "Ensalada mixta",
                2, // Cantidad
                "Cancelado por el cliente");
            
            // Act
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            // 1. Verificar que se llamó al servicio de integración para liberar ingredientes
            _integrationServiceMock.Verify(
                s => s.LiberarReservaIngredientesAsync(
                    comandaId, 
                    "Cancelado por el cliente", 
                    It.IsAny<CancellationToken>()),
                Times.Once);
                
            // 2. Verificar que se registró el evento
            _eventRegistryMock.Verify(
                e => e.RegisterAsync(
                    It.IsAny<RestaurantePro.Domain.Operaciones.Comandas.Events.ItemComanda.ProductoEliminadoDeComanda>(), 
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task ProductoAgregadoAComanda_ConErrorEnReserva_DebeRegistrarEvento()
        {
            // Arrange
            // 1. Crear IDs para el test
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var itemComandaId = Guid.NewGuid();
            
            // 2. Configurar el servicio de integración para simular un error en la reserva
            var errors = new List<Error> { new Error("No hay suficiente stock") };
            _integrationServiceMock
                .Setup(s => s.ReservarIngredientesComandaAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<bool>(errors));
                
            // 3. Configurar el event registry
            _eventRegistryMock
                .Setup(e => e.RegisterAsync(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // 4. Crear evento ProductoAgregadoAComanda
            var evento = new RestaurantePro.Domain.Operaciones.Comandas.Events.ItemComanda.ItemComandaCreado(
                comandaId, 
                itemComandaId,
                productoId, 
                "Ensalada mixta", 
                2); // Cantidad
            
            // Act
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            // 1. Verificar que se llamó al servicio de integración para reservar ingredientes
            _integrationServiceMock.Verify(
                s => s.ReservarIngredientesComandaAsync(comandaId, It.IsAny<CancellationToken>()),
                Times.Once);
                
            // 2. Verificar que se registró el evento a pesar del error
            _eventRegistryMock.Verify(
                e => e.RegisterAsync(
                    It.IsAny<RestaurantePro.Domain.Operaciones.Comandas.Events.ItemComanda.ItemComandaCreado>(), 
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task ProductoEliminadoDeComanda_ConErrorEnLiberacion_DebeRegistrarEvento()
        {
            // Arrange
            // 1. Crear IDs para el test
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var itemComandaId = Guid.NewGuid();
            
            // 2. Configurar el servicio de integración para simular un error en la liberación
            var errors = new List<Error> { new Error("Error al liberar los ingredientes") };
            _integrationServiceMock
                .Setup(s => s.LiberarReservaIngredientesAsync(comandaId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<bool>(errors));
                
            // 3. Configurar el event registry
            _eventRegistryMock
                .Setup(e => e.RegisterAsync(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // 4. Crear evento ProductoEliminadoDeComanda
            var evento = new RestaurantePro.Domain.Operaciones.Comandas.Events.ItemComanda.ProductoEliminadoDeComanda(
                comandaId,
                itemComandaId,
                productoId,
                "Ensalada mixta",
                2, // Cantidad
                "Cancelado por el cliente");
            
            // Act
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            // 1. Verificar que se llamó al servicio de integración para liberar ingredientes
            _integrationServiceMock.Verify(
                s => s.LiberarReservaIngredientesAsync(
                    comandaId, 
                    "Cancelado por el cliente", 
                    It.IsAny<CancellationToken>()),
                Times.Once);
                
            // 2. Verificar que se registró el evento a pesar del error
            _eventRegistryMock.Verify(
                e => e.RegisterAsync(
                    It.IsAny<RestaurantePro.Domain.Operaciones.Comandas.Events.ItemComanda.ProductoEliminadoDeComanda>(), 
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
} 