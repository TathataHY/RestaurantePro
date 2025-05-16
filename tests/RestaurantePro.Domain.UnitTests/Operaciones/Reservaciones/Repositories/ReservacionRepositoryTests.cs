namespace RestaurantePro.Domain.UnitTests.Operaciones.Reservaciones.Repositories
{
    public class ReservacionRepositoryTests
    {
        private readonly Mock<IReservacionRepository> _mockRepository;
        private readonly List<Reservacion> _reservaciones;
        private readonly Guid _clienteId;
        private readonly Guid _mesaId;

        public ReservacionRepositoryTests()
        {
            _mockRepository = new Mock<IReservacionRepository>();
            _clienteId = Guid.NewGuid();
            _mesaId = Guid.NewGuid();

            // Crear datos de prueba
            _reservaciones = new List<Reservacion>
            {
                Reservacion.Crear(_clienteId, _mesaId, DateTime.Now.AddDays(1), new TimeSpan(19, 0, 0), 2, "Celebración de aniversario"),
                Reservacion.Crear(_clienteId, _mesaId, DateTime.Now.AddDays(2), new TimeSpan(20, 0, 0), 4, "Cena familiar"),
                Reservacion.Crear(Guid.NewGuid(), Guid.NewGuid(), DateTime.Now.AddDays(1), new TimeSpan(21, 0, 0), 6, "Reunión de negocios")
            };

            // Confirmar una reservación para pruebas
            _reservaciones[0].Confirmar();

            // Cancelar una reservación para pruebas
            _reservaciones[2].Cancelar("El cliente canceló la reserva");
        }

        [Fact]
        public async Task ObtenerTodasAsync_DebeRetornarTodasLasReservaciones()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.ObtenerTodasAsync())
                .ReturnsAsync(_reservaciones);

            // Act
            var resultado = await _mockRepository.Object.ObtenerTodasAsync();

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(3);
            resultado.Should().BeEquivalentTo(_reservaciones);
        }

        [Fact]
        public async Task ObtenerPorIdAsync_IdExistente_DebeRetornarReservacion()
        {
            // Arrange
            var reservacionId = _reservaciones[0].Id;
            var reservacionEsperada = _reservaciones[0];

            _mockRepository.Setup(repo => repo.ObtenerPorIdAsync(reservacionId))
                .ReturnsAsync(reservacionEsperada);

            // Act
            var resultado = await _mockRepository.Object.ObtenerPorIdAsync(reservacionId);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().BeSameAs(reservacionEsperada);
            _mockRepository.Verify(repo => repo.ObtenerPorIdAsync(reservacionId), Times.Once);
        }

        [Fact]
        public async Task ObtenerPorFechaAsync_FechaConReservaciones_DebeRetornarReservacionesEnEsaFecha()
        {
            // Arrange
            var fecha = DateTime.Now.AddDays(1).Date;
            var reservacionesEsperadas = _reservaciones.Where(r => r.Fecha.Date == fecha).ToList();

            _mockRepository.Setup(repo => repo.ObtenerPorFechaAsync(fecha))
                .ReturnsAsync(reservacionesEsperadas);

            // Act
            var resultado = await _mockRepository.Object.ObtenerPorFechaAsync(fecha);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(2); // Tenemos 2 reservaciones en esa fecha
            resultado.All(r => r.Fecha.Date == fecha).Should().BeTrue();
        }

        [Fact]
        public async Task ObtenerReservacionesActivasPorMesaYFechaAsync_MesaYFechaConReservaciones_DebeRetornarReservacionesActivas()
        {
            // Arrange
            var fecha = DateTime.Now.AddDays(1).Date;
            var reservacionesActivas = _reservaciones
                .Where(r => r.MesaId == _mesaId && r.Fecha.Date == fecha &&
                           (r.Estado == EstadoReservacion.Pendiente || r.Estado == EstadoReservacion.Confirmada))
                .ToList();

            _mockRepository.Setup(repo => repo.ObtenerReservacionesActivasPorMesaYFechaAsync(_mesaId, fecha))
                .ReturnsAsync(reservacionesActivas);

            // Act
            var resultado = await _mockRepository.Object.ObtenerReservacionesActivasPorMesaYFechaAsync(_mesaId, fecha);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(1); // Solo tenemos 1 reservación activa para esa mesa y fecha
            resultado.All(r => r.MesaId == _mesaId && r.Fecha.Date == fecha).Should().BeTrue();
            resultado.All(r => r.Estado == EstadoReservacion.Confirmada || r.Estado == EstadoReservacion.Pendiente).Should().BeTrue();
        }

        [Fact]
        public async Task ObtenerPorClienteAsync_ClienteConReservaciones_DebeRetornarReservacionesDelCliente()
        {
            // Arrange
            var reservacionesCliente = _reservaciones.Where(r => r.ClienteId == _clienteId).ToList();

            _mockRepository.Setup(repo => repo.ObtenerPorClienteAsync(_clienteId))
                .ReturnsAsync(reservacionesCliente);

            // Act
            var resultado = await _mockRepository.Object.ObtenerPorClienteAsync(_clienteId);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(2); // El cliente tiene 2 reservaciones
            resultado.All(r => r.ClienteId == _clienteId).Should().BeTrue();
        }

        [Fact]
        public async Task ObtenerPorEstadoAsync_EstadoConfirmado_DebeRetornarReservacionesConfirmadas()
        {
            // Arrange
            var estado = EstadoReservacion.Confirmada;
            var reservacionesConfirmadas = _reservaciones.Where(r => r.Estado == estado).ToList();

            _mockRepository.Setup(repo => repo.ObtenerPorEstadoAsync(estado))
                .ReturnsAsync(reservacionesConfirmadas);

            // Act
            var resultado = await _mockRepository.Object.ObtenerPorEstadoAsync(estado);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(1); // Solo hay 1 reservación confirmada
            resultado.All(r => r.Estado == estado).Should().BeTrue();
        }

        [Fact]
        public async Task ExisteReservacionEnRangoHorarioAsync_MesaYRangoOcupado_DebeRetornarTrue()
        {
            // Arrange
            var fecha = DateTime.Now.AddDays(1).Date;
            var horaInicio = new TimeSpan(18, 30, 0); // 6:30 PM
            var horaFin = new TimeSpan(20, 30, 0); // 8:30 PM

            // Hay una reserva a las 7:00 PM que entra en este rango
            _mockRepository.Setup(repo => repo.ExisteReservacionEnRangoHorarioAsync(_mesaId, fecha, horaInicio, horaFin))
                .ReturnsAsync(true);

            // Act
            var resultado = await _mockRepository.Object.ExisteReservacionEnRangoHorarioAsync(_mesaId, fecha, horaInicio, horaFin);

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task AgregarAsync_ReservacionValida_DebeAgregarCorrectamente()
        {
            // Arrange
            var nuevaReservacion = Reservacion.Crear(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTime.Now.AddDays(3),
                new TimeSpan(19, 30, 0),
                2,
                "Nueva reserva");

            _mockRepository.Setup(repo => repo.AgregarAsync(nuevaReservacion))
                .Returns(Task.CompletedTask);

            // Act
            await _mockRepository.Object.AgregarAsync(nuevaReservacion);

            // Assert
            _mockRepository.Verify(repo => repo.AgregarAsync(nuevaReservacion), Times.Once);
        }

        [Fact]
        public async Task ActualizarAsync_ReservacionExistente_DebeActualizarCorrectamente()
        {
            // Arrange
            var reservacion = _reservaciones[1]; // Reservación pendiente
            reservacion.Confirmar(); // La confirmamos

            _mockRepository.Setup(repo => repo.ActualizarAsync(reservacion))
                .Returns(Task.CompletedTask);

            // Act
            await _mockRepository.Object.ActualizarAsync(reservacion);

            // Assert
            _mockRepository.Verify(repo => repo.ActualizarAsync(reservacion), Times.Once);
            reservacion.Estado.Should().Be(EstadoReservacion.Confirmada);
        }

        [Fact]
        public async Task EliminarAsync_ReservacionExistente_DebeEliminarCorrectamente()
        {
            // Arrange
            var reservacionId = _reservaciones[2].Id; // Reservación cancelada

            _mockRepository.Setup(repo => repo.EliminarAsync(reservacionId))
                .Returns(Task.CompletedTask);

            // Act
            await _mockRepository.Object.EliminarAsync(reservacionId);

            // Assert
            _mockRepository.Verify(repo => repo.EliminarAsync(reservacionId), Times.Once);
        }
    }
}
