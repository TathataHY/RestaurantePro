namespace RestaurantePro.Domain.UnitTests.Integration
{
    /// <summary>
    /// Tests de integración para verificar el flujo completo de cancelación de reservaciones
    /// cuando se desactiva un cliente.
    /// Demuestra la interacción entre los contextos de Comercial y Operaciones.
    /// </summary>
    public class CancelacionReservacionesTests
    {
        private readonly Mock<IReservacionRepository> _reservacionRepositoryMock = new();
        private readonly Mock<IClienteRepository> _clienteRepositoryMock = new();
        private readonly Mock<IDomainEventRegistry> _eventRegistryMock = new();
        
        private readonly ClienteDesactivado_CancelarReservacionesPendientesHandler _handler;
        private readonly DateTime _fechaActual = DateTime.Now;
        
        public CancelacionReservacionesTests()
        {
            // Inicializar handler
            _handler = new ClienteDesactivado_CancelarReservacionesPendientesHandler(
                _reservacionRepositoryMock.Object,
                _eventRegistryMock.Object);
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
            var horaReservacion1 = new TimeSpan(20, 0, 0); // 8:00 PM
            var horaReservacion2 = new TimeSpan(21, 0, 0); // 9:00 PM
            
            // Usar fechas futuras para las reservaciones
            var fechaFutura1 = DateTime.Now.AddDays(5); // 5 días en el futuro
            var fechaFutura2 = DateTime.Now.AddDays(7); // 7 días en el futuro
            
            var reservacion1 = Reservacion.Crear(
                clienteId, 
                mesaId, 
                fechaFutura1,
                horaReservacion1,
                2,
                "Cena de aniversario");
                
            var reservacion2 = Reservacion.Crear(
                clienteId,
                mesaId,
                fechaFutura2,
                horaReservacion2,
                4,
                "Reunión familiar");
                
            // Establecer IDs de las reservaciones usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(reservacion1, reservacion1Id);
            typeof(EntityBase).GetProperty("Id").SetValue(reservacion2, reservacion2Id);
            
            // Confirmar las reservaciones para que estén en estado confirmado
            reservacion1.Confirmar();
            reservacion2.Confirmar();
            
            // 4. Configurar mocks
            _reservacionRepositoryMock
                .Setup(r => r.ObtenerReservacionesPendientesPorClienteIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Reservacion> { reservacion1, reservacion2 });
                
            _reservacionRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Reservacion>()))
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
                r => r.ObtenerReservacionesPendientesPorClienteIdAsync(clienteId, It.IsAny<CancellationToken>()),
                Times.Once);
                
            // 2. Verificar que se actualizaron ambas reservaciones
            _reservacionRepositoryMock.Verify(
                r => r.ActualizarAsync(It.IsAny<Reservacion>()),
                Times.Exactly(2));
                
            // 3. Verificar que ambas reservaciones fueron canceladas
            Assert.Equal(EstadoReservacion.Cancelada, reservacion1.Estado);
            Assert.Equal(EstadoReservacion.Cancelada, reservacion2.Estado);
            Assert.Contains("Cliente desactivado", reservacion1.MotivoCancelacion);
            Assert.Contains("Cliente desactivado", reservacion2.MotivoCancelacion);
            
            // 4. Verificar que se registró el evento en el log
            _eventRegistryMock.Verify(
                l => l.RegisterAsync(
                    It.IsAny<ClienteDesactivado>(),
                    It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
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
                .Setup(r => r.ObtenerReservacionesPendientesPorClienteIdAsync(clienteId, It.IsAny<CancellationToken>()))
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
                r => r.ObtenerReservacionesPendientesPorClienteIdAsync(clienteId, It.IsAny<CancellationToken>()),
                Times.Once);
                
            // 2. Verificar que no se actualizó ninguna reservación
            _reservacionRepositoryMock.Verify(
                r => r.ActualizarAsync(It.IsAny<Reservacion>()),
                Times.Never);
                
            // 3. Verificar que se registró el evento en el log
            _eventRegistryMock.Verify(
                l => l.RegisterAsync(
                    It.IsAny<ClienteDesactivado>(),
                    It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
        }
    }
} 