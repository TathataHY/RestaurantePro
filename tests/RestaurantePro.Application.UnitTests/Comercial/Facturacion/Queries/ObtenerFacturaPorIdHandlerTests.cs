namespace RestaurantePro.Application.UnitTests.Comercial.Facturacion.Queries;

/// <summary>
/// Tests unitarios para ObtenerFacturaPorIdHandler
/// Cobertura completa de consulta de facturas, validaciones de seguridad y manejo de errores
/// </summary>
public class ObtenerFacturaPorIdHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ObtenerFacturaPorIdHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<DbSet<Factura>> _mockFacturasDbSet;
    private readonly ObtenerFacturaPorIdHandler _handler;
    private readonly List<Factura> _facturasEjemplo;

    public ObtenerFacturaPorIdHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ObtenerFacturaPorIdHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockFacturasDbSet = new Mock<DbSet<Factura>>();
        
        _handler = new ObtenerFacturaPorIdHandler(
            _mockContext.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockCurrentUserService.Object);

        _facturasEjemplo = CrearFacturasEjemplo();
        ConfigurarMockDbSet();
    }

    [Fact]
    public async Task Handle_ConFacturaExistente_DeberiaRetornarFacturaCorrectamente()
    {
        // Arrange
        var facturaId = _facturasEjemplo[0].Id;
        var query = new ObtenerFacturaPorIdQuery { FacturaId = facturaId };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.Id.Should().Be(facturaId);
        resultado.Value.Numero.Should().Be("FAC-2024-001");
        resultado.Value.NombreCliente.Should().Be("Juan Pérez");
        resultado.Value.Total.Should().Be(1500.00m);
        resultado.Value.Estado.Should().Be(EstadoFactura.Pagada);

        // Verificar logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Consultando factura por ID")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("consultada exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConFacturaInexistente_DeberiaRetornarError()
    {
        // Arrange
        var facturaIdInexistente = Guid.NewGuid();
        var query = new ObtenerFacturaPorIdQuery { FacturaId = facturaIdInexistente };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Be("La factura especificada no existe.");

        // Verificar logging de advertencia
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("no encontrada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConFacturaPendiente_DeberiaRetornarFacturaConEstadoCorrect()
    {
        // Arrange
        var facturaId = _facturasEjemplo[1].Id; // Factura pendiente
        var query = new ObtenerFacturaPorIdQuery { FacturaId = facturaId };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Estado.Should().Be(EstadoFactura.Emitida);
        resultado.Value.MontoPagado.Should().Be(0);
        resultado.Value.FechaPago.Should().BeNull();
        resultado.Value.Total.Should().Be(850.00m);
    }

    [Fact]
    public async Task Handle_ConFacturaAnulada_DeberiaRetornarFacturaConEstadoAnulada()
    {
        // Arrange
        var facturaId = _facturasEjemplo[2].Id; // Factura anulada
        var query = new ObtenerFacturaPorIdQuery { FacturaId = facturaId };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Estado.Should().Be(EstadoFactura.Anulada);
        resultado.Value.Total.Should().Be(0); // Las facturas anuladas tienen total 0
    }

    [Fact]
    public async Task Handle_ConFacturaVencida_DeberiaRetornarFacturaConEstadoVencida()
    {
        // Arrange
        var facturaId = _facturasEjemplo[3].Id; // Factura vencida
        var query = new ObtenerFacturaPorIdQuery { FacturaId = facturaId };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Estado.Should().Be(EstadoFactura.Vencida);
        resultado.Value.FechaVencimiento.Should().BeBefore(DateTime.Now);
        resultado.Value.MontoPagado.Should().Be(0);
    }

    [Theory]
    [InlineData(TipoFactura.Venta)]
    [InlineData(TipoFactura.Devolucion)]
    [InlineData(TipoFactura.NotaCredito)]
    [InlineData(TipoFactura.NotaDebito)]
    public async Task Handle_ConDiferentesTiposFactura_DeberiaRetornarTipoCorrectamente(TipoFactura tipoFactura)
    {
        // Arrange
        var factura = CrearFactura(Guid.NewGuid(), "FAC-TEST-001", tipoFactura, EstadoFactura.Pagada, 1000m);
        var facturas = new List<Factura> { factura };
        
        ConfigurarMockDbSetConFacturas(facturas);
        
        var query = new ObtenerFacturaPorIdQuery { FacturaId = factura.Id };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Tipo.Should().Be(tipoFactura);
    }

    [Fact]
    public async Task Handle_ConFacturaConDescuentos_DeberiaCalcularTotalesCorrectamente()
    {
        // Arrange
        var factura = CrearFactura(
            Guid.NewGuid(), 
            "FAC-DESC-001", 
            TipoFactura.Venta, 
            EstadoFactura.Pagada, 
            1000m,
            subtotal: 1000m,
            descuentos: 100m,
            impuestos: 144m); // 16% sobre (1000-100)

        var facturas = new List<Factura> { factura };
        ConfigurarMockDbSetConFacturas(facturas);
        
        var query = new ObtenerFacturaPorIdQuery { FacturaId = factura.Id };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Subtotal.Should().Be(1000m);
        resultado.Value.Descuentos.Should().Be(100m);
        resultado.Value.Impuestos.Should().Be(144m);
        resultado.Value.Total.Should().Be(1044m); // 1000 - 100 + 144
    }

    [Fact]
    public async Task Handle_ConFacturaConImpuestos_DeberiaCalcularImpuestosCorrectamente()
    {
        // Arrange
        var factura = CrearFactura(
            Guid.NewGuid(), 
            "FAC-IMP-001", 
            TipoFactura.Venta, 
            EstadoFactura.Pagada, 
            1160m,
            subtotal: 1000m,
            descuentos: 0m,
            impuestos: 160m); // 16% IVA

        var facturas = new List<Factura> { factura };
        ConfigurarMockDbSetConFacturas(facturas);
        
        var query = new ObtenerFacturaPorIdQuery { FacturaId = factura.Id };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Subtotal.Should().Be(1000m);
        resultado.Value.Impuestos.Should().Be(160m);
        resultado.Value.Total.Should().Be(1160m);
    }

    [Fact]
    public async Task Handle_ConExcepcionEnBaseDatos_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerFacturaPorIdQuery { FacturaId = Guid.NewGuid() };

        _mockFacturasDbSet.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Factura, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Be("Error interno al consultar la factura.");

        // Verificar logging de error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al consultar factura")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConCancelationToken_DeberiaRespetarCancelacion()
    {
        // Arrange
        var query = new ObtenerFacturaPorIdQuery { FacturaId = Guid.NewGuid() };
        var cancellationToken = new CancellationToken(canceled: true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(query, cancellationToken));
    }

    [Fact]
    public async Task Handle_ConFacturaConFechasCompletas_DeberiaRetornarFechasCorrectamente()
    {
        // Arrange
        var fechaEmision = DateTime.Now.AddDays(-5);
        var fechaVencimiento = DateTime.Now.AddDays(25);
        var fechaPago = DateTime.Now.AddDays(-2);
        var fechaCreacion = DateTime.Now.AddDays(-10);

        var factura = CrearFacturaConFechas(
            Guid.NewGuid(),
            "FAC-FECHAS-001",
            fechaEmision,
            fechaVencimiento,
            fechaPago,
            fechaCreacion);

        var facturas = new List<Factura> { factura };
        ConfigurarMockDbSetConFacturas(facturas);
        
        var query = new ObtenerFacturaPorIdQuery { FacturaId = factura.Id };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.FechaEmision.Should().BeCloseTo(fechaEmision, TimeSpan.FromSeconds(1));
        resultado.Value.FechaVencimiento.Should().BeCloseTo(fechaVencimiento, TimeSpan.FromSeconds(1));
        resultado.Value.FechaPago.Should().BeCloseTo(fechaPago, TimeSpan.FromSeconds(1));
        resultado.Value.FechaCreacion.Should().BeCloseTo(fechaCreacion, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_ConFacturaConClienteCompleto_DeberiaRetornarDatosCliente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var factura = CrearFacturaConCliente(
            Guid.NewGuid(),
            "FAC-CLIENTE-001",
            clienteId,
            "María García López");

        var facturas = new List<Factura> { factura };
        ConfigurarMockDbSetConFacturas(facturas);
        
        var query = new ObtenerFacturaPorIdQuery { FacturaId = factura.Id };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.ClienteId.Should().Be(clienteId);
        resultado.Value.NombreCliente.Should().Be("María García López");
    }

    [Theory]
    [InlineData(0, 1000, 160, 1160)] // Sin descuentos
    [InlineData(100, 1000, 144, 1044)] // Con descuentos
    [InlineData(200, 1000, 128, 928)] // Descuento mayor
    [InlineData(1000, 1000, 0, 0)] // Descuento total
    public async Task Handle_ConDiferentesCalculosFinancieros_DeberiaCalcularCorrectamente(
        decimal descuentos, decimal subtotal, decimal impuestosEsperados, decimal totalEsperado)
    {
        // Arrange
        var factura = CrearFactura(
            Guid.NewGuid(),
            "FAC-CALC-001",
            TipoFactura.Venta,
            EstadoFactura.Pagada,
            totalEsperado,
            subtotal: subtotal,
            descuentos: descuentos,
            impuestos: impuestosEsperados);

        var facturas = new List<Factura> { factura };
        ConfigurarMockDbSetConFacturas(facturas);
        
        var query = new ObtenerFacturaPorIdQuery { FacturaId = factura.Id };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Subtotal.Should().Be(subtotal);
        resultado.Value.Descuentos.Should().Be(descuentos);
        resultado.Value.Impuestos.Should().Be(impuestosEsperados);
        resultado.Value.Total.Should().Be(totalEsperado);
    }

    [Fact]
    public async Task Handle_ConFacturaConPagoParcial_DeberiaRetornarMontoPagadoCorrectamente()
    {
        // Arrange
        var factura = CrearFactura(
            Guid.NewGuid(),
            "FAC-PARCIAL-001",
            TipoFactura.Venta,
            EstadoFactura.PagadaParcialmente,
            1000m,
            montoPagado: 600m);

        var facturas = new List<Factura> { factura };
        ConfigurarMockDbSetConFacturas(facturas);
        
        var query = new ObtenerFacturaPorIdQuery { FacturaId = factura.Id };

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Total.Should().Be(1000m);
        resultado.Value.MontoPagado.Should().Be(600m);
        resultado.Value.Estado.Should().Be(EstadoFactura.PagadaParcialmente);
    }

    #region Métodos de Apoyo

    private void ConfigurarMockDbSet()
    {
        var queryableFacturas = _facturasEjemplo.AsQueryable();
        
        _mockFacturasDbSet.As<IQueryable<Factura>>().Setup(m => m.Provider).Returns(queryableFacturas.Provider);
        _mockFacturasDbSet.As<IQueryable<Factura>>().Setup(m => m.Expression).Returns(queryableFacturas.Expression);
        _mockFacturasDbSet.As<IQueryable<Factura>>().Setup(m => m.ElementType).Returns(queryableFacturas.ElementType);
        _mockFacturasDbSet.As<IQueryable<Factura>>().Setup(m => m.GetEnumerator()).Returns(queryableFacturas.GetEnumerator());

        _mockFacturasDbSet.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Factura, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Expression<Func<Factura, bool>>, CancellationToken>((predicate, ct) =>
            {
                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException();
                
                var compiledPredicate = predicate.Compile();
                var result = _facturasEjemplo.FirstOrDefault(compiledPredicate);
                return Task.FromResult(result);
            });

        _mockContext.Setup(c => c.Facturas).Returns(_mockFacturasDbSet.Object);
    }

    private void ConfigurarMockDbSetConFacturas(List<Factura> facturas)
    {
        var queryableFacturas = facturas.AsQueryable();
        
        _mockFacturasDbSet.As<IQueryable<Factura>>().Setup(m => m.Provider).Returns(queryableFacturas.Provider);
        _mockFacturasDbSet.As<IQueryable<Factura>>().Setup(m => m.Expression).Returns(queryableFacturas.Expression);
        _mockFacturasDbSet.As<IQueryable<Factura>>().Setup(m => m.ElementType).Returns(queryableFacturas.ElementType);
        _mockFacturasDbSet.As<IQueryable<Factura>>().Setup(m => m.GetEnumerator()).Returns(queryableFacturas.GetEnumerator());

        _mockFacturasDbSet.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Factura, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Expression<Func<Factura, bool>>, CancellationToken>((predicate, ct) =>
            {
                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException();
                
                var compiledPredicate = predicate.Compile();
                var result = facturas.FirstOrDefault(compiledPredicate);
                return Task.FromResult(result);
            });

        _mockContext.Setup(c => c.Facturas).Returns(_mockFacturasDbSet.Object);
    }

    private List<Factura> CrearFacturasEjemplo()
    {
        return new List<Factura>
        {
            CrearFactura(Guid.NewGuid(), "FAC-2024-001", TipoFactura.Venta, EstadoFactura.Pagada, 1500.00m, montoPagado: 1500.00m),
            CrearFactura(Guid.NewGuid(), "FAC-2024-002", TipoFactura.Venta, EstadoFactura.Emitida, 850.00m),
            CrearFactura(Guid.NewGuid(), "FAC-2024-003", TipoFactura.Venta, EstadoFactura.Anulada, 0m),
            CrearFactura(Guid.NewGuid(), "FAC-2024-004", TipoFactura.Venta, EstadoFactura.Vencida, 1200.00m)
        };
    }

    private Factura CrearFactura(
        Guid id, 
        string numero, 
        TipoFactura tipo, 
        EstadoFactura estado, 
        decimal total,
        decimal subtotal = 0,
        decimal descuentos = 0,
        decimal impuestos = 0,
        decimal montoPagado = 0)
    {
        // Usar reflection para crear la factura con propiedades privadas
        var factura = (Factura)Activator.CreateInstance(typeof(Factura), true)!;
        
        typeof(Factura).GetProperty("Id")?.SetValue(factura, id);
        typeof(Factura).GetProperty("NumeroFactura")?.SetValue(factura, numero);
        typeof(Factura).GetProperty("TipoFactura")?.SetValue(factura, tipo);
        typeof(Factura).GetProperty("Estado")?.SetValue(factura, estado);
        typeof(Factura).GetProperty("Total")?.SetValue(factura, total);
        typeof(Factura).GetProperty("Subtotal")?.SetValue(factura, subtotal > 0 ? subtotal : total);
        typeof(Factura).GetProperty("TotalDescuentos")?.SetValue(factura, descuentos);
        typeof(Factura).GetProperty("TotalImpuestos")?.SetValue(factura, impuestos);
        typeof(Factura).GetProperty("TotalPagado")?.SetValue(factura, montoPagado);
        typeof(Factura).GetProperty("ClienteId")?.SetValue(factura, Guid.NewGuid());
        typeof(Factura).GetProperty("NombreCliente")?.SetValue(factura, "Juan Pérez");
        typeof(Factura).GetProperty("FechaEmision")?.SetValue(factura, DateTime.Now.AddDays(-1));
        typeof(Factura).GetProperty("FechaVencimiento")?.SetValue(factura, DateTime.Now.AddDays(30));
        typeof(Factura).GetProperty("FechaCreacion")?.SetValue(factura, DateTime.Now.AddDays(-2));
        
        if (estado == EstadoFactura.Pagada && montoPagado > 0)
        {
            typeof(Factura).GetProperty("FechaPago")?.SetValue(factura, DateTime.Now.AddHours(-2));
        }
        
        return factura;
    }

    private Factura CrearFacturaConFechas(
        Guid id,
        string numero,
        DateTime fechaEmision,
        DateTime fechaVencimiento,
        DateTime? fechaPago,
        DateTime fechaCreacion)
    {
        var factura = CrearFactura(id, numero, TipoFactura.Venta, EstadoFactura.Pagada, 1000m);
        
        typeof(Factura).GetProperty("FechaEmision")?.SetValue(factura, fechaEmision);
        typeof(Factura).GetProperty("FechaVencimiento")?.SetValue(factura, fechaVencimiento);
        typeof(Factura).GetProperty("FechaPago")?.SetValue(factura, fechaPago);
        typeof(Factura).GetProperty("FechaCreacion")?.SetValue(factura, fechaCreacion);
        
        return factura;
    }

    private Factura CrearFacturaConCliente(Guid id, string numero, Guid clienteId, string nombreCliente)
    {
        var factura = CrearFactura(id, numero, TipoFactura.Venta, EstadoFactura.Pagada, 1000m);
        
        typeof(Factura).GetProperty("ClienteId")?.SetValue(factura, clienteId);
        typeof(Factura).GetProperty("NombreCliente")?.SetValue(factura, nombreCliente);
        
        return factura;
    }

    #endregion
} 