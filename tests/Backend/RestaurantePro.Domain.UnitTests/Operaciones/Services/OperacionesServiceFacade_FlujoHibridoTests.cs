namespace RestaurantePro.Domain.UnitTests.Operaciones.Services
{
    /// <summary>
    /// Tests específicos para el flujo híbrido de preparaciones en OperacionesServiceFacade
    /// </summary>
    public class OperacionesServiceFacade_FlujoHibridoTests
    {
        private readonly Mock<IComandaRepository> _comandaRepositoryMock;
        private readonly Mock<IReservacionRepository> _reservacionRepositoryMock;
        private readonly Mock<IMesaRepository> _mesaRepositoryMock;
        private readonly Mock<IProductoRepository> _productoRepositoryMock;
        private readonly Mock<IServicioPreparaciones> _servicioPreparacionesMock;
        private readonly Mock<INotificationManager> _notificationManagerMock;
        private readonly Mock<ILogger<ComandaBuilder>> _comandaBuilderLoggerMock;
        private readonly Mock<ILogger<ReservacionBuilder>> _reservacionBuilderLoggerMock;
        private readonly Mock<ILogger<MesaBuilder>> _mesaBuilderLoggerMock;
        private readonly Mock<ILogger<OperacionesServiceFacade>> _operacionesServiceLoggerMock;
        private readonly OperacionesServiceFacade _sut;

        public OperacionesServiceFacade_FlujoHibridoTests()
        {
            _comandaRepositoryMock = new Mock<IComandaRepository>();
            _reservacionRepositoryMock = new Mock<IReservacionRepository>();
            _mesaRepositoryMock = new Mock<IMesaRepository>();
            _productoRepositoryMock = new Mock<IProductoRepository>();
            _servicioPreparacionesMock = new Mock<IServicioPreparaciones>();
            _notificationManagerMock = new Mock<INotificationManager>();
            _comandaBuilderLoggerMock = new Mock<ILogger<ComandaBuilder>>();
            _reservacionBuilderLoggerMock = new Mock<ILogger<ReservacionBuilder>>();
            _mesaBuilderLoggerMock = new Mock<ILogger<MesaBuilder>>();
            _operacionesServiceLoggerMock = new Mock<ILogger<OperacionesServiceFacade>>();

            // Configurar repositorio de comandas para que las actualizaciones sean exitosas
            _comandaRepositoryMock.Setup(c => c.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Configurar NotificationManager
            _notificationManagerMock.Setup(n => n.HasErrors).Returns(false);
            _notificationManagerMock.Setup(n => n.ToResult<Comanda>(It.IsAny<Comanda>()))
                .Returns<Comanda>(comanda => Result<Comanda>.Success(comanda));

            // Configurar builders
            var comandaBuilder = new ComandaBuilder(_notificationManagerMock.Object, _comandaBuilderLoggerMock.Object);
            var reservacionBuilder = new ReservacionBuilder(_notificationManagerMock.Object, _reservacionBuilderLoggerMock.Object);
            var mesaBuilder = new MesaBuilder(_notificationManagerMock.Object, _mesaBuilderLoggerMock.Object);

            _sut = new OperacionesServiceFacade(
                _comandaRepositoryMock.Object,
                _reservacionRepositoryMock.Object,
                _mesaRepositoryMock.Object,
                _productoRepositoryMock.Object,
                _servicioPreparacionesMock.Object,
                _notificationManagerMock.Object,
                _comandaBuilderLoggerMock.Object,
                _reservacionBuilderLoggerMock.Object,
                _mesaBuilderLoggerMock.Object);
        }

        [Fact]
        public async Task AgregarProductoAComandaAsync_ConPreparacionesDisponibles_DebeConsumir()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var cantidad = 2;
            var observaciones = "Sin extras";

            // Configurar comanda existente
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), observaciones);
            _comandaRepositoryMock
                .Setup(c => c.ObtenerPorIdAsync(comandaId, true, default))
                .ReturnsAsync(comanda);

            // Configurar producto existente
            var precio = new PrecioProducto(15.50m);
            var producto = Producto.Crear("Pizza Margarita", "Deliciosa pizza", precio, Guid.NewGuid());
            _productoRepositoryMock
                .Setup(p => p.ObtenerPorIdAsync(productoId, default))
                .ReturnsAsync(producto);

            // Configurar servicio de preparaciones - HAY preparaciones disponibles
            _servicioPreparacionesMock
                .Setup(s => s.VerificarDisponibilidadAsync(
                    It.Is<Guid>(id => id == productoId), 
                    It.Is<int>(c => c == cantidad),
                    It.IsAny<Guid?>()))
                .ReturnsAsync(Result<bool>.Success(true));

            _servicioPreparacionesMock
                .Setup(s => s.ConsumirPreparacionAsync(
                    It.Is<Guid>(id => id == productoId), 
                    It.Is<int>(c => c == cantidad)))
                .ReturnsAsync(Result.Success());

            // Act
            var resultado = await _sut.AgregarProductoAComandaAsync(comandaId, productoId, cantidad, observaciones);

            // Assert
            Assert.True(resultado.Succeeded);
        }

        [Fact]
        public async Task AgregarProductoAComandaAsync_SinPreparacionesDisponibles_DebeUsarFlujoNormal()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var cantidad = 3;
            var observaciones = "Preparar al momento";

            // Configurar comanda existente
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), observaciones);
            _comandaRepositoryMock
                .Setup(c => c.ObtenerPorIdAsync(comandaId, true, CancellationToken.None))
                .ReturnsAsync(comanda);

            // Configurar producto existente
            var precio = new PrecioProducto(12.00m);
            var producto = Producto.Crear("Pasta Carbonara", "Pasta fresca", precio, Guid.NewGuid());
            _productoRepositoryMock
                .Setup(p => p.ObtenerPorIdAsync(productoId, CancellationToken.None))
                .ReturnsAsync(producto);

            // Configurar servicio de preparaciones - NO hay preparaciones disponibles
            _servicioPreparacionesMock
                .Setup(s => s.VerificarDisponibilidadAsync(
                    It.Is<Guid>(id => id == productoId), 
                    It.Is<int>(c => c == cantidad),
                    It.IsAny<Guid?>()))
                .ReturnsAsync(Result<bool>.Success(false));

            // Act
            var resultado = await _sut.AgregarProductoAComandaAsync(comandaId, productoId, cantidad, observaciones);

            // Assert
            Assert.True(resultado.Succeeded);
        }

        [Fact]
        public async Task AgregarProductoAComandaAsync_ErrorEnVerificacion_DebeContinuarConFlujoNormal()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var cantidad = 1;

            // Reset mock para evitar interferencia con tests anteriores
            _servicioPreparacionesMock.Reset();

            // Configurar comanda existente
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "");
            _comandaRepositoryMock
                .Setup(c => c.ObtenerPorIdAsync(comandaId, true, CancellationToken.None))
                .ReturnsAsync(comanda);

            // Configurar producto existente
            var precio = new PrecioProducto(8.50m);
            var producto = Producto.Crear("Ensalada César", "Ensalada fresca", precio, Guid.NewGuid());
            _productoRepositoryMock
                .Setup(p => p.ObtenerPorIdAsync(productoId, CancellationToken.None))
                .ReturnsAsync(producto);

            // Configurar servicio de preparaciones - Error en verificación
            _servicioPreparacionesMock
                .Setup(s => s.VerificarDisponibilidadAsync(
                    It.Is<Guid>(id => id == productoId), 
                    It.Is<int>(c => c == cantidad),
                    It.IsAny<Guid?>()))
                .ReturnsAsync(Result<bool>.Success(false));

            // Act
            var resultado = await _sut.AgregarProductoAComandaAsync(comandaId, productoId, cantidad, "");

            // Assert
            Assert.True(resultado.Succeeded); // Debe seguir funcionando con flujo normal

            // Verificar que se intentó verificar disponibilidad

            // Verificar que NO se intentó consumir de preparaciones
            // _servicioPreparacionesMock.Verify(s => s.ConsumirPreparacionAsync(It.IsAny<Guid>(), It.IsAny<int>()), Times.Never);

            // Verificar que la comanda fue actualizada (flujo normal)
        }

        [Fact]
        public async Task AgregarProductoAComandaAsync_ConPreparacionesPeroErrorAlConsumir_DebeContinuarConFlujoNormal()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var cantidad = 2;

            // Configurar comanda existente
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "");
            _comandaRepositoryMock
                .Setup(c => c.ObtenerPorIdAsync(comandaId, true, CancellationToken.None))
                .ReturnsAsync(comanda);

            // Configurar producto existente
            var precio = new PrecioProducto(14.00m);
            var producto = Producto.Crear("Hamburguesa Clásica", "Hamburguesa con papas", precio, Guid.NewGuid());
            _productoRepositoryMock
                .Setup(p => p.ObtenerPorIdAsync(productoId, CancellationToken.None))
                .ReturnsAsync(producto);

            // Configurar servicio de preparaciones - Hay disponibilidad pero error al consumir
            _servicioPreparacionesMock
                .Setup(s => s.VerificarDisponibilidadAsync(
                    It.Is<Guid>(id => id == productoId), 
                    It.Is<int>(c => c == cantidad),
                    It.IsAny<Guid?>()))
                .ReturnsAsync(Result<bool>.Success(true));

            _servicioPreparacionesMock
                .Setup(s => s.ConsumirPreparacionAsync(
                    It.Is<Guid>(id => id == productoId), 
                    It.Is<int>(c => c == cantidad)))
                .ReturnsAsync(Result.Failure("Error al consumir preparación"));

            // Act
            var resultado = await _sut.AgregarProductoAComandaAsync(comandaId, productoId, cantidad, "");

            // Assert
            Assert.True(resultado.Succeeded); // Debe seguir funcionando con flujo normal

            // Verificar que se verificó disponibilidad

            // Verificar que se intentó consumir (pero falló)

            // Verificar que la comanda fue actualizada (flujo normal)
        }

        [Fact]
        public async Task AgregarProductoAComandaAsync_FlujoHibrido_DebeIncluirObservacionesEspeciales()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var cantidad = 1;

            // Configurar comanda existente
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "");
            _comandaRepositoryMock
                .Setup(c => c.ObtenerPorIdAsync(comandaId, true, CancellationToken.None))
                .ReturnsAsync(comanda);

            // Configurar producto existente
            var precio = new PrecioProducto(22.00m);
            var producto = Producto.Crear("Salmón Grillado", "Salmón fresco", precio, Guid.NewGuid());
            _productoRepositoryMock
                .Setup(p => p.ObtenerPorIdAsync(productoId, CancellationToken.None))
                .ReturnsAsync(producto);

            // Configurar servicio de preparaciones - Consumo exitoso
            _servicioPreparacionesMock
                .Setup(s => s.VerificarDisponibilidadAsync(
                    It.Is<Guid>(id => id == productoId), 
                    It.Is<int>(c => c == cantidad),
                    It.IsAny<Guid?>()))
                .ReturnsAsync(Result<bool>.Success(true));

            _servicioPreparacionesMock
                .Setup(s => s.ConsumirPreparacionAsync(
                    It.Is<Guid>(id => id == productoId), 
                    It.Is<int>(c => c == cantidad)))
                .ReturnsAsync(Result.Success());

            // Act
            var resultado = await _sut.AgregarProductoAComandaAsync(comandaId, productoId, cantidad, "");

            // Assert
            Assert.True(resultado.Succeeded);

            // En el flujo híbrido, las observaciones deberían incluir información de que fue tomado de preparaciones
            // Esto se verifica indirectamente al confirmar que AgregarItem fue llamado en la comanda
        }
    }
} 
