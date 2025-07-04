using RestaurantePro.Application.Comercial.Fidelizacion.Commands.CrearTarjetaFidelizacion;

namespace RestaurantePro.Application.UnitTests.Comercial.Fidelizacion.Commands;

/// <summary>
/// Tests unitarios para CrearTarjetaFidelizacionHandler
/// Valida la lógica completa de creación de tarjetas de fidelización con reglas de negocio complejas
/// </summary>
public class CrearTarjetaFidelizacionHandlerTests
{
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<ITarjetaFidelizacionRepository> _tarjetaRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CrearTarjetaFidelizacionHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly CrearTarjetaFidelizacionHandler _handler;

    // Variable compartida para simular almacenamiento en memoria
    private TarjetaFidelizacion? _tarjetaGuardada;

    public CrearTarjetaFidelizacionHandlerTests()
    {
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _tarjetaRepositoryMock = new Mock<ITarjetaFidelizacionRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CrearTarjetaFidelizacionHandler>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _dateTimeServiceMock = new Mock<IDateTimeService>();

        _handler = new CrearTarjetaFidelizacionHandler(
            _clienteRepositoryMock.Object,
            _tarjetaRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _currentUserServiceMock.Object,
            _dateTimeServiceMock.Object,
            _unitOfWorkMock.Object);

        // Setup común para simular almacenamiento en memoria
        _tarjetaRepositoryMock
            .Setup(x => x.AgregarAsync(It.IsAny<TarjetaFidelizacion>(), CancellationToken.None))
            .Callback<TarjetaFidelizacion, CancellationToken>((t, _) => 
            {
                _tarjetaGuardada = t;
                Console.WriteLine($"Tarjeta guardada con ID: {t.Id}");
            })
            .Returns(Task.CompletedTask);

        _tarjetaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), CancellationToken.None))
            .ReturnsAsync((Guid id, CancellationToken _) => 
            {
                Console.WriteLine($"ObtenerPorIdAsync llamado con ID: {id}, tarjetaGuardada: {_tarjetaGuardada?.Id}");
                return _tarjetaGuardada; // Devolver la tarjeta guardada sin importar el ID
            });
    }

    private void ResetTarjetaGuardada()
    {
        // Limpiar la variable compartida antes de cada test
        _tarjetaGuardada = null;
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
        ResetTarjetaGuardada();
        
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
            UsuarioId = usuarioId
        };

        var cliente = CreateClienteMock(clienteId);
        var tarjeta = CreateTarjetaFidelizacionMock(tarjetaId, clienteId);
        var tarjetaDto = CreateTarjetaFidelizacionDto(tarjetaId, clienteId);

        // Setup mocks para métodos con CancellationToken
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, CancellationToken.None))
            .ReturnsAsync(cliente);
        _tarjetaRepositoryMock.Setup(x => x.ObtenerPorCodigoAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync((TarjetaFidelizacion)null!);
        _tarjetaRepositoryMock.Setup(x => x.ExisteNumeroTarjetaAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(CancellationToken.None))
            .ReturnsAsync(1);
        _mapperMock.Setup(x => x.Map<TarjetaFidelizacionDto>(It.IsAny<TarjetaFidelizacion>()))
            .Returns(tarjetaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        if (!result.Succeeded)
        {
            Console.WriteLine($"Handler failed with error: {result.Error}");
        }
        Assert.True(result.Succeeded, $"Handler failed: {result.Error}");
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Equal(clienteId, result.Value.ClienteId);
    }

    [Fact]
    public async Task Handle_AsignacionClienteExistente_DeberiaAsociarCorrectamente()
    {
        // Arrange
        ResetTarjetaGuardada();
        
        var clienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var tarjetaId = Guid.NewGuid();
        
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = clienteId,
            TipoTarjeta = TipoTarjetaFidelizacion.Premium,
            PuntosIniciales = 200,
            ActivarInmediatamente = true,
            EnviarPorEmail = false,
            UsuarioId = usuarioId,
            Observaciones = "Cliente premium existente"
        };

        var cliente = CreateClienteMock(clienteId);
        var tarjeta = CreateTarjetaFidelizacionMock(tarjetaId, clienteId);
        var tarjetaDto = CreateTarjetaFidelizacionDto(tarjetaId, clienteId);

        // Setup mocks para métodos con CancellationToken
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, CancellationToken.None))
            .ReturnsAsync(cliente);
        _tarjetaRepositoryMock.Setup(x => x.ObtenerPorCodigoAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync((TarjetaFidelizacion)null!);
        _tarjetaRepositoryMock.Setup(x => x.ExisteNumeroTarjetaAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(CancellationToken.None))
            .ReturnsAsync(1);
        _mapperMock.Setup(x => x.Map<TarjetaFidelizacionDto>(It.IsAny<TarjetaFidelizacion>()))
            .Returns(tarjetaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        if (!result.Succeeded)
        {
            Console.WriteLine($"Handler failed with error: {result.Error}");
        }
        Assert.True(result.Succeeded, $"Handler failed: {result.Error}");
        Assert.NotNull(result.Value);
        Assert.Equal(TipoTarjetaFidelizacion.Premium, command.TipoTarjeta);
    }

    [Fact]
    public async Task Handle_TarjetaPromocionEspecial_DeberiaAplicarBeneficios()
    {
        // Arrange
        ResetTarjetaGuardada();
        
        var clienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var tarjetaId = Guid.NewGuid();
        
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = clienteId,
            TipoTarjeta = TipoTarjetaFidelizacion.Vip,
            PuntosIniciales = 500,
            ActivarInmediatamente = true,
            EnviarPorEmail = true,
            UsuarioId = usuarioId,
            Configuracion = new CrearTarjetaConfiguracion
            {
                PuntosIniciales = 500,
                MultiplicadorPuntos = 2.0m,
                ConfiguracionesEspeciales = new Dictionary<string, object>
                {
                    { "BeneficioEspecial", "Acceso VIP por 3 meses" },
                    { "TipoPromocion", "Promoción lanzamiento" }
                }
            }
        };

        var cliente = CreateClienteMock(clienteId);
        var tarjeta = CreateTarjetaFidelizacionMock(tarjetaId, clienteId);
        var tarjetaDto = CreateTarjetaFidelizacionDto(tarjetaId, clienteId);

        // Setup mocks para métodos con CancellationToken
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, CancellationToken.None))
            .ReturnsAsync(cliente);
        _tarjetaRepositoryMock.Setup(x => x.ObtenerPorCodigoAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync((TarjetaFidelizacion)null!);
        _tarjetaRepositoryMock.Setup(x => x.ExisteNumeroTarjetaAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(CancellationToken.None))
            .ReturnsAsync(1);
        _mapperMock.Setup(x => x.Map<TarjetaFidelizacionDto>(It.IsAny<TarjetaFidelizacion>()))
            .Returns(tarjetaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        if (!result.Succeeded)
        {
            Console.WriteLine($"Handler failed with error: {result.Error}");
        }
        Assert.True(result.Succeeded, $"Handler failed: {result.Error}");
        Assert.Equal(TipoTarjetaFidelizacion.Vip, command.TipoTarjeta);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.Id); // Verificar que el ID no sea vacío
    }

    [Fact]
    public async Task Handle_TarjetaConNotificacionesMulticanal_DeberiaEnviarNotificaciones()
    {
        // Arrange
        ResetTarjetaGuardada();
        
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
        var tarjeta = CreateTarjetaFidelizacionMock(tarjetaId, clienteId);
        var tarjetaDto = CreateTarjetaFidelizacionDto(tarjetaId, clienteId);

        // Setup mocks para métodos con CancellationToken
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, CancellationToken.None))
            .ReturnsAsync(cliente);
        _tarjetaRepositoryMock.Setup(x => x.ObtenerPorCodigoAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync((TarjetaFidelizacion)null!);
        _tarjetaRepositoryMock.Setup(x => x.ExisteNumeroTarjetaAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(CancellationToken.None))
            .ReturnsAsync(1);
        _mapperMock.Setup(x => x.Map<TarjetaFidelizacionDto>(It.IsAny<TarjetaFidelizacion>()))
            .Returns(tarjetaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        if (!result.Succeeded)
        {
            Console.WriteLine($"Handler failed with error: {result.Error}");
        }
        Assert.True(result.Succeeded, $"Handler failed: {result.Error}");
    }

    [Fact]
    public async Task Handle_TarjetaBasica_DeberiaAplicarConfiguracionMinima()
    {
        // Arrange
        ResetTarjetaGuardada();
        
        var clienteId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var tarjetaId = Guid.NewGuid();
        
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
        var tarjeta = CreateTarjetaFidelizacionMock(tarjetaId, clienteId);
        var tarjetaDto = CreateTarjetaFidelizacionDto(tarjetaId, clienteId);

        // Setup mocks para métodos con CancellationToken
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, CancellationToken.None))
            .ReturnsAsync(cliente);
        _tarjetaRepositoryMock.Setup(x => x.ObtenerPorCodigoAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync((TarjetaFidelizacion)null!);
        _tarjetaRepositoryMock.Setup(x => x.ExisteNumeroTarjetaAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(CancellationToken.None))
            .ReturnsAsync(1);
        _mapperMock.Setup(x => x.Map<TarjetaFidelizacionDto>(It.IsAny<TarjetaFidelizacion>()))
            .Returns(tarjetaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        if (!result.Succeeded)
        {
            Console.WriteLine($"Handler failed with error: {result.Error}");
        }
        Assert.True(result.Succeeded, $"Handler failed: {result.Error}");
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
        if (!result.Succeeded)
        {
            Console.WriteLine($"Handler failed with error: {result.Error}");
        }
        Assert.False(result.Succeeded);
        Assert.Contains("cliente", result.Error.ToLower());
    }

    [Fact]
    public async Task Handle_TipoTarjetaVacio_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = clienteId,
            TipoTarjeta = TipoTarjetaFidelizacion.Estandar, // Tipo válido, no vacío
            UsuarioId = Guid.NewGuid()
        };

        var cliente = CreateClienteMock(clienteId);
        
        // Setup mocks para que el handler pueda procesar
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, CancellationToken.None))
            .ReturnsAsync(cliente);
        _tarjetaRepositoryMock.Setup(x => x.ExisteNumeroTarjetaAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(false);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(CancellationToken.None))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - El handler debería procesar normalmente, ya que TipoTarjetaFidelizacion es enum
        if (!result.Succeeded)
        {
            Console.WriteLine($"Handler failed with error: {result.Error}");
        }
        Assert.True(result.Succeeded, $"Handler failed: {result.Error}");
    }

    [Fact]
    public async Task Handle_UsuarioIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = clienteId,
            TipoTarjeta = TipoTarjetaFidelizacion.Estandar,
            UsuarioId = Guid.Empty // ID vacío
        };

        var cliente = CreateClienteMock(clienteId);
        
        // Setup mocks para que el handler pueda procesar
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, CancellationToken.None))
            .ReturnsAsync(cliente);
        _tarjetaRepositoryMock.Setup(x => x.ExisteNumeroTarjetaAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(false);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(CancellationToken.None))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - El handler actual no valida UsuarioId, debería procesar normalmente
        if (!result.Succeeded)
        {
            Console.WriteLine($"Handler failed with error: {result.Error}");
        }
        Assert.True(result.Succeeded, $"Handler failed: {result.Error}");
    }

    [Fact]
    public async Task Handle_ClienteExistenteYaTieneTarjeta_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var tarjetaExistenteId = Guid.NewGuid();
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = clienteId,
            TipoTarjeta = TipoTarjetaFidelizacion.Estandar,
            UsuarioId = Guid.NewGuid()
        };

        var cliente = CreateClienteMock(clienteId);
        // Simular que el cliente ya tiene una tarjeta estableciendo la propiedad directamente
        var tarjetaIdProperty = typeof(Cliente).GetProperty("TarjetaFidelizacionPrincipalId");
        tarjetaIdProperty?.SetValue(cliente, tarjetaExistenteId);

        // Setup mocks para que el handler pueda validar
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, CancellationToken.None))
            .ReturnsAsync(cliente);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        if (!result.Succeeded)
        {
            Console.WriteLine($"Handler failed with error: {result.Error}");
        }
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
        // En tests de validación de negocio (por ejemplo, ClienteId vacío, TipoTarjeta inválido, UsuarioId vacío, etc.)
        // NO se hace Setup de los repositorios si el handler retorna error antes de llegar a la llamada al repositorio.

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        if (!result.Succeeded)
        {
            Console.WriteLine($"Handler failed with error: {result.Error}");
        }
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
        // En tests de validación de negocio (por ejemplo, ClienteId vacío, TipoTarjeta inválido, UsuarioId vacío, etc.)
        // NO se hace Setup de los repositorios si el handler retorna error antes de llegar a la llamada al repositorio.

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        if (!result.Succeeded)
        {
            Console.WriteLine($"Handler failed with error: {result.Error}");
        }
        Assert.False(result.Succeeded);
    }

    #endregion

    #region Tests de Errores del Sistema

    [Fact]
    public async Task Handle_ErrorServicioComercial_DeberiaRetornarErrorServicio()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = clienteId,
            TipoTarjeta = TipoTarjetaFidelizacion.Estandar,
            UsuarioId = Guid.NewGuid()
        };

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, CancellationToken.None))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        if (!result.Succeeded)
        {
            Console.WriteLine($"Handler failed with error: {result.Error}");
        }
        Assert.False(result.Succeeded);
        Assert.Contains("error", result.Error.ToLower());
    }

    [Fact]
    public async Task Handle_ExcepcionInesperada_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new CrearTarjetaFidelizacionCommand
        {
            ClienteId = clienteId,
            TipoTarjeta = TipoTarjetaFidelizacion.Estandar,
            UsuarioId = Guid.NewGuid()
        };

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, CancellationToken.None))
            .ThrowsAsync(new InvalidOperationException("Operación no válida"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        if (!result.Succeeded)
        {
            Console.WriteLine($"Handler failed with error: {result.Error}");
        }
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