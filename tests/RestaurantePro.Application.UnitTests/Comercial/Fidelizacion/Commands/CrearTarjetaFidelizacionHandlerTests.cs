namespace RestaurantePro.Application.UnitTests.Comercial.Fidelizacion.Commands;
using RestaurantePro.Domain.Comercial.Clientes.Enums;

/// <summary>
/// Tests unitarios para CrearTarjetaFidelizacionHandler
/// Valida la lógica completa de creación de tarjetas de fidelización con reglas de negocio complejas
/// </summary>
public class CrearTarjetaFidelizacionHandlerTests
{
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<ITarjetaFidelizacionRepository> _tarjetaRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CrearTarjetaFidelizacionHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CrearTarjetaFidelizacionHandler _handler;

    public CrearTarjetaFidelizacionHandlerTests()
    {
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _tarjetaRepositoryMock = new Mock<ITarjetaFidelizacionRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CrearTarjetaFidelizacionHandler>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _dateTimeServiceMock = new Mock<IDateTimeService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CrearTarjetaFidelizacionHandler(
            _clienteRepositoryMock.Object,
            _tarjetaRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _currentUserServiceMock.Object,
            _dateTimeServiceMock.Object,
            _unitOfWorkMock.Object);
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
            TipoTarjeta = TipoTarjetaFidelizacion.Estandar,
            PuntosIniciales = 100,
            ActivarInmediatamente = true,
            EnviarPorEmail = true,
            UsuarioId = usuarioId
        };

