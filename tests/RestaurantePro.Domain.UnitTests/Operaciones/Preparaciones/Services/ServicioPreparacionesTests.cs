namespace RestaurantePro.Domain.UnitTests.Operaciones.Preparaciones.Services;

/// <summary>
/// Pruebas unitarias para ServicioPreparaciones
/// </summary>
public class ServicioPreparacionesTests
{
    private readonly Mock<ILogger<ServicioPreparaciones>> _loggerMock;
    private readonly Mock<INotificationManager> _notificationManagerMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly ServicioPreparaciones _servicio;
    private readonly DateTime _fechaActual = new DateTime(2025, 1, 15, 10, 30, 0);

    public ServicioPreparacionesTests()
    {
        _loggerMock = new Mock<ILogger<ServicioPreparaciones>>();
        _notificationManagerMock = new Mock<INotificationManager>();
        _dateTimeServiceMock = new Mock<IDateTimeService>();
        
        _dateTimeServiceMock.Setup(x => x.Now).Returns(_fechaActual);
        
        // Configurar comportamiento por defecto del NotificationManager
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(false);
        _notificationManagerMock.Setup(x => x.CreateNewNotification());
        
        // Configurar ToResult<PreparacionDiaria> para crear preparaciones exitosas por defecto  
        _notificationManagerMock.Setup(x => x.ToResult<PreparacionDiaria>(It.IsAny<PreparacionDiaria>()))
            .Returns<PreparacionDiaria>(preparacion => Result<PreparacionDiaria>.Success(preparacion));
            
        // Configurar ToResult<bool> para retornar true por defecto
        _notificationManagerMock.Setup(x => x.ToResult<bool>(It.IsAny<bool>()))
            .Returns<bool>(value => Result<bool>.Success(value));
            
        _servicio = new ServicioPreparaciones(
            _loggerMock.Object,
            _notificationManagerMock.Object,
            _dateTimeServiceMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ConParametrosValidos_DebeCrearInstancia()
    {
        // Arrange & Act
        var servicio = new ServicioPreparaciones(
            _loggerMock.Object,
            _notificationManagerMock.Object,
            _dateTimeServiceMock.Object);

        // Assert
        servicio.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ConLoggerNulo_DebeLanzarArgumentNullException()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => 
            new ServicioPreparaciones(null!, _notificationManagerMock.Object, _dateTimeServiceMock.Object));
        
        ex.ParamName.Should().Be("logger");
    }

    [Fact]
    public void Constructor_ConNotificationManagerNulo_DebeLanzarArgumentNullException()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => 
            new ServicioPreparaciones(_loggerMock.Object, null!, _dateTimeServiceMock.Object));
        
