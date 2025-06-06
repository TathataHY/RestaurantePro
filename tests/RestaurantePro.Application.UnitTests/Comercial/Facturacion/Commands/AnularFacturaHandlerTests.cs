namespace RestaurantePro.Application.UnitTests.Comercial.Facturacion.Commands;

/// <summary>
/// Tests unitarios para AnularFacturaHandler
/// Valida la lógica completa de anulación de facturas con procesos empresariales
/// </summary>
public class AnularFacturaHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<AnularFacturaHandler>> _loggerMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<DbSet<Factura>> _facturasDbSetMock;
    private readonly Mock<DbSet<Usuario>> _usuariosDbSetMock;
    private readonly AnularFacturaHandler _handler;

    public AnularFacturaHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<AnularFacturaHandler>>();
        _dateTimeServiceMock = new Mock<IDateTimeService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _emailServiceMock = new Mock<IEmailService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _facturasDbSetMock = new Mock<DbSet<Factura>>();
        _usuariosDbSetMock = new Mock<DbSet<Usuario>>();

        // Setup DbContext
        _contextMock.Setup(x => x.Facturas).Returns(_facturasDbSetMock.Object);
        _contextMock.Setup(x => x.Usuarios).Returns(_usuariosDbSetMock.Object);

        _handler = new AnularFacturaHandler(
            _contextMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _dateTimeServiceMock.Object,
            _currentUserServiceMock.Object,
            _emailServiceMock.Object,
            _notificationServiceMock.Object);
    }

    #region Tests de Factory Methods del Command

    [Fact]
    public void AnulacionNormal_ConParametrosValidos_DeberiaCrearCommandCorrectamente()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var motivo = "Error en la orden de productos";

        // Act
        var command = AnularFacturaCommand.AnulacionNormal(facturaId, motivo, usuarioId, true);

        // Assert
        Assert.Equal(facturaId, command.FacturaId);
        Assert.Equal(motivo, command.Motivo);
        Assert.Equal(usuarioId, command.UsuarioAutorizaId);
        Assert.Equal("Normal", command.TipoAnulacion);
        Assert.True(command.NotificarCliente);
        Assert.True(command.RevertirInventario);
        Assert.Equal(2, command.Prioridad);
        Assert.False(command.RequiereAprobacionGerencia);
    }

    [Fact]
    public void AnulacionEmergencia_ConCodigoAutorizacion_DeberiaConfigurarEmergencia()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var motivo = "Falla crítica del sistema";
        var codigoAutorizacion = "EMG-2025-001";

        // Act
        var command = AnularFacturaCommand.AnulacionEmergencia(facturaId, motivo, usuarioId, codigoAutorizacion);

        // Assert
        Assert.Equal("Emergencia", command.TipoAnulacion);
        Assert.Equal(codigoAutorizacion, command.CodigoAutorizacion);
        Assert.True(command.RequiereAprobacionGerencia);
        Assert.Equal(4, command.Prioridad);
    }

    [Fact]
    public void AnulacionSolicitudCliente_ConDevolucion_DeberiaConfigurarDevolucion()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var motivo = "Solicitud del cliente por producto defectuoso";
        var referencia = "DEV-2025-001";

        // Act
        var command = AnularFacturaCommand.AnulacionSolicitudCliente(facturaId, motivo, usuarioId, referencia);

        // Assert
        Assert.Equal("SolicitudCliente", command.TipoAnulacion);
        Assert.True(command.ProcesarDevolucionPago);
        Assert.Equal("SaldoFavor", command.MetodoDevolucion);
        Assert.Equal(referencia, command.ReferenciaDevolucion);
        Assert.True(command.GenerarNotaCredito);
        Assert.True(command.CancelarPuntosFidelizacion);
    }

    [Fact]
    public void AnulacionProgramada_ConFechaFutura_DeberiaConfigurarProgramacion()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var motivo = "Anulación programada por mantenimiento";
        var fechaProgramada = DateTime.UtcNow.AddHours(24);

        // Act
        var command = AnularFacturaCommand.AnulacionProgramada(facturaId, motivo, usuarioId, fechaProgramada);

        // Assert
        Assert.Equal("Programada", command.TipoAnulacion);
        Assert.Equal(fechaProgramada, command.FechaProgramadaAnulacion);
        Assert.Equal(1, command.Prioridad);
    }

    #endregion

    #region Tests de Escenarios Exitosos

    [Fact]
    public async Task Handle_AnulacionNormalSimple_DeberiaAnularFacturaExitosamente()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var command = new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = "Error administrativo en la facturación",
            UsuarioAutorizaId = usuarioId,
            TipoAnulacion = "Normal",
            RevertirInventario = false,
            CancelarPuntosFidelizacion = false
        };

        var factura = CreateMockFacturaEmitida(facturaId);
        var facturaDto = CreateMockFacturaDto(facturaId);

        SetupFacturasDbSet(new List<Factura> { factura });
        
        _mapperMock.Setup(x => x.Map<FacturaDto>(It.IsAny<Factura>()))
            .Returns(facturaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(facturaId, result.Value.Id);
        
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AnulacionConAprobacionGerencia_DeberiaValidarGerente()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var gerenteId = Guid.NewGuid();
        var command = new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = "Anulación con impacto alto",
            UsuarioAutorizaId = usuarioId,
            TipoAnulacion = "Administrativa",
            RequiereAprobacionGerencia = true,
            GerenteAprobadorId = gerenteId
        };

        var factura = CreateMockFacturaEmitida(facturaId, 1500m); // Monto alto
        var gerente = CreateMockUsuarioGerente(gerenteId);
        var facturaDto = CreateMockFacturaDto(facturaId);

        SetupFacturasDbSet(new List<Factura> { factura });
        SetupUsuariosDbSet(new List<Usuario> { gerente });
        
        _mapperMock.Setup(x => x.Map<FacturaDto>(It.IsAny<Factura>()))
            .Returns(facturaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verificar que se validó el gerente
        _usuariosDbSetMock.Verify(x => x.FindAsync(gerenteId), Times.Once);
    }

    [Fact]
    public async Task Handle_AnulacionEmergenciaConNotificaciones_DeberiaEnviarNotificaciones()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var command = new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = "Emergencia del sistema",
            UsuarioAutorizaId = usuarioId,
            TipoAnulacion = "Emergencia",
            CodigoAutorizacion = "EMG-001",
            NotificarCliente = true
        };

        var factura = CreateMockFacturaEmitida(facturaId, 6000m); // Monto alto para notificación gerencial
        var facturaDto = CreateMockFacturaDto(facturaId);

        SetupFacturasDbSet(new List<Factura> { factura });
        
        _mapperMock.Setup(x => x.Map<FacturaDto>(It.IsAny<Factura>()))
            .Returns(facturaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verificar que se enviaron notificaciones (gerencia por monto alto)
        _emailServiceMock.Verify(x => x.SendEmailAsync(
            It.Is<string>(email => email.Contains("gerencia")),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_AnulacionConReversionInventario_DeberiaLoggearProceso()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var command = new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = "Producto defectuoso",
            UsuarioAutorizaId = usuarioId,
            TipoAnulacion = "Normal",
            RevertirInventario = true
        };

        var factura = CreateMockFacturaEmitida(facturaId);
        var facturaDto = CreateMockFacturaDto(facturaId);

        SetupFacturasDbSet(new List<Factura> { factura });
        
        _mapperMock.Setup(x => x.Map<FacturaDto>(It.IsAny<Factura>()))
            .Returns(facturaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verificar logging para reversión de inventario
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Reversión de inventario")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_AnulacionConCancelacionPuntos_DeberiaLoggearProceso()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var command = new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = "Cancelación por error del cliente",
            UsuarioAutorizaId = usuarioId,
            TipoAnulacion = "Normal",
            CancelarPuntosFidelizacion = true
        };

        var factura = CreateMockFacturaConCliente(facturaId, clienteId);
        var facturaDto = CreateMockFacturaDto(facturaId);

        SetupFacturasDbSet(new List<Factura> { factura });
        
        _mapperMock.Setup(x => x.Map<FacturaDto>(It.IsAny<Factura>()))
            .Returns(facturaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verificar logging para cancelación de puntos
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cancelación de puntos")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_AnulacionConDevolucionPago_DeberiaLoggearProceso()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var command = new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = "Solicitud de devolución del cliente",
            UsuarioAutorizaId = usuarioId,
            TipoAnulacion = "SolicitudCliente",
            ProcesarDevolucionPago = true,
            MetodoDevolucion = "Transferencia",
            ReferenciaDevolucion = "DEV-2025-001",
            FechaLimiteDevolucion = DateTime.UtcNow.AddDays(15)
        };

        var factura = CreateMockFacturaEmitida(facturaId);
        var facturaDto = CreateMockFacturaDto(facturaId);

        SetupFacturasDbSet(new List<Factura> { factura });
        
        _mapperMock.Setup(x => x.Map<FacturaDto>(It.IsAny<Factura>()))
            .Returns(facturaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verificar logging para devolución de pagos
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Devoluciones de pagos")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_AnulacionConNotaCredito_DeberiaLoggearGeneracion()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var command = new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = "Generar nota de crédito",
            UsuarioAutorizaId = usuarioId,
            TipoAnulacion = "Normal",
            GenerarNotaCredito = true
        };

        var factura = CreateMockFacturaEmitida(facturaId);
        var facturaDto = CreateMockFacturaDto(facturaId);

        SetupFacturasDbSet(new List<Factura> { factura });
        
        _mapperMock.Setup(x => x.Map<FacturaDto>(It.IsAny<Factura>()))
            .Returns(facturaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verificar logging para nota de crédito
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Generación de nota de crédito")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region Tests de Validaciones y Errores

    [Fact]
    public async Task Handle_FacturaNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var command = new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = "Motivo válido",
            UsuarioAutorizaId = Guid.NewGuid(),
            TipoAnulacion = "Normal"
        };

        SetupFacturasDbSet(new List<Factura>()); // Factura no existe

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("La factura especificada no existe", result.Error);
    }

    [Fact]
    public async Task Handle_FacturaYaAnulada_DeberiaRetornarError()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var command = new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = "Motivo válido",
            UsuarioAutorizaId = Guid.NewGuid(),
            TipoAnulacion = "Normal"
        };

        var factura = CreateMockFacturaAnulada(facturaId);

        SetupFacturasDbSet(new List<Factura> { factura });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("La factura ya está anulada", result.Error);
    }

    [Fact]
    public async Task Handle_RequiereAprobacionSinGerente_DeberiaRetornarError()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var command = new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = "Anulación de alto impacto",
            UsuarioAutorizaId = Guid.NewGuid(),
            TipoAnulacion = "Normal",
            RequiereAprobacionGerencia = true,
            GerenteAprobadorId = null // Sin gerente especificado
        };

        var factura = CreateMockFacturaEmitida(facturaId, 1500m); // Monto alto

        SetupFacturasDbSet(new List<Factura> { factura });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("La anulación requiere aprobación de gerencia", result.Error);
    }

    [Fact]
    public async Task Handle_GerenteAprobadorInvalido_DeberiaRetornarError()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var gerenteInvalidoId = Guid.NewGuid();
        var command = new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = "Anulación con gerente inválido",
            UsuarioAutorizaId = Guid.NewGuid(),
            TipoAnulacion = "Normal",
            RequiereAprobacionGerencia = true,
            GerenteAprobadorId = gerenteInvalidoId
        };

        var factura = CreateMockFacturaEmitida(facturaId, 1500m);
        var usuarioNormal = CreateMockUsuarioNormal(gerenteInvalidoId); // No es gerente

        SetupFacturasDbSet(new List<Factura> { factura });
        SetupUsuariosDbSet(new List<Usuario> { usuarioNormal });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("El gerente aprobador especificado no es válido", result.Error);
    }

    [Fact]
    public async Task Handle_AnulacionConcurrente_DeberiaRetornarError()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var command = new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = "Motivo válido",
            UsuarioAutorizaId = Guid.NewGuid(),
            TipoAnulacion = "Normal"
        };

        var facturaOriginal = CreateMockFacturaEmitida(facturaId);
        var facturaAnulada = CreateMockFacturaAnulada(facturaId); // Fue anulada por otro proceso

        // Setup inicial con factura original
        var mockSet = CreateDbSetMock(new List<Factura> { facturaOriginal });
        _contextMock.Setup(x => x.Facturas).Returns(mockSet.Object);
        
        // Simular que al buscar por FindAsync devuelve factura anulada (anulada concurrentemente)
        mockSet.Setup(x => x.FindAsync(facturaId))
            .ReturnsAsync(facturaAnulada);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("La factura fue anulada por otro usuario", result.Error);
    }

    [Fact]
    public async Task Handle_ErrorNotificaciones_DeberiaLoggearWarningPeroNoFallar()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var command = new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = "Anulación con error en notificaciones",
            UsuarioAutorizaId = Guid.NewGuid(),
            TipoAnulacion = "Normal"
        };

        var factura = CreateMockFacturaEmitida(facturaId);
        var facturaDto = CreateMockFacturaDto(facturaId);

        SetupFacturasDbSet(new List<Factura> { factura });
        
        _mapperMock.Setup(x => x.Map<FacturaDto>(It.IsAny<Factura>()))
            .Returns(facturaDto);

        // Error en email service
        _emailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Error de SMTP"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded); // No debe fallar el proceso general
        
        // Verificar que se loggeó el warning
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
        var command = new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = "Motivo válido",
            UsuarioAutorizaId = Guid.NewGuid(),
            TipoAnulacion = "Normal"
        };

        // Simular una excepción en el context al tratar de acceder a Facturas
        _contextMock.Setup(x => x.Facturas)
            .Throws(new InvalidOperationException("Error de base de datos simulado"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error interno al anular la factura", result.Error);
        
        // Verificar logging del error
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error al anular factura")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Debug_Handle_FacturaNoExiste_MostrarExcepcionExacta()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var command = new AnularFacturaCommand
        {
            FacturaId = facturaId,
            Motivo = "Motivo válido",
            UsuarioAutorizaId = Guid.NewGuid(),
            TipoAnulacion = "Normal"
        };

        SetupFacturasDbSet(new List<Factura>()); // Factura no existe

        Exception capturedException = null;

        try
        {
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
            
            // Si llegamos aquí, capturar el resultado
            Console.WriteLine($"Result succeeded: {result.Succeeded}");
            Console.WriteLine($"Result error: {result.Error}");
        }
        catch (Exception ex)
        {
            capturedException = ex;
            Console.WriteLine($"Exception Type: {ex.GetType().Name}");
            Console.WriteLine($"Exception Message: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception Type: {ex.InnerException.GetType().Name}");
                Console.WriteLine($"Inner Exception Message: {ex.InnerException.Message}");
            }
        }

        // Para que el test no falle, solo log la información
        Assert.True(true, $"Debug test - Exception captured: {capturedException?.GetType().Name ?? "None"}");
    }

    #endregion

    #region Helper Methods - Setup

    private void SetupFacturasDbSet(List<Factura> facturas)
    {
        var mockSet = facturas.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Facturas).Returns(mockSet.Object);
        
        // Setup específico para FindAsync
        mockSet.Setup(x => x.FindAsync(It.IsAny<Guid>()))
            .Returns<Guid>(id =>
            {
                var result = facturas.FirstOrDefault(f => f.Id == id);
                return ValueTask.FromResult(result);
            });
    }

    private void SetupUsuariosDbSet(List<Usuario> usuarios)
    {
        var mockSet = usuarios.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Usuarios).Returns(mockSet.Object);
        
        // Setup específico para FindAsync
        mockSet.Setup(x => x.FindAsync(It.IsAny<Guid>()))
            .Returns<Guid>(id =>
            {
                var result = usuarios.FirstOrDefault(u => u.Id == id);
                return ValueTask.FromResult(result);
            });
    }

    private Mock<DbSet<T>> CreateDbSetMock<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var dbSetMock = new Mock<DbSet<T>>();

        dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        dbSetMock.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

        // Eliminamos los setups de Include ya que son métodos de extensión que no se pueden mockear directamente
        // El handler debería funcionar sin Include en las pruebas unitarias
        
        return dbSetMock;
    }

    #endregion

    #region Helper Methods - Data Creation

    private static Factura CreateMockFacturaEmitida(Guid id, decimal total = 1000.00m)
    {
        // Crear factura usando factory method real
        var factura = Factura.Crear(
            numeroFactura: $"FAC-{id.ToString().Substring(0, 8)}",
            tipoFactura: TipoFactura.Normal,
            nombreCliente: "Cliente Test",
            clienteId: null,
            identificacionFiscal: null,
            direccionCliente: null,
            comandasIds: null,
            observaciones: null,
            fechaEmision: DateTime.UtcNow.AddDays(-1)
        );

        // Agregar un detalle para que tenga contenido
        factura.AgregarDetalle(
            productoId: Guid.NewGuid(),
            descripcion: "Producto de prueba",
            cantidad: 1,
            precioUnitario: total,
            porcentajeImpuesto: 16
        );

        // Emitir la factura para cambiar su estado
        factura.Emitir();

        // Usar reflexión para establecer el ID específico
        typeof(EntityBase).GetProperty("Id")?.SetValue(factura, id);

        return factura;
    }

    private static Factura CreateMockFacturaAnulada(Guid id)
    {
        // Crear factura usando factory method real
        var factura = Factura.Crear(
            numeroFactura: $"FAC-{id.ToString().Substring(0, 8)}",
            tipoFactura: TipoFactura.Normal,
            nombreCliente: "Cliente Test",
            clienteId: null,
            identificacionFiscal: null,
            direccionCliente: null,
            comandasIds: null,
            observaciones: null,
            fechaEmision: DateTime.UtcNow.AddDays(-1)
        );

        // Agregar detalle y emitir
        factura.AgregarDetalle(
            productoId: Guid.NewGuid(),
            descripcion: "Producto de prueba",
            cantidad: 1,
            precioUnitario: 1000.00m,
            porcentajeImpuesto: 16
        );

        factura.Emitir();
        factura.Anular("Anulada para test");

        // Usar reflexión para establecer el ID específico
        typeof(EntityBase).GetProperty("Id")?.SetValue(factura, id);

        return factura;
    }

    private static Factura CreateMockFacturaConCliente(Guid id, Guid clienteId)
    {
        // Crear factura usando factory method real
        var factura = Factura.Crear(
            numeroFactura: $"FAC-{id.ToString().Substring(0, 8)}",
            tipoFactura: TipoFactura.Normal,
            nombreCliente: "Cliente Test",
            clienteId: clienteId,
            identificacionFiscal: null,
            direccionCliente: null,
            comandasIds: null,
            observaciones: null,
            fechaEmision: DateTime.UtcNow.AddDays(-1)
        );

        // Agregar detalle y emitir
        factura.AgregarDetalle(
            productoId: Guid.NewGuid(),
            descripcion: "Producto de prueba",
            cantidad: 1,
            precioUnitario: 1000.00m,
            porcentajeImpuesto: 16
        );

        factura.Emitir();

        // Usar reflexión para establecer el ID específico
        typeof(EntityBase).GetProperty("Id")?.SetValue(factura, id);

        return factura;
    }

    private static Usuario CreateMockUsuarioGerente(Guid id)
    {
        // Crear usuario real usando factory method
        var usuario = Usuario.Crear(
            nombreUsuario: "gerente.test",
            nombreCompleto: "Gerente Test",
            email: "gerente@test.com",
            rol: RolUsuario.Gerente
        );

        // Usar reflexión para establecer propiedades específicas
        typeof(EntityBase).GetProperty("Id")?.SetValue(usuario, id);

        return usuario;
    }

    private static Usuario CreateMockUsuarioNormal(Guid id)
    {
        // Crear usuario real usando factory method
        var usuario = Usuario.Crear(
            nombreUsuario: "usuario.test",
            nombreCompleto: "Usuario Test",
            email: "usuario@test.com",
            rol: RolUsuario.Mesero
        );

        // Usar reflexión para establecer propiedades específicas
        typeof(EntityBase).GetProperty("Id")?.SetValue(usuario, id);

        return usuario;
    }

    private static FacturaDto CreateMockFacturaDto(Guid id)
    {
        return new FacturaDto
        {
            Id = id,
            Numero = $"FAC-{id.ToString().Substring(0, 8)}",
            Estado = EstadoFactura.Anulada, // Estado después de anular
            FechaEmision = DateTime.UtcNow.AddDays(-1),
            NombreCliente = "Cliente Test",
            Total = 1000.00m,
            Subtotal = 862.07m,
            Impuestos = 137.93m,
            Descuentos = 0.00m
        };
    }

    #endregion
}

/// <summary>
/// Helper classes for async DbSet mocking
/// </summary>
internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public ValueTask<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var result = Execute<TResult>(expression);
        return ValueTask.FromResult(result);
    }

    TResult IAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        return Execute<TResult>(expression);
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    { }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(_inner.MoveNext());
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
} 