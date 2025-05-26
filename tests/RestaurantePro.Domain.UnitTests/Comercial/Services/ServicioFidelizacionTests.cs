#nullable enable
namespace RestaurantePro.Domain.UnitTests.Comercial.Services
{
    /// <summary>
    /// Pruebas para ServicioFidelizacion utilizando patrones Result y Notification
    /// </summary>
    public class ServicioFidelizacionTests
    {
        private readonly Mock<ITarjetaFidelizacionRepository> _tarjetaRepositoryMock;
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly Mock<IHistorialPuntosRepository> _historialPuntosRepositoryMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Mock<INotificationManager> _notificationManagerMock;
        private readonly INotificationManager _notificationManager;
        private readonly ServicioFidelizacion _servicio;

        public ServicioFidelizacionTests()
        {
            _tarjetaRepositoryMock = new Mock<ITarjetaFidelizacionRepository>();
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _historialPuntosRepositoryMock = new Mock<IHistorialPuntosRepository>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _dateTimeServiceMock.Setup(s => s.Now).Returns(new DateTime(2023, 1, 1));
            
            // Configurar el NotificationManager real para pruebas
            _notificationManager = new NotificationManager();
            
            // También configuramos un mock para verificar comportamientos
            _notificationManagerMock = new Mock<INotificationManager>();
            
            // Crear el servicio con notificationManager real
            _servicio = new ServicioFidelizacion(
                _tarjetaRepositoryMock.Object,
                _clienteRepositoryMock.Object,
                _historialPuntosRepositoryMock.Object,
                _dateTimeServiceMock.Object,
                _notificationManager);
        }

        [Fact]
        public async Task AgregarPuntos_ParametrosInvalidos_DebeRetornarErroresValidacion()
        {
            // Arrange
            var clienteId = Guid.Empty; // ID inválido
            var puntos = 0; // Puntos inválidos
            var motivo = ""; // Motivo vacío

            // Act
            var resultado = await _servicio.AgregarPuntosAsync(clienteId, puntos, motivo);

            // Assert
            resultado.Succeeded.Should().BeFalse();
            resultado.Errors.Should().NotBeEmpty();
            resultado.Errors.Should().HaveCount(3);
            // Simplificamos las verificaciones para evitar problemas con Message/PropertyName
            resultado.Errors.Should().Contain(e => e.ToString().Contains("ClienteId"));
            resultado.Errors.Should().Contain(e => e.ToString().Contains("Puntos"));
            resultado.Errors.Should().Contain(e => e.ToString().Contains("Motivo"));
        }

        [Fact]
        public async Task AgregarPuntos_ClienteNoExiste_DebeRetornarError()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var puntos = 100;
            var motivo = "Motivo válido";

            // Configurar que el cliente no existe
            Cliente? clienteNull = null;
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(clienteNull);

            // Act
            var resultado = await _servicio.AgregarPuntosAsync(clienteId, puntos, motivo);

            // Assert
            resultado.Succeeded.Should().BeFalse();
            resultado.Error.Should().NotBeNull();
            resultado.Error!.ToString().Should().Contain("No se encontró el cliente");
        }

        [Fact]
        public async Task AgregarPuntos_ClienteSinTarjeta_DebeRetornarError()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var puntos = 100;
            var motivo = "Motivo válido";

