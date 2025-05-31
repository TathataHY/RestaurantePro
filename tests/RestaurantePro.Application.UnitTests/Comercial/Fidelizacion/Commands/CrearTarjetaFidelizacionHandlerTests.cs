namespace RestaurantePro.Application.UnitTests.Comercial.Fidelizacion.Commands;

/// <summary>
/// Tests unitarios para CrearTarjetaFidelizacionHandler
/// Valida la lógica completa de creación de tarjetas de fidelización con reglas de negocio complejas
/// </summary>
public class CrearTarjetaFidelizacionHandlerTests
{
    private readonly Mock<IComercialServiceFacade> _comercialServiceFacadeMock;
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<ILogger<CrearTarjetaFidelizacionHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IBackgroundJobService> _backgroundJobServiceMock;
    private readonly CrearTarjetaFidelizacionHandler _handler;

    public CrearTarjetaFidelizacionHandlerTests()
    {
        _comercialServiceFacadeMock = new Mock<IComercialServiceFacade>();
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _loggerMock = new Mock<ILogger<CrearTarjetaFidelizacionHandler>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _backgroundJobServiceMock = new Mock<IBackgroundJobService>();

        _handler = new CrearTarjetaFidelizacionHandler(
            _comercialServiceFacadeMock.Object,
            _clienteRepositoryMock.Object,
            _loggerMock.Object,
            _currentUserServiceMock.Object,
            _backgroundJobServiceMock.Object);
    }

    #region Tests de Factory Methods del Command

    [Fact]
    public void CrearTarjetaNuevoCliente_ConDatosValidos_DeberiaCrearCommandCorrectamente()
    {
        // Arrange
        var nombre = "Juan Pérez";
        var email = "juan.perez@email.com";
        var telefono = "+5491123456789";
        var fechaNacimiento = DateTime.Today.AddYears(-30);

        // Act
        var command = CrearTarjetaFidelizacionCommand.CrearTarjetaNuevoCliente(
            nombre, email, telefono, fechaNacimiento, NivelFidelizacion.Basico);

        // Assert
        Assert.Equal(nombre, command.NombreCompleto);
        Assert.Equal(email, command.Email);
        Assert.Equal(telefono, command.Telefono);
        Assert.Equal(fechaNacimiento, command.FechaNacimiento);
        Assert.Equal(NivelFidelizacion.Basico, command.NivelInicial);
        Assert.True(command.CrearClienteNuevo);
        Assert.Null(command.ClienteExistenteId);
        Assert.True(command.EnviarNotificacionBienvenida);
        Assert.True(command.AplicarPuntosIniciales);
    }

    [Fact]
    public void CrearTarjetaClienteExistente_ConClienteId_DeberiaConfigurarClienteExistente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var nivelInicial = NivelFidelizacion.Plata;

        // Act
        var command = CrearTarjetaFidelizacionCommand.CrearTarjetaClienteExistente(
            clienteId, nivelInicial, false);

