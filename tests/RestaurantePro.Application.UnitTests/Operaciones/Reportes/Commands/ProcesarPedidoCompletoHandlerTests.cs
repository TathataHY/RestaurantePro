namespace RestaurantePro.Application.UnitTests.Operaciones.Reportes.Commands;

/// <summary>
/// Tests unitarios para ProcesarPedidoCompletoHandler
/// Valida la orquestación completa de workflows: Comanda → Pago → Factura → Fidelización → Mesa
/// </summary>
public class ProcesarPedidoCompletoHandlerTests
{
    private readonly Mock<IComandaRepository> _comandaRepositoryMock;
    private readonly Mock<IPagoService> _pagoServiceMock;
    private readonly Mock<IFacturacionService> _facturacionServiceMock;
    private readonly Mock<IFidelizacionService> _fidelizacionServiceMock;
    private readonly Mock<IMesaRepository> _mesaRepositoryMock;
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<ICommunicationService> _notificacionServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<ProcesarPedidoCompletoHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrquestadorWorkflowService> _orquestadorMock;
    private readonly ProcesarPedidoCompletoHandler _handler;

    public ProcesarPedidoCompletoHandlerTests()
    {
        _comandaRepositoryMock = new Mock<IComandaRepository>();
        _pagoServiceMock = new Mock<IPagoService>();
        _facturacionServiceMock = new Mock<IFacturacionService>();
        _fidelizacionServiceMock = new Mock<IFidelizacionService>();
        _mesaRepositoryMock = new Mock<IMesaRepository>();
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _notificacionServiceMock = new Mock<ICommunicationService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<ProcesarPedidoCompletoHandler>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orquestadorMock = new Mock<IOrquestadorWorkflowService>();

        _handler = new ProcesarPedidoCompletoHandler(
            _comandaRepositoryMock.Object,
            _pagoServiceMock.Object,
            _facturacionServiceMock.Object,
            _fidelizacionServiceMock.Object,
            _mesaRepositoryMock.Object,
            _clienteRepositoryMock.Object,
            _notificacionServiceMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _currentUserServiceMock.Object,
            _unitOfWorkMock.Object,
            _orquestadorMock.Object);
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
        Assert.True(result.Value.PuntosFidelizacionAcumulados);
        Assert.True(result.Value.MesaLiberada);

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
        Assert.False(result.Value.PagoExitoso); // No se procesó pago
        Assert.True(result.Value.FacturaGenerada);
        Assert.True(result.Value.MesaLiberada);

        // Verificar que NO se llamó al servicio de pagos
        _pagoServiceMock.Verify(x => x.ProcesarPagoAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
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

        _fidelizacionServiceMock.Setup(x => x.AcumularPuntosPorCompraAsync(
            clienteId, 200.00m, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(20)); // 20 puntos acumulados

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Value.PuntosFidelizacionAcumulados);
        Assert.Equal(20, result.Value.PuntosAcumulados);

        _fidelizacionServiceMock.Verify(x => x.AcumularPuntosPorCompraAsync(
            clienteId, 200.00m, It.IsAny<CancellationToken>()), Times.Once);
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
        Assert.Equal("67890", result.Value.TransaccionPagoId);

        _pagoServiceMock.Verify(x => x.ProcesarPagoAsync(
            It.Is<object>(p => ((dynamic)p).NumeroTarjeta == "5555555555554444"), 
            It.IsAny<CancellationToken>()), Times.Once);
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

        _facturacionServiceMock.Verify(x => x.GenerarFacturaAsync(
            It.Is<object>(f => 
                ((dynamic)f).TipoFactura == "Crédito Fiscal" &&
                ((dynamic)f).NombreCliente == "Empresa XYZ S.A."),
            It.IsAny<CancellationToken>()), Times.Once);
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
        comandaFinalizada.Finalizar(); // Ya finalizada

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

        _pagoServiceMock.Setup(x => x.ProcesarPagoAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<object>("Error en el procesamiento del pago"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error en el procesamiento del pago", result.Error);

        // No debe continuar con facturación si falla el pago
        _facturacionServiceMock.Verify(x => x.GenerarFacturaAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
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

        _pagoServiceMock.Setup(x => x.ProcesarPagoAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoPago));

        _facturacionServiceMock.Setup(x => x.GenerarFacturaAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<object>("Error al generar factura"));

        _pagoServiceMock.Setup(x => x.RevertirPagoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error al generar factura", result.Error);

        // Debe revertir el pago
        _pagoServiceMock.Verify(x => x.RevertirPagoAsync("12345", It.IsAny<CancellationToken>()), Times.Once);
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
        Assert.False(result.Value.MesaLiberada); // Pero la mesa no se libera

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

        _pagoServiceMock.Setup(x => x.ProcesarPagoAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoPago));

        _facturacionServiceMock.Setup(x => x.GenerarFacturaAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        if (comanda.ClienteId.HasValue)
        {
            _fidelizacionServiceMock.Setup(x => x.AcumularPuntosPorCompraAsync(
                comanda.ClienteId.Value, comanda.Total, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(15));
        }

        if (comanda.MesaId.HasValue)
        {
            var mesa = CreateMockMesa(comanda.MesaId.Value);
            _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comanda.MesaId.Value, It.IsAny<CancellationToken>()))
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

        _facturacionServiceMock.Setup(x => x.GenerarFacturaAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        if (comanda.MesaId.HasValue)
        {
            var mesa = CreateMockMesa(comanda.MesaId.Value);
            _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comanda.MesaId.Value, It.IsAny<CancellationToken>()))
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

        _pagoServiceMock.Setup(x => x.ProcesarPagoAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoPago));

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

        _facturacionServiceMock.Setup(x => x.GenerarFacturaAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

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
        _pagoServiceMock.Verify(x => x.ProcesarPagoAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        _facturacionServiceMock.Verify(x => x.GenerarFacturaAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        _fidelizacionServiceMock.Verify(x => x.AcumularPuntosPorCompraAsync(clienteId, It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Once);
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
        var cliente = Cliente.Crear(
            "Cliente Test",
            "cliente@test.com",
            "612345678",
            DateTime.Today.AddYears(-30),
            "Masculino");
        
        typeof(EntityBase).GetProperty("Id")?.SetValue(cliente, id);
        
        return cliente;
    }

    private static Mesa CreateMockMesa(Guid id)
    {
        var mesa = Mesa.Crear(1, 4, TipoMesa.Interior, EstadoMesa.Ocupada);
        
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
            Numero = $"FAC-{DateTime.Now:yyyyMMdd}-001",
            ComandaId = comandaId,
            Total = total,
            FechaEmision = DateTime.UtcNow
        };
    }

    private static ProcesarPedidoCompletoDto CreateMockProcesarPedidoCompletoDto(Guid comandaId, bool pagoExitoso)
    {
        return new ProcesarPedidoCompletoDto
        {
            ComandaId = comandaId,
            PagoExitoso = pagoExitoso,
            TransaccionPagoId = pagoExitoso ? "12345" : null,
            FacturaGenerada = true,
            FacturaNumero = "FAC-001",
            PuntosFidelizacionAcumulados = true,
            PuntosAcumulados = 15,
            MesaLiberada = true,
            TiempoProcesamientoMs = 1250,
            FechaProcesamiento = DateTime.UtcNow
        };
    }

    #endregion
} 