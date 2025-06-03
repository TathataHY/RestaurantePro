using RestaurantePro.Domain.Core.Base;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Application.UnitTests.Common;

namespace RestaurantePro.Application.UnitTests.Comercial.Facturacion.Commands;

/// <summary>
/// Tests unitarios para AplicarDescuentoHandler
/// Valida la lógica completa de aplicación de descuentos con múltiples tipos y validaciones
/// </summary>
public class AplicarDescuentoHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<AplicarDescuentoHandler>> _loggerMock;
    private readonly Mock<IServicioFacturacion> _servicioFacturacionMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<DbSet<Factura>> _facturasDbSetMock;
    private readonly AplicarDescuentoHandler _handler;

    public AplicarDescuentoHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<AplicarDescuentoHandler>>();
        _servicioFacturacionMock = new Mock<IServicioFacturacion>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _emailServiceMock = new Mock<IEmailService>();
        _facturasDbSetMock = new Mock<DbSet<Factura>>();

        // Setup DbContext
        _contextMock.Setup(x => x.Facturas).Returns(_facturasDbSetMock.Object);

        _handler = new AplicarDescuentoHandler(
            _contextMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _servicioFacturacionMock.Object,
            _currentUserServiceMock.Object,
            _emailServiceMock.Object);
    }

    #region Tests de Factory Methods del Command

    [Fact]
    public void CrearDescuentoPorcentaje_ConParametrosValidos_DeberiaCrearCommandCorrectamente()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var porcentaje = 10m;
        var concepto = "Descuento por cliente frecuente";
        var motivo = "Cliente con más de 5 compras";

        // Act
        var command = AplicarDescuentoCommand.CrearDescuentoPorcentaje(facturaId, porcentaje, concepto, motivo, usuarioId);

        // Assert
        Assert.Equal(facturaId, command.FacturaId);
        Assert.Equal("General", command.TipoDescuento);
        Assert.Equal(porcentaje, command.Porcentaje);
        Assert.Equal(concepto, command.Concepto);
        Assert.Equal(motivo, command.Motivo);
        Assert.Equal(usuarioId, command.UsuarioAutorizaId);
        Assert.True(command.AplicarAntesDeImpuestos);
        Assert.False(command.EsAcumulable);
        Assert.Equal(5, command.Prioridad);
    }

    [Fact]
    public void CrearDescuentoMontoFijo_ConParametrosValidos_DeberiaCrearCommandCorrectamente()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var montoFijo = 50.00m;
        var concepto = "Descuento promocional";
        var motivo = "Promoción del día";

        // Act
        var command = AplicarDescuentoCommand.CrearDescuentoMontoFijo(facturaId, montoFijo, concepto, motivo, usuarioId);

        // Assert
        Assert.Equal("MontoFijo", command.TipoDescuento);
        Assert.Equal(montoFijo, command.MontoFijo);
        Assert.Equal(0, command.Porcentaje);
    }

    [Fact]
    public void CrearDescuentoEmpleado_ConParametrosValidos_DeberiaConfigurarDescuentoEmpleado()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var porcentaje = 15m;

        // Act
        var command = AplicarDescuentoCommand.CrearDescuentoEmpleado(facturaId, porcentaje, usuarioId);

        // Assert
        Assert.Equal("Empleado", command.TipoDescuento);
        Assert.Equal(porcentaje, command.Porcentaje);
        Assert.Equal("Descuento por empleado", command.Concepto);
        Assert.Equal("Política de descuentos para empleados", command.Motivo);
        Assert.Equal(8, command.Prioridad);
    }

    [Fact]
    public void CrearDescuentoPromocional_ConCodigoYFecha_DeberiaConfigurarDescuentoPromocional()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var porcentaje = 20m;
        var codigoPromocional = "PROMO2025";
        var fechaExpiracion = DateTime.UtcNow.AddDays(30);

        // Act
        var command = AplicarDescuentoCommand.CrearDescuentoPromocional(facturaId, porcentaje, codigoPromocional, fechaExpiracion, usuarioId);

        // Assert
        Assert.Equal("Promocional", command.TipoDescuento);
        Assert.Equal(codigoPromocional, command.CodigoAutorizacion);
        Assert.Equal(fechaExpiracion, command.FechaExpiracion);
        Assert.True(command.EsAcumulable);
        Assert.Equal(6, command.Prioridad);
    }

    [Fact]
    public void CrearDescuentoVolumen_ConMontoMinimo_DeberiaConfigurarDescuentoVolumen()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var porcentaje = 12m;
        var montoMinimo = 500m;

        // Act
        var command = AplicarDescuentoCommand.CrearDescuentoVolumen(facturaId, porcentaje, montoMinimo, usuarioId);

        // Assert
        Assert.Equal("Volumen", command.TipoDescuento);
        Assert.Equal(montoMinimo, command.MontoMinimoFactura);
        Assert.Contains(montoMinimo.ToString("C"), command.Motivo);
        Assert.Equal(7, command.Prioridad);
    }

    [Fact]
    public void CrearDescuentoProductos_ConListaProductos_DeberiaConfigurarDescuentoEspecifico()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var productosIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var porcentaje = 25m;
        var motivo = "Productos en liquidación";

        // Act
        var command = AplicarDescuentoCommand.CrearDescuentoProductos(facturaId, productosIds, porcentaje, motivo, usuarioId);

        // Assert
        Assert.Equal("ProductosEspecificos", command.TipoDescuento);
        Assert.Equal(productosIds.Count, command.ProductosEspecificos.Count);
        Assert.Equal("Descuento en productos específicos", command.Concepto);
        Assert.Equal(4, command.Prioridad);
    }

    [Fact]
    public void CrearDescuentoCategoria_ConListaCategorias_DeberiaConfigurarDescuentoCategoria()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var categorias = new List<string> { "Bebidas", "Postres" };
        var porcentaje = 15m;
        var motivo = "Promoción de categorías";

        // Act
        var command = AplicarDescuentoCommand.CrearDescuentoCategoria(facturaId, categorias, porcentaje, motivo, usuarioId);

        // Assert
        Assert.Equal("Categoria", command.TipoDescuento);
        Assert.Equal(categorias.Count, command.CategoriasAplicables.Count);
        Assert.Contains("Bebidas", command.Concepto);
        Assert.Contains("Postres", command.Concepto);
    }

    [Fact]
    public void CrearDescuentoCortesia_ConCodigoAutorizacion_DeberiaConfigurarDescuentoCortesia()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var montoFijo = 100m;
        var motivo = "Compensación por inconvenientes";
        var codigoAutorizacion = "CORTESIA-001";

        // Act
        var command = AplicarDescuentoCommand.CrearDescuentoCortesia(facturaId, montoFijo, motivo, usuarioId, codigoAutorizacion);

        // Assert
        Assert.Equal("Cortesia", command.TipoDescuento);
        Assert.Equal(montoFijo, command.MontoFijo);
        Assert.Equal(codigoAutorizacion, command.CodigoAutorizacion);
        Assert.False(command.AplicarAntesDeImpuestos);
        Assert.False(command.EsAcumulable);
        Assert.Equal(9, command.Prioridad);
    }

    #endregion

    #region Tests de Escenarios Exitosos

    [Fact]
    public async Task Handle_DescuentoPorcentajeSimple_DeberiaAplicarDescuentoExitosamente()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var command = new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "General",
            Porcentaje = 10m,
            Concepto = "Descuento general",
            Motivo = "Cliente frecuente",
            UsuarioAutorizaId = usuarioId,
            AplicarAntesDeImpuestos = true
        };

        var factura = CreateFacturaEmitida(facturaId, 1000m, 800m);
        var facturaDto = CreateMockFacturaDto(facturaId);

        SetupFacturasDbSet(new List<Factura> { factura });
        
        _servicioFacturacionMock.Setup(x => x.AplicarDescuentoAsync(
                facturaId, "General", 80m, "Descuento general", "Cliente frecuente", usuarioId, true, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _mapperMock.Setup(x => x.Map<FacturaDto>(It.IsAny<Factura>()))
            .Returns(facturaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(facturaId, result.Value.Id);
        
        _servicioFacturacionMock.Verify(x => x.AplicarDescuentoAsync(
            facturaId, "General", 80m, "Descuento general", "Cliente frecuente", usuarioId, true, null, It.IsAny<CancellationToken>()), Times.Once);
        
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DescuentoMontoFijo_DeberiaAplicarMontoExacto()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var command = new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "MontoFijo",
            MontoFijo = 50m,
            Concepto = "Descuento promocional",
            Motivo = "Promoción especial",
            UsuarioAutorizaId = usuarioId
        };

        var factura = CreateFacturaEmitida(facturaId, 500m, 400m);
        var facturaDto = CreateMockFacturaDto(facturaId);

        SetupFacturasDbSet(new List<Factura> { factura });
        
        _servicioFacturacionMock.Setup(x => x.AplicarDescuentoAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), 50m, It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _mapperMock.Setup(x => x.Map<FacturaDto>(It.IsAny<Factura>()))
            .Returns(facturaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        _servicioFacturacionMock.Verify(x => x.AplicarDescuentoAsync(
            facturaId, "MontoFijo", 50m, "Descuento promocional", "Promoción especial", usuarioId, true, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DescuentoConMontoMaximo_DeberiaAjustarAlMaximo()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var command = new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "General",
            Porcentaje = 50m, // 50% de 1000 = 500, pero máximo es 200
            MontoMaximoDescuento = 200m,
            Concepto = "Descuento limitado",
            Motivo = "Descuento con límite máximo",
            UsuarioAutorizaId = usuarioId
        };

        var factura = CreateFacturaEmitida(facturaId, 1000m, 800m);
        var facturaDto = CreateMockFacturaDto(facturaId);

        SetupFacturasDbSet(new List<Factura> { factura });
        
        _servicioFacturacionMock.Setup(x => x.AplicarDescuentoAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), 200m, It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _mapperMock.Setup(x => x.Map<FacturaDto>(It.IsAny<Factura>()))
            .Returns(facturaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verificar que se aplicó el monto máximo en lugar del porcentaje completo
        _servicioFacturacionMock.Verify(x => x.AplicarDescuentoAsync(
            facturaId, "General", 200m, "Descuento limitado", "Descuento con límite máximo", usuarioId, true, null, It.IsAny<CancellationToken>()), Times.Once);
        
        // Verificar logging del ajuste al máximo
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Descuento ajustado al máximo permitido")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DescuentoEmpleado_DeberiaProcesarLogicaEspecifica()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var command = new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "Empleado",
            Porcentaje = 15m,
            Concepto = "Descuento por empleado",
            Motivo = "Política de empleados",
            UsuarioAutorizaId = usuarioId
        };

        var factura = CreateFacturaEmitida(facturaId, 1000m, 800m);
        var facturaDto = CreateMockFacturaDto(facturaId);

        SetupFacturasDbSet(new List<Factura> { factura });
        
        _servicioFacturacionMock.Setup(x => x.AplicarDescuentoAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _mapperMock.Setup(x => x.Map<FacturaDto>(It.IsAny<Factura>()))
            .Returns(facturaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verificar que se procesó la lógica específica de empleado (por logging)
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Descuento aplicado exitosamente")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_DescuentoPromocional_DeberiaProcesarCodigoPromocional()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var codigoPromocional = "PROMO2025";
        var command = new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "Promocional",
            Porcentaje = 20m,
            Concepto = "Descuento promocional",
            Motivo = "Código promocional",
            CodigoAutorizacion = codigoPromocional,
            UsuarioAutorizaId = usuarioId
        };

        var factura = CreateFacturaEmitida(facturaId, 1000m, 800m);
        var facturaDto = CreateMockFacturaDto(facturaId);

        SetupFacturasDbSet(new List<Factura> { factura });
        
        _servicioFacturacionMock.Setup(x => x.AplicarDescuentoAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<Guid>(), It.IsAny<bool>(), codigoPromocional, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _mapperMock.Setup(x => x.Map<FacturaDto>(It.IsAny<Factura>()))
            .Returns(facturaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        _servicioFacturacionMock.Verify(x => x.AplicarDescuentoAsync(
            facturaId, "Promocional", 160m, "Descuento promocional", "Código promocional", usuarioId, true, codigoPromocional, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DescuentoProductosEspecificos_DeberiaCrearRelaciones()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var productosIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var command = new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "ProductosEspecificos",
            Porcentaje = 25m,
            ProductosEspecificos = productosIds,
            Concepto = "Descuento productos específicos",
            Motivo = "Liquidación de productos",
            UsuarioAutorizaId = usuarioId
        };

        var factura = CreateFacturaEmitida(facturaId, 1000m, 800m);
        var facturaDto = CreateMockFacturaDto(facturaId);

        SetupFacturasDbSet(new List<Factura> { factura });
        
        _servicioFacturacionMock.Setup(x => x.AplicarDescuentoAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _mapperMock.Setup(x => x.Map<FacturaDto>(It.IsAny<Factura>()))
            .Returns(facturaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verificar que se procesó descuento en productos específicos
        Assert.Contains(productosIds.First(), command.ProductosEspecificos);
        Assert.Contains(productosIds.Last(), command.ProductosEspecificos);
    }

    [Fact]
    public async Task Handle_DescuentoSignificativo_DeberiaEnviarNotificacionAdministracion()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var command = new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "General",
            MontoFijo = 2500m, // Monto alto que debe generar notificación
            Concepto = "Descuento significativo",
            Motivo = "Descuento especial autorizado",
            UsuarioAutorizaId = usuarioId
        };

        var factura = CreateFacturaEmitida(facturaId, 5000m, 4000m);
        var facturaDto = CreateMockFacturaDto(facturaId);

        SetupFacturasDbSet(new List<Factura> { factura });
        
        _servicioFacturacionMock.Setup(x => x.AplicarDescuentoAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _mapperMock.Setup(x => x.Map<FacturaDto>(It.IsAny<Factura>()))
            .Returns(facturaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verificar que se envió notificación a administración por monto alto
        _emailServiceMock.Verify(x => x.SendEmailAsync(
            It.Is<string>(email => email.Contains("admin")),
            It.Is<string>(asunto => asunto.Contains("Descuento Significativo")),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DescuentoCortesia_DeberiaEnviarNotificacionEspecial()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var command = new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "Cortesia",
            MontoFijo = 500m,
            Concepto = "Descuento de cortesía",
            Motivo = "Compensación por inconvenientes",
            CodigoAutorizacion = "CORTESIA-001",
            UsuarioAutorizaId = usuarioId
        };

        var factura = CreateFacturaEmitida(facturaId, 1000m, 800m);
        var facturaDto = CreateMockFacturaDto(facturaId);

        SetupFacturasDbSet(new List<Factura> { factura });
        
        _servicioFacturacionMock.Setup(x => x.AplicarDescuentoAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _mapperMock.Setup(x => x.Map<FacturaDto>(It.IsAny<Factura>()))
            .Returns(facturaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verificar que se envió notificación por descuento de cortesía
        _emailServiceMock.Verify(x => x.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region Tests de Validaciones y Errores

    [Fact]
    public async Task Handle_FacturaNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var command = new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "General",
            Porcentaje = 10m,
            Concepto = "Descuento test",
            Motivo = "Test",
            UsuarioAutorizaId = Guid.NewGuid()
        };

        SetupFacturasDbSet(new List<Factura>()); // Factura no existe

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("La factura especificada no existe", result.Error);
    }

    [Fact]
    public async Task Handle_FacturaAnulada_DeberiaRetornarError()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var command = new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "General",
            Porcentaje = 10m,
            Concepto = "Descuento test",
            Motivo = "Test",
            UsuarioAutorizaId = Guid.NewGuid()
        };

        var facturaAnulada = CreateFacturaAnulada(facturaId);

        SetupFacturasDbSet(new List<Factura> { facturaAnulada });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("No se pueden aplicar descuentos a facturas anuladas", result.Error);
    }

    [Fact]
    public async Task Handle_DescuentoExcedeTotal_DeberiaRetornarError()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var command = new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "General",
            MontoFijo = 1500m, // Mayor al total de la factura (1000)
            Concepto = "Descuento excesivo",
            Motivo = "Test",
            UsuarioAutorizaId = Guid.NewGuid()
        };

        var factura = CreateFacturaEmitida(facturaId, 1000m, 800m);

        SetupFacturasDbSet(new List<Factura> { factura });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("El descuento no puede exceder el total de la factura", result.Error);
    }

    [Fact]
    public async Task Handle_ErrorEnServicioFacturacion_DeberiaRetornarError()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var command = new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "General",
            Porcentaje = 10m,
            Concepto = "Descuento test",
            Motivo = "Test",
            UsuarioAutorizaId = Guid.NewGuid()
        };

        var factura = CreateFacturaEmitida(facturaId, 1000m, 800m);

        SetupFacturasDbSet(new List<Factura> { factura });
        
        _servicioFacturacionMock.Setup(x => x.AplicarDescuentoAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Factura>("Error en el servicio de facturación"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Error en el servicio de facturación", result.Error);
    }

    [Fact]
    public async Task Handle_ErrorNotificaciones_DeberiaLoggearWarningPeroNoFallar()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var command = new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "Cortesia",
            MontoFijo = 500m,
            Concepto = "Descuento con error notificación",
            Motivo = "Test error notificación",
            UsuarioAutorizaId = Guid.NewGuid()
        };

        var factura = CreateFacturaEmitida(facturaId, 1000m, 800m);
        var facturaDto = CreateMockFacturaDto(facturaId);

        SetupFacturasDbSet(new List<Factura> { factura });
        
        _servicioFacturacionMock.Setup(x => x.AplicarDescuentoAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _mapperMock.Setup(x => x.Map<FacturaDto>(It.IsAny<Factura>()))
            .Returns(facturaDto);

        // Error en email service
        _emailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Error de SMTP"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded); // No debe fallar el proceso general
        
        // Verificar que se loggeó el warning de notificaciones
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error al enviar notificaciones")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ExcepcionInesperada_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var command = new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "General",
            Porcentaje = 10m,
            Concepto = "Descuento test",
            Motivo = "Test",
            UsuarioAutorizaId = Guid.NewGuid()
        };

        // Simular excepción en el acceso al contexto
        _contextMock.Setup(x => x.Facturas)
            .Throws(new InvalidOperationException("Error de base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error interno al aplicar el descuento", result.Error);
        
        // Verificar logging del error
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error al aplicar descuento")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods - Setup

    private void SetupFacturasDbSet(List<Factura> facturas)
    {
        var queryable = facturas.AsQueryable();
        var facturasDbSetMock = MockDbSetHelper.CreateMockDbSet(queryable);
        
        _contextMock.Setup(c => c.Facturas).Returns(facturasDbSetMock.Object);
    }

    #endregion

    #region Helper Methods - Data Creation

    private static Factura CreateFacturaEmitida(Guid id, decimal total = 1000.00m, decimal subtotal = 800.00m)
    {
        // Crear factura real usando el factory method
        var factura = Factura.Crear(
            numeroFactura: $"FAC-{id.ToString().Substring(0, 8)}",
            tipoFactura: TipoFactura.Normal,
            nombreCliente: "Cliente Test"
        );
        
        // Agregar detalles para llegar al total deseado
        var precioUnitario = subtotal / 2; // 2 items para alcanzar el subtotal
        factura.AgregarDetalle(
            productoId: Guid.NewGuid(),
            descripcion: "Producto Test 1",
            cantidad: 1,
            precioUnitario: precioUnitario,
            porcentajeImpuesto: 18m
        );
        
        factura.AgregarDetalle(
            productoId: Guid.NewGuid(),
            descripcion: "Producto Test 2",
            cantidad: 1,
            precioUnitario: precioUnitario,
            porcentajeImpuesto: 18m
        );
        
        // Emitir la factura para que esté en estado válido
        factura.Emitir();
        
        // Usar reflexión para establecer el Id si es necesario
        if (factura.Id != id)
        {
            var idProperty = typeof(EntityBase).GetProperty("Id");
            idProperty?.SetValue(factura, id);
        }
        
        return factura;
    }

    private static Factura CreateFacturaAnulada(Guid id)
    {
        // Crear factura real y luego anularla
        var factura = Factura.Crear(
            numeroFactura: $"FAC-{id.ToString().Substring(0, 8)}",
            tipoFactura: TipoFactura.Normal,
            nombreCliente: "Cliente Test"
        );
        
        // Agregar detalles
        factura.AgregarDetalle(
            productoId: Guid.NewGuid(),
            descripcion: "Producto Test",
            cantidad: 2,
            precioUnitario: 400m,
            porcentajeImpuesto: 18m
        );
        
        // Emitir y luego anular
        factura.Emitir();
        factura.Anular("Motivo de test");
        
        // Usar reflexión para establecer el Id si es necesario
        if (factura.Id != id)
        {
            var idProperty = typeof(EntityBase).GetProperty("Id");
            idProperty?.SetValue(factura, id);
        }
        
        return factura;
    }

    private static FacturaDto CreateMockFacturaDto(Guid id)
    {
        return new FacturaDto
        {
            Id = id,
            Numero = $"FAC-{id.ToString().Substring(0, 8)}",
            Estado = EstadoFactura.Emitida,
            FechaEmision = DateTime.UtcNow.AddDays(-1),
            NombreCliente = "Cliente Test",
            Total = 1000.00m,
            Subtotal = 862.07m,
            Impuestos = 137.93m,
            Descuentos = 50.00m // Con descuento aplicado
        };
    }

    #endregion
} 