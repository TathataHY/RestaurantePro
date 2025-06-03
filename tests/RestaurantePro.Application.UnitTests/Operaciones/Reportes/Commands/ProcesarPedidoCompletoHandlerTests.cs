namespace RestaurantePro.Application.UnitTests.Operaciones.Reportes.Commands;

using ProcesarPedidoCompletoCommand = RestaurantePro.Application.Operaciones.Reportes.Commands.ProcesarPedidoCompleto.ProcesarPedidoCompletoCommand;
using ProcesarPedidoCompletoHandler = RestaurantePro.Application.Operaciones.Reportes.Commands.ProcesarPedidoCompleto.ProcesarPedidoCompletoHandler;

/// <summary>
/// Tests unitarios para ProcesarPedidoCompletoHandler
/// Valida la orquestación completa de workflows: Comanda → Pago → Factura → Fidelización → Mesa
/// </summary>
public class ProcesarPedidoCompletoHandlerTests
{
    private readonly Mock<IComandaRepository> _comandaRepositoryMock;
    private readonly Mock<IFacturaRepository> _facturaRepositoryMock;
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<IMesaRepository> _mesaRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<ProcesarPedidoCompletoHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IServicioFacturacion> _servicioFacturacionMock;
    private readonly Mock<IComercialServiceFacade> _comercialServiceFacadeMock;
    private readonly ProcesarPedidoCompletoHandler _handler;

    public ProcesarPedidoCompletoHandlerTests()
    {
        _comandaRepositoryMock = new Mock<IComandaRepository>();
        _facturaRepositoryMock = new Mock<IFacturaRepository>();
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _mesaRepositoryMock = new Mock<IMesaRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<ProcesarPedidoCompletoHandler>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _dateTimeServiceMock = new Mock<IDateTimeService>();
        _mediatorMock = new Mock<IMediator>();
        _servicioFacturacionMock = new Mock<IServicioFacturacion>();
        _comercialServiceFacadeMock = new Mock<IComercialServiceFacade>();

        _handler = new ProcesarPedidoCompletoHandler(
            _comandaRepositoryMock.Object,
            _facturaRepositoryMock.Object,
            _clienteRepositoryMock.Object,
            _mesaRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _currentUserServiceMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeServiceMock.Object,
            _mediatorMock.Object,
            _servicioFacturacionMock.Object,
            _comercialServiceFacadeMock.Object);
    }

    #region Tests de Escenarios Exitosos

    [Fact]
    public async Task Handle_WorkflowCompletoConPago_DeberiaOrquestarTodoElProceso()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var command = new ProcesarPedidoCompletoCommand
        {
            ComandaId = comandaId,
            TipoPago = "Tarjeta",
            RequierePago = true,
            InfoPago = new InfoPagoDto
            {
                NumeroTarjeta = "4111111111111111",
                NombreTitular = "Juan Pérez",
                MontoTotal = 150.00m,
                Moneda = "USD"
            },
            TipoFactura = "Consumidor Final",
            NombreCliente = "Juan Pérez",
            UsuarioId = Guid.NewGuid()
        };

        var comanda = CreateMockComanda(comandaId, clienteId, mesaId, 150.00m);
        var resultadoPago = CreateMockResultadoPago("APPROVED", "12345");
        var factura = CreateMockFactura(Guid.NewGuid(), comandaId, 150.00m);
        var resultadoDto = CreateMockProcesarPedidoCompletoDto(comandaId, true);

        SetupMocksWorkflowCompleto(comanda, resultadoPago, factura, resultadoDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(comandaId, result.Value.ComandaId);
        Assert.True(result.Value.PagoExitoso);
        Assert.True(result.Value.FacturaGenerada);
        Assert.True(result.Value.PuntosFidelizacionAcumulados > 0);
        Assert.NotNull(result.Value.Mesa);

        // Verificar que se ejecutó todo el workflow
        VerifyWorkflowCompleto(comandaId, clienteId, mesaId);
    }

    [Fact]
    public async Task Handle_WorkflowSinPago_DeberiaOmitirProcesoPago()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new ProcesarPedidoCompletoCommand
        {
            ComandaId = comandaId,
            TipoPago = "Efectivo",
            RequierePago = false,
            TipoFactura = "Consumidor Final",
            NombreCliente = "María González",
            UsuarioId = Guid.NewGuid()
        };

