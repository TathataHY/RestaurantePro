namespace RestaurantePro.Domain.UnitTests.Core.EventHandlers
{
    /// <summary>
    /// Pruebas unitarias para ComandaFinalizada_ActualizarProductosHandler
    /// </summary>
    public class ComandaFinalizada_ActualizarProductosHandlerTests
    {
        private readonly Mock<RestaurantePro.Domain.Core.Services.ICoreOperacionesIntegrationService> _integrationServiceMock;
        private readonly Mock<ILogger<RestaurantePro.Domain.Core.EventHandlers.ComandaFinalizada_ActualizarProductosHandler>> _loggerMock;
        private readonly RestaurantePro.Domain.Core.EventHandlers.ComandaFinalizada_ActualizarProductosHandler _sut;
        
        public ComandaFinalizada_ActualizarProductosHandlerTests()
        {
            _integrationServiceMock = new Mock<RestaurantePro.Domain.Core.Services.ICoreOperacionesIntegrationService>();
            _loggerMock = new Mock<ILogger<RestaurantePro.Domain.Core.EventHandlers.ComandaFinalizada_ActualizarProductosHandler>>();
            
            _sut = new RestaurantePro.Domain.Core.EventHandlers.ComandaFinalizada_ActualizarProductosHandler(
                _integrationServiceMock.Object,
                _loggerMock.Object);
        }
        
        [Fact]
        public async Task Handle_ConDatosValidos_DebeLlamarAlServicioDeIntegracion()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId1 = Guid.NewGuid();
            var productoId2 = Guid.NewGuid();
            
            var items = new List<RestaurantePro.Domain.Operaciones.Comandas.ValueObjects.ItemComandaInfo>
            {
                new RestaurantePro.Domain.Operaciones.Comandas.ValueObjects.ItemComandaInfo 
                { 
                    ProductoId = productoId1,
                    Cantidad = 2
                },
                new RestaurantePro.Domain.Operaciones.Comandas.ValueObjects.ItemComandaInfo 
                { 
                    ProductoId = productoId2,
                    Cantidad = 3
                }
            };
            
            var evento = new RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada(
                comandaId,
                items,
                DateTime.Now);
                
            var expectedIdCantidad = new Dictionary<Guid, int>
            {
                { productoId1, 2 },
                { productoId2, 3 }
            };
                
            _integrationServiceMock
                .Setup(s => s.ProcesarComandaFinalizadaAsync(
                    comandaId, 
                    It.IsAny<Dictionary<Guid, int>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(Result.Success(true)))
                .Callback<Guid, Dictionary<Guid, int>, CancellationToken>((id, dict, ct) => 
                {
                    dict.Should().BeEquivalentTo(expectedIdCantidad);
                });
            
            // Act
            await _sut.Handle(evento, CancellationToken.None);
            
            // Assert
            _integrationServiceMock.Verify(
                s => s.ProcesarComandaFinalizadaAsync(
                    comandaId, 
                    It.IsAny<Dictionary<Guid, int>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task Handle_ConItemsRepetidos_DebeAgregarCantidades()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId1 = Guid.NewGuid();
            
            var items = new List<RestaurantePro.Domain.Operaciones.Comandas.ValueObjects.ItemComandaInfo>
            {
                new RestaurantePro.Domain.Operaciones.Comandas.ValueObjects.ItemComandaInfo 
                { 
                    ProductoId = productoId1,
                    Cantidad = 2
                },
                new RestaurantePro.Domain.Operaciones.Comandas.ValueObjects.ItemComandaInfo 
                { 
                    ProductoId = productoId1,
                    Cantidad = 3
                }
            };
            
            var evento = new RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada(
                comandaId,
                items,
                DateTime.Now);
                
            var expectedIdCantidad = new Dictionary<Guid, int>
            {
                { productoId1, 5 } // 2 + 3
            };
                
            _integrationServiceMock
                .Setup(s => s.ProcesarComandaFinalizadaAsync(
                    comandaId, 
                    It.IsAny<Dictionary<Guid, int>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(Result.Success(true)))
                .Callback<Guid, Dictionary<Guid, int>, CancellationToken>((id, dict, ct) => 
                {
                    dict.Should().BeEquivalentTo(expectedIdCantidad);
                });
            
            // Act
            await _sut.Handle(evento, CancellationToken.None);
            
            // Assert
            _integrationServiceMock.Verify(
                s => s.ProcesarComandaFinalizadaAsync(
                    comandaId, 
                    It.IsAny<Dictionary<Guid, int>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task Handle_ConErrorEnServicio_DebeLogearAdvertencia()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId1 = Guid.NewGuid();
            
            var items = new List<RestaurantePro.Domain.Operaciones.Comandas.ValueObjects.ItemComandaInfo>
            {
                new RestaurantePro.Domain.Operaciones.Comandas.ValueObjects.ItemComandaInfo 
                { 
                    ProductoId = productoId1,
                    Cantidad = 2
                }
            };
            
            var evento = new RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada(
                comandaId,
                items,
                DateTime.Now);
                
            var errorMessages = new List<string> { "Error de prueba" };
                
            _integrationServiceMock
                .Setup(s => s.ProcesarComandaFinalizadaAsync(
                    comandaId, 
                    It.IsAny<Dictionary<Guid, int>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(Result.Failure<bool>(errorMessages)));
            
            // Act
            await _sut.Handle(evento, CancellationToken.None);
            
            // Assert
            _integrationServiceMock.Verify(
                s => s.ProcesarComandaFinalizadaAsync(
                    comandaId, 
                    It.IsAny<Dictionary<Guid, int>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
                
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
        
        [Fact]
        public async Task Handle_ConExcepcion_DebeLogearError()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId1 = Guid.NewGuid();
            
            var items = new List<RestaurantePro.Domain.Operaciones.Comandas.ValueObjects.ItemComandaInfo>
            {
                new RestaurantePro.Domain.Operaciones.Comandas.ValueObjects.ItemComandaInfo 
                { 
                    ProductoId = productoId1,
                    Cantidad = 2
                }
            };
            
            var evento = new RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada(
                comandaId,
                items,
                DateTime.Now);
                
            _integrationServiceMock
                .Setup(s => s.ProcesarComandaFinalizadaAsync(
                    comandaId, 
                    It.IsAny<Dictionary<Guid, int>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error de prueba"));
            
            // Act
            await _sut.Handle(evento, CancellationToken.None);
            
            // Assert
            _integrationServiceMock.Verify(
                s => s.ProcesarComandaFinalizadaAsync(
                    comandaId, 
                    It.IsAny<Dictionary<Guid, int>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
                
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error no controlado")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
} 