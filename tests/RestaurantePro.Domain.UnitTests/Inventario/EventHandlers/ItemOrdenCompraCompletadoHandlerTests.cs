namespace RestaurantePro.Domain.UnitTests.Inventario.EventHandlers
{
    public class ItemOrdenCompraCompletadoHandlerTests
    {
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly ItemOrdenCompraCompletadoHandler _handler;
        private readonly Guid _ingredienteId;
        private readonly Guid _ordenCompraId;
        private readonly Guid _itemId;
        private readonly decimal _cantidadRecibida;

        public ItemOrdenCompraCompletadoHandlerTests()
        {
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();
            _handler = new ItemOrdenCompraCompletadoHandler(_ingredienteRepositoryMock.Object);
            
            _ingredienteId = Guid.NewGuid();
            _ordenCompraId = Guid.NewGuid();
            _itemId = Guid.NewGuid();
            _cantidadRecibida = 10.5m;
        }

        [Fact]
        public async Task Handle_ConIngredienteExistente_DebeIncrementarStock()
        {
            // Arrange
            var ingrediente = Ingrediente.Crear(
                "Tomate", 
                "TOM-001", 
                "Tomate para ensaladas", 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo, 
                5.0m, 
                10.0m);
            
            var stockInicial = ingrediente.Stock;
            
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(_ingredienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingrediente);
                
            _ingredienteRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            var evento = new ItemOrdenCompraCompletado(_itemId, _ordenCompraId, _ingredienteId, _cantidadRecibida);
            
            // Act
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            // Verificar que se obtuvo el ingrediente
            _ingredienteRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(_ingredienteId, It.IsAny<CancellationToken>()),
                Times.Once);
                
            // Verificar que se incrementó el stock correctamente
            _ingredienteRepositoryMock.Verify(
                r => r.ActualizarAsync(
                    It.Is<Ingrediente>(i => 
                        i.Id == ingrediente.Id &&
                        i.Stock == stockInicial + _cantidadRecibida),
                    It.IsAny<CancellationToken>()),
                Times.Once);
                
            // Verificar que el stock se incrementó con el motivo correcto
            ingrediente.Stock.Should().Be(stockInicial + _cantidadRecibida);
            ingrediente.Movimientos.Should().Contain(m => 
                m.Cantidad == _cantidadRecibida &&
                m.TipoMovimiento == TipoMovimientoInventario.Ingreso &&
                m.Motivo.Contains(_ordenCompraId.ToString()));
        }
        
        [Fact]
        public async Task Handle_SinIngredienteExistente_NoDebeHacerNada()
        {
            // Arrange
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(_ingredienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Ingrediente)null);
                
            var evento = new ItemOrdenCompraCompletado(_itemId, _ordenCompraId, _ingredienteId, _cantidadRecibida);
            
            // Act
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            // Verificar que se intentó obtener el ingrediente
            _ingredienteRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(_ingredienteId, It.IsAny<CancellationToken>()),
                Times.Once);
                
            // Verificar que no se actualizó nada
            _ingredienteRepositoryMock.Verify(
                r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
} 