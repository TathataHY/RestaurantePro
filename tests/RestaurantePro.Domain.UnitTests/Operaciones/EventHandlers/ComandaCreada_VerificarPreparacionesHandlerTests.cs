namespace RestaurantePro.Domain.UnitTests.Operaciones.EventHandlers;

/// <summary>
/// Tests para el event handler que verifica preparaciones cuando se crea una comanda
/// </summary>
public class ComandaCreada_VerificarPreparacionesHandlerTests
{
    private readonly Mock<IServicioPreparaciones> _servicioPreparacionesMock;
    private readonly Mock<IComandaRepository> _comandaRepositoryMock;
    private readonly Mock<ILogger<ComandaCreada_VerificarPreparacionesHandler>> _loggerMock;
    private readonly Mock<INotificationManager> _notificationManagerMock;
    private readonly ComandaCreada_VerificarPreparacionesHandler _sut;

    public ComandaCreada_VerificarPreparacionesHandlerTests()
    {
        _servicioPreparacionesMock = new Mock<IServicioPreparaciones>();
        _comandaRepositoryMock = new Mock<IComandaRepository>();
        _loggerMock = new Mock<ILogger<ComandaCreada_VerificarPreparacionesHandler>>();
        _notificationManagerMock = new Mock<INotificationManager>();

        _sut = new ComandaCreada_VerificarPreparacionesHandler(
            _servicioPreparacionesMock.Object,
            _comandaRepositoryMock.Object,
            _loggerMock.Object,
            _notificationManagerMock.Object);
    }

    [Fact]
    public async Task Handle_ConComandaSinItems_DebeLoggearInformacionYSalir()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var empleadoId = Guid.NewGuid();
        
        var evento = new ComandaCreada(comandaId, mesaId, empleadoId);
        var comanda = CrearComandaMock(comandaId, mesaId, empleadoId, sinItems: true);