        ex.ParamName.Should().Be("notificationManager");
    }

    [Fact]
    public void Constructor_ConDateTimeServiceNulo_DebeLanzarArgumentNullException()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => 
            new ServicioPreparaciones(_loggerMock.Object, _notificationManagerMock.Object, null!));
        
        ex.ParamName.Should().Be("dateTimeService");
    }

    #endregion

    #region PrepararProductoAsync Tests

    [Fact]
    public async Task PrepararProductoAsync_ConParametrosValidos_DebeCrearPreparacion()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var chefId = Guid.NewGuid();
        var cantidad = 10;
        var fechaVencimiento = _fechaActual.AddHours(8);
        var observaciones = "Preparación especial";

        _notificationManagerMock.Setup(x => x.HasErrors).Returns(false);

        // Act
        var resultado = await _servicio.PrepararProductoAsync(
            productoId, cantidad, chefId, fechaVencimiento, observaciones);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.ProductoId.Should().Be(productoId);
        resultado.Value.ChefId.Should().Be(chefId);
        resultado.Value.CantidadPreparada.Should().Be(cantidad);
        resultado.Value.FechaVencimiento.Should().Be(fechaVencimiento);
        resultado.Value.Observaciones.Should().Be(observaciones);
        
        // Verificar que se creó una nueva notificación
        _notificationManagerMock.Verify(x => x.CreateNewNotification(), Times.Once);
    }

    [Fact]
    public async Task PrepararProductoAsync_ConProductoIdVacio_DebeRetornarFallo()
    {
        // Arrange
        var chefId = Guid.NewGuid();
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);
        _notificationManagerMock.Setup(x => x.ToResult<PreparacionDiaria>(It.IsAny<PreparacionDiaria>()))
            .Returns(Result<PreparacionDiaria>.Failure("ProductoId es requerido"));

        // Act
        var resultado = await _servicio.PrepararProductoAsync(Guid.Empty, 10, chefId);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        
        // Verificar que se creó una nueva notificación
        _notificationManagerMock.Verify(x => x.CreateNewNotification(), Times.Once);
    }

    [Fact]
    public async Task PrepararProductoAsync_ConCantidadCero_DebeRetornarFallo()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var chefId = Guid.NewGuid();
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);

        // Act
        var resultado = await _servicio.PrepararProductoAsync(productoId, 0, chefId);

        // Assert
        resultado.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task PrepararProductoAsync_ConCantidadNegativa_DebeRetornarFallo()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var chefId = Guid.NewGuid();
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);

        // Act
        var resultado = await _servicio.PrepararProductoAsync(productoId, -5, chefId);

        // Assert
        resultado.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task PrepararProductoAsync_ConChefIdVacio_DebeRetornarFallo()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);

        // Act
        var resultado = await _servicio.PrepararProductoAsync(productoId, 10, Guid.Empty);

        // Assert
        resultado.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task PrepararProductoAsync_ConFechaVencimientoPasada_DebeRetornarFallo()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var chefId = Guid.NewGuid();
        var fechaVencimientoPasada = _fechaActual.AddHours(-2);
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);

        // Act
        var resultado = await _servicio.PrepararProductoAsync(
            productoId, 10, chefId, fechaVencimientoPasada);

        // Assert
        resultado.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task PrepararProductoAsync_ConFechaVencimientoMuyLejana_DebeAgregarAdvertencia()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var chefId = Guid.NewGuid();
        var fechaVencimientoLejana = _fechaActual.AddDays(10);
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(false);

        // Act
        var resultado = await _servicio.PrepararProductoAsync(
            productoId, 10, chefId, fechaVencimientoLejana);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        
        _notificationManagerMock.Verify(
            x => x.AddInformation(It.Is<string>(s => s.Contains("ADVERTENCIA")), It.IsAny<string>()), 
            Times.Once);
    }

    [Fact]
    public async Task PrepararProductoAsync_SinFechaVencimiento_DebeCrearPreparacionSinFecha()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var chefId = Guid.NewGuid();
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(false);

        // Act
        var resultado = await _servicio.PrepararProductoAsync(productoId, 5, chefId);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.FechaVencimiento.Should().BeNull();
    }

    #endregion

    #region VerificarDisponibilidadAsync Tests

    [Fact]
    public async Task VerificarDisponibilidadAsync_ConParametrosValidos_DebeRetornarTrue()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var cantidadRequerida = 5;
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(false);

        // Act
        var resultado = await _servicio.VerificarDisponibilidadAsync(productoId, cantidadRequerida);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().BeTrue(); // Implementación temporal siempre retorna true
    }

    [Fact]
    public async Task VerificarDisponibilidadAsync_ConProductoIdVacio_DebeRetornarFallo()
    {
        // Arrange
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);
        _notificationManagerMock.Setup(x => x.ToResult<bool>(It.IsAny<bool>()))
            .Returns(Result<bool>.Failure("ProductoId es requerido"));

        // Act
        var resultado = await _servicio.VerificarDisponibilidadAsync(Guid.Empty, 5);

        // Assert
        resultado.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task VerificarDisponibilidadAsync_ConCantidadCero_DebeRetornarFallo()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);
        _notificationManagerMock.Setup(x => x.ToResult<bool>(It.IsAny<bool>()))
            .Returns(Result<bool>.Failure("Cantidad debe ser mayor que cero"));

        // Act
        var resultado = await _servicio.VerificarDisponibilidadAsync(productoId, 0);

        // Assert
        resultado.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task VerificarDisponibilidadAsync_ConCantidadNegativa_DebeRetornarFallo()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);
        _notificationManagerMock.Setup(x => x.ToResult<bool>(It.IsAny<bool>()))
            .Returns(Result<bool>.Failure("Cantidad debe ser mayor que cero"));

        // Act
        var resultado = await _servicio.VerificarDisponibilidadAsync(productoId, -3);

        // Assert
        resultado.Succeeded.Should().BeFalse();
    }

    #endregion

    #region ConsumirPreparacionAsync Tests

    [Fact]
    public async Task ConsumirPreparacionAsync_ConParametrosValidos_DebeRetornarExito()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var cantidad = 3;
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(false);

        // Act
        var resultado = await _servicio.ConsumirPreparacionAsync(productoId, cantidad);

        // Assert
        resultado.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task ConsumirPreparacionAsync_ConProductoIdVacio_DebeRetornarFallo()
    {
        // Arrange
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);

        // Act
        var resultado = await _servicio.ConsumirPreparacionAsync(Guid.Empty, 3);

        // Assert
        resultado.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task ConsumirPreparacionAsync_ConCantidadCero_DebeRetornarFallo()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);

        // Act
        var resultado = await _servicio.ConsumirPreparacionAsync(productoId, 0);

        // Assert
        resultado.Succeeded.Should().BeFalse();
    }

    #endregion

    #region ObtenerPreparacionesDelDiaAsync Tests

    [Fact]
    public async Task ObtenerPreparacionesDelDiaAsync_DebeRetornarListaVacia()
    {
        // Act
        var resultado = await _servicio.ObtenerPreparacionesDelDiaAsync();

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.Should().BeEmpty(); // Implementación temporal retorna lista vacía
    }

    #endregion

    #region ObtenerPreparacionesPorProductoAsync Tests

    [Fact]
    public async Task ObtenerPreparacionesPorProductoAsync_ConProductoValido_DebeRetornarListaVacia()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(false);

        // Act
        var resultado = await _servicio.ObtenerPreparacionesPorProductoAsync(productoId);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerPreparacionesPorProductoAsync_ConProductoIdVacio_DebeRetornarFallo()
    {
        // Arrange
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);

        // Act
        var resultado = await _servicio.ObtenerPreparacionesPorProductoAsync(Guid.Empty);

        // Assert
        resultado.Succeeded.Should().BeFalse();
    }

    #endregion

    #region MarcarVencidasAsync Tests

    [Fact]
    public async Task MarcarVencidasAsync_DebeRetornarCero()
    {
        // Act
        var resultado = await _servicio.MarcarVencidasAsync();

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().Be(0); // Implementación temporal
    }

    #endregion

    #region ObtenerPreparacionesPorVencerAsync Tests

    [Fact]
    public async Task ObtenerPreparacionesPorVencerAsync_ConHorasValidas_DebeRetornarListaVacia()
    {
        // Arrange
        var horasAnticipacion = 2;
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(false);

        // Act
        var resultado = await _servicio.ObtenerPreparacionesPorVencerAsync(horasAnticipacion);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerPreparacionesPorVencerAsync_ConHorasNegativas_DebeRetornarFallo()
    {
        // Arrange
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);

        // Act
        var resultado = await _servicio.ObtenerPreparacionesPorVencerAsync(-1);

        // Assert
        resultado.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task ObtenerPreparacionesPorVencerAsync_SinParametros_DebeUsarValorPorDefecto()
    {
        // Arrange
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(false);

        // Act
        var resultado = await _servicio.ObtenerPreparacionesPorVencerAsync();

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
    }

    #endregion

    #region MarcarComoDisponibleAsync Tests

    [Fact]
    public async Task MarcarComoDisponibleAsync_ConIdValido_DebeRetornarExito()
    {
        // Arrange
        var preparacionId = Guid.NewGuid();
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(false);

        // Act
        var resultado = await _servicio.MarcarComoDisponibleAsync(preparacionId);

        // Assert
        resultado.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task MarcarComoDisponibleAsync_ConIdVacio_DebeRetornarFallo()
    {
        // Arrange
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);

        // Act
        var resultado = await _servicio.MarcarComoDisponibleAsync(Guid.Empty);

        // Assert
        resultado.Succeeded.Should().BeFalse();
    }

    #endregion

    #region AgregarCantidadAsync Tests

    [Fact]
    public async Task AgregarCantidadAsync_ConParametrosValidos_DebeRetornarExito()
    {
        // Arrange
        var preparacionId = Guid.NewGuid();
        var cantidadAdicional = 5;
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(false);

        // Act
        var resultado = await _servicio.AgregarCantidadAsync(preparacionId, cantidadAdicional);

        // Assert
        resultado.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task AgregarCantidadAsync_ConIdVacio_DebeRetornarFallo()
    {
        // Arrange
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);

        // Act
        var resultado = await _servicio.AgregarCantidadAsync(Guid.Empty, 5);

        // Assert
        resultado.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task AgregarCantidadAsync_ConCantidadCero_DebeRetornarFallo()
    {
        // Arrange
        var preparacionId = Guid.NewGuid();
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);

        // Act
        var resultado = await _servicio.AgregarCantidadAsync(preparacionId, 0);

        // Assert
        resultado.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task AgregarCantidadAsync_ConCantidadNegativa_DebeRetornarFallo()
    {
        // Arrange
        var preparacionId = Guid.NewGuid();
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);

        // Act
        var resultado = await _servicio.AgregarCantidadAsync(preparacionId, -3);

        // Assert
        resultado.Succeeded.Should().BeFalse();
    }

    #endregion

    #region ObtenerEstadisticasDelDiaAsync Tests

    [Fact]
    public async Task ObtenerEstadisticasDelDiaAsync_DebeRetornarEstadisticasVacias()
    {
        // Act
        var resultado = await _servicio.ObtenerEstadisticasDelDiaAsync();

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.TotalPreparaciones.Should().Be(0);
        resultado.Value.PreparacionesDisponibles.Should().Be(0);
        resultado.Value.PreparacionesAgotadas.Should().Be(0);
        resultado.Value.PreparacionesVencidas.Should().Be(0);
        resultado.Value.PreparacionesPorVencer.Should().Be(0);
        resultado.Value.PorcentajeEficiencia.Should().Be(0);
        resultado.Value.CantidadTotalPreparada.Should().Be(0);
        resultado.Value.CantidadTotalConsumida.Should().Be(0);
        resultado.Value.CantidadDesperdiciada.Should().Be(0);
    }

    #endregion
} 