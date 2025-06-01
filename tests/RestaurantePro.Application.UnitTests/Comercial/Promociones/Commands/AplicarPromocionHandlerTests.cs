namespace RestaurantePro.Application.UnitTests.Comercial.Promociones.Commands;

/// <summary>
/// Tests unitarios para AplicarPromocionHandler
/// Cobertura completa de aplicación de promociones, validaciones de elegibilidad y cálculo de descuentos
/// </summary>
public class AplicarPromocionHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<AplicarPromocionHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<INotificacionService> _mockNotificacionService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ICalculadoraPromocionesService> _mockCalculadoraPromociones;
    private readonly AplicarPromocionHandler _handler;

    public AplicarPromocionHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<AplicarPromocionHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockNotificacionService = new Mock<INotificacionService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCalculadoraPromociones = new Mock<ICalculadoraPromocionesService>();

        _handler = new AplicarPromocionHandler(
            _mockContext.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockCurrentUserService.Object,
            _mockNotificacionService.Object,
            _mockUnitOfWork.Object,
            _mockCalculadoraPromociones.Object);

        ConfigurarMocksBase();
    }

    [Fact]
    public async Task Handle_ConPromocionValidaEnFactura_DeberiaAplicarExitosamente()
    {
        // Arrange
        var promocionId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();

        var command = new AplicarPromocionCommand
        {
            PromocionId = promocionId,
            FacturaId = facturaId,
            ClienteId = clienteId,
            TipoAplicacion = TipoAplicacionPromocion.FacturaCompleta,
            AutorizadoPor = "Supervisor",
            NotasAplicacion = "Aplicación por aniversario"
        };

        var promocion = CrearPromocionValida(promocionId);
        var factura = CrearFactura(facturaId, 1000m);
        var calculoDescuento = new CalculoDescuentoDto
        {
            MontoDescuento = 100m,
            PorcentajeDescuento = 10m
        };

        ConfigurarMocksParaAplicacionExitosa(promocion, factura, null, calculoDescuento);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.PromocionId.Should().Be(promocionId);
        resultado.Value.FacturaId.Should().Be(facturaId);
        resultado.Value.MontoDescuento.Should().Be(100m);
        resultado.Value.AplicacionExitosa.Should().BeTrue();

        // Verificar que se aplicó el descuento a la factura
        VerificarAplicacionDescuentoFactura(facturaId, 100m);

        // Verificar logging
        VerificarLoggingAplicacionExitosa(promocionId);
    }

    [Fact]
    public async Task Handle_ConPromocionValidaEnComanda_DeberiaAplicarExitosamente()
    {
        // Arrange
        var promocionId = Guid.NewGuid();
        var comandaId = Guid.NewGuid();

        var command = new AplicarPromocionCommand
        {
            PromocionId = promocionId,
            ComandaId = comandaId,
            TipoAplicacion = TipoAplicacionPromocion.ComandaCompleta,
            AutorizadoPor = "Mesero"
        };

        var promocion = CrearPromocionValida(promocionId);
        var comanda = CrearComanda(comandaId, 500m);
        var calculoDescuento = new CalculoDescuentoDto
        {
            MontoDescuento = 50m,
            PorcentajeDescuento = 10m
        };

        ConfigurarMocksParaAplicacionExitosa(promocion, null, comanda, calculoDescuento);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.ComandaId.Should().Be(comandaId);
        resultado.Value.MontoDescuento.Should().Be(50m);

        // Verificar que se aplicó el descuento a la comanda
        VerificarAplicacionDescuentoComanda(comandaId, 50m);
    }

    [Fact]
    public async Task Handle_ConPromocionPorCodigo_DeberiaEncontrarYAplicar()
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

        var promocion = CrearPromocionValida(Guid.NewGuid(), codigoPromocion);
        var factura = CrearFactura(facturaId, 800m);
        var calculoDescuento = new CalculoDescuentoDto { MontoDescuento = 80m };

        ConfigurarMocksParaAplicacionExitosa(promocion, factura, null, calculoDescuento);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.CodigoPromocion.Should().Be(codigoPromocion);
        resultado.Value.MontoDescuento.Should().Be(80m);
    }

    [Fact]
    public async Task Handle_ConPromocionInexistente_DeberiaRetornarError()
    {
        // Arrange
        var command = new AplicarPromocionCommand
        {
            PromocionId = Guid.NewGuid(),
            FacturaId = Guid.NewGuid(),
            TipoAplicacion = TipoAplicacionPromocion.FacturaCompleta
        };

        ConfigurarMockPromocionesVacio();

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("promoción especificada no existe");

        VerificarNoSeGuardaronCambios();
    }

    [Fact]
    public async Task Handle_ConFacturaInexistente_DeberiaRetornarError()
    {
        // Arrange
        var promocionId = Guid.NewGuid();
        var command = new AplicarPromocionCommand
        {
            PromocionId = promocionId,
            FacturaId = Guid.NewGuid(),
            TipoAplicacion = TipoAplicacionPromocion.FacturaCompleta
        };

        var promocion = CrearPromocionValida(promocionId);
        ConfigurarMockPromociones(new[] { promocion });
        ConfigurarMockFacturasVacio();

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("factura especificada no existe");

        VerificarNoSeGuardaronCambios();
    }

    [Fact]
    public async Task Handle_ConComandaInexistente_DeberiaRetornarError()
    {
        // Arrange
        var promocionId = Guid.NewGuid();
        var command = new AplicarPromocionCommand
        {
            PromocionId = promocionId,
            ComandaId = Guid.NewGuid(),
            TipoAplicacion = TipoAplicacionPromocion.ComandaCompleta
        };

        var promocion = CrearPromocionValida(promocionId);
        ConfigurarMockPromociones(new[] { promocion });
        ConfigurarMockComandasVacio();

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("comanda especificada no existe");

        VerificarNoSeGuardaronCambios();
    }

    [Fact]
    public async Task Handle_ConPromocionVencida_DeberiaRetornarError()
    {
        // Arrange
        var promocionId = Guid.NewGuid();
        var command = new AplicarPromocionCommand
        {
            PromocionId = promocionId,
            FacturaId = Guid.NewGuid(),
            TipoAplicacion = TipoAplicacionPromocion.FacturaCompleta
        };

        var promocion = CrearPromocionVencida(promocionId);
        var factura = CrearFactura(command.FacturaId!.Value, 500m);

        ConfigurarMockPromociones(new[] { promocion });
        ConfigurarMockFacturas(new[] { factura });

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("promoción no está vigente");

        VerificarNoSeGuardaronCambios();
    }

    [Fact]
    public async Task Handle_ConPromocionInactiva_DeberiaRetornarError()
    {
        // Arrange
        var promocionId = Guid.NewGuid();
        var command = new AplicarPromocionCommand
        {
            PromocionId = promocionId,
            FacturaId = Guid.NewGuid(),
            TipoAplicacion = TipoAplicacionPromocion.FacturaCompleta
        };

        var promocion = CrearPromocionInactiva(promocionId);
        var factura = CrearFactura(command.FacturaId!.Value, 500m);

        ConfigurarMockPromociones(new[] { promocion });
        ConfigurarMockFacturas(new[] { factura });

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("promoción no está activa");

        VerificarNoSeGuardaronCambios();
    }

    [Fact]
    public async Task Handle_ConLimiteUsoAlcanzado_DeberiaRetornarError()
    {
        // Arrange
        var promocionId = Guid.NewGuid();
        var command = new AplicarPromocionCommand
        {
            PromocionId = promocionId,
            FacturaId = Guid.NewGuid(),
            TipoAplicacion = TipoAplicacionPromocion.FacturaCompleta
        };

        var promocion = CrearPromocionConLimiteUso(promocionId, limiteUso: 5);
        var factura = CrearFactura(command.FacturaId!.Value, 500m);

        ConfigurarMockPromociones(new[] { promocion });
        ConfigurarMockFacturas(new[] { factura });
        ConfigurarMockAplicacionesPromocion(5); // Ya se usó 5 veces

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("promoción ha alcanzado su límite de uso");

        VerificarNoSeGuardaronCambios();
    }

    [Fact]
    public async Task Handle_ConLimitePorClienteAlcanzado_DeberiaRetornarError()
    {
        // Arrange
        var promocionId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var command = new AplicarPromocionCommand
        {
            PromocionId = promocionId,
            FacturaId = Guid.NewGuid(),
            ClienteId = clienteId,
            TipoAplicacion = TipoAplicacionPromocion.FacturaCompleta
        };

        var promocion = CrearPromocionConLimitePorCliente(promocionId, limitePorCliente: 2);
        var factura = CrearFactura(command.FacturaId!.Value, 500m);

        ConfigurarMockPromociones(new[] { promocion });
        ConfigurarMockFacturas(new[] { factura });
        ConfigurarMockAplicacionesPromocionPorCliente(promocionId, clienteId, 2); // Ya usó 2 veces

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("cliente ha alcanzado el límite de uso");

        VerificarNoSeGuardaronCambios();
    }

    [Fact]
    public async Task Handle_ConMontoMinimoNoAlcanzado_DeberiaRetornarError()
    {
        // Arrange
        var promocionId = Guid.NewGuid();
        var command = new AplicarPromocionCommand
        {
            PromocionId = promocionId,
            FacturaId = Guid.NewGuid(),
            TipoAplicacion = TipoAplicacionPromocion.FacturaCompleta
        };

        var promocion = CrearPromocionConMontoMinimo(promocionId, montoMinimo: 1000m);
        var factura = CrearFactura(command.FacturaId!.Value, 500m); // Monto menor al mínimo

        ConfigurarMockPromociones(new[] { promocion });
        ConfigurarMockFacturas(new[] { factura });

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("monto mínimo requerido");

        VerificarNoSeGuardaronCambios();
    }

    [Fact]
    public async Task Handle_ConProductosNoElegibles_DeberiaRetornarError()
    {
        // Arrange
        var promocionId = Guid.NewGuid();
        var productoNoElegible = Guid.NewGuid();
        var command = new AplicarPromocionCommand
        {
            PromocionId = promocionId,
            FacturaId = Guid.NewGuid(),
            TipoAplicacion = TipoAplicacionPromocion.ProductosEspecificos,
            ProductosIds = new List<Guid> { productoNoElegible }
        };

        var promocion = CrearPromocionConProductosElegibles(promocionId, new[] { Guid.NewGuid() }); // Producto diferente
        var factura = CrearFactura(command.FacturaId!.Value, 500m);

        ConfigurarMockPromociones(new[] { promocion });
        ConfigurarMockFacturas(new[] { factura });

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("productos no son elegibles");

        VerificarNoSeGuardaronCambios();
    }

    [Fact]
    public async Task Handle_ConErrorEnCalculoDescuento_DeberiaRetornarError()
    {
        // Arrange
        var promocionId = Guid.NewGuid();
        var command = new AplicarPromocionCommand
        {
            PromocionId = promocionId,
            FacturaId = Guid.NewGuid(),
            TipoAplicacion = TipoAplicacionPromocion.FacturaCompleta
        };

        var promocion = CrearPromocionValida(promocionId);
        var factura = CrearFactura(command.FacturaId!.Value, 500m);

        ConfigurarMockPromociones(new[] { promocion });
        ConfigurarMockFacturas(new[] { factura });

        _mockCalculadoraPromociones.Setup(c => c.CalcularDescuentoAsync(
                It.IsAny<CalcularDescuentoRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Error en cálculo"));

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("Error al calcular el descuento");

        VerificarNoSeGuardaronCambios();
    }

    [Fact]
    public async Task Handle_ConAplicacionExitosa_DeberiaActualizarLimitesUso()
    {
        // Arrange
        var promocionId = Guid.NewGuid();
        var command = new AplicarPromocionCommand
        {
            PromocionId = promocionId,
            FacturaId = Guid.NewGuid(),
            TipoAplicacion = TipoAplicacionPromocion.FacturaCompleta
        };

        var promocion = CrearPromocionConLimiteUso(promocionId, limiteUso: 10, usosActuales: 5);
        var factura = CrearFactura(command.FacturaId!.Value, 500m);
        var calculoDescuento = new CalculoDescuentoDto { MontoDescuento = 50m };

        ConfigurarMocksParaAplicacionExitosa(promocion, factura, null, calculoDescuento);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();

        // Verificar que se actualizaron los usos
        promocion.UsosActuales.Should().Be(6);
        promocion.FechaUltimoUso.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ConLimiteUsoAlcanzadoTrasAplicacion_DeberiaDesactivarPromocion()
    {
        // Arrange
        var promocionId = Guid.NewGuid();
        var command = new AplicarPromocionCommand
        {
            PromocionId = promocionId,
            FacturaId = Guid.NewGuid(),
            TipoAplicacion = TipoAplicacionPromocion.FacturaCompleta
        };

        var promocion = CrearPromocionConLimiteUso(promocionId, limiteUso: 10, usosActuales: 9); // Un uso más y se alcanza el límite
        var factura = CrearFactura(command.FacturaId!.Value, 500m);
        var calculoDescuento = new CalculoDescuentoDto { MontoDescuento = 50m };

        ConfigurarMocksParaAplicacionExitosa(promocion, factura, null, calculoDescuento);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();

        // Verificar que se desactivó la promoción
        promocion.Activa.Should().BeFalse();
        promocion.MotivoDesactivacion.Should().Be("Límite de uso alcanzado");
    }

    [Fact]
    public async Task Handle_ConErrorEnTransaccion_DeberiaRevertirCambios()
    {
        // Arrange
        var command = new AplicarPromocionCommand
        {
            PromocionId = Guid.NewGuid(),
            FacturaId = Guid.NewGuid(),
            TipoAplicacion = TipoAplicacionPromocion.FacturaCompleta
        };

        var promocion = CrearPromocionValida(command.PromocionId);
        var factura = CrearFactura(command.FacturaId!.Value, 500m);

        ConfigurarMockPromociones(new[] { promocion });
        ConfigurarMockFacturas(new[] { factura });

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Error en base de datos"));

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("Error interno al aplicar la promoción");

        // Verificar logging de error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al aplicar promoción")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #region Métodos de apoyo

    private void ConfigurarMocksBase()
    {
        _mockCurrentUserService.Setup(u => u.UserId)
            .Returns(Guid.NewGuid().ToString());

        var mockTransaction = new Mock<IDbContextTransaction>();
        _mockUnitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);
    }

    private void ConfigurarMocksParaAplicacionExitosa(Promocion promocion, Factura? factura, Comanda? comanda, CalculoDescuentoDto calculoDescuento)
    {
        ConfigurarMockPromociones(new[] { promocion });
        
        if (factura != null)
            ConfigurarMockFacturas(new[] { factura });
        
        if (comanda != null)
            ConfigurarMockComandas(new[] { comanda });

        ConfigurarMockAplicacionesPromocionVacio();

        _mockCalculadoraPromociones.Setup(c => c.CalcularDescuentoAsync(
                It.IsAny<CalcularDescuentoRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(calculoDescuento);

        var mockDescuentosFacturaSet = new Mock<DbSet<DescuentoFactura>>();
        var mockDescuentosComandaSet = new Mock<DbSet<DescuentoComanda>>();
        var mockAplicacionesSet = new Mock<DbSet<AplicacionPromocion>>();
        var mockAuditoriaSet = new Mock<DbSet<RegistroAuditoria>>();

        _mockContext.Setup(c => c.DescuentosFactura).Returns(mockDescuentosFacturaSet.Object);
        _mockContext.Setup(c => c.DescuentosComanda).Returns(mockDescuentosComandaSet.Object);
        _mockContext.Setup(c => c.AplicacionesPromocion).Returns(mockAplicacionesSet.Object);
        _mockContext.Setup(c => c.RegistrosAuditoria).Returns(mockAuditoriaSet.Object);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    private void ConfigurarMockPromociones(IEnumerable<Promocion> promociones)
    {
        var mockSet = MockDbSetHelper.CreateMockDbSet(promociones.AsQueryable());
        _mockContext.Setup(c => c.Promociones).Returns(mockSet.Object);
    }

    private void ConfigurarMockPromocionesVacio()
    {
        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<Promocion>().AsQueryable());
        _mockContext.Setup(c => c.Promociones).Returns(mockSet.Object);
    }

    private void ConfigurarMockFacturas(IEnumerable<Factura> facturas)
    {
        var mockSet = MockDbSetHelper.CreateMockDbSet(facturas.AsQueryable());
        _mockContext.Setup(c => c.Facturas).Returns(mockSet.Object);
    }

    private void ConfigurarMockFacturasVacio()
    {
        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<Factura>().AsQueryable());
        _mockContext.Setup(c => c.Facturas).Returns(mockSet.Object);
    }

    private void ConfigurarMockComandas(IEnumerable<Comanda> comandas)
    {
        var mockSet = MockDbSetHelper.CreateMockDbSet(comandas.AsQueryable());
        _mockContext.Setup(c => c.Comandas).Returns(mockSet.Object);
    }

    private void ConfigurarMockComandasVacio()
    {
        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<Comanda>().AsQueryable());
        _mockContext.Setup(c => c.Comandas).Returns(mockSet.Object);
    }

    private void ConfigurarMockAplicacionesPromocion(int cantidadAplicaciones)
    {
        var aplicaciones = Enumerable.Range(1, cantidadAplicaciones)
            .Select(_ => new AplicacionPromocion())
            .AsQueryable();
        
        var mockSet = MockDbSetHelper.CreateMockDbSet(aplicaciones);
        _mockContext.Setup(c => c.AplicacionesPromocion).Returns(mockSet.Object);
    }

    private void ConfigurarMockAplicacionesPromocionPorCliente(Guid promocionId, Guid clienteId, int cantidadAplicaciones)
    {
        var aplicaciones = Enumerable.Range(1, cantidadAplicaciones)
            .Select(_ => new AplicacionPromocion { PromocionId = promocionId, ClienteId = clienteId })
            .AsQueryable();
        
        var mockSet = MockDbSetHelper.CreateMockDbSet(aplicaciones);
        _mockContext.Setup(c => c.AplicacionesPromocion).Returns(mockSet.Object);
    }

    private void ConfigurarMockAplicacionesPromocionVacio()
    {
        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<AplicacionPromocion>().AsQueryable());
        _mockContext.Setup(c => c.AplicacionesPromocion).Returns(mockSet.Object);
    }

    private Promocion CrearPromocionValida(Guid id, string? codigo = null)
    {
        return new Promocion
        {
            Id = id,
            Codigo = codigo ?? "PROMO10",
            Nombre = "Descuento del 10%",
            TipoDescuento = "Porcentaje",
            Activa = true,
            FechaInicio = DateTime.UtcNow.AddDays(-1),
            FechaFin = DateTime.UtcNow.AddDays(30),
            Condiciones = new List<CondicionPromocion>(),
            ProductosElegibles = new List<PromocionProducto>()
        };
    }

    private Promocion CrearPromocionVencida(Guid id)
    {
        return new Promocion
        {
            Id = id,
            Codigo = "VENCIDA",
            Activa = true,
            FechaInicio = DateTime.UtcNow.AddDays(-30),
            FechaFin = DateTime.UtcNow.AddDays(-1), // Vencida
            Condiciones = new List<CondicionPromocion>(),
            ProductosElegibles = new List<PromocionProducto>()
        };
    }

    private Promocion CrearPromocionInactiva(Guid id)
    {
        return new Promocion
        {
            Id = id,
            Codigo = "INACTIVA",
            Activa = false, // Inactiva
            FechaInicio = DateTime.UtcNow.AddDays(-1),
            FechaFin = DateTime.UtcNow.AddDays(30),
            Condiciones = new List<CondicionPromocion>(),
            ProductosElegibles = new List<PromocionProducto>()
        };
    }

    private Promocion CrearPromocionConLimiteUso(Guid id, int limiteUso, int usosActuales = 0)
    {
        return new Promocion
        {
            Id = id,
            Codigo = "LIMITE",
            Activa = true,
            FechaInicio = DateTime.UtcNow.AddDays(-1),
            FechaFin = DateTime.UtcNow.AddDays(30),
            LimiteUso = limiteUso,
            UsosActuales = usosActuales,
            Condiciones = new List<CondicionPromocion>(),
            ProductosElegibles = new List<PromocionProducto>()
        };
    }

    private Promocion CrearPromocionConLimitePorCliente(Guid id, int limitePorCliente)
    {
        return new Promocion
        {
            Id = id,
            Codigo = "LIMITE_CLIENTE",
            Activa = true,
            FechaInicio = DateTime.UtcNow.AddDays(-1),
            FechaFin = DateTime.UtcNow.AddDays(30),
            LimitePorCliente = limitePorCliente,
            Condiciones = new List<CondicionPromocion>(),
            ProductosElegibles = new List<PromocionProducto>()
        };
    }

    private Promocion CrearPromocionConMontoMinimo(Guid id, decimal montoMinimo)
    {
        return new Promocion
        {
            Id = id,
            Codigo = "MONTO_MIN",
            Activa = true,
            FechaInicio = DateTime.UtcNow.AddDays(-1),
            FechaFin = DateTime.UtcNow.AddDays(30),
            MontoMinimo = montoMinimo,
            Condiciones = new List<CondicionPromocion>(),
            ProductosElegibles = new List<PromocionProducto>()
        };
    }

    private Promocion CrearPromocionConProductosElegibles(Guid id, IEnumerable<Guid> productosElegibles)
    {
        return new Promocion
        {
            Id = id,
            Codigo = "PRODUCTOS",
            Activa = true,
            FechaInicio = DateTime.UtcNow.AddDays(-1),
            FechaFin = DateTime.UtcNow.AddDays(30),
            Condiciones = new List<CondicionPromocion>(),
            ProductosElegibles = productosElegibles.Select(p => new PromocionProducto { ProductoId = p }).ToList()
        };
    }

    private Factura CrearFactura(Guid id, decimal total)
    {
        return new Factura
        {
            Id = id,
            Total = total,
            Items = new List<ItemFactura>(),
            Descuentos = new List<DescuentoFactura>()
        };
    }

    private Comanda CrearComanda(Guid id, decimal total)
    {
        return new Comanda
        {
            Id = id,
            Total = total,
            Items = new List<ItemComanda>(),
            Descuentos = new List<DescuentoComanda>()
        };
    }

    private void VerificarAplicacionDescuentoFactura(Guid facturaId, decimal montoEsperado)
    {
        _mockContext.Verify(
            c => c.DescuentosFactura.Add(It.Is<DescuentoFactura>(d => 
                d.FacturaId == facturaId && d.Monto == montoEsperado)),
            Times.Once);
    }

    private void VerificarAplicacionDescuentoComanda(Guid comandaId, decimal montoEsperado)
    {
        _mockContext.Verify(
            c => c.DescuentosComanda.Add(It.Is<DescuentoComanda>(d => 
                d.ComandaId == comandaId && d.Monto == montoEsperado)),
            Times.Once);
    }

    private void VerificarNoSeGuardaronCambios()
    {
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private void VerificarLoggingAplicacionExitosa(Guid promocionId)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Promoción aplicada exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
} 