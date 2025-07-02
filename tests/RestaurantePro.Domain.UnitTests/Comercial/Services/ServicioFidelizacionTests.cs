#nullable disable
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
            var clienteId = Guid.Empty;
            var puntos = 0;
            var motivo = string.Empty;

            // Act
            var resultado = await _servicio.AgregarPuntosAsync(clienteId, puntos, motivo);

            // Assert
            resultado.Succeeded.Should().BeFalse();
            resultado.Errors.Should().NotBeEmpty();
            // Verificamos que los errores contienen información relacionada con los parámetros inválidos
            resultado.Errors.Should().Contain(e => e.Contains("cliente"));
            resultado.Errors.Should().Contain(e => e.Contains("punto"));
            resultado.Errors.Should().Contain(e => e.Contains("motivo"));
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
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, default))
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
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789", DateTime.Now.AddYears(-30));
            SetPrivateId(cliente, clienteId);

            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, default))
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
            var puntos = 50;
            var motivo = "Compra de prueba";
            var puntosActuales = 100;
            var puntosEsperados = puntosActuales + puntos;
            
            // Crear una tarjeta usando el método de fábrica
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "TEST-CARD");
            
            // Forzar la activación de la tarjeta
            var method = typeof(TarjetaFidelizacion).GetMethod("Activar", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            method?.Invoke(tarjeta, null);
            
            // Configurar las propiedades de puntos usando reflexión
            typeof(TarjetaFidelizacion).GetProperty("PuntosDisponibles")?.SetValue(tarjeta, puntosActuales);
            typeof(TarjetaFidelizacion).GetProperty("PuntosAcumulados")?.SetValue(tarjeta, puntosActuales);

            // Configurar el ID de la tarjeta
            SetPrivateId(tarjeta, tarjetaId);
            
            // Crear cliente con la tarjeta asociada
            var clienteNombre = ClienteNombre.Crear("Test", "Cliente");
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789", DateTime.Now.AddYears(-30));
            SetPrivateId(cliente, clienteId);
            cliente.AsociarTarjetaFidelizacion(tarjetaId);
            
            // Configurar los mocks
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, default))
                .ReturnsAsync(cliente);
                
            _tarjetaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(tarjetaId, default, false))
                .ReturnsAsync(tarjeta);
                
            _tarjetaRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<TarjetaFidelizacion>(), default))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _servicio.AgregarPuntosAsync(clienteId, puntos, motivo);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().Be(puntosEsperados);
        }

        [Fact]
        public async Task CalcularDescuento_ParametrosInvalidos_DebeRetornarErroresValidacion()
        {
            // Arrange
            var clienteId = Guid.Empty;
            var montoTotal = 0m;

            // Act
            var resultado = await _servicio.CalcularDescuentoAsync(clienteId, montoTotal);

            // Assert
            resultado.Succeeded.Should().BeFalse();
            resultado.Errors.Should().NotBeEmpty();
            // Verificamos que los errores contienen información relacionada con los parámetros inválidos
            resultado.Errors.Should().Contain(e => e.Contains("cliente"));
            resultado.Errors.Should().Contain(e => e.Contains("monto"));
        }

        [Fact]
        public async Task CalcularDescuento_ClienteSinTarjeta_DebeRetornarCero()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var totalComanda = 1000m;
            
            // Cliente sin tarjeta
            var clienteNombre = ClienteNombre.Crear("Test", "Cliente");
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789", DateTime.Now.AddYears(-30));
            SetPrivateId(cliente, clienteId);
                
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, default))
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
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789", DateTime.Now.AddYears(-30));
            SetPrivateId(cliente, clienteId);
            
            // Crear tarjeta inactiva
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "TEST-CARD");
            SetPrivateId(tarjeta, tarjetaId);
            
            // Asociar tarjeta al cliente
            cliente.AsociarTarjetaFidelizacion(tarjetaId);
            
            // Configurar mocks
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, default))
                .ReturnsAsync(cliente);
                
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(clienteId, default))
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
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789", DateTime.Now.AddYears(-30));
            SetPrivateId(cliente, clienteId);
            
            // Asignar ID a la tarjeta
            SetPrivateId(tarjeta, tarjetaId);
            
            // Asociar tarjeta al cliente
            cliente.AsociarTarjetaFidelizacion(tarjetaId);
            
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, default))
                .ReturnsAsync(cliente);
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(clienteId, default))
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
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789", DateTime.Now.AddYears(-30));
            SetPrivateId(cliente, clienteId);
            
            // Asignar ID a la tarjeta
            SetPrivateId(tarjeta, tarjetaId);
            
            // Asociar tarjeta al cliente
            cliente.AsociarTarjetaFidelizacion(tarjetaId);
            
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, default))
                .ReturnsAsync(cliente);
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(clienteId, default))
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
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789", DateTime.Now.AddYears(-30));
            SetPrivateId(cliente, clienteId);
            
            // Asignar ID a la tarjeta
            SetPrivateId(tarjeta, tarjetaId);
            
            // Asociar tarjeta al cliente
            cliente.AsociarTarjetaFidelizacion(tarjetaId);
            
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, default))
                .ReturnsAsync(cliente);
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(clienteId, default))
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
            var puntos = 100;
            var motivo = "Descuento en factura";
            var puntosDisponibles = 50; // Menos puntos que los que se intentan canjear
            
            // Crear una tarjeta usando el método de fábrica
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "TEST-CARD");
            
            // Forzar la activación de la tarjeta
            var method = typeof(TarjetaFidelizacion).GetMethod("Activar", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            method?.Invoke(tarjeta, null);
            
            // Configurar la propiedad PuntosDisponibles usando reflexión con solo 50 puntos (menos que los 100 solicitados)
            typeof(TarjetaFidelizacion).GetProperty("PuntosDisponibles")?.SetValue(tarjeta, puntosDisponibles);
            
            // Configurar el ID de la tarjeta
            SetPrivateId(tarjeta, tarjetaId);

            // Crear cliente con la tarjeta asociada
            var clienteNombre = ClienteNombre.Crear("Test", "Cliente");
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789", DateTime.Now.AddYears(-30));
            SetPrivateId(cliente, clienteId);
            cliente.AsociarTarjetaFidelizacion(tarjetaId);
            
            // Configurar los mocks
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, default))
                .ReturnsAsync(cliente);

            _tarjetaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(tarjetaId, default, false))
                .ReturnsAsync(tarjeta);
                
            // Act
            var resultado = await _servicio.CanjearPuntosAsync(clienteId, puntos, motivo);

            // Assert
            resultado.Succeeded.Should().BeFalse();
            resultado.Error.Should().NotBeNull();
            resultado.Error!.ToString().Should().Contain("puntos");
        }

        [Fact]
        public async Task CanjearPuntos_CasoExitoso_DebeRetornarPuntosRestantes()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjetaId = Guid.NewGuid();
            var puntos = 50;
            var motivo = "Descuento en factura";
            var puntosIniciales = 100;
            var puntosEsperados = puntosIniciales - puntos;
            
            // Crear una tarjeta usando el método de fábrica
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "TEST-CARD");
            
            // Forzar la activación de la tarjeta
            var method = typeof(TarjetaFidelizacion).GetMethod("Activar", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            method?.Invoke(tarjeta, null);
            
            // Configurar las propiedades de puntos usando reflexión
            typeof(TarjetaFidelizacion).GetProperty("PuntosDisponibles")?.SetValue(tarjeta, puntosIniciales);
            typeof(TarjetaFidelizacion).GetProperty("PuntosAcumulados")?.SetValue(tarjeta, puntosIniciales);
            
            // Configurar el ID de la tarjeta
            SetPrivateId(tarjeta, tarjetaId);

            // Crear cliente con la tarjeta asociada
            var clienteNombre = ClienteNombre.Crear("Test", "Cliente");
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789", DateTime.Now.AddYears(-30));
            SetPrivateId(cliente, clienteId);
            cliente.AsociarTarjetaFidelizacion(tarjetaId);
            
            // Configurar los mocks
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, default))
                .ReturnsAsync(cliente);
                
            _tarjetaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(tarjetaId, default, false))
                .ReturnsAsync(tarjeta);
                
            _tarjetaRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<TarjetaFidelizacion>(), default))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _servicio.CanjearPuntosAsync(clienteId, puntos, motivo);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().Be(puntosEsperados);
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
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789", DateTime.Now.AddYears(-30));
            SetPrivateId(cliente, clienteId);
            
            // Crear tarjeta
            var tarjeta = CrearTarjetaActiva(clienteId, NivelFidelizacion.Oro, 50);
            SetPrivateId(tarjeta, Guid.NewGuid());
            
            // Asociar tarjeta al cliente
            cliente.AsociarTarjetaFidelizacion(tarjeta.Id);
            
            // Configurar mocks
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, default))
                .ReturnsAsync(cliente);
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerPorIdAsync(cliente.TarjetaFidelizacionPrincipalId!.Value, default, false))
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
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, default))
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
            var puntos = 50;
            var motivo = "Descuento en factura";
            
            // Crear una tarjeta usando el método de fábrica
            var tarjeta = TarjetaFidelizacion.Crear(clienteId, "TEST-CARD");
            
            // Forzar la activación y luego suspensión de la tarjeta
            var activarMethod = typeof(TarjetaFidelizacion).GetMethod("Activar", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            activarMethod?.Invoke(tarjeta, null);
            
            var suspenderMethod = typeof(TarjetaFidelizacion).GetMethod("Suspender", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            suspenderMethod?.Invoke(tarjeta, new object[] { "Suspendida para test" });
            
            // Configurar la propiedad PuntosDisponibles usando reflexión
            typeof(TarjetaFidelizacion).GetProperty("PuntosDisponibles")?.SetValue(tarjeta, 100);

            // Configurar el ID de la tarjeta
            SetPrivateId(tarjeta, tarjetaId);
            
            // Crear cliente con la tarjeta asociada
            var clienteNombre = ClienteNombre.Crear("Test", "Cliente");
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789", DateTime.Now.AddYears(-30));
            SetPrivateId(cliente, clienteId);
            cliente.AsociarTarjetaFidelizacion(tarjetaId);
            
            // Configurar los mocks
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, default))
                .ReturnsAsync(cliente);
            
            _tarjetaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(tarjetaId, default, false))
                .ReturnsAsync(tarjeta);

            // Act
            var resultado = await _servicio.CanjearPuntosAsync(clienteId, puntos, motivo);

            // Assert
            resultado.Succeeded.Should().BeFalse();
            resultado.Error.Should().NotBeNull();
            resultado.Error!.ToString().Should().Contain("activa");
        }
        
        [Fact]
        public async Task CrearTarjetaFidelizacion_DebeCrearTarjetaYAsociarlaAlCliente()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            
            // Cliente sin tarjeta
            var clienteNombre = ClienteNombre.Crear("Test", "Cliente");
            var cliente = Cliente.Crear(clienteNombre, "test@example.com", "123456789", DateTime.Now.AddYears(-30));
            SetPrivateId(cliente, clienteId);
            
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(clienteId, default))
                .ReturnsAsync(cliente);
                
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(clienteId, default))
                .ReturnsAsync((TarjetaFidelizacion?)null);
                
            _tarjetaRepositoryMock.Setup(r => r.AgregarAsync(It.IsAny<TarjetaFidelizacion>(), default))
                .Returns(Task.CompletedTask);
                
            _clienteRepositoryMock.Setup(r => r.ActualizarAsync(It.IsAny<Cliente>(), default))
                .Returns(Task.CompletedTask);
                
            // Act
            var resultado = await _servicio.CrearTarjetaFidelizacionAsync(clienteId);
            
            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().NotBeNull();
            resultado.Value.ClienteId.Should().Be(clienteId);
            resultado.Value.Estado.Should().Be(EstadoTarjeta.Emitida);
            
            _tarjetaRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<TarjetaFidelizacion>(), default), Times.Once);
            _clienteRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Cliente>(), default), Times.Once);
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