        // Assert
        Assert.Equal(clienteId, command.ClienteExistenteId);
        Assert.Equal(nivelInicial, command.NivelInicial);
        Assert.False(command.CrearClienteNuevo);
        Assert.False(command.EnviarNotificacionBienvenida);
        Assert.True(command.AplicarPuntosIniciales);
    }

    [Fact]
    public void CrearTarjetaPromocion_ConDatosPromocionales_DeberiaConfigurarPromocion()
    {
        // Arrange
        var puntosExtra = 500;
        var beneficioEspecial = "Descuento 20% primer mes";

        // Act
        var command = CrearTarjetaFidelizacionCommand.CrearTarjetaPromocion(
            "María González", "maria@email.com", "+5491198765432", 
            puntosExtra, beneficioEspecial, DateTime.Today.AddDays(30));

        // Assert
        Assert.Equal(puntosExtra, command.PuntosIniciales);
        Assert.Equal(beneficioEspecial, command.BeneficioEspecial);
        Assert.True(command.EsPromocionEspecial);
        Assert.Equal(DateTime.Today.AddDays(30), command.FechaVencimientoBeneficio);
    }

    #endregion

    #region Tests de Escenarios Exitosos

    [Fact]
    public async Task Handle_CreacionNuevoClienteExitosa_DeberiaRetornarTarjetaCreada()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            NombreCompleto = "Ana García",
            Email = "ana.garcia@email.com",
            Telefono = "+5491156789012",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            NivelInicial = NivelFidelizacion.Basico,
            CrearClienteNuevo = true,
            EnviarNotificacionBienvenida = true,
            AplicarPuntosIniciales = true
        };

        var nuevoClienteId = Guid.NewGuid();
        var tarjetaId = Guid.NewGuid();
        var resultadoCreacion = CreateMockResultadoCreacionExitosa(nuevoClienteId, tarjetaId);

        _comercialServiceFacadeMock.Setup(x => x.CrearTarjetaFidelizacionAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoCreacion));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(tarjetaId, result.Value.TarjetaId);
        Assert.Equal(nuevoClienteId, result.Value.ClienteId);
        Assert.Equal("TF-2025-000001", result.Value.NumeroTarjeta);
        Assert.Equal(NivelFidelizacion.Basico, result.Value.NivelAsignado);
        Assert.Equal(100, result.Value.PuntosAsignados);
        Assert.True(result.Value.NotificacionEnviada);
        Assert.True(result.Value.ClienteNuevoCreado);
    }

    [Fact]
    public async Task Handle_AsignacionClienteExistente_DeberiaAsociarCorrectamente()
    {
        // Arrange
        var clienteExistente = Guid.NewGuid();
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteExistenteId = clienteExistente,
            NivelInicial = NivelFidelizacion.Oro,
            CrearClienteNuevo = false,
            AplicarPuntosIniciales = true,
            EnviarNotificacionBienvenida = false
        };

        var tarjetaId = Guid.NewGuid();
        var resultadoAsignacion = CreateMockResultadoAsignacionExistente(clienteExistente, tarjetaId);

        _comercialServiceFacadeMock.Setup(x => x.CrearTarjetaFidelizacionAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoAsignacion));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(tarjetaId, result.Value.TarjetaId);
        Assert.Equal(clienteExistente, result.Value.ClienteId);
        Assert.Equal(NivelFidelizacion.Oro, result.Value.NivelAsignado);
        Assert.Equal(500, result.Value.PuntosAsignados); // Oro recibe más puntos
        Assert.False(result.Value.NotificacionEnviada);
        Assert.False(result.Value.ClienteNuevoCreado);
    }

    [Fact]
    public async Task Handle_TarjetaPromocionEspecial_DeberiaAplicarBeneficios()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            NombreCompleto = "Carlos Premium",
            Email = "carlos.premium@email.com",
            Telefono = "+5491145678901",
            FechaNacimiento = DateTime.Today.AddYears(-35),
            NivelInicial = NivelFidelizacion.Platino,
            CrearClienteNuevo = true,
            EsPromocionEspecial = true,
            PuntosIniciales = 1000,
            BeneficioEspecial = "Acceso VIP por 3 meses",
            FechaVencimientoBeneficio = DateTime.Today.AddMonths(3)
        };

        var clienteId = Guid.NewGuid();
        var tarjetaId = Guid.NewGuid();
        var resultadoPromocion = CreateMockResultadoPromocionEspecial(clienteId, tarjetaId);

        _comercialServiceFacadeMock.Setup(x => x.CrearTarjetaFidelizacionAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoPromocion));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(NivelFidelizacion.Platino, result.Value.NivelAsignado);
        Assert.Equal(1000, result.Value.PuntosAsignados);
        Assert.True(result.Value.BeneficioEspecialAplicado);
        Assert.Equal("Acceso VIP por 3 meses", result.Value.DescripcionBeneficio);
        Assert.Equal(DateTime.Today.AddMonths(3), result.Value.FechaVencimientoBeneficio);
    }

    [Fact]
    public async Task Handle_TarjetaConNotificacionesMulticanal_DeberiaEnviarNotificaciones()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            NombreCompleto = "Laura Comunicaciones",
            Email = "laura@email.com",
            Telefono = "+5491187654321",
            FechaNacimiento = DateTime.Today.AddYears(-28),
            NivelInicial = NivelFidelizacion.Plata,
            CrearClienteNuevo = true,
            EnviarNotificacionBienvenida = true,
            EnviarNotificacionSMS = true,
            EnviarNotificacionEmail = true
        };

        var clienteId = Guid.NewGuid();
        var tarjetaId = Guid.NewGuid();
        var resultadoNotificaciones = CreateMockResultadoConNotificaciones(clienteId, tarjetaId);

        _comercialServiceFacadeMock.Setup(x => x.CrearTarjetaFidelizacionAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoNotificaciones));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Value.NotificacionEnviada);
        Assert.True(result.Value.NotificacionEmailEnviada);
        Assert.True(result.Value.NotificacionSMSEnviada);
        Assert.Contains("Email enviado", result.Value.DetallesNotificacion);
        Assert.Contains("SMS enviado", result.Value.DetallesNotificacion);
    }

    [Fact]
    public async Task Handle_TarjetaBasica_DeberiaAplicarConfiguracionMinima()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            NombreCompleto = "Pedro Básico",
            Email = "pedro.basico@email.com",
            Telefono = "+5491134567890",
            FechaNacimiento = DateTime.Today.AddYears(-40),
            NivelInicial = NivelFidelizacion.Basico,
            CrearClienteNuevo = true,
            EnviarNotificacionBienvenida = false,
            AplicarPuntosIniciales = false
        };

        var clienteId = Guid.NewGuid();
        var tarjetaId = Guid.NewGuid();
        var resultadoBasico = CreateMockResultadoBasico(clienteId, tarjetaId);

        _comercialServiceFacadeMock.Setup(x => x.CrearTarjetaFidelizacionAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoBasico));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(NivelFidelizacion.Basico, result.Value.NivelAsignado);
        Assert.Equal(0, result.Value.PuntosAsignados);
        Assert.False(result.Value.NotificacionEnviada);
        Assert.False(result.Value.BeneficioEspecialAplicado);
    }

    #endregion

    #region Tests de Validaciones de Negocio

    [Fact]
    public async Task Handle_ClienteExistenteSinId_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            CrearClienteNuevo = false,
            ClienteExistenteId = null, // No especifica cliente existente
            NivelInicial = NivelFidelizacion.Basico
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Debe especificar el ID del cliente existente", result.Error);
    }

    [Fact]
    public async Task Handle_NuevoClienteSinDatosCompletos_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            CrearClienteNuevo = true,
            NombreCompleto = "", // Nombre vacío
            Email = "email.invalido", // Email inválido
            NivelInicial = NivelFidelizacion.Basico
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Datos del cliente incompletos o inválidos", result.Error);
    }

    [Fact]
    public async Task Handle_EmailDuplicado_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            NombreCompleto = "Cliente Duplicado",
            Email = "email.existente@email.com",
            Telefono = "+5491123456789",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            CrearClienteNuevo = true,
            NivelInicial = NivelFidelizacion.Basico
        };

        _comercialServiceFacadeMock.Setup(x => x.CrearTarjetaFidelizacionAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<CrearTarjetaFidelizacionResult>("Email ya registrado en el sistema"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Email ya registrado en el sistema", result.Error);
    }

    [Fact]
    public async Task Handle_ClienteExistenteYaTieneTarjeta_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteExistenteId = clienteId,
            CrearClienteNuevo = false,
            NivelInicial = NivelFidelizacion.Oro
        };

        _comercialServiceFacadeMock.Setup(x => x.CrearTarjetaFidelizacionAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<CrearTarjetaFidelizacionResult>("Cliente ya posee tarjeta de fidelización activa"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Cliente ya posee tarjeta de fidelización activa", result.Error);
    }

    [Fact]
    public async Task Handle_FechaNacimientoInvalida_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            NombreCompleto = "Cliente Joven",
            Email = "cliente.joven@email.com",
            Telefono = "+5491123456789",
            FechaNacimiento = DateTime.Today.AddYears(-10), // Menor de edad
            CrearClienteNuevo = true,
            NivelInicial = NivelFidelizacion.Basico
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Cliente debe ser mayor de 18 años", result.Error);
    }

    [Fact]
    public async Task Handle_PromocionEspecialSinFechaVencimiento_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            NombreCompleto = "Cliente Promoción",
            Email = "promo@email.com",
            Telefono = "+5491123456789",
            FechaNacimiento = DateTime.Today.AddYears(-30),
            CrearClienteNuevo = true,
            EsPromocionEspecial = true,
            BeneficioEspecial = "Descuento especial",
            FechaVencimientoBeneficio = null // No especifica vencimiento
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Promoción especial debe especificar fecha de vencimiento", result.Error);
    }

    #endregion

    #region Tests de Manejo de Errores

    [Fact]
    public async Task Handle_ErrorServicioComercial_DeberiaRetornarErrorServicio()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            NombreCompleto = "Cliente Test",
            Email = "test@email.com",
            Telefono = "+5491123456789",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            CrearClienteNuevo = true,
            NivelInicial = NivelFidelizacion.Basico
        };

        _comercialServiceFacadeMock.Setup(x => x.CrearTarjetaFidelizacionAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<CrearTarjetaFidelizacionResult>("Error en proceso de creación de tarjeta"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error en proceso de creación de tarjeta", result.Error);
    }

    [Fact]
    public async Task Handle_ExcepcionInesperada_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            NombreCompleto = "Cliente Error",
            Email = "error@email.com",
            Telefono = "+5491123456789",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            CrearClienteNuevo = true,
            NivelInicial = NivelFidelizacion.Basico
        };

        _comercialServiceFacadeMock.Setup(x => x.CrearTarjetaFidelizacionAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de conectividad con base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error interno del sistema", result.Error);
    }

    [Fact]
    public async Task Handle_ErrorNotificacionesYContinua_DeberiaLogearYProceder()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            NombreCompleto = "Cliente Notificaciones",
            Email = "notif@email.com",
            Telefono = "+5491123456789",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            CrearClienteNuevo = true,
            EnviarNotificacionBienvenida = true,
            NivelInicial = NivelFidelizacion.Basico
        };

        var clienteId = Guid.NewGuid();
        var tarjetaId = Guid.NewGuid();
        var resultado = CreateMockResultadoCreacionExitosa(clienteId, tarjetaId);

        _comercialServiceFacadeMock.Setup(x => x.CrearTarjetaFidelizacionAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultado));

        _backgroundJobServiceMock.Setup(x => x.EnqueueBackgroundJob(It.IsAny<string>(), It.IsAny<object>()))
            .Throws(new Exception("Error en servicio de notificaciones"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded); // Debe continuar exitosamente

        // Verificar que se loggeó el warning
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al programar notificaciones")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Tests de Logging

    [Fact]
    public async Task Handle_CreacionExitosa_DeberiaLoggearProceso()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            NombreCompleto = "Cliente Log",
            Email = "log@email.com",
            Telefono = "+5491123456789",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            CrearClienteNuevo = true,
            NivelInicial = NivelFidelizacion.Basico
        };

        var clienteId = Guid.NewGuid();
        var tarjetaId = Guid.NewGuid();
        var resultado = CreateMockResultadoCreacionExitosa(clienteId, tarjetaId);

        _comercialServiceFacadeMock.Setup(x => x.CrearTarjetaFidelizacionAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultado));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);

        // Verificar logging de inicio
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando creación de tarjeta de fidelización")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar logging de éxito
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Tarjeta de fidelización creada exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Métodos Helper

    private static CrearTarjetaFidelizacionResult CreateMockResultadoCreacionExitosa(Guid clienteId, Guid tarjetaId)
    {
        return new CrearTarjetaFidelizacionResult
        {
            TarjetaId = tarjetaId,
            ClienteId = clienteId,
            NumeroTarjeta = "TF-2025-000001",
            NivelAsignado = NivelFidelizacion.Basico,
            PuntosAsignados = 100,
            FechaCreacion = DateTime.UtcNow,
            NotificacionEnviada = true,
            ClienteNuevoCreado = true,
            BeneficioEspecialAplicado = false,
            TiempoGeneracion = TimeSpan.FromSeconds(1.5),
            EstadoTarjeta = "Activa"
        };
    }

    private static CrearTarjetaFidelizacionResult CreateMockResultadoAsignacionExistente(Guid clienteId, Guid tarjetaId)
    {
        return new CrearTarjetaFidelizacionResult
        {
            TarjetaId = tarjetaId,
            ClienteId = clienteId,
            NumeroTarjeta = "TF-2025-000002",
            NivelAsignado = NivelFidelizacion.Oro,
            PuntosAsignados = 500,
            FechaCreacion = DateTime.UtcNow,
            NotificacionEnviada = false,
            ClienteNuevoCreado = false,
            BeneficioEspecialAplicado = false,
            TiempoGeneracion = TimeSpan.FromSeconds(0.8),
            EstadoTarjeta = "Activa"
        };
    }

    private static CrearTarjetaFidelizacionResult CreateMockResultadoPromocionEspecial(Guid clienteId, Guid tarjetaId)
    {
        return new CrearTarjetaFidelizacionResult
        {
            TarjetaId = tarjetaId,
            ClienteId = clienteId,
            NumeroTarjeta = "TF-2025-VIP001",
            NivelAsignado = NivelFidelizacion.Platino,
            PuntosAsignados = 1000,
            FechaCreacion = DateTime.UtcNow,
            NotificacionEnviada = true,
            ClienteNuevoCreado = true,
            BeneficioEspecialAplicado = true,
            DescripcionBeneficio = "Acceso VIP por 3 meses",
            FechaVencimientoBeneficio = DateTime.Today.AddMonths(3),
            TiempoGeneracion = TimeSpan.FromSeconds(2.1),
            EstadoTarjeta = "Activa",
            CodigoPromocion = "VIP2025"
        };
    }

    private static CrearTarjetaFidelizacionResult CreateMockResultadoConNotificaciones(Guid clienteId, Guid tarjetaId)
    {
        return new CrearTarjetaFidelizacionResult
        {
            TarjetaId = tarjetaId,
            ClienteId = clienteId,
            NumeroTarjeta = "TF-2025-000003",
            NivelAsignado = NivelFidelizacion.Plata,
            PuntosAsignados = 250,
            FechaCreacion = DateTime.UtcNow,
            NotificacionEnviada = true,
            NotificacionEmailEnviada = true,
            NotificacionSMSEnviada = true,
            DetallesNotificacion = "Email enviado a laura@email.com; SMS enviado a +5491187654321",
            ClienteNuevoCreado = true,
            TiempoGeneracion = TimeSpan.FromSeconds(1.8),
            EstadoTarjeta = "Activa"
        };
    }

    private static CrearTarjetaFidelizacionResult CreateMockResultadoBasico(Guid clienteId, Guid tarjetaId)
    {
        return new CrearTarjetaFidelizacionResult
        {
            TarjetaId = tarjetaId,
            ClienteId = clienteId,
            NumeroTarjeta = "TF-2025-000004",
            NivelAsignado = NivelFidelizacion.Basico,
            PuntosAsignados = 0,
            FechaCreacion = DateTime.UtcNow,
            NotificacionEnviada = false,
            ClienteNuevoCreado = true,
            BeneficioEspecialAplicado = false,
            TiempoGeneracion = TimeSpan.FromSeconds(0.5),
            EstadoTarjeta = "Activa"
        };
    }

    #endregion
} 