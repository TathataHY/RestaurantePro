namespace RestaurantePro.Domain.UnitTests.Comercial.Clientes.Factories
{
    /// <summary>
    /// Pruebas unitarias para ClienteFactory - Validando patrón Factory con Result/Notification
    /// </summary>
    public class ClienteFactoryTests
    {
        private readonly Mock<ILogger<ClienteFactory>> _loggerMock;
        private readonly NotificationManager _notificationManager;
        private readonly ClienteFactory _clienteFactory;

        public ClienteFactoryTests()
        {
            _loggerMock = new Mock<ILogger<ClienteFactory>>();
            _notificationManager = new NotificationManager();
            _clienteFactory = new ClienteFactory(_notificationManager, _loggerMock.Object);
        }

        #region Crear Tests

        [Fact]
        public void Crear_ConParametrosValidos_DebeRetornarExito()
        {
            // Arrange
            var parametros = new ClienteCreationParameters
            {
                Nombre = "Juan",
                Apellidos = "Pérez García",
                Email = "juan.perez@example.com",
                Telefono = "601234567",
                FechaNacimiento = DateTime.Now.AddYears(-30),
                EstaActivo = true
            };

            // Act
            var resultado = _clienteFactory.Crear(parametros);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().NotBeNull();
            
            var cliente = resultado.Value!;
            cliente.Nombre.NombreCompleto.Should().Contain("Juan");
            cliente.Nombre.NombreCompleto.Should().Contain("Pérez García");
            cliente.Email.Value.Should().Be("juan.perez@example.com");
            cliente.Telefono.Value.Should().Be("601234567");
            cliente.EstaActivo.Should().BeTrue();
        }

        [Fact]
        public void Crear_ConNombreVacio_DebeRetornarError()
        {
            // Arrange
            var parametros = new ClienteCreationParameters
            {
                Nombre = "",
                Apellidos = "Pérez",
                Email = "juan@example.com",
                Telefono = "123456789",
                FechaNacimiento = DateTime.Now.AddYears(-25)
            };

            // Act
            var resultado = _clienteFactory.Crear(parametros);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeFalse();
            resultado.Errors.Should().NotBeEmpty();
            resultado.Errors.Should().Contain(error => error.Contains("Nombre"));
        }

        [Fact]
        public void Crear_ConEmailInvalido_DebeRetornarError()
        {
            // Arrange
            var parametros = new ClienteCreationParameters
            {
                Nombre = "María",
                Apellidos = "González",
                Email = "email-no-valido",
                Telefono = "987654321",
                FechaNacimiento = DateTime.Now.AddYears(-28)
            };

            // Act
            var resultado = _clienteFactory.Crear(parametros);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeFalse();
            resultado.Errors.Should().NotBeEmpty();
            resultado.Errors.Should().Contain(error => error.Contains("Email") || error.Contains("email"));
        }

        [Fact]
        public void Crear_ConTelefonoInvalido_DebeRetornarError()
        {
            // Arrange
            var parametros = new ClienteCreationParameters
            {
                Nombre = "Carlos",
                Apellidos = "López",
                Email = "carlos@example.com",
                Telefono = "123", // Muy corto
                FechaNacimiento = DateTime.Now.AddYears(-35)
            };

            // Act
            var resultado = _clienteFactory.Crear(parametros);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeFalse();
            resultado.Errors.Should().NotBeEmpty();
            resultado.Errors.Should().Contain(error => error.Contains("Telefono") || error.Contains("teléfono"));
        }

        [Fact]
        public void Crear_ConMenorDeEdad_DebeRetornarError()
        {
            // Arrange
            var parametros = new ClienteCreationParameters
            {
                Nombre = "Ana",
                Apellidos = "Martín",
                Email = "ana@example.com",
                Telefono = "666777888",
                FechaNacimiento = DateTime.Now.AddYears(-16) // Menor de edad
            };

            // Act
            var resultado = _clienteFactory.Crear(parametros);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeFalse();
            resultado.Errors.Should().NotBeEmpty();
            resultado.Errors.Should().Contain(error => error.Contains("18 años") || error.Contains("edad"));
        }

        [Fact]
        public void Crear_ConParametrosNulos_DebeRetornarError()
        {
            // Arrange
            object? parametrosNulos = null;

            // Act
            var resultado = _clienteFactory.Crear(parametrosNulos!);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeFalse();
            resultado.Errors.Should().NotBeEmpty();
        }

        #endregion

        #region Reconstruir Tests

        [Fact]
        public void Reconstruir_ConDatosValidos_DebeRetornarExito()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var datosReconstruccion = new ClienteReconstructionData
            {
                Nombre = "Pedro",
                Apellidos = "Sánchez",
                Email = "pedro@example.com",
                Telefono = "611222333",
                FechaNacimiento = DateTime.Now.AddYears(-40),
                EstaActivo = true,
                PuntosAcumulados = 250,
                CantidadVisitas = 15,
                Segmento = SegmentoCliente.FrecuenciaAlta
            };

            // Act
            var resultado = _clienteFactory.Reconstruir(clienteId, datosReconstruccion);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().NotBeNull();
            
            var cliente = resultado.Value!;
            cliente.Id.Should().Be(clienteId);
            cliente.Nombre.NombreCompleto.Should().Contain("Pedro");
            cliente.Nombre.NombreCompleto.Should().Contain("Sánchez");
            cliente.PuntosAcumulados.Should().Be(250);
            cliente.CantidadVisitas.Should().Be(15);
            cliente.Segmento.Should().Be(SegmentoCliente.FrecuenciaAlta);
        }

        [Fact]
        public void Reconstruir_ConIdVacio_DebeRetornarError()
        {
            // Arrange
            var idVacio = Guid.Empty;
            var datos = new ClienteReconstructionData
            {
                Nombre = "Test",
                Apellidos = "User",
                Email = "test@example.com",
                Telefono = "123456789",
                FechaNacimiento = DateTime.Now.AddYears(-25)
            };

            // Act
            var resultado = _clienteFactory.Reconstruir(idVacio, datos);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeFalse();
            resultado.Errors.Should().NotBeEmpty();
        }

        #endregion

        #region ValidarParametros Tests

        [Fact]
        public void ValidarParametros_ConParametrosValidos_DebeRetornarExito()
        {
            // Arrange
            var parametros = new ClienteCreationParameters
            {
                Nombre = "Luis",
                Apellidos = "Rodríguez",
                Email = "luis@example.com",
                Telefono = "655444333",
                FechaNacimiento = DateTime.Now.AddYears(-32)
            };

            // Act
            var resultado = _clienteFactory.ValidarParametros(parametros);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeTrue();
        }

        [Fact]
        public void ValidarParametros_ConMultiplesErrores_DebeRetornarTodosLosErrores()
        {
            // Arrange
            var parametros = new ClienteCreationParameters
            {
                Nombre = "", // Error: vacío
                Apellidos = "", // Error: vacío
                Email = "email-invalido", // Error: formato incorrecto
                Telefono = "12", // Error: muy corto
                FechaNacimiento = DateTime.Now.AddYears(-10) // Error: menor de edad
            };

            // Act
            var resultado = _clienteFactory.ValidarParametros(parametros);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeFalse();
            resultado.Errors.Should().HaveCountGreaterThan(1);
        }

        #endregion

        #region NotificationManager Integration Tests

        [Fact]
        public void Factory_DebeUsarNotificationManagerCorrectamente()
        {
            // Arrange
            var parametrosInvalidos = new ClienteCreationParameters
            {
                Nombre = "",
                Apellidos = "Test",
                Email = "invalid-email",
                Telefono = "123",
                FechaNacimiento = DateTime.Now.AddYears(-15)
            };

            // Act
            var resultado = _clienteFactory.Crear(parametrosInvalidos);

            // Assert
            resultado.Succeeded.Should().BeFalse();
            _notificationManager.HasErrors.Should().BeTrue();
            _notificationManager.GetErrors().Should().NotBeEmpty();
        }

        [Fact]
        public void Factory_DebeLimpiarNotificacionesAntesDeCadaOperacion()
        {
            // Arrange
            // Primero agregamos un error al notification manager
            _notificationManager.AddError("Error previo");
            _notificationManager.HasErrors.Should().BeTrue();

            var parametrosValidos = new ClienteCreationParameters
            {
                Nombre = "Ana",
                Apellidos = "García",
                Email = "ana@example.com",
                Telefono = "666777888",
                FechaNacimiento = DateTime.Now.AddYears(-25)
            };

            // Act
            var resultado = _clienteFactory.Crear(parametrosValidos);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            // Verificar que se creó una nueva notificación limpia
            _notificationManager.CurrentNotification.Should().NotBeNull();
        }

        #endregion

        #region Logging Tests

        [Fact]
        public void Factory_DebeLoggearExitoCorrectamente()
        {
            // Arrange
            var parametros = new ClienteCreationParameters
            {
                Nombre = "Test",
                Apellidos = "Logger",
                Email = "test@example.com",
                Telefono = "123456789",
                FechaNacimiento = DateTime.Now.AddYears(-30)
            };

            // Act
            var resultado = _clienteFactory.Crear(parametros);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            
            // Verificar que se llamó al logger para información
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cliente") && v.ToString()!.Contains("exitosamente")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion
    }
} 