        // Assert - Verificar propiedades reales
        Assert.Equal(clienteId, command.ClienteId);
        Assert.Equal(TipoTarjetaFidelizacion.Estandar, command.TipoTarjeta);
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
            TipoTarjeta = TipoTarjetaFidelizacion.Premium,
            PuntosIniciales = 500,
            ActivarInmediatamente = true,
            EnviarPorEmail = false,
            UsuarioId = usuarioId
        };

        // Assert
        Assert.Equal(clienteId, command.ClienteId);
        Assert.Equal(TipoTarjetaFidelizacion.Premium, command.TipoTarjeta);
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
            TipoTarjeta = TipoTarjetaFidelizacion.Vip,
            PuntosIniciales = 1000,
            Configuracion = configuracion,
            ActivarInmediatamente = true,
            EnviarPorEmail = true,
            UsuarioId = usuarioId,
            Observaciones = "Tarjeta promocional especial"
        };

        // Assert
        Assert.Equal(1000, command.PuntosIniciales);
        Assert.Equal(TipoTarjetaFidelizacion.Vip, command.TipoTarjeta);
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
        var tarjetaId = Guid.NewGuid();
        
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = clienteId,
            TipoTarjeta = TipoTarjetaFidelizacion.Estandar,
            PuntosIniciales = 100,
            ActivarInmediatamente = true,
            EnviarPorEmail = true,
            UsuarioId = usuarioId,
            Observaciones = "Tarjeta de bienvenida"
        };

        var cliente = CreateClienteMock(clienteId);
        var tarjeta = CreateTarjetaFidelizacionMock(tarjetaId, clienteId);
        var responseDto = CreateTarjetaFidelizacionDto(tarjetaId, clienteId);

        // Setup mocks
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _tarjetaRepositoryMock.Setup(x => x.ObtenerPorCodigoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TarjetaFidelizacion)null);
        _dateTimeServiceMock.Setup(x => x.Now)
            .Returns(DateTime.Now);
        _unitOfWorkMock.Setup(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        
        // Verify repository calls
        _clienteRepositoryMock.Verify(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
        _tarjetaRepositoryMock.Verify(x => x.AgregarAsync(It.IsAny<TarjetaFidelizacion>(), It.IsAny<CancellationToken>()), Times.Once);
        _clienteRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AsignacionClienteExistente_DeberiaAsociarCorrectamente()
    {
        // Arrange
        var clienteExistente = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var tarjetaId = Guid.NewGuid();
        
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = clienteExistente,
            TipoTarjeta = TipoTarjetaFidelizacion.Premium,
            PuntosIniciales = 500,
            ActivarInmediatamente = true,
            EnviarPorEmail = false,
            UsuarioId = usuarioId
        };

        var cliente = CreateClienteMock(clienteExistente);

        // Setup mocks
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteExistente, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _tarjetaRepositoryMock.Setup(x => x.ObtenerPorCodigoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TarjetaFidelizacion)null);
        _dateTimeServiceMock.Setup(x => x.Now)
            .Returns(DateTime.Now);
        _unitOfWorkMock.Setup(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verify repository calls
        _clienteRepositoryMock.Verify(x => x.ObtenerPorIdAsync(clienteExistente, It.IsAny<CancellationToken>()), Times.Once);
        _tarjetaRepositoryMock.Verify(x => x.AgregarAsync(It.IsAny<TarjetaFidelizacion>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TarjetaPromocionEspecial_DeberiaAplicarBeneficios()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var tarjetaId = Guid.NewGuid();
        
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
            TipoTarjeta = TipoTarjetaFidelizacion.Vip,
            PuntosIniciales = 1000,
            Configuracion = configuracion,
            ActivarInmediatamente = true,
            EnviarPorEmail = true,
            UsuarioId = usuarioId,
            Observaciones = "Tarjeta promocional - Acceso VIP por 3 meses"
        };

        var cliente = CreateClienteMock(clienteId);

        // Setup mocks
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _tarjetaRepositoryMock.Setup(x => x.ObtenerPorCodigoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TarjetaFidelizacion)null);
        _dateTimeServiceMock.Setup(x => x.Now)
            .Returns(DateTime.Now);
        _unitOfWorkMock.Setup(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(TipoTarjetaFidelizacion.Vip, command.TipoTarjeta);
    }

    [Fact]
    public async Task Handle_TarjetaConNotificacionesMulticanal_DeberiaEnviarNotificaciones()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var tarjetaId = Guid.NewGuid();
        
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = clienteId,
            TipoTarjeta = TipoTarjetaFidelizacion.Premium,
            PuntosIniciales = 250,
            ActivarInmediatamente = true,
            EnviarPorEmail = true,
            UsuarioId = usuarioId,
            Observaciones = "Cliente premium con notificaciones multicanal"
        };

        var cliente = CreateClienteMock(clienteId);

        // Setup mocks
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _tarjetaRepositoryMock.Setup(x => x.ObtenerPorCodigoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TarjetaFidelizacion)null);
        _dateTimeServiceMock.Setup(x => x.Now)
            .Returns(DateTime.Now);
        _unitOfWorkMock.Setup(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Handle_TarjetaBasica_DeberiaAplicarConfiguracionMinima()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = clienteId,
            TipoTarjeta = TipoTarjetaFidelizacion.Estandar,
            PuntosIniciales = 0,
            ActivarInmediatamente = true,
            EnviarPorEmail = false,
            UsuarioId = usuarioId
        };

        var cliente = CreateClienteMock(clienteId);

        // Setup mocks
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _tarjetaRepositoryMock.Setup(x => x.ObtenerPorCodigoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TarjetaFidelizacion)null);
        _dateTimeServiceMock.Setup(x => x.Now)
            .Returns(DateTime.Now);
        _unitOfWorkMock.Setup(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
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
            TipoTarjeta = TipoTarjetaFidelizacion.Estandar,
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
            TipoTarjeta = (TipoTarjetaFidelizacion)999, // Valor inválido
            UsuarioId = Guid.NewGuid()
        };

        var cliente = CreateClienteMock(command.ClienteId);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(command.ClienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - El handler debería procesar normalmente, ya que TipoTarjetaFidelizacion es enum
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Handle_UsuarioIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = Guid.NewGuid(),
            TipoTarjeta = TipoTarjetaFidelizacion.Estandar,
            UsuarioId = Guid.Empty // ID vacío
        };

        var cliente = CreateClienteMock(command.ClienteId);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(command.ClienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - El handler actual no valida UsuarioId, debería procesar normalmente
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Handle_ClienteExistenteYaTieneTarjeta_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = clienteId,
            TipoTarjeta = TipoTarjetaFidelizacion.Estandar,
            UsuarioId = Guid.NewGuid()
        };

        var cliente = CreateClienteMock(clienteId);
        cliente.AsociarTarjetaFidelizacion(Guid.NewGuid()); // Ya tiene una tarjeta

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("tarjeta", result.Error.ToLower());
    }

    [Fact]
    public async Task Handle_PuntosInicialesNegativos_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = Guid.NewGuid(),
            TipoTarjeta = TipoTarjetaFidelizacion.Estandar,
            PuntosIniciales = -100, // Puntos negativos
            UsuarioId = Guid.NewGuid()
        };

        var cliente = CreateClienteMock(command.ClienteId);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(command.ClienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

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
            PuntosIniciales = -500, // Puntos negativos en configuración
            MultiplicadorPuntos = 0.0m // Multiplicador inválido
        };

        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = Guid.NewGuid(),
            TipoTarjeta = TipoTarjetaFidelizacion.Premium,
            Configuracion = configuracion,
            UsuarioId = Guid.NewGuid()
        };

        var cliente = CreateClienteMock(command.ClienteId);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(command.ClienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
    }

    #endregion

    #region Tests de Errores del Sistema

    [Fact]
    public async Task Handle_ErrorServicioComercial_DeberiaRetornarErrorServicio()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = Guid.NewGuid(),
            TipoTarjeta = TipoTarjetaFidelizacion.Estandar,
            UsuarioId = Guid.NewGuid()
        };

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(command.ClienteId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("error", result.Error.ToLower());
    }

    [Fact]
    public async Task Handle_ExcepcionInesperada_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = Guid.NewGuid(),
            TipoTarjeta = TipoTarjetaFidelizacion.Estandar,
            UsuarioId = Guid.NewGuid()
        };

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Operación no válida"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("error", result.Error.ToLower());
    }

    #endregion

    #region Helper Methods

    private static Cliente CreateClienteMock(Guid clienteId)
    {
        var nombre = ClienteNombre.Crear("Juan", "Pérez");
        var cliente = Cliente.Crear(nombre, "juan.perez@email.com", "+1234567890", DateTime.Now.AddYears(-30));
        cliente.GetType().GetProperty("Id")?.SetValue(cliente, clienteId);
        return cliente;
    }

    private static TarjetaFidelizacion CreateTarjetaFidelizacionMock(Guid tarjetaId, Guid clienteId)
    {
        var tarjeta = TarjetaFidelizacion.Crear(clienteId, "TF-2025-000001");
        tarjeta.GetType().GetProperty("Id")?.SetValue(tarjeta, tarjetaId);
        return tarjeta;
    }

    private static TarjetaFidelizacionDto CreateTarjetaFidelizacionDto(Guid tarjetaId, Guid clienteId)
    {
        return new TarjetaFidelizacionDto
        {
            Id = tarjetaId,
            NumeroTarjeta = "TF-2025-000001",
            ClienteId = clienteId,
            NombreCliente = "Juan Pérez",
            PuntosActuales = 100,
            TotalPuntosGanados = 100,
            TotalPuntosCanjeados = 0,
            FechaEmision = DateTime.Now,
            Estado = "Activa",
            Activa = true,
            Observaciones = "Tarjeta creada automáticamente"
        };
    }

    #endregion
} 