namespace RestaurantePro.Application.UnitTests.Operaciones.Mesas.Commands;

/// <summary>
/// Tests unitarios para TransferirMesaHandler
/// Cobertura completa de transferencia de comandas entre mesas, validaciones de estado y auditoría
/// </summary>
public class TransferirMesaHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<TransferirMesaHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ICommunicationService> _mockNotificacionService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly TransferirMesaHandler _handler;

    public TransferirMesaHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<TransferirMesaHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockNotificacionService = new Mock<ICommunicationService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _handler = new TransferirMesaHandler(
            _mockContext.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockCurrentUserService.Object,
            _mockNotificacionService.Object,
            _mockUnitOfWork.Object);

        ConfigurarMocksBase();
    }

    [Fact]
    public async Task Handle_ConTransferenciaValida_DeberiaTransferirExitosamente()
    {
        // Arrange
        var command = new TransferirMesaCommand
        {
            ComandaId = Guid.NewGuid(),
            MesaOrigenId = Guid.NewGuid(),
            MesaDestinoId = Guid.NewGuid(),
            MotivoTransferencia = "Solicitud del cliente",
            NotificarMesero = true,
            AutorizadoPor = Guid.NewGuid()
        };

        var comanda = CrearComanda(command.ComandaId, command.MesaOrigenId, EstadoComanda.EnProceso);
        var mesaOrigen = CrearMesa(command.MesaOrigenId, EstadoMesa.Ocupada);
        var mesaDestino = CrearMesa(command.MesaDestinoId, EstadoMesa.Disponible);

        ConfigurarMocksParaTransferenciaExitosa(comanda, mesaOrigen, mesaDestino);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.ComandaId.Should().Be(command.ComandaId);
        resultado.Value.MesaAnteriorId.Should().Be(command.MesaOrigenId);
        resultado.Value.MesaNuevaId.Should().Be(command.MesaDestinoId);
        resultado.Value.TransferenciaExitosa.Should().BeTrue();

        // Verificar que se actualizó la comanda
        VerificarActualizacionComanda(command.MesaDestinoId);

        // Verificar que se actualizaron las mesas
        VerificarActualizacionMesas();

        // Verificar logging
        VerificarLoggingTransferenciaExitosa(command.ComandaId);
    }

    [Fact]
    public async Task Handle_ConComandaInexistente_DeberiaRetornarError()
    {
        // Arrange
        var command = new TransferirMesaCommand
        {
            ComandaId = Guid.NewGuid(),
            MesaOrigenId = Guid.NewGuid(),
            MesaDestinoId = Guid.NewGuid(),
            MotivoTransferencia = "Test"
        };

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
    public async Task Handle_ConMesaOrigenInexistente_DeberiaRetornarError()
    {
        // Arrange
        var command = new TransferirMesaCommand
        {
            ComandaId = Guid.NewGuid(),
            MesaOrigenId = Guid.NewGuid(),
            MesaDestinoId = Guid.NewGuid(),
            MotivoTransferencia = "Test"
        };

        var comanda = CrearComanda(command.ComandaId, command.MesaOrigenId, EstadoComanda.EnProceso);
        ConfigurarMockComandas(new[] { comanda });
        ConfigurarMockMesasVacio();

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("mesa de origen especificada no existe");

        VerificarNoSeGuardaronCambios();
    }

    [Fact]
    public async Task Handle_ConMesaDestinoInexistente_DeberiaRetornarError()
    {
        // Arrange
        var command = new TransferirMesaCommand
        {
            ComandaId = Guid.NewGuid(),
            MesaOrigenId = Guid.NewGuid(),
            MesaDestinoId = Guid.NewGuid(),
            MotivoTransferencia = "Test"
        };

        var comanda = CrearComanda(command.ComandaId, command.MesaOrigenId, EstadoComanda.EnProceso);
        var mesaOrigen = CrearMesa(command.MesaOrigenId, EstadoMesa.Ocupada);

        ConfigurarMockComandas(new[] { comanda });
        ConfigurarMockMesas(new[] { mesaOrigen });

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("mesa de destino especificada no existe");

        VerificarNoSeGuardaronCambios();
    }

    [Fact]
    public async Task Handle_ConComandaNoPerteneceAMesaOrigen_DeberiaRetornarError()
    {
        // Arrange
        var command = new TransferirMesaCommand
        {
            ComandaId = Guid.NewGuid(),
            MesaOrigenId = Guid.NewGuid(),
            MesaDestinoId = Guid.NewGuid(),
            MotivoTransferencia = "Test"
        };

        var comanda = CrearComanda(command.ComandaId, Guid.NewGuid(), EstadoComanda.EnProceso); // Mesa diferente
        var mesaOrigen = CrearMesa(command.MesaOrigenId, EstadoMesa.Ocupada);
        var mesaDestino = CrearMesa(command.MesaDestinoId, EstadoMesa.Disponible);

        ConfigurarMockComandas(new[] { comanda });
        ConfigurarMockMesas(new[] { mesaOrigen, mesaDestino });

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("comanda no pertenece a la mesa de origen");

        VerificarNoSeGuardaronCambios();
    }

    [Fact]
    public async Task Handle_ConMesaDestinoOcupada_DeberiaRetornarError()
    {
        // Arrange
        var command = new TransferirMesaCommand
        {
            ComandaId = Guid.NewGuid(),
            MesaOrigenId = Guid.NewGuid(),
            MesaDestinoId = Guid.NewGuid(),
            MotivoTransferencia = "Test"
        };

        var comanda = CrearComanda(command.ComandaId, command.MesaOrigenId, EstadoComanda.EnProceso);
        var mesaOrigen = CrearMesa(command.MesaOrigenId, EstadoMesa.Ocupada);
        var mesaDestino = CrearMesa(command.MesaDestinoId, EstadoMesa.Disponible);

        var comandaActivaDestino = CrearComanda(Guid.NewGuid(), command.MesaDestinoId, EstadoComanda.EnProceso);

        ConfigurarMockComandas(new[] { comanda, comandaActivaDestino });
        ConfigurarMockMesas(new[] { mesaOrigen, mesaDestino });

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("mesa de destino ya tiene comandas activas");

        VerificarNoSeGuardaronCambios();
    }

    [Fact]
    public async Task Handle_ConCapacidadInsuficienteMesaDestino_DeberiaRetornarError()
    {
        // Arrange
        var command = new TransferirMesaCommand
        {
            ComandaId = Guid.NewGuid(),
            MesaOrigenId = Guid.NewGuid(),
            MesaDestinoId = Guid.NewGuid(),
            MotivoTransferencia = "Test"
        };

        var comanda = CrearComanda(command.ComandaId, command.MesaOrigenId, EstadoComanda.EnProceso, numeroPersonas: 6);
        var mesaOrigen = CrearMesa(command.MesaOrigenId, EstadoMesa.Ocupada, capacidad: 8);
        var mesaDestino = CrearMesa(command.MesaDestinoId, EstadoMesa.Disponible, capacidad: 4); // Capacidad insuficiente

        ConfigurarMockComandas(new[] { comanda });
        ConfigurarMockMesas(new[] { mesaOrigen, mesaDestino });

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("mesa de destino no tiene capacidad suficiente");

        VerificarNoSeGuardaronCambios();
    }

    [Theory]
    [InlineData(EstadoComanda.Creada, true)]
    [InlineData(EstadoComanda.EnProceso, true)]
    [InlineData(EstadoComanda.Finalizada, false)]
    [InlineData(EstadoComanda.Cancelada, false)]
    public async Task Handle_ConDiferentesEstadosComanda_DeberiaValidarCorrectamente(
        EstadoComanda estadoComanda, bool deberiaTransferir)
    {
        // Arrange
        var command = new TransferirMesaCommand
        {
            ComandaId = Guid.NewGuid(),
            MesaOrigenId = Guid.NewGuid(),
            MesaDestinoId = Guid.NewGuid(),
            MotivoTransferencia = "Test estado"
        };

        var comanda = CrearComanda(command.ComandaId, command.MesaOrigenId, estadoComanda);
        var mesaOrigen = CrearMesa(command.MesaOrigenId, EstadoMesa.Ocupada);
        var mesaDestino = CrearMesa(command.MesaDestinoId, EstadoMesa.Disponible);

        if (deberiaTransferir)
        {
            ConfigurarMocksParaTransferenciaExitosa(comanda, mesaOrigen, mesaDestino);
        }
        else
        {
            ConfigurarMockComandas(new[] { comanda });
            ConfigurarMockMesas(new[] { mesaOrigen, mesaDestino });
        }

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        if (deberiaTransferir)
        {
            resultado.Succeeded.Should().BeTrue();
        }
        else
        {
            resultado.Succeeded.Should().BeFalse();
            resultado.Error.Should().Contain("no es transferible");
        }
    }

    [Fact]
    public async Task Handle_ConNotificacionHabilitada_DeberiaEnviarNotificacion()
    {
        // Arrange
        var command = new TransferirMesaCommand
        {
            ComandaId = Guid.NewGuid(),
            MesaOrigenId = Guid.NewGuid(),
            MesaDestinoId = Guid.NewGuid(),
            MotivoTransferencia = "Cambio de ubicación",
            NotificarMesero = true,
            NotasTransferencia = "Cliente solicita mesa más tranquila"
        };

        var comanda = CrearComanda(command.ComandaId, command.MesaOrigenId, EstadoComanda.EnProceso);
        var mesaOrigen = CrearMesa(command.MesaOrigenId, EstadoMesa.Ocupada, numero: "M01");
        var mesaDestino = CrearMesa(command.MesaDestinoId, EstadoMesa.Disponible, numero: "M05");

        ConfigurarMocksParaTransferenciaExitosa(comanda, mesaOrigen, mesaDestino);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();

        // Verificar que se envió la notificación
        _mockNotificacionService.Verify(
            n => n.EnviarNotificacionAsync(
                It.Is<string[]>(dest => dest.Contains(comanda.MeseroId.ToString())),
                "Transferencia de Mesa",
                It.Is<string>(msg => msg.Contains("M01") && msg.Contains("M05")),
                TipoComunicacion.TransferenciaMesa,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConNotificacionDeshabilitada_NoDeberiaEnviarNotificacion()
    {
        // Arrange
        var command = new TransferirMesaCommand
        {
            ComandaId = Guid.NewGuid(),
            MesaOrigenId = Guid.NewGuid(),
            MesaDestinoId = Guid.NewGuid(),
            MotivoTransferencia = "Test",
            NotificarMesero = false
        };

        var comanda = CrearComanda(command.ComandaId, command.MesaOrigenId, EstadoComanda.EnProceso);
        var mesaOrigen = CrearMesa(command.MesaOrigenId, EstadoMesa.Ocupada);
        var mesaDestino = CrearMesa(command.MesaDestinoId, EstadoMesa.Disponible);

        ConfigurarMocksParaTransferenciaExitosa(comanda, mesaOrigen, mesaDestino);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();

        // Verificar que NO se envió notificación
        _mockNotificacionService.Verify(
            n => n.EnviarNotificacionAsync(
                It.IsAny<string[]>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TipoComunicacion>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ConMantenerEstadoTrue_NoDeberiaModificarEstadoComanda()
    {
        // Arrange
        var command = new TransferirMesaCommand
        {
            ComandaId = Guid.NewGuid(),
            MesaOrigenId = Guid.NewGuid(),
            MesaDestinoId = Guid.NewGuid(),
            MotivoTransferencia = "Test",
            MantenerEstado = true
        };

        var estadoOriginal = EstadoComanda.EnProceso;
        var comanda = CrearComanda(command.ComandaId, command.MesaOrigenId, estadoOriginal);
        var mesaOrigen = CrearMesa(command.MesaOrigenId, EstadoMesa.Ocupada);
        var mesaDestino = CrearMesa(command.MesaDestinoId, EstadoMesa.Disponible);

        ConfigurarMocksParaTransferenciaExitosa(comanda, mesaOrigen, mesaDestino);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();

        // Verificar que el estado se mantuvo
        comanda.Estado.Should().Be(estadoOriginal);
    }

    [Fact]
    public async Task Handle_ConErrorEnTransaccion_DeberiaRevertirCambios()
    {
        // Arrange
        var command = new TransferirMesaCommand
        {
            ComandaId = Guid.NewGuid(),
            MesaOrigenId = Guid.NewGuid(),
            MesaDestinoId = Guid.NewGuid(),
            MotivoTransferencia = "Test"
        };

        var comanda = CrearComanda(command.ComandaId, command.MesaOrigenId, EstadoComanda.EnProceso);
        var mesaOrigen = CrearMesa(command.MesaOrigenId, EstadoMesa.Ocupada);
        var mesaDestino = CrearMesa(command.MesaDestinoId, EstadoMesa.Disponible);

        ConfigurarMockComandas(new[] { comanda });
        ConfigurarMockMesas(new[] { mesaOrigen, mesaDestino });

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Error en base de datos"));

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("Error interno al transferir la comanda");

        // Verificar logging de error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al transferir comanda")),
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
            .Returns(Task.FromResult(mockTransaction.Object));
    }

    private void ConfigurarMocksParaTransferenciaExitosa(Comanda comanda, Mesa mesaOrigen, Mesa mesaDestino)
    {
        ConfigurarMockComandas(new[] { comanda });
        ConfigurarMockMesas(new[] { mesaOrigen, mesaDestino });

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    private void ConfigurarMockComandas(IEnumerable<Comanda> comandas)
    {
        var mockSet = CrearMockDbSet(comandas.AsQueryable());
        _mockContext.Setup(c => c.Comandas).Returns(mockSet.Object);
    }

    private void ConfigurarMockComandasVacio()
    {
        var mockSet = CrearMockDbSet(new List<Comanda>().AsQueryable());
        _mockContext.Setup(c => c.Comandas).Returns(mockSet.Object);
    }

    private void ConfigurarMockMesas(IEnumerable<Mesa> mesas)
    {
        var mockSet = CrearMockDbSet(mesas.AsQueryable());
        _mockContext.Setup(c => c.Mesas).Returns(mockSet.Object);
    }

    private void ConfigurarMockMesasVacio()
    {
        var mockSet = CrearMockDbSet(new List<Mesa>().AsQueryable());
        _mockContext.Setup(c => c.Mesas).Returns(mockSet.Object);
    }

    private Mock<DbSet<T>> CrearMockDbSet<T>(IQueryable<T> data) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        return mockSet;
    }

    private Comanda CrearComanda(Guid id, Guid mesaId, EstadoComanda estado, int numeroPersonas = 4)
    {
        // Crear comanda usando el factory method
        var comanda = Comanda.Crear(
            meseroId: Guid.NewGuid(),
            clienteId: null, 
            mesaId: mesaId,
            observaciones: "Comanda de prueba"
        );
        
        // Actualizar estado si es diferente al inicial usando métodos públicos
        if (estado != EstadoComanda.Creada)
        {
            switch (estado)
            {
                case EstadoComanda.EnProceso:
                    comanda.ActualizarEstado(EstadoComanda.EnProceso);
                    break;
                case EstadoComanda.Lista:
                    comanda.ActualizarEstado(EstadoComanda.EnProceso);
                    comanda.ActualizarEstado(EstadoComanda.Lista);
                    break;
                case EstadoComanda.Entregada:
                    comanda.ActualizarEstado(EstadoComanda.EnProceso);
                    comanda.ActualizarEstado(EstadoComanda.Lista);
                    comanda.ActualizarEstado(EstadoComanda.Entregada);
                    break;
                case EstadoComanda.Finalizada:
                    // Primero agregar un producto para que la validación pase
                    comanda.AgregarProducto(Guid.NewGuid(), 1, 10.00m, "Producto de prueba");
                    comanda.ActualizarEstado(EstadoComanda.EnProceso);
                    comanda.ActualizarEstado(EstadoComanda.Lista);
                    comanda.ActualizarEstado(EstadoComanda.Entregada);
                    comanda.ActualizarEstado(EstadoComanda.Finalizada);
                    break;
                case EstadoComanda.Cancelada:
                    // Usar el método Cancelar específico de la entidad
                    comanda.Cancelar("Cancelada por test");
                    break;
                case EstadoComanda.Dividida:
                    // Para Dividida, agregar producto primero y usar el comportamiento por defecto
                    // El estado Dividida se maneja en procesos específicos, para tests usamos Finalizada
                    comanda.AgregarProducto(Guid.NewGuid(), 1, 10.00m, "Producto de prueba");
                    comanda.ActualizarEstado(EstadoComanda.EnProceso);
                    comanda.ActualizarEstado(EstadoComanda.Lista);
                    comanda.ActualizarEstado(EstadoComanda.Entregada);
                    comanda.ActualizarEstado(EstadoComanda.Finalizada);
                    break;
            }
        }
        
        return comanda;
    }

    private Mesa CrearMesa(Guid id, EstadoMesa estado, int capacidad = 6, string numero = "M01")
    {
        // Crear mesa usando el factory method 
        var mesa = Mesa.Crear(
            numero: int.Parse(numero.Replace("M", "")), // Convertir "M01" a 1
            capacidad: capacidad,
            ubicacion: "Zona General"
        );
        
        // Si necesitamos un ID específico, usar reflection solo para la propiedad Id
        if (id != Guid.Empty && id != mesa.Id)
        {
            typeof(Mesa).GetProperty("Id")?.SetValue(mesa, id);
        }
        
        // Actualizar estado si es diferente al inicial
        if (estado != EstadoMesa.Disponible)
        {
            switch (estado)
            {
                case EstadoMesa.Ocupada:
                    mesa.MarcarComoOcupada();
                    break;
                case EstadoMesa.Reservada:
                    mesa.MarcarComoReservada();
                    break;
                case EstadoMesa.FueraDeServicio:
                    mesa.MarcarComoFueraDeServicio("Test");
                    break;
            }
        }
        
        return mesa;
    }

    private void VerificarActualizacionComanda(Guid mesaDestinoId)
    {
        _mockContext.Verify(c => c.Comandas.Update(It.Is<Comanda>(cmd => cmd.MesaId == mesaDestinoId)), Times.Once);
    }

    private void VerificarActualizacionMesas()
    {
        _mockContext.Verify(c => c.Mesas.Update(It.IsAny<Mesa>()), Times.AtLeast(1));
    }

    private void VerificarNoSeGuardaronCambios()
    {
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private void VerificarLoggingTransferenciaExitosa(Guid comandaId)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Transferencia completada exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
} 