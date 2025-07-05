namespace RestaurantePro.Domain.UnitTests.Integration.BetweenContexts.Comercial_Operaciones
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
        private readonly Mock<IDomainEventRegistry> _eventRegistryMock = new();
        
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
                _eventRegistryMock.Object);
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
            var clienteNombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(clienteNombre, "juan@example.com", "612345678", DateTime.Now.AddYears(-30));
            
            // Establecer ID del cliente usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(cliente, clienteId);
            
            // 3. Crear comanda (usar el orden correcto de parámetros: meseroId, clienteId, mesaId)
            var comanda = Comanda.Crear(meseroId, clienteId, mesaId);
            
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
                .Setup(r => r.ObtenerPorIdAsync(clienteId, It.Is<CancellationToken>(c => true)))
                .ReturnsAsync(cliente);
                
            _comandaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(comandaId, It.Is<CancellationToken>(c => true)))
                .ReturnsAsync(comanda);
                
            _servicioFidelizacionMock
                .Setup(s => s.AcumularPuntosAsync(clienteId, comandaId, montoTotal))
                .ReturnsAsync(Result.Success(37)); // Devolver un resultado exitoso con los puntos acumulados
            
            // 7. Capturar el evento ComandaFinalizada y cambiarlo por uno creado manualmente
            // ya que necesitamos controlar el monto total
            var eventoComandaFinalizada = new ComandaFinalizada(comandaId, montoTotal);
            
            // Act
            // Procesar el evento con el handler
            await _handler.Handle(eventoComandaFinalizada, CancellationToken.None);
            
            // Assert
            // 1. Verificar que se consultó al cliente
            _clienteRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(clienteId, It.Is<CancellationToken>(c => true)),
                Times.Once);
                
            // 2. Verificar que se consultó a la comanda
            _comandaRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(comandaId, It.Is<CancellationToken>(c => true)),
                Times.Once);
                
            // 3. Verificar que se llamó al servicio de fidelización
            _servicioFidelizacionMock.Verify(
                s => s.AcumularPuntosAsync(clienteId, comandaId, montoTotal),
                Times.Once);
                
            // 4. Verificar que se registró el evento en el log
            _eventRegistryMock.Verify(
                l => l.RegisterAsync(
                    It.Is<ComandaFinalizada>(e => true),
                    It.Is<CancellationToken>(c => true)),
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
            var clienteNombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(clienteNombre, "juan@example.com", "612345678", DateTime.Now.AddYears(-30));
            cliente.Desactivar(); // Desactivar cliente
            
            // Establecer ID del cliente usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(cliente, clienteId);
            
            // 3. Crear comanda (usar el orden correcto de parámetros: meseroId, clienteId, mesaId) 
            var comanda = Comanda.Crear(meseroId, clienteId, mesaId);
            
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
                .Setup(r => r.ObtenerPorIdAsync(clienteId, It.Is<CancellationToken>(c => true)))
                .ReturnsAsync(cliente);
                
            _comandaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(comandaId, It.Is<CancellationToken>(c => true)))
                .ReturnsAsync(comanda);
            
            // 7. Crear evento de comanda finalizada
            var eventoComandaFinalizada = new ComandaFinalizada(comandaId, 300.0m);
            
            // Act
            // Procesar el evento con el handler
            await _handler.Handle(eventoComandaFinalizada, CancellationToken.None);
            
            // Assert
            // 1. Verificar que se consultó al cliente y la comanda
            _clienteRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(clienteId, It.Is<CancellationToken>(c => true)),
                Times.Once);
                
            _comandaRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(comandaId, It.Is<CancellationToken>(c => true)),
                Times.Once);
            
            // 2. Verificar que NO se llamó al servicio de fidelización
            _servicioFidelizacionMock.Verify(
                s => s.AcumularPuntosAsync(It.Is<Guid>(g => true), It.Is<Guid>(g => true), It.Is<decimal>(d => true)),
                Times.Never);
                
            // 3. Verificar que se registró el evento en el log
            _eventRegistryMock.Verify(
                l => l.RegisterAsync(
                    It.Is<ComandaFinalizada>(e => true),
                    It.Is<CancellationToken>(c => true)),
                Times.Once);
        }
    }
} 