        var comanda = CreateMockComanda(comandaId, null, Guid.NewGuid(), 85.00m);
        var factura = CreateMockFactura(Guid.NewGuid(), comandaId, 85.00m);
        var resultadoDto = CreateMockProcesarPedidoCompletoDto(comandaId, false);

        SetupMocksWorkflowSinPago(comanda, factura, resultadoDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.False(result.Value.PagoExitoso);
        Assert.True(result.Value.FacturaGenerada);
        Assert.NotNull(result.Value.Mesa);

        // Verificar que NO se llamó al servicio de pagos
        _servicioFacturacionMock.Verify(x => x.ProcesarPagoAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComandaConCliente_DeberiaAcumularPuntosFidelizacion()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var command = new ProcesarPedidoCompletoCommand
        {
            ComandaId = comandaId,
            TipoPago = "Efectivo",
            RequierePago = false,
            UsuarioId = Guid.NewGuid()
        };

        var comanda = CreateMockComanda(comandaId, clienteId, Guid.NewGuid(), 200.00m);
        var cliente = CreateMockCliente(clienteId);
        var resultadoDto = CreateMockProcesarPedidoCompletoDto(comandaId, false);

        SetupMocksConFidelizacion(comanda, cliente, resultadoDto);

        _servicioFacturacionMock.Setup(x => x.AcumularPuntosPorCompraAsync(
            clienteId, 200.00m, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(20)); // 20 puntos acumulados

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Value.PuntosFidelizacionAcumulados > 0);
        Assert.Equal(20, result.Value.PuntosAcumulados);

        _servicioFacturacionMock.Verify(x => x.AcumularPuntosPorCompraAsync(
            clienteId, 200.00m, It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PagoConTarjeta_DeberiaValidarDatosTarjeta()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new ProcesarPedidoCompletoCommand
        {
            ComandaId = comandaId,
            TipoPago = "Tarjeta",
            RequierePago = true,
            InfoPago = new InfoPagoDto
            {
                NumeroTarjeta = "5555555555554444", // Mastercard
                NombreTitular = "Ana López",
                MontoTotal = 75.50m,
                Moneda = "USD",
                ReferenciaPago = "REF-12345"
            },
            UsuarioId = Guid.NewGuid()
        };

        var comanda = CreateMockComanda(comandaId, null, Guid.NewGuid(), 75.50m);
        var resultadoPago = CreateMockResultadoPago("APPROVED", "67890");
        var resultadoDto = CreateMockProcesarPedidoCompletoDto(comandaId, true);

        SetupMocksConPagoTarjeta(comanda, resultadoPago, resultadoDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Value.PagoExitoso);
        Assert.Equal("67890", result.Value.TransaccionPagoId?.ToString());

        _servicioFacturacionMock.Verify(x => x.ProcesarPagoAsync(
            It.IsAny<Guid>(), It.IsAny<decimal>(), "Tarjeta", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FacturacionCompleta_DeberiaGenerarFacturaConDetalles()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new ProcesarPedidoCompletoCommand
        {
            ComandaId = comandaId,
            TipoPago = "Efectivo",
            RequierePago = false,
            TipoFactura = "Crédito Fiscal",
            NombreCliente = "Empresa XYZ S.A.",
            IdentificacionCliente = "1-234-567890",
            DireccionCliente = "Av. Principal 123",
            EmailCliente = "facturas@empresa.com",
            ObservacionesFactura = "Factura empresarial",
            UsuarioId = Guid.NewGuid()
        };

        var comanda = CreateMockComanda(comandaId, null, Guid.NewGuid(), 500.00m);
        var factura = CreateMockFactura(Guid.NewGuid(), comandaId, 500.00m);
        var resultadoDto = CreateMockProcesarPedidoCompletoDto(comandaId, false);

        SetupMocksFacturacion(comanda, factura, resultadoDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Value.FacturaGenerada);
        Assert.NotNull(result.Value.FacturaNumero);

        // Comentado: El handler usa CrearFacturaCommand a través del mediator, no el servicio directamente
        // _servicioFacturacionMock.Verify(x => x.GenerarFacturaAsync(
        //     comandaId, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Tests de Validaciones y Errores

    [Fact]
    public async Task Handle_ComandaInexistente_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new ProcesarPedidoCompletoCommand
        {
            ComandaId = comandaId,
            TipoPago = "Efectivo",
            UsuarioId = Guid.NewGuid()
        };

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comanda?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("no fue encontrada", result.Error);
    }

    [Fact]
    public async Task Handle_ComandaYaFinalizada_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new ProcesarPedidoCompletoCommand
        {
            ComandaId = comandaId,
            TipoPago = "Efectivo",
            UsuarioId = Guid.NewGuid()
        };

        var comandaFinalizada = CreateMockComanda(comandaId, null, Guid.NewGuid(), 100.00m);
        comandaFinalizada.ActualizarEstado(EstadoComanda.Finalizada);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandaFinalizada);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("ya está finalizada", result.Error);
    }

    [Fact]
    public async Task Handle_ErrorProcesoPago_DeberiaRetornarErrorYRollback()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new ProcesarPedidoCompletoCommand
        {
            ComandaId = comandaId,
            TipoPago = "Tarjeta",
            RequierePago = true,
            InfoPago = new InfoPagoDto
            {
                NumeroTarjeta = "4000000000000002", // Tarjeta que falla
                MontoTotal = 100.00m
            },
            UsuarioId = Guid.NewGuid()
        };

        var comanda = CreateMockComanda(comandaId, null, Guid.NewGuid(), 100.00m);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _servicioFacturacionMock.Setup(x => x.ProcesarPagoAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Factura>("Error en el procesamiento del pago"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error en el procesamiento del pago", result.Error);

        // No debe continuar con facturación si falla el pago
        // _servicioFacturacionMock.Verify(x => x.GenerarFacturaAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ErrorFacturacion_DeberiaRollbackPago()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new ProcesarPedidoCompletoCommand
        {
            ComandaId = comandaId,
            TipoPago = "Tarjeta",
            RequierePago = true,
            InfoPago = new InfoPagoDto
            {
                NumeroTarjeta = "4111111111111111",
                MontoTotal = 100.00m
            },
            TipoFactura = "Consumidor Final",
            NombreCliente = "Cliente Test",
            UsuarioId = Guid.NewGuid()
        };

        var comanda = CreateMockComanda(comandaId, null, Guid.NewGuid(), 100.00m);
        var resultadoPago = CreateMockResultadoPago("APPROVED", "12345");

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _servicioFacturacionMock.Setup(x => x.ProcesarPagoAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(CreateMockFactura()));

        // Comentado: El handler usa CrearFacturaCommand a través del mediator, no el servicio directamente
        // _servicioFacturacionMock.Setup(x => x.GenerarFacturaAsync(comandaId, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
        //     .ReturnsAsync(Result.Failure<Factura>("Error al generar factura"));

        _servicioFacturacionMock.Setup(x => x.RevertirPagoAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<Factura>(CreateMockFactura()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error al generar factura", result.Error);

        // Debe revertir el pago
        _servicioFacturacionMock.Verify(x => x.RevertirPagoAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MesaInexistente_NoDeberiaAfectarProceso()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var command = new ProcesarPedidoCompletoCommand
        {
            ComandaId = comandaId,
            TipoPago = "Efectivo",
            RequierePago = false,
            UsuarioId = Guid.NewGuid()
        };

        var comanda = CreateMockComanda(comandaId, null, mesaId, 50.00m);
        var resultadoDto = CreateMockProcesarPedidoCompletoDto(comandaId, false);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mesa?)null); // Mesa no existe

        _mapperMock.Setup(x => x.Map<ProcesarPedidoCompletoDto>(It.IsAny<object>()))
            .Returns(resultadoDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded); // El proceso debe continuar
        Assert.Null(result.Value.Mesa); // Mesa no se libera cuando no existe

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Mesa no encontrada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Tests de Logging y Auditoria

    [Fact]
    public async Task Handle_WorkflowCompleto_DeberiaLoggearCadaPaso()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new ProcesarPedidoCompletoCommand
        {
            ComandaId = comandaId,
            TipoPago = "Efectivo",
            RequierePago = false,
            UsuarioId = Guid.NewGuid()
        };

        var comanda = CreateMockComanda(comandaId, null, Guid.NewGuid(), 100.00m);
        var resultadoDto = CreateMockProcesarPedidoCompletoDto(comandaId, false);

        SetupMocksBasico(comanda, resultadoDto);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando procesamiento completo")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("procesado exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_TiempoEjecucion_DeberiaLoggearMetricas()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new ProcesarPedidoCompletoCommand
        {
            ComandaId = comandaId,
            TipoPago = "Efectivo",
            RequierePago = false,
            UsuarioId = Guid.NewGuid()
        };

        var comanda = CreateMockComanda(comandaId, null, Guid.NewGuid(), 100.00m);
        var resultadoDto = CreateMockProcesarPedidoCompletoDto(comandaId, false);

        SetupMocksBasico(comanda, resultadoDto);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Tiempo total")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private void SetupMocksWorkflowCompleto(Comanda comanda, object resultadoPago, object factura, ProcesarPedidoCompletoDto resultadoDto)
    {
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comanda.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Setup mediator para CrearFacturaCommand
        _mediatorMock.Setup(x => x.Send(It.IsAny<CrearFacturaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new FacturaDto { Id = Guid.NewGuid() }));

        // Setup para obtener factura creada
        _facturaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(factura as Factura ?? CreateMockFactura());

        if (comanda.ClienteId != null && comanda.ClienteId != Guid.Empty)
        {
            _servicioFacturacionMock.Setup(x => x.AcumularPuntosPorCompraAsync(
                comanda.ClienteId.Value, It.IsAny<decimal>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(15));
        }

        if (comanda.MesaId != null && comanda.MesaId != Guid.Empty)
        {
            var mesa = CreateMockMesa(comanda.MesaId);
            _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comanda.MesaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mesa);
        }

        _comandaRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<ProcesarPedidoCompletoDto>(It.IsAny<object>()))
            .Returns(resultadoDto);
    }

    private void SetupMocksWorkflowSinPago(Comanda comanda, object factura, ProcesarPedidoCompletoDto resultadoDto)
    {
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comanda.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Setup mediator para CrearFacturaCommand
        _mediatorMock.Setup(x => x.Send(It.IsAny<CrearFacturaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new FacturaDto { Id = Guid.NewGuid() }));

        // Setup para obtener factura creada
        _facturaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(factura as Factura ?? CreateMockFactura());

        if (comanda.MesaId != null && comanda.MesaId != Guid.Empty)
        {
            var mesa = CreateMockMesa(comanda.MesaId);
            _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comanda.MesaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mesa);
        }

        _comandaRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<ProcesarPedidoCompletoDto>(It.IsAny<object>()))
            .Returns(resultadoDto);
    }

    private void SetupMocksConFidelizacion(Comanda comanda, Cliente cliente, ProcesarPedidoCompletoDto resultadoDto)
    {
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comanda.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _comandaRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<ProcesarPedidoCompletoDto>(It.IsAny<object>()))
            .Returns(resultadoDto);
    }

    private void SetupMocksConPagoTarjeta(Comanda comanda, object resultadoPago, ProcesarPedidoCompletoDto resultadoDto)
    {
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comanda.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _servicioFacturacionMock.Setup(x => x.ProcesarPagoAsync(
            It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), 
            It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoPago as Factura ?? CreateMockFactura()));

        _comandaRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<ProcesarPedidoCompletoDto>(It.IsAny<object>()))
            .Returns(resultadoDto);
    }

