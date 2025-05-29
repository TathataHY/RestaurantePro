namespace RestaurantePro.Domain.UnitTests.Comercial.Services
{
    /// <summary>
    /// Clases de datos necesarias para las pruebas
    /// </summary>
    public record DatosFacturacion(string Nombre, string NumeroDocumento, string Direccion, TipoContribuyente TipoContribuyente);
    
    public record DatosCliente(string Nombre, string Apellido, string Email, string Telefono);
    
    /// <summary>
    /// Enumeración para los tipos de contribuyentes
    /// </summary>
    public enum TipoContribuyente
    {
        NoDefinido = 0,
        PersonaNatural = 1,
        PersonaJuridica = 2
    }

    /// <summary>
    /// Pruebas unitarias para ComercialServiceFacade - Validando integración de patrones Result/Notification
    /// </summary>
    public class ComercialServiceFacadeTests
    {
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly Mock<IClientesFrecuentesPolicy> _clientesFrecuentesPolicyMock;
        private readonly Mock<IServicioFidelizacion> _servicioFidelizacionMock;
        private readonly Mock<ILogger<ComercialServiceFacade>> _loggerMock;
        private readonly NotificationManager _notificationManager;
        private readonly ComercialServiceFacade _sut;

        public ComercialServiceFacadeTests()
        {
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _clientesFrecuentesPolicyMock = new Mock<IClientesFrecuentesPolicy>();
            _servicioFidelizacionMock = new Mock<IServicioFidelizacion>();
            _loggerMock = new Mock<ILogger<ComercialServiceFacade>>();
            _notificationManager = new NotificationManager();
            
            _sut = new ComercialServiceFacade(
                _clienteRepositoryMock.Object,
                _clientesFrecuentesPolicyMock.Object,
                _servicioFidelizacionMock.Object,
                _notificationManager,
                _loggerMock.Object);
        }

        #region ObtenerClientePorIdAsync Tests

        [Fact]
        public async Task ObtenerClientePorIdAsync_ConIdValido_DebeRetornarResultadoExitoso()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var nombreCliente = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombreCliente, "juan@example.com", "123456789", DateTime.Now.AddYears(-30));
            
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
            
            // Act
            var resultado = await _sut.ObtenerClientePorIdAsync(clienteId);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().NotBeNull();
            resultado.Value!.Nombre.Should().Be(nombreCliente);
        }

        [Fact]
        public async Task ObtenerClientePorIdAsync_ConIdVacio_DebeRetornarError()
        {
            // Arrange
            var idVacio = Guid.Empty;
            
            // Act
            var resultado = await _sut.ObtenerClientePorIdAsync(idVacio);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeFalse();
            // Verificar que hay errores (puede estar en Error o Errors)
            resultado.HasErrors.Should().BeTrue();
            (resultado.Error != null || (resultado.Errors != null && resultado.Errors.Count > 0)).Should().BeTrue();
        }

        [Fact]
        public async Task ObtenerClientePorIdAsync_ClienteNoExiste_DebeRetornarNulo()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Cliente?)null);
            
            // Act
            var resultado = await _sut.ObtenerClientePorIdAsync(clienteId);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().BeNull();
        }

        #endregion

        #region RegistrarNuevoClienteConTarjetaAsync Tests

        [Fact]
        public async Task RegistrarNuevoClienteConTarjetaAsync_ConDatosValidos_DebeRetornarExito()
        {
            // Arrange
            var nombre = "Juan";
            var apellidos = "Pérez";
            var email = "juan.perez@example.com";
            var telefono = "612345678";

            // Configurar repositorio para que no exista cliente con ese email
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Cliente?)null);

            // Configurar que la tarjeta se crea exitosamente
            _servicioFidelizacionMock
                .Setup(s => s.CrearTarjetaFidelizacionAsync(It.IsAny<Guid>()))
                .ReturnsAsync(Result.Success(TarjetaFidelizacion.Crear(Guid.NewGuid(), "TEST-CARD")));

            // Act
            var resultado = await _sut.RegistrarNuevoClienteConTarjetaAsync(nombre, apellidos, email, telefono);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().NotBeNull();
            resultado.Value.Nombre.Nombre.Should().Be(nombre);
            resultado.Value.Nombre.Apellido.Should().Be(apellidos);
            resultado.Value.Email.Value.Should().Be(email);
        }

        [Fact]
        public async Task RegistrarNuevoClienteConTarjetaAsync_ConNombreVacio_DebeRetornarError()
        {
            // Arrange
            var nombre = "";
            var apellidos = "García";
            var email = "maria@example.com";
            
            // Act
            var resultado = await _sut.RegistrarNuevoClienteConTarjetaAsync(nombre, apellidos, email);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeFalse();
            // Verificar que hay errores (puede estar en Error o Errors)
            resultado.HasErrors.Should().BeTrue();
        }

        [Fact]
        public async Task RegistrarNuevoClienteConTarjetaAsync_ConEmailInvalido_DebeRetornarError()
        {
            // Arrange
            var nombre = "María";
            var apellidos = "García";
            var emailInvalido = "email-no-valido";
            
            // Act
            var resultado = await _sut.RegistrarNuevoClienteConTarjetaAsync(nombre, apellidos, emailInvalido);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeFalse();
            // Verificar que hay errores (puede estar en Error o Errors)
            resultado.HasErrors.Should().BeTrue();
            // Verificar que el mensaje contiene información sobre email
            var errorMessage = resultado.Error ?? string.Join(", ", resultado.Errors ?? new List<string>());
            errorMessage.ToLower().Should().Contain("email");
        }

        #endregion

        #region ProcesarReactivacionClienteAsync Tests

        [Fact]
        public async Task ProcesarReactivacionClienteAsync_ConClienteExistente_DebeRetornarExito()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var motivoReactivacion = "Reactivación por solicitud del cliente";

            var clienteNombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(clienteNombre, "juan@example.com", "612345678", DateTime.Now.AddYears(-30));
            
            // ✅ CONFIGURAR: El cliente debe tener historial de visitas
            cliente.RegistrarVisita(); // Al menos 1 visita
            cliente.RegistrarVisita(); // 2 visitas para asegurar historial
            
            // ✅ CONFIGURAR: Configurar el cliente como inactivo para poder reactivarlo
            cliente.Desactivar();

            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
                
            _clienteRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _sut.ProcesarReactivacionClienteAsync(clienteId, motivoReactivacion);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task ProcesarReactivacionClienteAsync_ClienteNoExiste_DebeRetornarError()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var motivoReactivacion = "Reactivación de prueba";

            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Cliente?)null);

            // Act
            var resultado = await _sut.ProcesarReactivacionClienteAsync(clienteId, motivoReactivacion);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeFalse();
            // Verificar que hay errores (puede estar en Error o Errors)
            resultado.HasErrors.Should().BeTrue();
        }

        #endregion

        #region AgregarPuntosAsync Tests

        [Fact]
        public async Task AgregarPuntosAsync_ConClienteValido_DebeRetornarExito()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var puntos = 100;
            var comandaId = Guid.NewGuid();

            var clienteNombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(clienteNombre, "juan@example.com", "612345678", DateTime.Now.AddYears(-30));

            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);

            _servicioFidelizacionMock
                .Setup(s => s.AgregarPuntosAsync(clienteId, puntos, It.IsAny<string>()))
                .ReturnsAsync(Result.Success(puntos));

            // Act
            var resultado = await _sut.AsignarPuntosClienteAsync(clienteId, puntos, comandaId);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task AgregarPuntosAsync_ConPuntosNegativos_DebeRetornarError()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var puntosNegativos = -50;
            var comandaId = Guid.NewGuid();

            // Act
            var resultado = await _sut.AsignarPuntosClienteAsync(clienteId, puntosNegativos, comandaId);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeFalse();
            // Verificar que hay errores (puede estar en Error o Errors)
            resultado.HasErrors.Should().BeTrue();
        }

        #endregion

        #region Validación de NotificationManager

        [Fact]
        public void Constructor_ConParametrosNulos_DebeLanzarExcepcion()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new ComercialServiceFacade(null!, _clientesFrecuentesPolicyMock.Object, _servicioFidelizacionMock.Object, _notificationManager, _loggerMock.Object));
            
            Assert.Throws<ArgumentNullException>(() => 
                new ComercialServiceFacade(_clienteRepositoryMock.Object, null!, _servicioFidelizacionMock.Object, _notificationManager, _loggerMock.Object));
            
            Assert.Throws<ArgumentNullException>(() => 
                new ComercialServiceFacade(_clienteRepositoryMock.Object, _clientesFrecuentesPolicyMock.Object, null!, _notificationManager, _loggerMock.Object));
            
            Assert.Throws<ArgumentNullException>(() => 
                new ComercialServiceFacade(_clienteRepositoryMock.Object, _clientesFrecuentesPolicyMock.Object, _servicioFidelizacionMock.Object, null!, _loggerMock.Object));
        }

        [Fact]
        public async Task MetodosAsync_DebenCrearNuevaNotificacion()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Cliente?)null);
            
            // Act
            await _sut.ObtenerClientePorIdAsync(clienteId);
            
            // Assert
            // Verificar que el NotificationManager tiene una notificación activa
            _notificationManager.CurrentNotification.Should().NotBeNull();
        }

        #endregion
    }
} 