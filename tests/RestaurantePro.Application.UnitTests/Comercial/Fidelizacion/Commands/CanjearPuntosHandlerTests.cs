namespace RestaurantePro.Application.UnitTests.Comercial.Fidelizacion.Commands;

/// <summary>
/// Tests unitarios para CanjearPuntosHandler
/// Valida la lógica completa de canje de puntos con integraciones de servicios de dominio
/// </summary>
public class CanjearPuntosHandlerTests
{
    private readonly Mock<IComercialServiceFacade> _comercialServiceFacadeMock;
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<IComandaRepository> _comandaRepositoryMock;
    private readonly Mock<ILogger<CanjearPuntosHandler>> _loggerMock;
    private readonly CanjearPuntosHandler _handler;

    public CanjearPuntosHandlerTests()
    {
        _comercialServiceFacadeMock = new Mock<IComercialServiceFacade>();
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _comandaRepositoryMock = new Mock<IComandaRepository>();
        _loggerMock = new Mock<ILogger<CanjearPuntosHandler>>();

        _handler = new CanjearPuntosHandler(
            _comercialServiceFacadeMock.Object,
            _clienteRepositoryMock.Object,
            _comandaRepositoryMock.Object,
            _loggerMock.Object);
    }

    #region Tests de Factory Methods del Command

    [Fact]
    public void Crear_ConParametrosValidos_DeberiaCrearCommandCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var puntosAUtilizar = 100;
        var comandaId = Guid.NewGuid();
        var motivo = "Canje promocional especial";

        // Act
        var command = CanjearPuntosCommand.Crear(clienteId, puntosAUtilizar, comandaId, motivo);

