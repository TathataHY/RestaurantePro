namespace RestaurantePro.Domain.UnitTests.Operaciones.Services
{
    /// <summary>
    /// Pruebas unitarias para OperacionesServiceFacade
    /// </summary>
    public class OperacionesServiceFacadeTests
    {
        private readonly Mock<IComandaRepository> _comandaRepositoryMock;
        private readonly Mock<IReservacionRepository> _reservacionRepositoryMock;
        private readonly Mock<IMesaRepository> _mesaRepositoryMock;
        private readonly Mock<IProductoRepository> _productoRepositoryMock;
        private readonly NotificationManager _notificationManager;
        private readonly OperacionesServiceFacade _sut;

        public OperacionesServiceFacadeTests()
        {
            _comandaRepositoryMock = new Mock<IComandaRepository>();
            _reservacionRepositoryMock = new Mock<IReservacionRepository>();
            _mesaRepositoryMock = new Mock<IMesaRepository>();
            _productoRepositoryMock = new Mock<IProductoRepository>();
            _notificationManager = new NotificationManager();
            
            _sut = new OperacionesServiceFacade(
                _comandaRepositoryMock.Object,
                _reservacionRepositoryMock.Object,
                _mesaRepositoryMock.Object,
                _productoRepositoryMock.Object,
                _notificationManager);
        }
        
        #region Reservaciones
        
        [Fact]
        public async Task CrearReservacionAsync_ConDatosValidos_DebeRetornarReservacionCreada()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var fecha = DateTime.Now.AddDays(1);
            var cantidadPersonas = 4;
            var observaciones = "Observaciones de prueba";
            
            var mesasDisponibles = new List<Guid> { mesaId };
            
            _reservacionRepositoryMock
                .Setup(r => r.ObtenerMesasDisponiblesAsync(
                    It.IsAny<DateTime>(), 
                    It.IsAny<TimeSpan>(), 
                    It.IsAny<int>(), 
                    It.IsAny<int>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mesasDisponibles);
            
            _reservacionRepositoryMock
                .Setup(r => r.AgregarAsync(It.IsAny<Reservacion>()))
                .Returns(Task.CompletedTask);
            
            _reservacionRepositoryMock
                .Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var result = await _sut.CrearReservacionAsync(
                clienteId, 
                fecha, 
                cantidadPersonas, 
                observaciones);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.ClienteId.Should().Be(clienteId);
            result.Value.FechaReservacion.Should().Be(fecha);
            result.Value.CantidadPersonas.Should().Be(cantidadPersonas);
            result.Value.Observaciones.Should().Be(observaciones);
            result.Value.MesaId.Should().Be(mesaId);
            
            _reservacionRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<Reservacion>()), Times.Once);
            _reservacionRepositoryMock.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task CrearReservacionAsync_FechaEnPasado_DebeRetornarError()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var fecha = DateTime.Now.AddDays(-1); // Fecha en el pasado
            var cantidadPersonas = 4;
            
            // Act
            var result = await _sut.CrearReservacionAsync(
                clienteId, 
                fecha, 
                cantidadPersonas);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Value.Should().BeNull();
            result.Errors.Should().ContainSingle().Which.Message.Should().Contain("fecha");
            
            _reservacionRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<Reservacion>()), Times.Never);
            _reservacionRepositoryMock.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task CrearReservacionAsync_SinMesasDisponibles_DebeRetornarError()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var fecha = DateTime.Now.AddDays(1);
            var cantidadPersonas = 4;
            
            var mesasDisponibles = new List<Guid>(); // Sin mesas disponibles
            
            _reservacionRepositoryMock
                .Setup(r => r.ObtenerMesasDisponiblesAsync(
                    It.IsAny<DateTime>(), 
                    It.IsAny<TimeSpan>(), 
                    It.IsAny<int>(), 
                    It.IsAny<int>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mesasDisponibles);
            
            // Act
            var result = await _sut.CrearReservacionAsync(
                clienteId, 
                fecha, 
                cantidadPersonas);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Value.Should().BeNull();
            result.Errors.Should().ContainSingle().Which.Message.Should().Contain("mesas disponibles");
            
            _reservacionRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<Reservacion>()), Times.Never);
            _reservacionRepositoryMock.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task AsignarMesaAReservacionAsync_ConDatosValidos_DebeRetornarExito()
        {
            // Arrange
            var reservacionId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var reservacion = Reservacion.Crear(
                Guid.NewGuid(), // Mesa actual
                Guid.NewGuid(), // Cliente
                DateTime.Now.AddDays(1),
                TimeSpan.FromMinutes(90),
                4,
                "",
                "",
                "");
            
            _reservacionRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(reservacionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservacion);
            
            _mesaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(mesaId))
                .ReturnsAsync(Mesa.Crear(4, "Mesa de prueba", EstadoMesa.Disponible));
            
            _reservacionRepositoryMock
                .Setup(r => r.VerificarDisponibilidadMesaAsync(
                    mesaId,
                    It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true); // Mesa disponible
            
            _reservacionRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Reservacion>()))
                .Returns(Task.CompletedTask);
            
            _reservacionRepositoryMock
                .Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var result = await _sut.AsignarMesaAReservacionAsync(
                reservacionId, 
                mesaId);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().BeTrue();
            
            _reservacionRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Reservacion>()), Times.Once);
            _reservacionRepositoryMock.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task ActualizarEstadoReservacionAsync_ConEstadoValido_DebeRetornarExito()
        {
            // Arrange
            var reservacionId = Guid.NewGuid();
            var nuevoEstado = EstadoReservacion.Confirmada;
            var reservacion = Reservacion.Crear(
                Guid.NewGuid(), // Mesa
                Guid.NewGuid(), // Cliente
                DateTime.Now.AddDays(1),
                TimeSpan.FromMinutes(90),
                4,
                "",
                "",
                "");
            
            _reservacionRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(reservacionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservacion);
            
            _reservacionRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Reservacion>()))
                .Returns(Task.CompletedTask);
            
            _reservacionRepositoryMock
                .Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var result = await _sut.ActualizarEstadoReservacionAsync(
                reservacionId, 
                nuevoEstado);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().BeTrue();
            
            _reservacionRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Reservacion>()), Times.Once);
            _reservacionRepositoryMock.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task ObtenerReservacionesPorRangoFechasAsync_DebeRetornarReservaciones()
        {
            // Arrange
            var fechaInicio = DateTime.Now;
            var fechaFin = DateTime.Now.AddDays(7);
            var reservaciones = new List<Reservacion>
            {
                Reservacion.Crear(
                    Guid.NewGuid(), // Mesa
                    Guid.NewGuid(), // Cliente
                    DateTime.Now.AddDays(1),
                    TimeSpan.FromMinutes(90),
                    4,
                    "",
                    "",
                    ""),
                Reservacion.Crear(
                    Guid.NewGuid(), // Mesa
                    Guid.NewGuid(), // Cliente
                    DateTime.Now.AddDays(2),
                    TimeSpan.FromMinutes(90),
                    2,
                    "",
                    "",
                    "")
            };
            
            _reservacionRepositoryMock
                .Setup(r => r.ObtenerPorRangoFechasAsync(
                    fechaInicio,
                    fechaFin,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservaciones);
            
            // Act
            var result = await _sut.ObtenerReservacionesPorRangoFechasAsync(
                fechaInicio, 
                fechaFin);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().HaveCount(2);
            result.Value.Should().BeEquivalentTo(reservaciones);
        }
        
        [Fact]
        public async Task ConvertirReservacionAComandaAsync_ConReservacionConfirmada_DebeRetornarComanda()
        {
            // Arrange
            var reservacionId = Guid.NewGuid();
            var meseroId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            
            var reservacion = Reservacion.Crear(
                mesaId,
                clienteId,
                DateTime.Now.AddDays(1),
                TimeSpan.FromMinutes(90),
                4,
                "",
                "",
                "");
            
            // Confirmar la reservación
            reservacion.Confirmar();
            
            _reservacionRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(reservacionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservacion);
            
            _comandaRepositoryMock
                .Setup(r => r.AgregarAsync(It.IsAny<Comanda>()))
                .Returns(Task.CompletedTask);
            
            _reservacionRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Reservacion>()))
                .Returns(Task.CompletedTask);
            
            _comandaRepositoryMock
                .Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            _reservacionRepositoryMock
                .Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var result = await _sut.ConvertirReservacionAComandaAsync(
                reservacionId, 
                meseroId);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.MeseroId.Should().Be(meseroId);
            result.Value.ClienteId.Should().Be(clienteId);
            result.Value.MesaId.Should().Be(mesaId);
            
            _comandaRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<Comanda>()), Times.Once);
            _reservacionRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Reservacion>()), Times.Once);
            _comandaRepositoryMock.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
            _reservacionRepositoryMock.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task ConvertirReservacionAComandaAsync_ConReservacionNoConfirmada_DebeRetornarError()
        {
            // Arrange
            var reservacionId = Guid.NewGuid();
            var meseroId = Guid.NewGuid();
            
            var reservacion = Reservacion.Crear(
                Guid.NewGuid(), // Mesa
                Guid.NewGuid(), // Cliente
                DateTime.Now.AddDays(1),
                TimeSpan.FromMinutes(90),
                4,
                "",
                "",
                "");
            
            // No confirmar la reservación (queda en estado pendiente)
            
            _reservacionRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(reservacionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservacion);
            
            // Act
            var result = await _sut.ConvertirReservacionAComandaAsync(
                reservacionId, 
                meseroId);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Value.Should().BeNull();
            result.Errors.Should().ContainSingle().Which.Message.Should().Contain("confirmadas");
            
            _comandaRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<Comanda>()), Times.Never);
            _reservacionRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Reservacion>()), Times.Never);
            _comandaRepositoryMock.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
            _reservacionRepositoryMock.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        
        #endregion
    }
} 