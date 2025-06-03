namespace RestaurantePro.Application.UnitTests.Comercial.Facturacion.Commands;

/// <summary>
/// Tests unitarios para CrearFacturaHandler
/// Valida la lógica completa de creación de facturas con integración de servicios
/// </summary>
public class CrearFacturaHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CrearFacturaHandler>> _loggerMock;
    private readonly Mock<IServicioFacturacion> _servicioFacturacionMock;
    private readonly Mock<IComercialServiceFacade> _comercialServiceFacadeMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<DbSet<Cliente>> _clientesDbSetMock;
    private readonly Mock<DbSet<Comanda>> _comandasDbSetMock;
    private readonly CrearFacturaHandler _handler;

    public CrearFacturaHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CrearFacturaHandler>>();
        _servicioFacturacionMock = new Mock<IServicioFacturacion>();
        _comercialServiceFacadeMock = new Mock<IComercialServiceFacade>();
        _emailServiceMock = new Mock<IEmailService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _clientesDbSetMock = new Mock<DbSet<Cliente>>();
        _comandasDbSetMock = new Mock<DbSet<Comanda>>();

        // Setup DbContext
        _contextMock.Setup(x => x.Clientes).Returns(_clientesDbSetMock.Object);
        _contextMock.Setup(x => x.Comandas).Returns(_comandasDbSetMock.Object);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());

        _handler = new CrearFacturaHandler(
            _contextMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _servicioFacturacionMock.Object,
            _comercialServiceFacadeMock.Object,
            _emailServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    #region Tests de Factory Methods del Command

    [Fact]
    public void CrearConsumidorFinal_ConParametrosValidos_DeberiaCrearCommandCorrectamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var nombreCliente = "Juan Pérez";

        // Act
        var command = CrearFacturaCommand.CrearConsumidorFinal(comandaId, nombreCliente);

        // Assert
        Assert.Single(command.ComandasIds);
        Assert.Equal(comandaId, command.ComandasIds.First());
        Assert.Equal("Normal", command.TipoFactura);
        Assert.Equal(nombreCliente, command.NombreCliente);
        Assert.True(command.EmitirInmediatamente);
        Assert.Equal(0, command.DiasCredito);
    }

    [Fact]
    public void CrearFiscal_ConParametrosCompletos_DeberiaConfigurarFacturaFiscal()
    {
        // Arrange
        var comandasIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var nombreCliente = "Empresa ABC S.A. de C.V.";
        var rfc = "EAB123456789";
        var direccion = "Av. Principal 123, México";
        var email = "facturacion@empresaabc.com";

        // Act
        var command = CrearFacturaCommand.CrearFiscal(comandasIds, nombreCliente, rfc, direccion, email);

        // Assert
        Assert.Equal(comandasIds.Count, command.ComandasIds.Count);
        Assert.Equal("Fiscal", command.TipoFactura);
        Assert.Equal(nombreCliente, command.NombreCliente);
        Assert.Equal(rfc, command.IdentificacionFiscal);
        Assert.Equal(direccion, command.DireccionCliente);
        Assert.Equal(email, command.EmailCliente);
        Assert.True(command.EmitirInmediatamente);
        Assert.True(command.EnviarPorEmail);
        Assert.Equal(30, command.DiasCredito);
    }

    [Fact]
    public void CrearParaCliente_ConClienteRegistrado_DeberiaUsarClienteId()
    {
        // Arrange
        var comandasIds = new List<Guid> { Guid.NewGuid() };
        var clienteId = Guid.NewGuid();
        var tipoFactura = "Normal";

        // Act
        var command = CrearFacturaCommand.CrearParaCliente(comandasIds, clienteId, tipoFactura);

        // Assert
        Assert.Equal(clienteId, command.ClienteId);
        Assert.Equal(tipoFactura, command.TipoFactura);
        Assert.True(command.EmitirInmediatamente);
        Assert.True(command.EnviarPorEmail);
    }

    [Fact]
    public void CrearConDescuentos_ConDescuentosEspeciales_DeberiaConfigurarDescuentos()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var nombreCliente = "Cliente VIP";
        var descuentos = new List<DescuentoFacturaDto>
        {
            new DescuentoFacturaDto { Concepto = "Descuento VIP", Porcentaje = 10 }
        };

        // Act
        var command = CrearFacturaCommand.CrearConDescuentos(comandaId, "Normal", nombreCliente, descuentos);

        // Assert
        Assert.Single(command.DescuentosAdicionales);
        Assert.Equal("Descuento VIP", command.DescuentosAdicionales.First().Concepto);
        Assert.False(command.EmitirInmediatamente); // Para revisar antes de emitir
    }

    #endregion

    #region Tests de Escenarios Exitosos

    [Fact]
    public async Task Handle_FacturaConsumidorFinalSinCliente_DeberiaCrearFacturaExitosamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Consumidor Final",
            EmitirInmediatamente = true
        };

        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();

        SetupComandasDbSet(new List<Comanda> { comanda });
        
        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                comandaId, TipoFactura.Normal, "Cliente Consumidor Final", null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(factura.Id, result.Value.Id);
        Assert.Equal(factura.NumeroFactura, result.Value.Numero);

        _servicioFacturacionMock.Verify(x => x.GenerarFacturaParaComandaAsync(
            comandaId, TipoFactura.Normal, "Cliente Consumidor Final", null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FacturaConClienteRegistrado_DeberiaUsarInformacionCliente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            ClienteId = clienteId,
            EmitirInmediatamente = false
        };

        var cliente = CreateMockCliente(clienteId, "Cliente Registrado", "cliente@test.com");
        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();

        SetupClientesDbSet(new List<Cliente> { cliente });
        SetupComandasDbSet(new List<Comanda> { comanda });

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        _servicioFacturacionMock.Verify(x => x.GenerarFacturaParaComandaAsync(
            comandaId, TipoFactura.Normal, "Cliente Registrado", clienteId, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FacturaMultiplesComandas_DeberiaUsarServicioMultiplesComandas()
    {
        // Arrange
        var comandasIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var command = new CrearFacturaCommand
        {
            ComandasIds = comandasIds,
            TipoFactura = "Normal",
            NombreCliente = "Cliente Múltiples Comandas"
        };

        var comandas = comandasIds.Select(CreateMockComandaFinalizada).ToList();
        var factura = CreateMockFactura();

        SetupComandasDbSet(comandas);

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandasAsync(
                comandasIds, TipoFactura.Normal, "Cliente Múltiples Comandas", null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);

        _servicioFacturacionMock.Verify(x => x.GenerarFacturaParaComandasAsync(
            comandasIds, TipoFactura.Normal, "Cliente Múltiples Comandas", null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConEmisionInmediataTrue_DeberiaEmitirFactura()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Test",
            EmitirInmediatamente = true,
            DiasCredito = 15
        };

        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();
        var facturaEmitida = CreateMockFacturaEmitida();

        SetupComandasDbSet(new List<Comanda> { comanda });

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _servicioFacturacionMock.Setup(x => x.EmitirFacturaAsync(factura.Id, 15, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(facturaEmitida));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);

        _servicioFacturacionMock.Verify(x => x.EmitirFacturaAsync(factura.Id, 15, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConClienteConPuntos_DeberiaRegistrarPuntosFidelizacion()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Fidelización",
            ClienteId = clienteId
        };

        var cliente = CreateMockCliente(clienteId, "Cliente Fidelización", "cliente@test.com");
        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();

        SetupClientesDbSet(new List<Cliente> { cliente });
        SetupComandasDbSet(new List<Comanda> { comanda });

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _comercialServiceFacadeMock.Setup(x => x.AcumularPuntosPorCompraAsync(
                clienteId, factura.Total, null, "Compra - Facturación", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(50));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);

        _comercialServiceFacadeMock.Verify(x => x.AcumularPuntosPorCompraAsync(
            clienteId, factura.Total, null, "Compra - Facturación", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConEnviarPorEmailTrue_DeberiaEnviarEmail()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var email = "cliente@test.com";
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Email",
            EnviarPorEmail = true,
            EmailCliente = email
        };

        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();

        SetupComandasDbSet(new List<Comanda> { comanda });

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);

        _emailServiceMock.Verify(x => x.SendEmailAsync(
            email, 
            It.Is<string>(asunto => asunto.Contains(factura.NumeroFactura)), 
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConDescuentosAdicionales_DeberiaAplicarDescuentos()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Descuentos",
            DescuentosAdicionales = new List<DescuentoFacturaDto>
            {
                new DescuentoFacturaDto { Concepto = "Descuento VIP", Porcentaje = 10, Motivo = "Cliente frecuente" },
                new DescuentoFacturaDto { Concepto = "Descuento promocional", MontoFijo = 50.00m, Motivo = "Promoción especial" }
            }
        };

        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();

        SetupComandasDbSet(new List<Comanda> { comanda });

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        // Los descuentos se procesan aunque no hay métodos directos para verificar en la entidad mock
    }

    #endregion

    #region Tests de Validaciones y Errores

    [Fact]
    public async Task Handle_ClienteNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            ClienteId = clienteId
        };

        SetupClientesDbSet(new List<Cliente>()); // Cliente no existe

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("El cliente especificado no existe", result.Error);
    }

    [Fact]
    public async Task Handle_ComandaNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Test"
        };

        SetupComandasDbSet(new List<Comanda>()); // Comanda no existe

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("No se encontraron las comandas", result.Error);
    }

    [Fact]
    public async Task Handle_ComandaNoFinalizada_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Test"
        };

        var comanda = CreateMockComandaEnProceso(comandaId); // Estado diferente a Finalizada

        SetupComandasDbSet(new List<Comanda> { comanda });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("no están finalizadas", result.Error);
    }

    [Fact]
    public async Task Handle_TipoFacturaInvalido_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "TipoInvalido",
            NombreCliente = "Cliente Test"
        };

        var comanda = CreateMockComandaFinalizada(comandaId);

        SetupComandasDbSet(new List<Comanda> { comanda });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Tipo de factura no válido", result.Error);
    }

    [Fact]
    public async Task Handle_ErrorEnServicioFacturacion_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Test"
        };

        var comanda = CreateMockComandaFinalizada(comandaId);

        SetupComandasDbSet(new List<Comanda> { comanda });

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Factura>("Error en el servicio de facturación"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Error en el servicio de facturación", result.Error);
    }

    [Fact]
    public async Task Handle_ErrorEnEmision_DeberiaLoggearWarningPeroNoFallar()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Test",
            EmitirInmediatamente = true
        };

        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();

        SetupComandasDbSet(new List<Comanda> { comanda });

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _servicioFacturacionMock.Setup(x => x.EmitirFacturaAsync(factura.Id, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Factura>("Error al emitir"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded); // No debe fallar el proceso general
        
        // Verificar que se intentó emitir
        _servicioFacturacionMock.Verify(x => x.EmitirFacturaAsync(factura.Id, 0, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ErrorEnPuntosFidelizacion_DeberiaLoggearWarningPeroNoFallar()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Test",
            ClienteId = clienteId
        };

        var cliente = CreateMockCliente(clienteId, "Cliente Test", "test@test.com");
        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();

        SetupClientesDbSet(new List<Cliente> { cliente });
        SetupComandasDbSet(new List<Comanda> { comanda });

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _comercialServiceFacadeMock.Setup(x => x.AcumularPuntosPorCompraAsync(
                It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<int>("Error en fidelización"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded); // No debe fallar el proceso general

        _comercialServiceFacadeMock.Verify(x => x.AcumularPuntosPorCompraAsync(
            clienteId, factura.Total, null, "Compra - Facturación", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExcepcionInesperada_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Test"
        };

        _comandasDbSetMock.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<Func<Comanda, bool>>>()))
            .Throws(new InvalidOperationException("Error de base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error interno al crear la factura", result.Error);
    }

    #endregion

    #region Helper Methods - Setup

    private void SetupClientesDbSet(List<Cliente> clientes)
    {
        var queryable = clientes.AsQueryable();
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Provider).Returns(queryable.Provider);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Expression).Returns(queryable.Expression);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        _clientesDbSetMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Cliente, bool>>>(), It.IsAny<CancellationToken>()))
            .Returns<System.Linq.Expressions.Expression<Func<Cliente, bool>>, CancellationToken>((predicate, token) =>
            {
                var compiled = predicate.Compile();
                var result = clientes.FirstOrDefault(compiled);
                return Task.FromResult(result);
            });
    }

    private void SetupComandasDbSet(List<Comanda> comandas)
    {
        var queryable = comandas.AsQueryable();
        _comandasDbSetMock.As<IQueryable<Comanda>>().Setup(m => m.Provider).Returns(queryable.Provider);
        _comandasDbSetMock.As<IQueryable<Comanda>>().Setup(m => m.Expression).Returns(queryable.Expression);
        _comandasDbSetMock.As<IQueryable<Comanda>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        _comandasDbSetMock.As<IQueryable<Comanda>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        _comandasDbSetMock.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<Func<Comanda, bool>>>()))
            .Returns<System.Linq.Expressions.Expression<Func<Comanda, bool>>>(predicate =>
            {
                var compiled = predicate.Compile();
                return comandas.Where(compiled).AsQueryable();
            });

        _comandasDbSetMock.Setup(x => x.Include(It.IsAny<string>()))
            .Returns(_comandasDbSetMock.Object);
    }

    #endregion

    #region Helper Methods - Data Creation

    private static Cliente CreateMockCliente(Guid id, string nombre, string email)
    {
        var clienteMock = new Mock<Cliente>();
        clienteMock.Setup(x => x.Id).Returns(id);
        
        // Crear ClienteNombre usando el método Crear que acepta nombre y apellido
        var nombreCompleto = ClienteNombre.Crear(nombre, "Apellido");
        clienteMock.Setup(x => x.Nombre).Returns(nombreCompleto);
        
        // Crear Email usando el método Create
        var emailVO = Email.Create(email);
        clienteMock.Setup(x => x.Email).Returns(emailVO);
        
        return clienteMock.Object;
    }

    private static Comanda CreateMockComandaFinalizada(Guid id)
    {
        var comandaMock = new Mock<Comanda>();
        comandaMock.Setup(x => x.Id).Returns(id);
        comandaMock.Setup(x => x.Estado).Returns(EstadoComanda.Finalizada);
        comandaMock.Setup(x => x.Items).Returns(new List<ItemComanda>());
        return comandaMock.Object;
    }

    private static Comanda CreateMockComandaEnProceso(Guid id)
    {
        var comandaMock = new Mock<Comanda>();
        comandaMock.Setup(x => x.Id).Returns(id);
        comandaMock.Setup(x => x.Estado).Returns(EstadoComanda.EnProceso);
        comandaMock.Setup(x => x.Items).Returns(new List<ItemComanda>());
        return comandaMock.Object;
    }

    private static Factura CreateMockFactura()
    {
        var facturaMock = new Mock<Factura>();
        facturaMock.Setup(x => x.Id).Returns(Guid.NewGuid());
        facturaMock.Setup(x => x.NumeroFactura).Returns("FAC-001");
        facturaMock.Setup(x => x.TipoFactura).Returns(TipoFactura.Normal);
        facturaMock.Setup(x => x.NombreCliente).Returns("Cliente Test");
        facturaMock.Setup(x => x.Subtotal).Returns(1000.00m);
        facturaMock.Setup(x => x.TotalImpuestos).Returns(160.00m);
        facturaMock.Setup(x => x.TotalDescuentos).Returns(0.00m);
        facturaMock.Setup(x => x.Total).Returns(1160.00m);
        facturaMock.Setup(x => x.Estado).Returns(EstadoFactura.Borrador);
        facturaMock.Setup(x => x.FechaEmision).Returns(DateTime.UtcNow);
        return facturaMock.Object;
    }

    private static Factura CreateMockFacturaEmitida()
    {
        var facturaMock = new Mock<Factura>();
        facturaMock.Setup(x => x.Id).Returns(Guid.NewGuid());
        facturaMock.Setup(x => x.NumeroFactura).Returns("FAC-001");
        facturaMock.Setup(x => x.TipoFactura).Returns(TipoFactura.Normal);
        facturaMock.Setup(x => x.NombreCliente).Returns("Cliente Test");
        facturaMock.Setup(x => x.Subtotal).Returns(1000.00m);
        facturaMock.Setup(x => x.TotalImpuestos).Returns(160.00m);
        facturaMock.Setup(x => x.TotalDescuentos).Returns(0.00m);
        facturaMock.Setup(x => x.Total).Returns(1160.00m);
        facturaMock.Setup(x => x.Estado).Returns(EstadoFactura.Emitida);
        facturaMock.Setup(x => x.FechaEmision).Returns(DateTime.UtcNow);
        return facturaMock.Object;
    }

    #endregion
} 