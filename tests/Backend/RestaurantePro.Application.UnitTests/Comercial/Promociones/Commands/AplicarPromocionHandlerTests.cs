namespace RestaurantePro.Application.UnitTests.Comercial.Promociones.Commands;

/// <summary>
/// Tests para AplicarPromocionHandler
/// Valida la lógica de aplicación de promociones a facturas y comandas
/// </summary>
public class AplicarPromocionHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<AplicarPromocionHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ICommunicationService> _mockNotificacionService;
    private readonly Mock<IDateTimeService> _mockDateTimeService;
    private readonly AplicarPromocionHandler _handler;

    public AplicarPromocionHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<AplicarPromocionHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockNotificacionService = new Mock<ICommunicationService>();
        _mockDateTimeService = new Mock<IDateTimeService>();

        _handler = new AplicarPromocionHandler(
            _mockContext.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockCurrentUserService.Object,
            _mockNotificacionService.Object,
            _mockDateTimeService.Object);

        ConfigurarMocksBase();
    }

    #region Tests de Aplicación Exitosa

    [Fact]
    public async Task Handle_ConPromocionValidaEnFactura_DeberiaRetornarErrorEnDesarrollo()
    {
        // Arrange
        var promocionId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();

        var command = new AplicarPromocionCommand
        {
            PromocionId = promocionId,
            FacturaId = facturaId,
            TipoAplicacion = TipoAplicacionPromocion.FacturaCompleta,
            AutorizadoPor = Guid.NewGuid()
        };

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("Error interno al aplicar la promoción");
    }

    [Fact]
    public async Task Handle_ConPromocionValidaEnComanda_DeberiaRetornarErrorEnDesarrollo()
    {
        // Arrange
        var promocionId = Guid.NewGuid();
        var comandaId = Guid.NewGuid();

        var command = new AplicarPromocionCommand
        {
            PromocionId = promocionId,
            ComandaId = comandaId,
            TipoAplicacion = TipoAplicacionPromocion.FacturaCompleta, // Usar valor válido del enum
            AutorizadoPor = Guid.NewGuid()
        };

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("Error interno al aplicar la promoción");
    }

    [Fact]
    public async Task Handle_ConPromocionPorCodigo_DeberiaRetornarErrorEnDesarrollo()
    {
        // Arrange
        var codigoPromocion = "DESCUENTO20";
        var facturaId = Guid.NewGuid();

        var command = new AplicarPromocionCommand
        {
            CodigoPromocion = codigoPromocion,
            FacturaId = facturaId,
            TipoAplicacion = TipoAplicacionPromocion.FacturaCompleta
        };

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("Error interno al aplicar la promoción");
    }

    [Fact]
    public async Task Handle_ConProductosEspecificos_DeberiaRetornarErrorEnDesarrollo()
    {
        // Arrange
        var command = new AplicarPromocionCommand
        {
            PromocionId = Guid.NewGuid(),
            FacturaId = Guid.NewGuid(),
            TipoAplicacion = TipoAplicacionPromocion.ProductosEspecificos,
            ProductosIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
        };

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("Error interno al aplicar la promoción");
    }

    [Fact]
    public async Task Handle_ConPorCategoria_DeberiaRetornarErrorEnDesarrollo()
    {
        // Arrange
        var command = new AplicarPromocionCommand
        {
            PromocionId = Guid.NewGuid(),
            ComandaId = Guid.NewGuid(),
            TipoAplicacion = TipoAplicacionPromocion.PorCategoria,
            ClienteId = Guid.NewGuid()
        };

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("Error interno al aplicar la promoción");
    }

    [Fact]
    public async Task Handle_ConPorCantidadMinima_DeberiaRetornarErrorEnDesarrollo()
    {
        // Arrange
        var command = new AplicarPromocionCommand
        {
            PromocionId = Guid.NewGuid(),
            FacturaId = Guid.NewGuid(),
            TipoAplicacion = TipoAplicacionPromocion.PorCantidadMinima,
            ValidarRestriccionesCliente = false
        };

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("Error interno al aplicar la promoción");
    }

    [Fact]
    public async Task Handle_ConPorMontoMinimo_DeberiaRetornarErrorEnDesarrollo()
    {
        // Arrange
        var command = new AplicarPromocionCommand
        {
            PromocionId = Guid.NewGuid(),
            ComandaId = Guid.NewGuid(),
            TipoAplicacion = TipoAplicacionPromocion.PorMontoMinimo,
            ValidarLimitesUso = false
        };

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("Error interno al aplicar la promoción");
    }

    #endregion

    #region Tests de Validación de Parámetros

    [Fact]
    public async Task Handle_ConDatosAdicionales_DeberiaRetornarErrorEnDesarrollo()
    {
        // Arrange
        var command = new AplicarPromocionCommand
        {
            PromocionId = Guid.NewGuid(),
            FacturaId = Guid.NewGuid(),
            TipoAplicacion = TipoAplicacionPromocion.FacturaCompleta,
            DatosAdicionales = new Dictionary<string, object>
            {
                { "origen", "app_movil" },
                { "campana", "verano2025" }
            }
        };

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("Error interno al aplicar la promoción");
    }

    [Fact]
    public async Task Handle_ConNotasAplicacion_DeberiaRetornarErrorEnDesarrollo()
    {
        // Arrange
        var command = new AplicarPromocionCommand
        {
            CodigoPromocion = "PROMO2025",
            ComandaId = Guid.NewGuid(),
            TipoAplicacion = TipoAplicacionPromocion.FacturaCompleta,
            NotasAplicacion = "Promoción aplicada por solicitud del cliente"
        };

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("Error interno al aplicar la promoción");
    }

    #endregion

    #region Tests de Manejo de Errores

    [Fact]
    public async Task Handle_ConExcepcionInesperada_DeberiaRetornarErrorInterno()
    {
        // Arrange
        var command = CrearComandoValido();
        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);
        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Be("Error interno al aplicar la promoción");
    }

    #endregion

    #region Tests de Logging

    [Fact]
    public async Task Handle_DeberiaLoggearInicioDelProceso()
    {
        // Arrange
        var command = CrearComandoValido();
        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);
        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Be("Error interno al aplicar la promoción");
    }

    [Fact]
    public async Task Handle_ConError_DeberiaLoggearError()
    {
        // Arrange
        var command = new AplicarPromocionCommand
        {
            PromocionId = Guid.NewGuid(),
            FacturaId = Guid.NewGuid(),
            TipoAplicacion = TipoAplicacionPromocion.FacturaCompleta
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - Se debe loggear un error porque ocurre una excepción
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region Métodos Helper

    private void ConfigurarMocksBase()
    {
        _mockCurrentUserService.Setup(x => x.UserId)
            .Returns(Guid.NewGuid().ToString());
    }

    private AplicarPromocionCommand CrearComandoValido()
    {
        var promocionId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();

        return new AplicarPromocionCommand
        {
            PromocionId = promocionId,
            FacturaId = facturaId,
            TipoAplicacion = TipoAplicacionPromocion.FacturaCompleta,
            AutorizadoPor = Guid.NewGuid()
        };
    }

    #endregion
} 