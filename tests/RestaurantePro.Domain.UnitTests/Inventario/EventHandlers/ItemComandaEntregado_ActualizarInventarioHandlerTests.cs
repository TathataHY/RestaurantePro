namespace RestaurantePro.Domain.UnitTests.Inventario.EventHandlers
{
    public class ItemComandaEntregado_ActualizarInventarioHandlerTests
    {
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly Mock<IMovimientoInventarioRepository> _movimientoRepositoryMock;
        private readonly Mock<IProductoRepository> _productoRepositoryMock;
        private readonly Mock<IProductoIngredienteRepository> _productoIngredienteRepositoryMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Mock<IDomainEventRegistry> _eventRegistryMock;
        private readonly ItemComandaEntregado_ActualizarInventarioHandler _handler;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;
        private readonly DateTime _fechaActual = new DateTime(2023, 1, 1, 12, 0, 0);
        
        public ItemComandaEntregado_ActualizarInventarioHandlerTests()
        {
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();
            _movimientoRepositoryMock = new Mock<IMovimientoInventarioRepository>();
            _productoRepositoryMock = new Mock<IProductoRepository>();
            _productoIngredienteRepositoryMock = new Mock<IProductoIngredienteRepository>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _eventRegistryMock = new Mock<IDomainEventRegistry>();
            
            _dateTimeServiceMock.Setup(s => s.Now).Returns(_fechaActual);
            
            _handler = new ItemComandaEntregado_ActualizarInventarioHandler(
                _ingredienteRepositoryMock.Object,
                _movimientoRepositoryMock.Object,
                _productoRepositoryMock.Object,
                _productoIngredienteRepositoryMock.Object,
                _dateTimeServiceMock.Object,
                _eventRegistryMock.Object);
        }
        
        [Fact]
        public async Task Handle_ItemEntregado_DebeActualizarInventario()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var comandaId = Guid.NewGuid();
            var categoriaId = Guid.NewGuid();
            var cantidad = 2;
            
            // Crear evento
            var evento = new ItemComandaEntregado(itemId, productoId, comandaId, cantidad);
            
            // Configurar producto
            var precioProducto = new PrecioProducto(100m);
            var producto = Producto.Crear("Producto Test", "Descripción", precioProducto, categoriaId);
            typeof(EntityBase).GetProperty("Id").SetValue(producto, productoId);
            
            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);
                
            // Crear ingredientes con stock inicial para que sea coherente con los movimientos
            var ingredienteId1 = Guid.NewGuid();
            var ingredienteId2 = Guid.NewGuid();
            
            // Ingredientes con 0 stock inicial para que los movimientos sean coherentes
            var ingrediente1 = Ingrediente.Crear(
                "Ingrediente 1", 
                "ING-001", 
                "Descripción 1", 
                UnidadMedida.Kilogramo, 
                10.0m, // stock mínimo
                0m);   // stock inicial cero
                
            var ingrediente2 = Ingrediente.Crear(
                "Ingrediente 2", 
                "ING-002", 
                "Descripción 2", 
                UnidadMedida.Kilogramo, 
                8.0m,  // stock mínimo
                0m);   // stock inicial cero
            
            // Establecer IDs usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(ingrediente1, ingredienteId1);
            typeof(EntityBase).GetProperty("Id").SetValue(ingrediente2, ingredienteId2);
            
            // Agregar movimientos iniciales
            var movimiento1 = ingrediente1.IncrementarStock(15.0m, "Stock inicial");
            var movimiento2 = ingrediente2.IncrementarStock(12.0m, "Stock inicial");
            
            // Mock para simular el comportamiento de MovimientoInventario.CrearEgreso y otros métodos
            // que el handler llama internamente para evitar problemas con las validaciones
            _movimientoRepositoryMock.Setup(r => r.AgregarAsync(It.IsAny<MovimientoInventario>()))
                .Callback<MovimientoInventario>(m => {
                    // Cuando el handler crea un movimiento, simulamos su aplicación aquí
                    if (m.IngredienteId == ingredienteId1)
                    {
                        ingrediente1.DecrementarStock(m.Cantidad, "Simulado en test");
                    }
                    else if (m.IngredienteId == ingredienteId2)
                    {
                        ingrediente2.DecrementarStock(m.Cantidad, "Simulado en test");
                    }
                })
                .Returns(Task.CompletedTask);
            
            // Configurar ingredientes asociados al producto
            _ingredienteRepositoryMock.Setup(r => r.ObtenerIngredientesPorProductoAsync(productoId, _cancellationToken))
                .ReturnsAsync(new List<Ingrediente> { ingrediente1, ingrediente2 });
                
            // Configurar relaciones producto-ingrediente
            var productoIngrediente1 = new ProductoIngrediente(productoId, ingredienteId1, 0.25m);
            var productoIngrediente2 = new ProductoIngrediente(productoId, ingredienteId2, 0.1m);
            
            _productoIngredienteRepositoryMock.Setup(r => r.ObtenerPorProductoEIngredienteAsync(productoId, ingredienteId1, _cancellationToken))
                .ReturnsAsync(productoIngrediente1);
                
            _productoIngredienteRepositoryMock.Setup(r => r.ObtenerPorProductoEIngredienteAsync(productoId, ingredienteId2, _cancellationToken))
                .ReturnsAsync(productoIngrediente2);
                
            // Configurar actualizaciones de ingredientes
            _ingredienteRepositoryMock.Setup(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), _cancellationToken))
                .Returns(Task.CompletedTask);
                
            // Configurar el mock del event registry
            _eventRegistryMock
                .Setup(l => l.RegisterAsync(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            // Act - El test ya no llamará a DecrementarStock directamente, sólo dejamos que el handler lo haga
            await _handler.Handle(evento, _cancellationToken);
            
            // Assert
            // Verificar que se consultó el producto
            _productoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(productoId, _cancellationToken), Times.Once);
            
            // Verificar que se consultaron los ingredientes
            _ingredienteRepositoryMock.Verify(r => r.ObtenerIngredientesPorProductoAsync(productoId, _cancellationToken), Times.Once);
            
            // Verificar que se consultaron las relaciones producto-ingrediente
            _productoIngredienteRepositoryMock.Verify(
                r => r.ObtenerPorProductoEIngredienteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), _cancellationToken), 
                Times.AtLeast(1));
            
            // Verificar que se agregaron movimientos de inventario
            _movimientoRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<MovimientoInventario>()), Times.Exactly(2));
                
            // Verificar que se registró el éxito
            _eventRegistryMock.Verify(l => l.RegisterAsync(
                It.IsAny<DomainEvent>(),
                It.Is<CancellationToken>(c => c == _cancellationToken)), 
                Times.Once);
        }
        
        [Fact]
        public async Task Handle_ProductoNoExiste_NoDebeActualizarInventario()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var comandaId = Guid.NewGuid();
            var cantidad = 2;
            
            // Crear evento
            var evento = new ItemComandaEntregado(itemId, productoId, comandaId, cantidad);
            
            // Configurar que el producto no existe
            Producto? productoNull = null;
            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(productoNull);
                
            // Act
            await _handler.Handle(evento, _cancellationToken);
            
            // Assert
            // Verificar que se registró el error
            _eventRegistryMock.Verify(l => l.RegisterAsync(
                It.IsAny<DomainEvent>(),
                It.Is<CancellationToken>(c => c == _cancellationToken)), 
                Times.Once);
                
            // Verificar que no se intentó actualizar nada
            _ingredienteRepositoryMock.Verify(r => r.ObtenerIngredientesPorProductoAsync(It.IsAny<Guid>(), _cancellationToken), Times.Never);
            _productoIngredienteRepositoryMock.Verify(r => r.ObtenerPorProductoEIngredienteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), _cancellationToken), Times.Never);
            _ingredienteRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), _cancellationToken), Times.Never);
            _movimientoRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<MovimientoInventario>()), Times.Never);
        }
        
        [Fact]
        public async Task Handle_NoIngredientesAsociados_NoDebeActualizarInventario()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var comandaId = Guid.NewGuid();
            var categoriaId = Guid.NewGuid();
            var cantidad = 2;
            
            // Crear evento
            var evento = new ItemComandaEntregado(itemId, productoId, comandaId, cantidad);
            
            // Configurar producto
            var precioProducto = new PrecioProducto(100m);
            var producto = Producto.Crear("Producto Test", "Descripción", precioProducto, categoriaId);
            var idField = typeof(EntityBase).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance);
            idField?.SetValue(producto, productoId);
            
            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);
                
            // Configurar que no hay ingredientes asociados
            _ingredienteRepositoryMock.Setup(r => r.ObtenerIngredientesPorProductoAsync(productoId, _cancellationToken))
                .ReturnsAsync(new List<Ingrediente>());
                
            // Act
            await _handler.Handle(evento, _cancellationToken);
            
            // Assert
            _productoIngredienteRepositoryMock.Verify(r => r.ObtenerPorProductoEIngredienteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), _cancellationToken), Times.Never);
            _ingredienteRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Ingrediente>(), _cancellationToken), Times.Never);
            _movimientoRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<MovimientoInventario>()), Times.Never);
        }
    }
} 