namespace RestaurantePro.Domain.UnitTests.Inventario.EventHandlers
{
    public class ComandaCreada_ActualizarInventarioHandlerTests
    {
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly Mock<IMovimientoInventarioRepository> _movimientoRepositoryMock;
        private readonly Mock<IProductoRepository> _productoRepositoryMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Mock<IDomainEventLog> _eventLogMock;
        private readonly ComandaCreada_ActualizarInventarioHandler _handler;

        public ComandaCreada_ActualizarInventarioHandlerTests()
        {
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();
            _movimientoRepositoryMock = new Mock<IMovimientoInventarioRepository>();
            _productoRepositoryMock = new Mock<IProductoRepository>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _eventLogMock = new Mock<IDomainEventLog>();

            _dateTimeServiceMock.Setup(s => s.Now).Returns(new DateTime(2023, 1, 1));

            _handler = new ComandaCreada_ActualizarInventarioHandler(
                _ingredienteRepositoryMock.Object,
                _movimientoRepositoryMock.Object,
                _productoRepositoryMock.Object,
                _dateTimeServiceMock.Object,
                _eventLogMock.Object);
        }

        [Fact]
        public async Task Handle_ConComandaValida_DebeActualizarInventario()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var ingrediente1Id = Guid.NewGuid();
            var ingrediente2Id = Guid.NewGuid();

            // Crear evento de comanda
            var itemComanda = new ItemComandaDto(productoId, "Ensalada César", 2, 15.99m);
            var eventoComanda = new ComandaCreada(
                comandaId,
                new List<ItemComandaDto> { itemComanda },
                DateTime.Now,
                50.00m);

            // Configurar producto
            var producto = Producto.Crear("Ensalada César", "Ensalada fresca", new Dinero(15.99m, "MXN"));
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto);

            // Configurar ingredientes del producto
            var ingrediente1 = Ingrediente.Crear("Lechuga", "kg", 10);
            ingrediente1.SetId(ingrediente1Id);
            ingrediente1.ActualizarStock(5, DateTime.Now); // 5kg disponibles

            var ingrediente2 = Ingrediente.Crear("Pollo", "kg", 2);
            ingrediente2.SetId(ingrediente2Id);
            ingrediente2.ActualizarStock(3, DateTime.Now); // 3kg disponibles

            // Configurar relación ingrediente-producto
            var ingredientesProducto = new List<IngredienteProducto>
            {
                new IngredienteProducto(ingrediente1, 0.2m), // 200g de lechuga por ensalada
                new IngredienteProducto(ingrediente2, 0.1m)  // 100g de pollo por ensalada
            };

            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerIngredientesPorProductoAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingredientesProducto);

            // Act
            await _handler.Handle(eventoComanda);

            // Assert
            // Verificar que se crearon los movimientos de inventario (2 ensaladas * ingredientes)
            _movimientoRepositoryMock.Verify(
                r => r.AgregarAsync(
                    It.Is<MovimientoInventario>(m => 
                        m.IngredienteId == ingrediente1Id && 
                        m.Cantidad == 0.4m && // 0.2kg * 2 ensaladas
                        m.Tipo == TipoMovimientoInventario.Salida),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _movimientoRepositoryMock.Verify(
                r => r.AgregarAsync(
                    It.Is<MovimientoInventario>(m => 
                        m.IngredienteId == ingrediente2Id && 
                        m.Cantidad == 0.2m && // 0.1kg * 2 ensaladas
                        m.Tipo == TipoMovimientoInventario.Salida),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // Verificar que se actualizó el stock de ingredientes
            _ingredienteRepositoryMock.Verify(
                r => r.ActualizarAsync(
                    It.Is<Ingrediente>(i => 
                        i.Id == ingrediente1Id && 
                        i.StockActual == 4.6m), // 5kg - 0.4kg
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _ingredienteRepositoryMock.Verify(
                r => r.ActualizarAsync(
                    It.Is<Ingrediente>(i => 
                        i.Id == ingrediente2Id && 
                        i.StockActual == 2.8m), // 3kg - 0.2kg
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // Verificar que se registró el éxito en el log
            _eventLogMock.Verify(
                l => l.LogEvent(
                    eventoComanda,
                    "Inventario actualizado correctamente",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ProductoNoEncontrado_DebeRegistrarErrorYContinuar()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();

            // Crear evento de comanda
            var itemComanda = new ItemComandaDto(productoId, "Producto Inexistente", 1, 10.00m);
            var eventoComanda = new ComandaCreada(
                comandaId,
                new List<ItemComandaDto> { itemComanda },
                DateTime.Now,
                10.00m);

            // Configurar que el producto no existe
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Producto)null);

            // Act
            await _handler.Handle(eventoComanda);

            // Assert
            // Verificar que se registró el error
            _eventLogMock.Verify(
                l => l.LogEvent(
                    eventoComanda,
                    It.Is<string>(msg => msg.Contains("No se encontró el producto")),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // Verificar que no se actualizó el inventario
            _movimientoRepositoryMock.Verify(
                r => r.AgregarAsync(It.IsAny<MovimientoInventario>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _ingredienteRepositoryMock.Verify(
                r => r.ActualizarAsync(It.IsAny<Ingrediente>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // Clase auxiliar para las pruebas
        private class IngredienteProducto
        {
            public Ingrediente Ingrediente { get; }
            public decimal CantidadPorProducto { get; }
            public Guid Id => Ingrediente.Id;
            public decimal StockActual => Ingrediente.StockActual;
            public decimal CantidadMinima => Ingrediente.CantidadMinima;
            public string Nombre => Ingrediente.Nombre;

            public IngredienteProducto(Ingrediente ingrediente, decimal cantidadPorProducto)
            {
                Ingrediente = ingrediente;
                CantidadPorProducto = cantidadPorProducto;
            }

            public void ActualizarStock(decimal nuevoStock, DateTime fecha)
            {
                Ingrediente.ActualizarStock(nuevoStock, fecha);
            }
        }
    }
} 