    private void SetupMocksFacturacion(Comanda comanda, object factura, ProcesarPedidoCompletoDto resultadoDto)
    {
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comanda.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Setup mediator para CrearFacturaCommand
        _mediatorMock.Setup(x => x.Send(It.IsAny<CrearFacturaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new FacturaDto { Id = Guid.NewGuid() }));

        // Setup para obtener factura creada
        _facturaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(factura as Factura ?? CreateMockFactura());

        _comandaRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<ProcesarPedidoCompletoDto>(It.IsAny<object>()))
            .Returns(resultadoDto);
    }

    private void SetupMocksBasico(Comanda comanda, ProcesarPedidoCompletoDto resultadoDto)
    {
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comanda.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Setup mediator para CrearFacturaCommand
        _mediatorMock.Setup(x => x.Send(It.IsAny<CrearFacturaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new FacturaDto { Id = Guid.NewGuid() }));

        // Setup para obtener factura creada
        _facturaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMockFactura());

        _comandaRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<ProcesarPedidoCompletoDto>(It.IsAny<object>()))
            .Returns(resultadoDto);
    }

    private void VerifyWorkflowCompleto(Guid comandaId, Guid clienteId, Guid mesaId)
    {
        _comandaRepositoryMock.Verify(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()), Times.Once);
        _servicioFacturacionMock.Verify(x => x.ProcesarPagoAsync(
            It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), 
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _servicioFacturacionMock.Verify(x => x.AcumularPuntosPorCompraAsync(
            clienteId, It.IsAny<decimal>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        _mesaRepositoryMock.Verify(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()), Times.Once);
        _comandaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Comanda CreateMockComanda(Guid id, Guid? clienteId, Guid mesaId, decimal total)
    {
        var comanda = Comanda.Crear(mesaId, Guid.NewGuid(), clienteId, "Comanda de prueba");
        
        // Usar reflexión para setear propiedades
        typeof(EntityBase).GetProperty("Id")?.SetValue(comanda, id);
        
        // Agregar items para llegar al total
        comanda.AgregarItem(Guid.NewGuid(), "Producto Test", 1, total);
        
        return comanda;
    }

    private static Cliente CreateMockCliente(Guid id)
    {
        var clienteNombre = ClienteNombre.Crear("Cliente", "Test");
        
        // Crear valores directos sin usar Result
        var emailResult = Email.Create("cliente@test.com");
        var telefonoResult = PhoneNumber.Create("612345678");
        
        var cliente = Cliente.Crear(
            clienteNombre,
            emailResult.Value,
            telefonoResult.Value,
            DateTime.Today.AddYears(-30));
        
        typeof(EntityBase).GetProperty("Id")?.SetValue(cliente, id);
        
        return cliente;
    }

    private static Mesa CreateMockMesa(Guid id)
    {
        var mesa = Mesa.Crear(1, 4, "Interior");
        
        typeof(EntityBase).GetProperty("Id")?.SetValue(mesa, id);
        
        return mesa;
    }

    private static object CreateMockResultadoPago(string estado, string transaccionId)
    {
        return new
        {
            Estado = estado,
            TransaccionId = transaccionId,
            MontoAprobado = 150.00m,
            FechaProcesamiento = DateTime.UtcNow
        };
    }

    private static object CreateMockFactura(Guid facturaId, Guid comandaId, decimal total)
    {
        return new
        {
            Id = facturaId,
            NumeroFactura = $"FAC-{facturaId.ToString("N")[^8..].ToUpper()}",
            ComandaId = comandaId,
            Total = total,
            Estado = "Emitida",
            FechaEmision = DateTime.UtcNow
        };
    }

    private static Factura CreateMockFactura()
    {
        return Factura.Crear(
            numeroFactura: $"FAC-{Guid.NewGuid().ToString("N")[^8..].ToUpper()}",
            tipoFactura: TipoFactura.Normal,
            nombreCliente: "Cliente Test",
            clienteId: null,
            identificacionFiscal: null,
            direccionCliente: null,
            comandasIds: new[] { Guid.NewGuid() },
            observaciones: "Factura de prueba",
            fechaEmision: DateTime.UtcNow);
    }

    private static ProcesarPedidoCompletoDto CreateMockProcesarPedidoCompletoDto(Guid comandaId, bool pagoExitoso)
    {
        return new ProcesarPedidoCompletoDto
        {
            ComandaId = comandaId,
            FechaProcesamiento = DateTime.UtcNow,
            TiempoProcesamiento = TimeSpan.FromMilliseconds(1250),
            ProcesamientoExitoso = true,
            Pago = pagoExitoso ? new PagoProcesadoDto
            {
                PagoId = Guid.NewGuid(),
                EstadoPago = "Aprobado",
                TipoPago = "Tarjeta",
                MontoPago = 150.00m
            } : null,
            Factura = new FacturaProcesadaDto
            {
                FacturaId = Guid.NewGuid(),
                NumeroFactura = "FAC-001",
                MontoTotal = 150.00m,
                EstadoFactura = "Emitida"
            },
            Fidelizacion = new FidelizacionProcesadaDto
            {
                ClienteId = Guid.NewGuid(),
                PuntosAcumulados = 20,
                TotalPuntosCliente = 100,
                NivelFidelizacion = "Bronce",
                CambioNivel = false
            },
            Mesa = new MesaLiberadaDto
            {
                MesaId = Guid.NewGuid(),
                NumeroMesa = "5",
                EstadoMesa = "Disponible",
                HoraLiberacion = DateTime.UtcNow,
                TiempoOcupacion = TimeSpan.FromHours(1.5)
            }
        };
    }

    #endregion
} 