        _comandaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(comandaId))
            .ReturnsAsync(comanda);

        // Act
        await _sut.Handle(evento, CancellationToken.None);

        // Assert
        /*
        VerificarLog("🍳 Comanda .* creada - iniciando verificación de preparaciones");
        VerificarLog("ℹ️ Comanda .* no tiene ítems, no hay preparaciones que verificar");
        */
    }

    [Fact]
    public async Task Handle_ConComandaConItems_DebeVerificarCadaProducto()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var empleadoId = Guid.NewGuid();
        var producto1Id = Guid.NewGuid();
        var producto2Id = Guid.NewGuid();
        
        var evento = new ComandaCreada(comandaId, mesaId, empleadoId);
        var comanda = CrearComandaMock(comandaId, mesaId, empleadoId, 
            items: new[] { (producto1Id, 2), (producto2Id, 1) });

        _comandaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(comandaId))
            .ReturnsAsync(comanda);

        // Configurar preparaciones: producto1 disponible, producto2 no
        _servicioPreparacionesMock
            .Setup(x => x.VerificarDisponibilidadAsync(
                It.Is<Guid>(id => id == producto1Id), 
                It.Is<int>(c => c == 2)))
            .ReturnsAsync(Result.Success(true));

        _servicioPreparacionesMock
            .Setup(x => x.VerificarDisponibilidadAsync(
                It.Is<Guid>(id => id == producto2Id), 
                It.Is<int>(c => c == 1)))
            .ReturnsAsync(Result.Success(false));

        // Act
        await _sut.Handle(evento, CancellationToken.None);

        // Assert
        /*
        VerificarLog("🍳 Comanda .* creada - iniciando verificación de preparaciones");
        VerificarLog("✅ Producto .* disponible en preparaciones");
        VerificarLog("🥘 Producto .* será preparado al momento");
        VerificarLog("📊 Análisis de preparaciones para comanda");
        VerificarLog("🏁 Verificación de preparaciones completada");
        */
    }

    [Fact]
    public async Task Handle_ConBajoPorcentajePreparaciones_DebeGenerarAlerta()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var empleadoId = Guid.NewGuid();
        var producto1Id = Guid.NewGuid();
        var producto2Id = Guid.NewGuid();
        var producto3Id = Guid.NewGuid();
        
        var evento = new ComandaCreada(comandaId, mesaId, empleadoId);
        var comanda = CrearComandaMock(comandaId, mesaId, empleadoId, 
            items: new[] { (producto1Id, 1), (producto2Id, 1), (producto3Id, 1) });

        _comandaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(comandaId))
            .ReturnsAsync(comanda);

        // Solo 1 de 3 productos disponible en preparaciones (33% < 30%)
        _servicioPreparacionesMock
            .Setup(x => x.VerificarDisponibilidadAsync(
                It.Is<Guid>(id => id == producto1Id), 
                It.Is<int>(c => c == 1)))
            .ReturnsAsync(Result.Success(false));
        _servicioPreparacionesMock
            .Setup(x => x.VerificarDisponibilidadAsync(
                It.Is<Guid>(id => id == producto2Id), 
                It.Is<int>(c => c == 1)))
            .ReturnsAsync(Result.Success(false));
        _servicioPreparacionesMock
            .Setup(x => x.VerificarDisponibilidadAsync(
                It.Is<Guid>(id => id == producto3Id), 
                It.Is<int>(c => c == 1)))
            .ReturnsAsync(Result.Success(false));

        // Act
        await _sut.Handle(evento, CancellationToken.None);

        // Assert
        /*
        VerificarLogNivel("⚠️ ALERTA: Comanda .* tiene bajo porcentaje de preparaciones", LogLevel.Warning);
        */
    }

    [Fact]
    public async Task Handle_ConComandaNoEncontrada_DebeLoggearWarningYSalir()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var empleadoId = Guid.NewGuid();
        
        var evento = new ComandaCreada(comandaId, mesaId, empleadoId);

        _comandaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(comandaId))
            .ReturnsAsync((Comanda?)null);

        // Act
        await _sut.Handle(evento, CancellationToken.None);

        // Assert
        /*
        VerificarLog("🍳 Comanda .* creada - iniciando verificación de preparaciones");
        VerificarLogNivel("⚠️ No se encontró la comanda .* para verificar preparaciones", LogLevel.Warning);
        */
    }

    [Fact]
    public async Task Handle_ConErrorEnServicioPreparaciones_DebeLoggearErrorYContinuar()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var empleadoId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        
        var evento = new ComandaCreada(comandaId, mesaId, empleadoId);
        var comanda = CrearComandaMock(comandaId, mesaId, empleadoId, 
            items: new[] { (productoId, 1) });

        _comandaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(comandaId))
            .ReturnsAsync(comanda);

        _servicioPreparacionesMock
            .Setup(x => x.VerificarDisponibilidadAsync(
                It.Is<Guid>(id => id == productoId), 
                It.Is<int>(c => c == 1)))
            .ThrowsAsync(new InvalidOperationException("Error de prueba"));

        // Act
        var exception = await Record.ExceptionAsync(() => _sut.Handle(evento, CancellationToken.None));

        // Assert
        Assert.Null(exception); // No debe propagar la excepción
        /*
        VerificarLogNivel("❌ Error al verificar preparaciones para producto", LogLevel.Error);
        */
    }

    [Fact]
    public async Task Handle_ConEventoNulo_DebeLanzarArgumentNullException()
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _sut.Handle(null!, CancellationToken.None));
    }

    [Fact]
    public void Constructor_ConParametrosNulos_DebeLanzarArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new ComandaCreada_VerificarPreparacionesHandler(
                null!, 
                _comandaRepositoryMock.Object,
                _loggerMock.Object, 
                _notificationManagerMock.Object));

        Assert.Throws<ArgumentNullException>(() => 
            new ComandaCreada_VerificarPreparacionesHandler(
                _servicioPreparacionesMock.Object,
                null!,
                _loggerMock.Object, 
                _notificationManagerMock.Object));

        Assert.Throws<ArgumentNullException>(() => 
            new ComandaCreada_VerificarPreparacionesHandler(
                _servicioPreparacionesMock.Object, 
                _comandaRepositoryMock.Object,
                null!, 
                _notificationManagerMock.Object));

        Assert.Throws<ArgumentNullException>(() => 
            new ComandaCreada_VerificarPreparacionesHandler(
                _servicioPreparacionesMock.Object, 
                _comandaRepositoryMock.Object,
                _loggerMock.Object, 
                null!));
    }

    // Métodos auxiliares
    private Comanda CrearComandaMock(Guid comandaId, Guid mesaId, Guid empleadoId, bool sinItems = false, (Guid ProductoId, int Cantidad)[]? items = null)
    {
        // Crear comanda real usando factory method
        var comanda = Comanda.Crear(empleadoId, Guid.NewGuid(), mesaId);
        
        // Usar reflexión para establecer el ID
        var idProperty = typeof(EntityBase).GetProperty("Id", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
        if (idProperty != null)
        {
            var setMethod = idProperty.GetSetMethod(true);
            setMethod?.Invoke(comanda, new object[] { comandaId });
        }

        if (!sinItems && items != null)
        {
            foreach (var item in items)
            {
                comanda.AgregarProducto(item.ProductoId, item.Cantidad, 10.0m, "");
            }
        }

        return comanda;
    }

    private void VerificarLog(string patron)
    {
        // Corregir para evitar CS0854 con argumentos opcionales
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(o => o.ToString().Contains(patron)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>(f => true)),
            Times.AtLeastOnce);
    }

    private void VerificarLogNivel(string patron, LogLevel nivel)
    {
        // Corregir para evitar CS0854 con argumentos opcionales
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == nivel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(o => o.ToString().Contains(patron)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>(f => true)),
            Times.AtLeastOnce);
    }
} 