        // Assert
        Assert.Equal(clienteId, command.ClienteId);
        Assert.Equal(puntosAUtilizar, command.PuntosAUtilizar);
        Assert.Equal(comandaId, command.ComandaId);
        Assert.Equal(motivo, command.Motivo);
    }

    [Fact]
    public void Crear_ConMotivoAutomatico_DeberiaGenerarMotivoDefault()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var puntosAUtilizar = 250;

        // Act
        var command = CanjearPuntosCommand.Crear(clienteId, puntosAUtilizar);

        // Assert
        Assert.Equal($"Canje de {puntosAUtilizar} puntos por descuento", command.Motivo);
        Assert.Null(command.ComandaId);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void Crear_ConClienteIdInvalido_DeberiaLanzarExcepcion(string clienteIdStr)
    {
        // Arrange
        var clienteId = Guid.Parse(clienteIdStr);
        var puntosAUtilizar = 100;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            CanjearPuntosCommand.Crear(clienteId, puntosAUtilizar));
        Assert.Contains("ClienteId no puede estar vacío", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    [InlineData(-1)]
    public void Crear_ConPuntosInvalidos_DeberiaLanzarExcepcion(int puntosInvalidos)
    {
        // Arrange
        var clienteId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            CanjearPuntosCommand.Crear(clienteId, puntosInvalidos));
        Assert.Contains("PuntosAUtilizar debe ser mayor a 0", exception.Message);
    }

    #endregion

    #region Tests de Escenarios Exitosos

    [Fact]
    public async Task Handle_CanjeSimpleExitoso_DeberiaRetornarResultadoCompleto()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var comandaId = Guid.NewGuid();
        var puntosAUtilizar = 100;
        var command = new CanjearPuntosCommand
        {
            ClienteId = clienteId,
            PuntosAUtilizar = puntosAUtilizar,
            ComandaId = comandaId,
            Motivo = "Canje de puntos regular"
        };

        var cliente = CreateMockCliente(clienteId, "Juan", "Pérez", 500); // Cliente con 500 puntos
        var comanda = CreateMockComanda(comandaId, clienteId);

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(clienteId, result.Value.ClienteId);
        Assert.Equal(puntosAUtilizar, result.Value.PuntosUtilizados);
        Assert.Equal(10.00m, result.Value.MontoDescuento); // 100 puntos × $0.10 = $10.00
        Assert.Equal(400, result.Value.PuntosRestantes); // 500 - 100 = 400
        Assert.Equal(comandaId, result.Value.ComandaId);
        Assert.Contains("Juan Pérez", result.Value.Motivo);
        Assert.Contains("100 puntos", result.Value.Motivo);
        Assert.Contains("$10.00", result.Value.Motivo);
    }

    [Fact]
    public async Task Handle_CanjeConPuntosAltos_DeberiaCalcularDescuentoCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var comandaId = Guid.NewGuid();
        var puntosAUtilizar = 1500; // Canje alto
        var command = new CanjearPuntosCommand
        {
            ClienteId = clienteId,
            PuntosAUtilizar = puntosAUtilizar,
            ComandaId = comandaId,
            Motivo = "Canje de puntos premium"
        };

        var cliente = CreateMockCliente(clienteId, "María", "González", 2000); // Cliente premium
        var comanda = CreateMockComanda(comandaId, clienteId);

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(150.00m, result.Value.MontoDescuento); // 1500 puntos × $0.10 = $150.00
        Assert.Equal(500, result.Value.PuntosRestantes); // 2000 - 1500 = 500
        Assert.Contains("María González", result.Value.Motivo);
    }

    [Fact]
    public async Task Handle_CanjeClienteNuevo_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var comandaId = Guid.NewGuid();
        var puntosAUtilizar = 50;
        var command = new CanjearPuntosCommand
        {
            ClienteId = clienteId,
            PuntosAUtilizar = puntosAUtilizar,
            ComandaId = comandaId,
            Motivo = "Primer canje del cliente"
        };

        var cliente = CreateMockCliente(clienteId, "Pedro", "Martínez", 60); // Cliente nuevo con pocos puntos
        var comanda = CreateMockComanda(comandaId, clienteId);

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(5.00m, result.Value.MontoDescuento); // 50 puntos × $0.10 = $5.00
        Assert.Equal(10, result.Value.PuntosRestantes); // 60 - 50 = 10
    }

    #endregion

    #region Tests de Validaciones de Negocio

    [Fact]
    public async Task Handle_ClienteNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var comandaId = Guid.NewGuid();
        var command = new CanjearPuntosCommand
        {
            ClienteId = clienteId,
            PuntosAUtilizar = 100,
            ComandaId = comandaId
        };

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Cliente no encontrado", result.Error);
    }

    [Fact]
    public async Task Handle_ComandaNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var comandaId = Guid.NewGuid();
        var command = new CanjearPuntosCommand
        {
            ClienteId = clienteId,
            PuntosAUtilizar = 100,
            ComandaId = comandaId
        };

        var cliente = CreateMockCliente(clienteId, "Ana", "López", 200);

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comanda?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Comanda no encontrada", result.Error);
    }

    [Fact]
    public async Task Handle_PuntosInsuficientes_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var comandaId = Guid.NewGuid();
        var command = new CanjearPuntosCommand
        {
            ClienteId = clienteId,
            PuntosAUtilizar = 300, // Solicita más puntos de los disponibles
            ComandaId = comandaId
        };

        var cliente = CreateMockCliente(clienteId, "Luis", "Rodríguez", 100); // Solo 100 puntos disponibles
        var comanda = CreateMockComanda(comandaId, clienteId);

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Puntos insuficientes para el canje", result.Error);
    }

    [Fact]
    public async Task Handle_ComandaSinEspecificar_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new CanjearPuntosCommand
        {
            ClienteId = clienteId,
            PuntosAUtilizar = 100,
            ComandaId = null // No especifica comanda
        };

        var cliente = CreateMockCliente(clienteId, "Sofia", "Herrera", 200);

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Comanda requerida para el canje", result.Error);
    }

    #endregion

    #region Tests de Manejo de Errores

    [Fact]
    public async Task Handle_ExcepcionRepositorioCliente_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var comandaId = Guid.NewGuid();
        var command = new CanjearPuntosCommand
        {
            ClienteId = clienteId,
            PuntosAUtilizar = 100,
            ComandaId = comandaId
        };

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Error interno del sistema", result.Error);
    }

    [Fact]
    public async Task Handle_ExcepcionRepositorioComanda_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var comandaId = Guid.NewGuid();
        var command = new CanjearPuntosCommand
        {
            ClienteId = clienteId,
            PuntosAUtilizar = 100,
            ComandaId = comandaId
        };

        var cliente = CreateMockCliente(clienteId, "Roberto", "Silva", 300);

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de conexión"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Error interno del sistema", result.Error);
    }

    #endregion

    #region Tests de Logging

    [Fact]
    public async Task Handle_CanjeExitoso_DeberiaLoggearInformacionCompleta()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var comandaId = Guid.NewGuid();
        var puntosAUtilizar = 200;
        var command = new CanjearPuntosCommand
        {
            ClienteId = clienteId,
            PuntosAUtilizar = puntosAUtilizar,
            ComandaId = comandaId
        };

        var cliente = CreateMockCliente(clienteId, "Elena", "Castillo", 400);
        var comanda = CreateMockComanda(comandaId, clienteId);

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verificar que se loggeó el inicio del proceso
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando canje")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar que se loggeó el éxito
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Canje de puntos exitoso")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ErrorPuntosInsuficientes_DeberiaLoggearWarning()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var comandaId = Guid.NewGuid();
        var command = new CanjearPuntosCommand
        {
            ClienteId = clienteId,
            PuntosAUtilizar = 500,
            ComandaId = comandaId
        };

        var cliente = CreateMockCliente(clienteId, "Carlos", "Vega", 100);
        var comanda = CreateMockComanda(comandaId, clienteId);

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        
        // Verificar que se loggeó el warning
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("no tiene puntos suficientes")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Métodos Helper

    private static Cliente CreateMockCliente(Guid id, string nombre, string apellido, int puntosAcumulados)
    {
        // TODO: Ajustar según la implementación real de Cliente
        var cliente = new Cliente();
        typeof(Cliente).GetProperty("Id")?.SetValue(cliente, id);
        typeof(Cliente).GetProperty("Nombre")?.SetValue(cliente, nombre);
        typeof(Cliente).GetProperty("Apellido")?.SetValue(cliente, apellido);
        typeof(Cliente).GetProperty("PuntosAcumulados")?.SetValue(cliente, puntosAcumulados);
        
        return cliente;
    }

    private static Comanda CreateMockComanda(Guid id, Guid clienteId)
    {
        // TODO: Ajustar según la implementación real de Comanda
        var comanda = new Comanda();
        typeof(Comanda).GetProperty("Id")?.SetValue(comanda, id);
        typeof(Comanda).GetProperty("ClienteId")?.SetValue(comanda, clienteId);
        
        return comanda;
    }

    #endregion
} 