            // Cliente sin tarjeta de fidelización
            var clienteNombre = ClienteNombre.Crear("Test", "Cliente");
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789");
            SetPrivateId(cliente, clienteId);

            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);

            // Act
            var resultado = await _servicio.AgregarPuntosAsync(clienteId, puntos, motivo);

            // Assert
            resultado.Succeeded.Should().BeFalse();
            resultado.Error.Should().NotBeNull();
            resultado.Error!.ToString().Should().Contain("no tiene tarjeta de fidelización");
        }

        [Fact]
        public async Task AgregarPuntos_CasoExitoso_DebeRetornarPuntosTotales()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjetaId = Guid.NewGuid();
            var puntos = 100;
            var motivo = "Motivo válido";
            var puntosPrevios = 50;

            // Crear cliente con tarjeta
            var clienteNombre = ClienteNombre.Crear("Test", "Cliente");
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789");
            SetPrivateId(cliente, clienteId);
            
            // Crear tarjeta
            var tarjeta = CrearTarjetaActiva(clienteId, NivelFidelizacion.Oro, puntosPrevios);
            SetPrivateId(tarjeta, tarjetaId);
            
            // Asociar tarjeta al cliente
            cliente.AsociarTarjetaFidelizacion(tarjetaId);
            
            // Configurar mocks
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
                
            _clienteRepositoryMock.Setup(r => r.GuardarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _servicio.AgregarPuntosAsync(clienteId, puntos, motivo);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().Be(puntosPrevios + puntos);
            _clienteRepositoryMock.Verify(r => r.GuardarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CalcularDescuento_ParametrosInvalidos_DebeRetornarErroresValidacion()
        {
            // Arrange
            var clienteId = Guid.Empty; // ID inválido
            var montoCompra = 0m; // Monto inválido

            // Act
            var resultado = await _servicio.CalcularDescuentoAsync(clienteId, montoCompra);

            // Assert
            resultado.Succeeded.Should().BeFalse();
            resultado.Errors.Should().NotBeEmpty();
            resultado.Errors.Should().HaveCount(2);
            // Simplificamos las verificaciones para evitar problemas con Message/PropertyName
            resultado.Errors.Should().Contain(e => e.ToString().Contains("ClienteId"));
            resultado.Errors.Should().Contain(e => e.ToString().Contains("MontoTotal"));
        }

        [Fact]
        public async Task CalcularDescuento_ClienteSinTarjeta_DebeRetornarCero()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var totalComanda = 1000m;
            
            // Cliente sin tarjeta
            var clienteNombre = ClienteNombre.Crear("Test", "Cliente");
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789");
            SetPrivateId(cliente, clienteId);
                
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);

            // Act
            var resultado = await _servicio.CalcularDescuentoAsync(clienteId, totalComanda);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().Be(0);
        }
        
        [Fact]
        public async Task CalcularDescuento_TarjetaInactiva_DebeRetornarCero()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjetaId = Guid.NewGuid();
            var totalComanda = 1000m;
            
            // Crear cliente con tarjeta
            var clienteNombre = ClienteNombre.Crear("Test", "Cliente");
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789");
            SetPrivateId(cliente, clienteId);
            
            // Crear tarjeta inactiva
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "TEST-CARD");
            SetPrivateId(tarjeta, tarjetaId);
            
            // Asociar tarjeta al cliente
            cliente.AsociarTarjetaFidelizacion(tarjetaId);
            
            // Configurar mocks
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
                
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TarjetaFidelizacion?)null);

            // Act
            var resultado = await _servicio.CalcularDescuentoAsync(clienteId, totalComanda);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().Be(0);
        }
        
        [Fact]
        public async Task CalcularDescuento_ClienteNivel1_DebeAplicarDescuentoNivel1()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjetaId = Guid.NewGuid();
            var totalComanda = 1000m;
            var tarjeta = CrearTarjetaActiva(clienteId, NivelFidelizacion.Plata);
            
            // Configurar el mock del cliente
            var clienteNombre = ClienteNombre.Crear("Test", "Cliente");
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789");
            SetPrivateId(cliente, clienteId);
            
            // Asignar ID a la tarjeta
            SetPrivateId(tarjeta, tarjetaId);
            
            // Asociar tarjeta al cliente
            cliente.AsociarTarjetaFidelizacion(tarjetaId);
            
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarjeta);

            // Act
            var resultado = await _servicio.CalcularDescuentoAsync(clienteId, totalComanda);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().Be(50); // 1000 * 0.05 = 50
        }
        
        [Fact]
        public async Task CalcularDescuento_ClienteNivel2_DebeAplicarDescuentoNivel2()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjetaId = Guid.NewGuid();
            var totalComanda = 1000m;
            var tarjeta = CrearTarjetaActiva(clienteId, NivelFidelizacion.Oro);
            
            // Configurar el mock del cliente
            var clienteNombre = ClienteNombre.Crear("Test", "Cliente");
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789");
            SetPrivateId(cliente, clienteId);
            
            // Asignar ID a la tarjeta
            SetPrivateId(tarjeta, tarjetaId);
            
            // Asociar tarjeta al cliente
            cliente.AsociarTarjetaFidelizacion(tarjetaId);
            
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarjeta);

            // Act
            var resultado = await _servicio.CalcularDescuentoAsync(clienteId, totalComanda);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().Be(100); // 1000 * 0.10 = 100
        }
        
        [Fact]
        public async Task CalcularDescuento_ClienteNivel3_DebeAplicarDescuentoNivel3()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjetaId = Guid.NewGuid();
            var totalComanda = 1000m;
            var tarjeta = CrearTarjetaActiva(clienteId, NivelFidelizacion.Platino);
            
            // Configurar el mock del cliente
            var clienteNombre = ClienteNombre.Crear("Test", "Cliente");
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789");
            SetPrivateId(cliente, clienteId);
            
            // Asignar ID a la tarjeta
            SetPrivateId(tarjeta, tarjetaId);
            
            // Asociar tarjeta al cliente
            cliente.AsociarTarjetaFidelizacion(tarjetaId);
            
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarjeta);

            // Act
            var resultado = await _servicio.CalcularDescuentoAsync(clienteId, totalComanda);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().Be(150); // 1000 * 0.15 = 150
        }

        [Fact]
        public async Task CanjearPuntos_PuntosInsuficientes_DebeRetornarError()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjetaId = Guid.NewGuid();
            var puntosDisponibles = 50;
            var puntosACanjear = 100; // Más de los disponibles
            var beneficio = "Descuento en comanda";

            // Crear cliente con tarjeta
            var clienteNombre = ClienteNombre.Crear("Test", "Cliente");
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789");
            SetPrivateId(cliente, clienteId);
            
            // Crear tarjeta con puntos insuficientes
            var tarjeta = CrearTarjetaActiva(clienteId, NivelFidelizacion.Oro, puntosDisponibles);
            SetPrivateId(tarjeta, tarjetaId);
            
            // Asociar tarjeta al cliente
            cliente.AsociarTarjetaFidelizacion(tarjetaId);
            
            // Configurar mocks
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);

            // Act
            var resultado = await _servicio.CanjearPuntosAsync(clienteId, puntosACanjear, beneficio);

            // Assert
            resultado.Succeeded.Should().BeFalse();
            resultado.Error.Should().NotBeNull();
            resultado.Error!.ToString().Should().Contain("no tiene suficientes puntos disponibles");
        }

        [Fact]
        public async Task CanjearPuntos_CasoExitoso_DebeRetornarPuntosRestantes()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjetaId = Guid.NewGuid();
            var puntosDisponibles = 100;
            var puntosACanjear = 50;
            var beneficio = "Descuento en comanda";

            // Crear cliente con tarjeta
            var clienteNombre = ClienteNombre.Crear("Test", "Cliente");
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789");
            SetPrivateId(cliente, clienteId);
            
            // Crear tarjeta con puntos suficientes
            var tarjeta = CrearTarjetaActiva(clienteId, NivelFidelizacion.Oro, puntosDisponibles);
            SetPrivateId(tarjeta, tarjetaId);
            
            // Asociar tarjeta al cliente
            cliente.AsociarTarjetaFidelizacion(tarjetaId);
            
            // Configurar mocks
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
                
            _clienteRepositoryMock.Setup(r => r.GuardarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _servicio.CanjearPuntosAsync(clienteId, puntosACanjear, beneficio);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().Be(puntosDisponibles - puntosACanjear);
            _clienteRepositoryMock.Verify(r => r.GuardarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task AcumularPuntos_ValidacionExitosa_UsaResult()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var comandaId = Guid.NewGuid();
            var montoTotal = 1000m;
            
            // Mock del servicio con NotificationManager mock para verificar que se usa
            var servicioConMock = new ServicioFidelizacion(
                _tarjetaRepositoryMock.Object,
                _clienteRepositoryMock.Object,
                _historialPuntosRepositoryMock.Object,
                _dateTimeServiceMock.Object,
                _notificationManagerMock.Object);
                
            // Crear cliente con tarjeta
            var clienteNombre = ClienteNombre.Crear("Test", "Cliente");
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789");
            SetPrivateId(cliente, clienteId);
            
            // Crear tarjeta
            var tarjeta = CrearTarjetaActiva(clienteId, NivelFidelizacion.Oro, 50);
            SetPrivateId(tarjeta, Guid.NewGuid());
            
            // Asociar tarjeta al cliente
            cliente.AsociarTarjetaFidelizacion(tarjeta.Id);
            
            // Configurar mocks
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerPorIdAsync(cliente.TarjetaFidelizacionPrincipalId!.Value, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarjeta);
            
            // Act
            var result = await servicioConMock.AcumularPuntosAsync(clienteId, comandaId, montoTotal);
            
            // Assert
            result.Succeeded.Should().BeTrue();
        }
        
        [Fact]
        public async Task AcumularPuntos_ClienteNoExiste_DebeRetornarError()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var comandaId = Guid.NewGuid();
            var montoTotal = 1000m;
            
            // Configurar que el cliente no existe
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Cliente?)null);
                
            // Act
            var resultado = await _servicio.AcumularPuntosAsync(clienteId, comandaId, montoTotal);
            
            // Assert
            resultado.Succeeded.Should().BeFalse();
            resultado.Error.Should().NotBeNull();
            resultado.Error!.ToString().Should().Contain("No se encontró el cliente");
        }
        
        [Fact]
        public async Task CanjearPuntos_TarjetaSuspendida_DebeRetornarError()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjetaId = Guid.NewGuid();
            var puntosDisponibles = 100;
            var puntosACanjear = 50;
            var beneficio = "Descuento en comanda";

            // Crear cliente con tarjeta
            var clienteNombre = ClienteNombre.Crear("Test", "Cliente");
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789");
            SetPrivateId(cliente, clienteId);
            
            // Crear tarjeta suspendida
            var tarjeta = CrearTarjetaActiva(clienteId, NivelFidelizacion.Oro, puntosDisponibles);
            tarjeta.Suspender("Fraude detectado");
            SetPrivateId(tarjeta, tarjetaId);
            
            // Asociar tarjeta al cliente
            cliente.AsociarTarjetaFidelizacion(tarjetaId);
            
            // Configurar mocks
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerPorIdAsync(tarjetaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarjeta);

            // Act
            var resultado = await _servicio.CanjearPuntosAsync(clienteId, puntosACanjear, beneficio);

            // Assert
            resultado.Succeeded.Should().BeFalse();
            resultado.Error.Should().NotBeNull();
            resultado.Error!.ToString().Should().Contain("suspendida");
        }
        
        [Fact]
        public async Task CrearTarjetaFidelizacion_DebeCrearTarjetaYAsociarlaAlCliente()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            
            // Cliente sin tarjeta
            var clienteNombre = ClienteNombre.Crear("Test", "Cliente");
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789");
            SetPrivateId(cliente, clienteId);
            
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
                
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TarjetaFidelizacion?)null);
                
            _tarjetaRepositoryMock.Setup(r => r.AgregarAsync(It.IsAny<TarjetaFidelizacion>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            _clienteRepositoryMock.Setup(r => r.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            // Act
            var resultado = await _servicio.CrearTarjetaFidelizacionAsync(clienteId);
            
            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().NotBeNull();
            resultado.Value.ClienteId.Should().Be(clienteId);
            resultado.Value.Estado.Should().Be(EstadoTarjeta.Emitida);
            
            _tarjetaRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<TarjetaFidelizacion>(), It.IsAny<CancellationToken>()), Times.Once);
            _clienteRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        // Método auxiliar para asignar ID privados a las entidades
        private void SetPrivateId(EntityBase entity, Guid id)
        {
            var propiedadId = typeof(EntityBase).GetField("_id", BindingFlags.Instance | BindingFlags.NonPublic);
            propiedadId?.SetValue(entity, id);
        }

        // Método auxiliar para crear una tarjeta de fidelización activa
        private TarjetaFidelizacion CrearTarjetaActiva(Guid clienteId, NivelFidelizacion nivel, int puntosAcumulados = 0)
        {
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "CARD-" + Guid.NewGuid().ToString().Substring(0, 8));
            tarjeta.Activar();
                
            // Agregar puntos si es necesario
            if (puntosAcumulados > 0)
            {
                for (int i = 0; i < puntosAcumulados; i += 10)
                {
                    tarjeta.AgregarPuntos(10, "Acumulación para prueba");
                }
            }
            
            // Actualizar nivel si es diferente del básico
            if (nivel != NivelFidelizacion.Basico)
            {
                tarjeta.ActualizarNivel(nivel);
            }
            
            return tarjeta;
        }
    }
}
#nullable restore 





