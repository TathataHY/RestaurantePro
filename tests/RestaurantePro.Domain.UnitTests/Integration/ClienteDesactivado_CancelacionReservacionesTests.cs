namespace RestaurantePro.Domain.UnitTests.Integration
{
    /// <summary>
    /// Tests de integración para verificar el flujo completo de cancelación de reservaciones
    /// cuando se desactiva un cliente.
    /// Demuestra la interacción entre los contextos de Comercial y Operaciones.
    /// </summary>
    public class ClienteDesactivado_CancelacionReservacionesTests
    {
        private readonly Mock<IReservacionRepository> _reservacionRepositoryMock = new();
        private readonly Mock<IClienteRepository> _clienteRepositoryMock = new();
        private readonly Mock<IDomainEventLog> _eventLogMock = new();
        
        private readonly ClienteDesactivado_CancelarReservacionesPendientesHandler _handler;
        private readonly DateTime _fechaActual = new DateTime(2023, 5, 15, 10, 0, 0);
        
        public ClienteDesactivado_CancelacionReservacionesTests()
        {
            // Configurar mock de fecha
            var dateTimeServiceMock = new Mock<IDateTimeService>();
            dateTimeServiceMock.Setup(s => s.Now).Returns(_fechaActual);
            
            // Inicializar handler
            _handler = new ClienteDesactivado_CancelarReservacionesPendientesHandler(
                _reservacionRepositoryMock.Object,
                dateTimeServiceMock.Object,
                _eventLogMock.Object);
        }
        
        [Fact]
        public async Task ClienteDesactivado_ConReservacionesPendientes_DebeCancelarlas()
        {
            // Arrange
            // 1. Crear IDs para el test
            var clienteId = Guid.NewGuid();
            var reservacion1Id = Guid.NewGuid();
            var reservacion2Id = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            
            // 2. Crear cliente
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            
            // Establecer ID del cliente usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(cliente, clienteId);
            
            // 3. Crear reservaciones pendientes para el cliente
            var reservacion1 = Reservacion.Crear(
                clienteId,
                _fechaActual.AddDays(1), // Fecha futura
                _fechaActual.AddDays(1).AddHours(2),
                2,
                "Cena de aniversario");
                
            var reservacion2 = Reservacion.Crear(
                clienteId,
                _fechaActual.AddDays(3), // Fecha futura
                _fechaActual.AddDays(3).AddHours(2),
                4,
                "Reunión familiar");
                
            // Establecer IDs de las reservaciones usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(reservacion1, reservacion1Id);
            typeof(EntityBase).GetProperty("Id").SetValue(reservacion2, reservacion2Id);
            
            // Asignar mesas a las reservaciones para que estén confirmadas
            reservacion1.AsignarMesa(mesaId);
            reservacion2.AsignarMesa(mesaId);
            
            // 4. Configurar mocks
            _reservacionRepositoryMock
                .Setup(r => r.ObtenerPendientesPorClienteAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Reservacion> { reservacion1, reservacion2 });
                
            _reservacionRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Reservacion>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // 5. Desactivar el cliente y capturar el evento
            cliente.Desactivar();
            var evento = cliente.DomainEvents.OfType<ClienteDesactivado>().First();
            
            // Act
            // Procesar el evento con el handler
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            // 1. Verificar que se consultaron las reservaciones pendientes
            _reservacionRepositoryMock.Verify(
                r => r.ObtenerPendientesPorClienteAsync(clienteId, It.IsAny<CancellationToken>()),
                Times.Once);
                
            // 2. Verificar que se actualizaron ambas reservaciones
            _reservacionRepositoryMock.Verify(
                r => r.ActualizarAsync(It.IsAny<Reservacion>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
                
            // 3. Verificar que ambas reservaciones fueron canceladas
            Assert.Equal(EstadoReservacion.Cancelada, reservacion1.Estado);
            Assert.Equal(EstadoReservacion.Cancelada, reservacion2.Estado);
            Assert.Contains("Cliente desactivado", reservacion1.MotivoCancelacion);
            Assert.Contains("Cliente desactivado", reservacion2.MotivoCancelacion);
            
            // 4. Verificar que se registró el evento en el log
            _eventLogMock.Verify(
                l => l.LogEvent(
                    It.IsAny<ClienteDesactivado>(),
                    It.Is<string>(s => s.Contains("reservaciones canceladas")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task ClienteDesactivado_SinReservacionesPendientes_NoDebeCancelarNada()
        {
            // Arrange
            // 1. Crear ID del cliente
            var clienteId = Guid.NewGuid();
            
            // 2. Crear cliente
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            
            // Establecer ID del cliente usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(cliente, clienteId);
            
            // 3. Configurar mock para devolver lista vacía de reservaciones
            _reservacionRepositoryMock
                .Setup(r => r.ObtenerPendientesPorClienteAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Reservacion>());
            
            // 4. Desactivar el cliente y capturar el evento
            cliente.Desactivar();
            var evento = cliente.DomainEvents.OfType<ClienteDesactivado>().First();
            
            // Act
            // Procesar el evento con el handler
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            // 1. Verificar que se consultaron las reservaciones pendientes
            _reservacionRepositoryMock.Verify(
                r => r.ObtenerPendientesPorClienteAsync(clienteId, It.IsAny<CancellationToken>()),
                Times.Once);
                
            // 2. Verificar que no se actualizó ninguna reservación
            _reservacionRepositoryMock.Verify(
                r => r.ActualizarAsync(It.IsAny<Reservacion>(), It.IsAny<CancellationToken>()),
                Times.Never);
                
            // 3. Verificar que se registró el evento en el log
            _eventLogMock.Verify(
                l => l.LogEvent(
                    It.IsAny<ClienteDesactivado>(),
                    It.Is<string>(s => s.Contains("No se encontraron reservaciones pendientes")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
} 