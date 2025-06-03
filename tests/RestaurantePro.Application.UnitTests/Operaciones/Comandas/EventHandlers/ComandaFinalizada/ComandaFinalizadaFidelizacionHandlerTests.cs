namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.EventHandlers.ComandaFinalizada;

/// <summary>
/// Tests para ComandaFinalizadaFidelizacionHandler - Gestión automática de puntos de fidelización
/// </summary>
public class ComandaFinalizadaFidelizacionHandlerTests
{
    private readonly Mock<IComandaRepository> _mockComandaRepository;
    private readonly Mock<IClienteRepository> _mockClienteRepository;
    private readonly Mock<IServicioFidelizacion> _mockServicioFidelizacion;
    private readonly Mock<ILogger<ComandaFinalizadaFidelizacionHandler>> _mockLogger;
    private readonly Mock<IMediator> _mockMediator;
    private readonly ComandaFinalizadaFidelizacionHandler _handler;

    public ComandaFinalizadaFidelizacionHandlerTests()
    {
        _mockComandaRepository = new Mock<IComandaRepository>();
        _mockClienteRepository = new Mock<IClienteRepository>();
        _mockServicioFidelizacion = new Mock<IServicioFidelizacion>();
        _mockLogger = new Mock<ILogger<ComandaFinalizadaFidelizacionHandler>>();
        _mockMediator = new Mock<IMediator>();
        
        _handler = new ComandaFinalizadaFidelizacionHandler(
            _mockComandaRepository.Object,
            _mockClienteRepository.Object,
            _mockServicioFidelizacion.Object,
            _mockLogger.Object,
            _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_ComandaFinalizadaConCliente_DeberiaAcumularPuntos()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var montoTotal = 150.00m;
        var evento = new ComandaFinalizadaEvent(comandaId, montoTotal);

        var comanda = CreateMockComanda(comandaId, clienteId);
        var cliente = CreateMockCliente(clienteId, "Juan Pérez", "juan@email.com");
        
        _mockComandaRepository.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockServicioFidelizacion.Setup(x => x.AcumularPuntosAsync(
                clienteId, comandaId, montoTotal))
            .ReturnsAsync(Result.Success(15)); // 15 puntos acumulados

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockComandaRepository.Verify(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()), Times.Once);
        _mockClienteRepository.Verify(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
        
        _mockServicioFidelizacion.Verify(x => x.AcumularPuntosAsync(
            clienteId, comandaId, montoTotal), Times.Once);

        // Debería loggear acumulación exitosa
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Se acumularon")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ComandaSinCliente_NoDeberiaAcumularPuntos()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var montoTotal = 150.00m;
        var evento = new ComandaFinalizadaEvent(comandaId, montoTotal);

        var comanda = CreateMockComandaSinCliente(comandaId);
        
        _mockComandaRepository.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockComandaRepository.Verify(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()), Times.Once);
        _mockClienteRepository.Verify(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockServicioFidelizacion.Verify(x => x.AcumularPuntosAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>()), Times.Never);

        // Debería loggear que no hay cliente
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("no tiene cliente asociado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ClienteNoEncontrado_DeberiaLoggearAdvertencia()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var montoTotal = 150.00m;
        var evento = new ComandaFinalizadaEvent(comandaId, montoTotal);

        var comanda = CreateMockComanda(comandaId, clienteId);
        
        _mockComandaRepository.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente)null!); // Cliente no encontrado

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockClienteRepository.Verify(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
        _mockServicioFidelizacion.Verify(x => x.AcumularPuntosAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>()), Times.Never);

        // Debería loggear advertencia
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No se encontró el cliente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ErrorEnAcumulacionPuntos_DeberiaLoggearError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var montoTotal = 150.00m;
        var evento = new ComandaFinalizadaEvent(comandaId, montoTotal);

        var comanda = CreateMockComanda(comandaId, clienteId);
        var cliente = CreateMockCliente(clienteId, "Juan Pérez", "juan@email.com");
        
        _mockComandaRepository.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockServicioFidelizacion.Setup(x => x.AcumularPuntosAsync(
                clienteId, comandaId, montoTotal))
            .ReturnsAsync(Result.Failure<int>("Error procesando puntos"));

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al acumular puntos")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MontoMuyBajo_NoDeberiaAcumularPuntos()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var montoTotal = 5.00m; // Monto muy bajo
        var evento = new ComandaFinalizadaEvent(comandaId, montoTotal);

        var comanda = CreateMockComanda(comandaId, clienteId);
        var cliente = CreateMockCliente(clienteId, "Juan Pérez", "juan@email.com");
        
        _mockComandaRepository.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        // El servicio debería ser llamado siempre que haya cliente, independientemente del monto
        _mockServicioFidelizacion.Verify(x => x.AcumularPuntosAsync(clienteId, comandaId, montoTotal), Times.Once);

