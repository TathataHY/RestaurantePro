namespace RestaurantePro.Domain.UnitTests.Integration
{
    /// <summary>
    /// Tests de integración para verificar el flujo completo de acumulación de puntos
    /// de fidelización cuando se finaliza una comanda.
    /// Demuestra la interacción entre los contextos de Comercial y Operaciones.
    /// </summary>
    public class ComandaFinalizada_AcumulacionPuntosFidelizacionTests
    {
        private readonly Mock<IClienteRepository> _clienteRepositoryMock = new();
        private readonly Mock<IComandaRepository> _comandaRepositoryMock = new();
        private readonly Mock<IServicioFidelizacion> _servicioFidelizacionMock = new();
        private readonly Mock<IDomainEventLog> _eventLogMock = new();
        
        private readonly ComandaFinalizada_AcumularPuntosHandler _handler;
        private readonly DateTime _fechaActual = new DateTime(2023, 5, 15, 10, 0, 0);
        
        public ComandaFinalizada_AcumulacionPuntosFidelizacionTests()
        {
            // Configurar mock de fecha
            var dateTimeServiceMock = new Mock<IDateTimeService>();
            dateTimeServiceMock.Setup(s => s.Now).Returns(_fechaActual);
            
            // Inicializar handler
            _handler = new ComandaFinalizada_AcumularPuntosHandler(
                _clienteRepositoryMock.Object,
                _comandaRepositoryMock.Object,
                Mock.Of<ITarjetaFidelizacionRepository>(), // No se usa directamente en la implementación actual
                _servicioFidelizacionMock.Object,
                dateTimeServiceMock.Object,
                _eventLogMock.Object);
        }
        
        [Fact]
        public async Task ComandaFinalizada_ClienteActivo_DebeAcumularPuntos()
        {
            // Arrange
            // 1. Crear IDs para el test
            var comandaId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var meseroId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            
            // 2. Crear cliente
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            
            // Establecer ID del cliente usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(cliente, clienteId);
            
            // 3. Crear comanda 
            var comanda = Comanda.Crear(mesaId, meseroId, clienteId);
            
            // Establecer ID de la comanda usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(comanda, comandaId);
            
            // 4. Agregar productos a la comanda
            comanda.AgregarProducto(productoId, 2, 150.0m); // 2 unidades a 150 cada una = 300
            comanda.AgregarProducto(Guid.NewGuid(), 1, 70.0m); // 1 unidad a 70 = 70
            
            // Total comanda: 370
            decimal montoTotal = 370.0m;
            
            // 5. Avanzar la comanda por todos los estados hasta Finalizada
            comanda.ActualizarEstado(EstadoComanda.EnProceso);
            comanda.ActualizarEstado(EstadoComanda.Lista);
            comanda.ActualizarEstado(EstadoComanda.Entregada);
            comanda.ActualizarEstado(EstadoComanda.Finalizada);
            
            // 6. Configurar mocks
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
                
            _comandaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);
                
            _servicioFidelizacionMock
                .Setup(s => s.AcumularPuntosAsync(clienteId, comandaId, montoTotal))
                .Returns(Task.CompletedTask);
            
            // 7. Capturar el evento ComandaFinalizada y cambiarlo por uno creado manualmente
            // ya que necesitamos controlar el monto total
            var eventoComandaFinalizada = new ComandaFinalizada(comandaId, montoTotal);
            
            // Act
            // Procesar el evento con el handler
            await _handler.Handle(eventoComandaFinalizada, CancellationToken.None);
            
            // Assert
            // 1. Verificar que se consultó al cliente
            _clienteRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()),
                Times.Once);
                
            // 2. Verificar que se consultó a la comanda
            _comandaRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()),
                Times.Once);
                
            // 3. Verificar que se llamó al servicio de fidelización
            _servicioFidelizacionMock.Verify(
                s => s.AcumularPuntosAsync(clienteId, comandaId, montoTotal),
                Times.Once);
                
            // 4. Verificar que se registró el evento en el log
            _eventLogMock.Verify(
                l => l.LogEvent(
                    It.IsAny<ComandaFinalizada>(),
                    It.Is<string>(s => s.Contains("Se procesó acumulación de puntos")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task ComandaFinalizada_ClienteInactivo_NoDebeAcumularPuntos()
        {
            // Arrange
            // 1. Crear IDs para el test
            var comandaId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var meseroId = Guid.NewGuid();
            
            // 2. Crear cliente inactivo
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678");
            cliente.Desactivar(); // Desactivar cliente
            
            // Establecer ID del cliente usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(cliente, clienteId);
            
            // 3. Crear comanda 
            var comanda = Comanda.Crear(mesaId, meseroId, clienteId);
            
            // Establecer ID de la comanda usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(comanda, comandaId);
            
            // 4. Agregar productos a la comanda
            comanda.AgregarProducto(Guid.NewGuid(), 2, 150.0m);
            
            // 5. Avanzar la comanda por todos los estados hasta Finalizada
            comanda.ActualizarEstado(EstadoComanda.EnProceso);
            comanda.ActualizarEstado(EstadoComanda.Lista);
            comanda.ActualizarEstado(EstadoComanda.Entregada);
            comanda.ActualizarEstado(EstadoComanda.Finalizada);
            
            // 6. Configurar mocks
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
                
            _comandaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);
            
            // 7. Crear evento de comanda finalizada
            var eventoComandaFinalizada = new ComandaFinalizada(comandaId, 300.0m);
            
            // Act
            // Procesar el evento con el handler
            await _handler.Handle(eventoComandaFinalizada, CancellationToken.None);
            
            // Assert
            // 1. Verificar que se consultó al cliente y la comanda
            _clienteRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()),
                Times.Once);
                
            _comandaRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()),
                Times.Once);
            
            // 2. Verificar que NO se llamó al servicio de fidelización
            _servicioFidelizacionMock.Verify(
                s => s.AcumularPuntosAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>()),
                Times.Never);
                
            // 3. Verificar que se registró el evento en el log
            _eventLogMock.Verify(
                l => l.LogEvent(
                    It.IsAny<ComandaFinalizada>(),
                    It.Is<string>(s => s.Contains("no encontrado o inactivo")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
} 