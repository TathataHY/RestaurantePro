namespace RestaurantePro.Domain.UnitTests.Operaciones.EventHandlers
{
    public class ClienteDesactivado_CancelarReservacionesPendientesHandlerTests
    {
        private readonly Mock<IReservacionRepository> _reservacionRepositoryMock;
        private readonly Mock<IDomainEventLog> _eventLogMock;
        
        private readonly ClienteDesactivado_CancelarReservacionesPendientesHandler _handler;
        
        // Datos de prueba
        private readonly Guid _clienteId = Guid.NewGuid();
        private readonly string _nombreCliente = "Juan Pérez";
        
        public ClienteDesactivado_CancelarReservacionesPendientesHandlerTests()
        {
            _reservacionRepositoryMock = new Mock<IReservacionRepository>();
            _eventLogMock = new Mock<IDomainEventLog>();
            
            _handler = new ClienteDesactivado_CancelarReservacionesPendientesHandler(
                _reservacionRepositoryMock.Object,
                _eventLogMock.Object);
        }
        
        [Fact]
        public async Task Handle_ConReservacionesPendientes_DebeCancelarlas()
        {
            // Arrange
            var evento = new ClienteDesactivado(_clienteId, _nombreCliente);
            
            // Crear reservaciones de prueba usando los métodos de fábrica
            var fechaReservacion = DateTime.Now.AddDays(5);
            var horaReservacion = new TimeSpan(20, 0, 0); // 8:00 PM
            
            var mesaId1 = Guid.NewGuid();
            var mesaId2 = Guid.NewGuid();
            var mesaId3 = Guid.NewGuid();
            
            var reservacionPendiente = Reservacion.Crear(_clienteId, mesaId1, fechaReservacion, horaReservacion, 4, "Reserva pendiente");
            typeof(EntityBase).GetProperty("Id").SetValue(reservacionPendiente, Guid.NewGuid());
            
            var reservacionConfirmada = Reservacion.Crear(_clienteId, mesaId2, fechaReservacion, horaReservacion, 6, "Reserva confirmada");
            typeof(EntityBase).GetProperty("Id").SetValue(reservacionConfirmada, Guid.NewGuid());
            reservacionConfirmada.Confirmar();
            
            var reservacionCancelada = Reservacion.Crear(_clienteId, mesaId3, fechaReservacion, horaReservacion, 2, "Reserva a cancelar");
            typeof(EntityBase).GetProperty("Id").SetValue(reservacionCancelada, Guid.NewGuid());
            reservacionCancelada.Cancelar("Cancelada previamente");
            
            var reservaciones = new List<Reservacion>
            {
                reservacionPendiente,
                reservacionConfirmada,
                reservacionCancelada
            };
            
            // Configurar mocks
            _reservacionRepositoryMock
                .Setup(r => r.ObtenerReservacionesPendientesPorClienteIdAsync(_clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservaciones);
                
            _reservacionRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Reservacion>()))
                .Returns(Task.CompletedTask);
                
            // Act
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            // Verificar que se llamó a actualizar 2 veces (para las reservaciones pendiente y confirmada)
            _reservacionRepositoryMock.Verify(
                r => r.ActualizarAsync(It.IsAny<Reservacion>()),
                Times.Exactly(2));
                
            // Verificar que se actualizaron las reservaciones correctas
            _reservacionRepositoryMock.Verify(
                r => r.ActualizarAsync(reservacionPendiente),
                Times.Once);
                
            _reservacionRepositoryMock.Verify(
                r => r.ActualizarAsync(reservacionConfirmada),
                Times.Once);
                
            // Verificar que NO se actualizó la reservación ya cancelada
            _reservacionRepositoryMock.Verify(
                r => r.ActualizarAsync(reservacionCancelada),
                Times.Never);
                
            // Verificar que se registró el evento correctamente
            _eventLogMock.Verify(
                l => l.LogEvent(
                    evento,
                    It.Is<string>(s => s.Contains("Se cancelaron 2 reservaciones")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
                
            // Verificar que el estado de las reservaciones ahora es Cancelada
            Assert.Equal(EstadoReservacion.Cancelada, reservacionPendiente.Estado);
            Assert.Equal(EstadoReservacion.Cancelada, reservacionConfirmada.Estado);
        }
        
        [Fact]
        public async Task Handle_SinReservacionesPendientes_NoDebeHacerNada()
        {
            // Arrange
            var evento = new ClienteDesactivado(_clienteId, _nombreCliente);
            
            // Configurar mocks para devolver lista vacía
            _reservacionRepositoryMock
                .Setup(r => r.ObtenerReservacionesPendientesPorClienteIdAsync(_clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Reservacion>());
                
            // Act
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            // Verificar que NO se llamó a actualizar ninguna reservación
            _reservacionRepositoryMock.Verify(
                r => r.ActualizarAsync(It.IsAny<Reservacion>()),
                Times.Never);
                
            // Verificar que se registró el evento correctamente
            _eventLogMock.Verify(
                l => l.LogEvent(
                    evento,
                    It.Is<string>(s => s.Contains("No se encontraron reservaciones pendientes")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task Handle_ErrorAlProcesar_DebeRegistrarError()
        {
            // Arrange
            var evento = new ClienteDesactivado(_clienteId, _nombreCliente);
            
            // Configurar mock para lanzar excepción
            _reservacionRepositoryMock
                .Setup(r => r.ObtenerReservacionesPendientesPorClienteIdAsync(_clienteId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error de prueba"));
                
            // Act
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            // Verificar que se registró el error correctamente
            _eventLogMock.Verify(
                l => l.LogEvent(
                    evento,
                    It.Is<string>(s => s.Contains("Error al cancelar reservaciones")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
} 