        // Debería loggear el procesamiento
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando procesamiento de fidelización")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(50.00, 5)]    // $50 = 5 puntos (1 punto por cada $10)
    [InlineData(100.00, 10)]  // $100 = 10 puntos
    [InlineData(155.50, 15)]  // $155.50 = 15 puntos (redondeo hacia abajo)
    [InlineData(200.00, 20)]  // $200 = 20 puntos
    public async Task Handle_DiferentesMontos_DeberiaCalcularPuntosCorrectamente(
        decimal montoTotal, int puntosEsperados)
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var evento = new ComandaFinalizadaEvent(comandaId, montoTotal);

        var comanda = CreateMockComanda(comandaId, clienteId);
        var cliente = CreateMockCliente(clienteId, "Juan Pérez", "juan@email.com");
        
        _mockComandaRepository.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockServicioFidelizacion.Setup(x => x.AcumularPuntosAsync(
                clienteId, comandaId, montoTotal))
            .ReturnsAsync(Result.Success(puntosEsperados));

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockServicioFidelizacion.Verify(x => x.AcumularPuntosAsync(
            clienteId, comandaId, montoTotal), Times.Once);

        // Debería loggear los puntos calculados
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(puntosEsperados.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ExcepcionEnRepositorio_DeberiaLoggearYPropagar()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var montoTotal = 150.00m;
        var evento = new ComandaFinalizadaEvent(comandaId, montoTotal);

        var repositoryException = new Exception("Error de conexión a base de datos");
        _mockComandaRepository.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(repositoryException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(evento, CancellationToken.None));

        exception.Should().Be(repositoryException);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al procesar fidelización")),
                repositoryException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConCancellationToken_DeberiaRespetarCancelacion()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var montoTotal = 150.00m;
        var evento = new ComandaFinalizadaEvent(comandaId, montoTotal);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _mockComandaRepository.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(evento, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Handle_ClienteVIP_DeberiaAplicarBonificacion()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var montoTotal = 100.00m;
        var evento = new ComandaFinalizadaEvent(comandaId, montoTotal);

        var comanda = CreateMockComanda(comandaId, clienteId);
        var clienteVIP = CreateMockClienteVIP(clienteId, "María VIP", "maria@vip.com");
        
        _mockComandaRepository.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteVIP);

        _mockServicioFidelizacion.Setup(x => x.AcumularPuntosAsync(
                clienteId, comandaId, montoTotal))
            .ReturnsAsync(Result.Success(15)); // 10 puntos base + 5 bonificación VIP

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Se acumularon")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_DeberiaIncluirContextoComandaEnLogs()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var montoTotal = 275.50m;
        var evento = new ComandaFinalizadaEvent(comandaId, montoTotal);

        var comanda = CreateMockComanda(comandaId, clienteId);
        var cliente = CreateMockCliente(clienteId, "Carlos Cliente", "carlos@email.com");
        
        _mockComandaRepository.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockServicioFidelizacion.Setup(x => x.AcumularPuntosAsync(
                clienteId, comandaId, montoTotal))
            .ReturnsAsync(Result.Success(27));

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(comandaId.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("275")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_AcumulacionExitosa_DeberiaLoggearCorrectamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var montoTotal = 500.00m;
        var evento = new ComandaFinalizadaEvent(comandaId, montoTotal);

        var comanda = CreateMockComanda(comandaId, clienteId);
        var cliente = CreateMockCliente(clienteId, "Ana Cliente", "ana@email.com");
        
        _mockComandaRepository.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockServicioFidelizacion.Setup(x => x.AcumularPuntosAsync(
                clienteId, comandaId, montoTotal))
            .ReturnsAsync(Result.Success(50));

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockServicioFidelizacion.Verify(x => x.AcumularPuntosAsync(
            clienteId, comandaId, montoTotal), Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Procesamiento de fidelización completo")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // Helper methods para crear clientes mock
    private static Cliente CreateMockCliente(Guid id, string nombre, string email)
    {
        var nombreCompleto = ClienteNombre.Crear(nombre, "Apellido");
        var emailObj = Email.Create(email);
        var telefono = PhoneNumber.Create("1234567890");
        
        var cliente = Cliente.Crear(
            id,
            nombreCompleto,
            emailObj,
            telefono,
            DateTime.Now.AddYears(-25));
        
        return cliente;
    }

    private static Cliente CreateMockClienteVIP(Guid id, string nombre, string email)
    {
        var cliente = CreateMockCliente(id, nombre, email);
        
        // Para simplificar, retornamos el mismo cliente
        // En un escenario real tendríamos un método para establecer el segmento
        return cliente;
    }

    private static Comanda CreateMockComanda(Guid id, Guid clienteId)
    {
        var mesaId = Guid.NewGuid();
        
        var comanda = Comanda.Crear(mesaId, clienteId);
        
        // Usar reflection para establecer el ID
        var idProperty = typeof(Comanda).GetProperty("Id");
        idProperty?.SetValue(comanda, id);
        
        return comanda;
    }

    private static Comanda CreateMockComandaSinCliente(Guid id)
    {
        var mesaId = Guid.NewGuid();
        
        var comanda = Comanda.Crear(mesaId, null);
        
        // Usar reflection para establecer el ID
        var idProperty = typeof(Comanda).GetProperty("Id");
        idProperty?.SetValue(comanda, id);
        
        return comanda;
    }
} 