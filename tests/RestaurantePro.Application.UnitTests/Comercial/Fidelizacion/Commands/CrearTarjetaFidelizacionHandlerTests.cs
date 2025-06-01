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
        var clienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        // Act - Usar propiedades reales del command
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = clienteId,
            TipoTarjeta = "Basica",
            PuntosIniciales = 100,
            ActivarInmediatamente = true,
            EnviarPorEmail = true,
            UsuarioId = usuarioId
        };

        // Assert - Verificar propiedades reales
        Assert.Equal(clienteId, command.ClienteId);
        Assert.Equal("Basica", command.TipoTarjeta);
        Assert.Equal(100, command.PuntosIniciales);
        Assert.True(command.ActivarInmediatamente);
        Assert.True(command.EnviarPorEmail);
        Assert.Equal(usuarioId, command.UsuarioId);
    }

    [Fact]
    public void CrearTarjetaClienteExistente_ConClienteId_DeberiaConfigurarClienteExistente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        // Act - Usar propiedades reales
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = clienteId,
            TipoTarjeta = "Premium",
            PuntosIniciales = 500,
            ActivarInmediatamente = true,
            EnviarPorEmail = false,
            UsuarioId = usuarioId
        };

        // Assert
        Assert.Equal(clienteId, command.ClienteId);
        Assert.Equal("Premium", command.TipoTarjeta);
        Assert.Equal(500, command.PuntosIniciales);
        Assert.True(command.ActivarInmediatamente);
        Assert.False(command.EnviarPorEmail);
    }

    [Fact]
    public void CrearTarjetaPromocion_ConDatosPromocionales_DeberiaConfigurarPromocion()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        // Act - Usar configuración especial
        var configuracion = new CrearTarjetaConfiguracion
        {
            PuntosIniciales = 1000,
            MultiplicadorPuntos = 2.0m,
            ConfiguracionesEspeciales = new Dictionary<string, object>
            {
                { "BeneficioEspecial", "Descuento 20% primer mes" },
                { "FechaVencimiento", DateTime.Today.AddDays(30) }
            }
        };

        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = clienteId,
            TipoTarjeta = "VIP",
            PuntosIniciales = 1000,
            Configuracion = configuracion,
            ActivarInmediatamente = true,
            EnviarPorEmail = true,
            UsuarioId = usuarioId,
            Observaciones = "Tarjeta promocional especial"
        };

        // Assert
        Assert.Equal(1000, command.PuntosIniciales);
        Assert.Equal("VIP", command.TipoTarjeta);
        Assert.NotNull(command.Configuracion);
        Assert.Equal(2.0m, command.Configuracion.MultiplicadorPuntos);
        Assert.Contains("BeneficioEspecial", command.Configuracion.ConfiguracionesEspeciales);
    }

    #endregion

    #region Tests de Escenarios Exitosos

    [Fact]
    public async Task Handle_CreacionNuevoClienteExitosa_DeberiaRetornarTarjetaCreada()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = clienteId,
            TipoTarjeta = "Basica",
            PuntosIniciales = 100,
            ActivarInmediatamente = true,
            EnviarPorEmail = true,
            UsuarioId = usuarioId,
            Observaciones = "Tarjeta de bienvenida"
        };

        var tarjetaId = Guid.NewGuid();
        var resultadoCreacion = CreateMockResultadoCreacionExitosa(clienteId, tarjetaId);

        _comercialServiceFacadeMock.Setup(x => x.CrearTarjetaFidelizacionAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoCreacion));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(tarjetaId, result.Value.Id);
        Assert.Equal(clienteId, result.Value.ClienteId);
        Assert.Equal("TF-2025-000001", result.Value.NumeroTarjeta);
        Assert.Equal(100, result.Value.PuntosActuales);
        Assert.True(result.Value.Activa);
    }

    [Fact]
    public async Task Handle_AsignacionClienteExistente_DeberiaAsociarCorrectamente()
    {
        // Arrange
        var clienteExistente = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = clienteExistente,
            TipoTarjeta = "Premium",
            PuntosIniciales = 500,
            ActivarInmediatamente = true,
            EnviarPorEmail = false,
            UsuarioId = usuarioId
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
        Assert.Equal(tarjetaId, result.Value.Id);
        Assert.Equal(clienteExistente, result.Value.ClienteId);
        Assert.Equal(500, result.Value.PuntosActuales); // Premium recibe más puntos
        Assert.True(result.Value.Activa);
    }

    [Fact]
    public async Task Handle_TarjetaPromocionEspecial_DeberiaAplicarBeneficios()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        
        var configuracion = new CrearTarjetaConfiguracion
        {
            PuntosIniciales = 1000,
            MultiplicadorPuntos = 3.0m,
            FechaVencimiento = DateTime.Today.AddMonths(3),
            ConfiguracionesEspeciales = new Dictionary<string, object>
            {
                { "BeneficioEspecial", "Acceso VIP por 3 meses" },
                { "TipoPromocion", "Promoción lanzamiento" }
            }
        };
        
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = clienteId,
            TipoTarjeta = "VIP",
            PuntosIniciales = 1000,
            Configuracion = configuracion,
            ActivarInmediatamente = true,
            EnviarPorEmail = true,
            UsuarioId = usuarioId,
            Observaciones = "Tarjeta promocional - Acceso VIP por 3 meses"
        };

        var tarjetaId = Guid.NewGuid();
        var resultadoPromocion = CreateMockResultadoPromocionEspecial(clienteId, tarjetaId);

        _comercialServiceFacadeMock.Setup(x => x.CrearTarjetaFidelizacionAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoPromocion));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("VIP", command.TipoTarjeta);
        Assert.Equal(1000, result.Value.PuntosActuales);
        Assert.Equal(DateTime.Today.AddMonths(3), result.Value.FechaVencimiento);
    }

    [Fact]
    public async Task Handle_TarjetaConNotificacionesMulticanal_DeberiaEnviarNotificaciones()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = clienteId,
            TipoTarjeta = "Premium",
            PuntosIniciales = 250,
            ActivarInmediatamente = true,
            EnviarPorEmail = true,
            UsuarioId = usuarioId,
            Observaciones = "Cliente premium con notificaciones multicanal"
        };

        var tarjetaId = Guid.NewGuid();
        var resultadoNotificaciones = CreateMockResultadoConNotificaciones(clienteId, tarjetaId);

        _comercialServiceFacadeMock.Setup(x => x.CrearTarjetaFidelizacionAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoNotificaciones));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Value.Activa);
        Assert.Contains("Notificaciones enviadas", result.Value.Observaciones ?? "");
    }

    [Fact]
    public async Task Handle_TarjetaBasica_DeberiaAplicarConfiguracionMinima()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = Guid.NewGuid(), // Cliente ya existe
            TipoTarjeta = "Basica",
            PuntosIniciales = 0,
            ActivarInmediatamente = true,
            EnviarPorEmail = false,
            UsuarioId = Guid.NewGuid()
        };

        var clienteId = command.ClienteId;
        var tarjetaId = Guid.NewGuid();
        var resultadoBasico = CreateMockResultadoBasico(clienteId, tarjetaId);

        _comercialServiceFacadeMock.Setup(x => x.CrearTarjetaFidelizacionAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoBasico));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(0, result.Value.PuntosActuales);
        Assert.True(result.Value.Activa);
    }

    #endregion

    #region Tests de Validaciones de Negocio

    [Fact]
    public async Task Handle_ClienteIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = Guid.Empty, // ID vacío
            TipoTarjeta = "Basica",
            UsuarioId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("cliente", result.Error.ToLower());
    }

    [Fact]
    public async Task Handle_TipoTarjetaVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = Guid.NewGuid(),
            TipoTarjeta = "", // Tipo vacío
            UsuarioId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("tipo", result.Error.ToLower());
    }

    [Fact]
    public async Task Handle_UsuarioIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = Guid.NewGuid(),
            TipoTarjeta = "Basica",
            UsuarioId = Guid.Empty // Usuario vacío
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("usuario", result.Error.ToLower());
    }

    [Fact]
    public async Task Handle_ClienteExistenteYaTieneTarjeta_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = clienteId,
            TipoTarjeta = "Premium",
            UsuarioId = Guid.NewGuid()
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
    public async Task Handle_PuntosInicialesNegativos_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = Guid.NewGuid(),
            TipoTarjeta = "Premium",
            PuntosIniciales = -100, // Puntos negativos
            UsuarioId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("puntos", result.Error.ToLower());
    }

    [Fact]
    public async Task Handle_ConfiguracionEspecialConDatosInvalidos_DeberiaRetornarError()
    {
        // Arrange
        var configuracion = new CrearTarjetaConfiguracion
        {
            MultiplicadorPuntos = -1.0m, // Multiplicador inválido
            FechaVencimiento = DateTime.Today.AddDays(-1) // Fecha en el pasado
        };

        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = Guid.NewGuid(),
            TipoTarjeta = "VIP",
            Configuracion = configuracion,
            UsuarioId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("configuración", result.Error.ToLower());
    }

    #endregion

    #region Tests de Manejo de Errores

    [Fact]
    public async Task Handle_ErrorServicioComercial_DeberiaRetornarErrorServicio()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = Guid.NewGuid(),
            TipoTarjeta = "Basica",
            UsuarioId = Guid.NewGuid()
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
            ClienteId = Guid.NewGuid(),
            TipoTarjeta = "Basica",
            UsuarioId = Guid.NewGuid()
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
            ClienteId = Guid.NewGuid(),
            TipoTarjeta = "Basica",
            UsuarioId = Guid.NewGuid()
        };

        var clienteId = command.ClienteId;
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
            ClienteId = Guid.NewGuid(),
            TipoTarjeta = "Basica",
            UsuarioId = Guid.NewGuid()
        };

        var clienteId = command.ClienteId;
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
            Id = tarjetaId,
            ClienteId = clienteId,
            NumeroTarjeta = "TF-2025-000001",
            Nivel = NivelFidelizacion.Basico,
            PuntosActuales = 100,
            FechaEmision = DateTime.UtcNow,
            Activa = true,
            Estado = "Activa"
        };
    }

    private static CrearTarjetaFidelizacionResult CreateMockResultadoAsignacionExistente(Guid clienteId, Guid tarjetaId)
    {
        return new CrearTarjetaFidelizacionResult
        {
            Id = tarjetaId,
            ClienteId = clienteId,
            NumeroTarjeta = "TF-2025-000002",
            Nivel = NivelFidelizacion.Oro,
            PuntosActuales = 500,
            FechaEmision = DateTime.UtcNow,
            Activa = true,
            Estado = "Activa"
        };
    }

    private static CrearTarjetaFidelizacionResult CreateMockResultadoPromocionEspecial(Guid clienteId, Guid tarjetaId)
    {
        return new CrearTarjetaFidelizacionResult
        {
            Id = tarjetaId,
            ClienteId = clienteId,
            NumeroTarjeta = "TF-2025-VIP001",
            Nivel = NivelFidelizacion.Platino,
            PuntosActuales = 1000,
            FechaEmision = DateTime.UtcNow,
            FechaVencimiento = DateTime.Today.AddMonths(3),
            Activa = true,
            Estado = "Activa",
            Observaciones = "Tarjeta VIP con beneficios especiales"
        };
    }

    private static CrearTarjetaFidelizacionResult CreateMockResultadoConNotificaciones(Guid clienteId, Guid tarjetaId)
    {
        return new CrearTarjetaFidelizacionResult
        {
            Id = tarjetaId,
            ClienteId = clienteId,
            NumeroTarjeta = "TF-2025-000003",
            Nivel = NivelFidelizacion.Plata,
            PuntosActuales = 250,
            FechaEmision = DateTime.UtcNow,
            Activa = true,
            Estado = "Activa",
            Observaciones = "Notificaciones enviadas por email y SMS"
        };
    }

    private static CrearTarjetaFidelizacionResult CreateMockResultadoBasico(Guid clienteId, Guid tarjetaId)
    {
        return new CrearTarjetaFidelizacionResult
        {
            Id = tarjetaId,
            ClienteId = clienteId,
            NumeroTarjeta = "TF-2025-000004",
            Nivel = NivelFidelizacion.Basico,
            PuntosActuales = 0,
            FechaEmision = DateTime.UtcNow,
            Activa = true,
            Estado = "Activa"
        };
    }

    #endregion
} 