namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.Commands;

/// <summary>
/// Tests unitarios para DividirComandaHandler
/// Cobertura completa de división de comandas, distribución de items y manejo de descuentos
/// </summary>
public class DividirComandaHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<DividirComandaHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ICommunicationService> _mockNotificacionService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IGeneradorNumeroComandaService> _mockGeneradorNumero;
    private readonly DividirComandaHandler _handler;

    public DividirComandaHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<DividirComandaHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockNotificacionService = new Mock<ICommunicationService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockGeneradorNumero = new Mock<IGeneradorNumeroComandaService>();

        _handler = new DividirComandaHandler(
            _mockContext.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockCurrentUserService.Object,
            _mockNotificacionService.Object,
            _mockUnitOfWork.Object,
            _mockGeneradorNumero.Object);

        ConfigurarMocksBase();
    }

    [Fact]
    public async Task Handle_ConDivisionValida_DeberiaDividirExitosamente()
    {
        // Arrange
        var comandaOriginalId = Guid.NewGuid();
        var command = new DividirComandaCommand
        {
            ComandaOriginalId = comandaOriginalId,
            TipoDivision = TipoDivision.PorItems,
            MotivoDivision = "Solicitud del cliente",
            DistribuirDescuentos = true,
            MantenerComandaOriginal = false,
            AutorizadoPor = "Supervisor",
            DivisionItems = new List<DivisionItemsDto>
            {
                new DivisionItemsDto
                {
                    MesaDestinoId = Guid.NewGuid(),
                    Items = new List<ItemDivisionDto>
                    {
                        new ItemDivisionDto { ItemId = Guid.NewGuid(), Cantidad = 2 }
                    }
                },
                new DivisionItemsDto
                {
                    MesaDestinoId = Guid.NewGuid(),
                    Items = new List<ItemDivisionDto>
                    {
                        new ItemDivisionDto { ItemId = Guid.NewGuid(), Cantidad = 1 }
                    }
                }
            }
        };

        var comandaOriginal = CrearComandaConItems(comandaOriginalId);
        ConfigurarMocksParaDivisionExitosa(comandaOriginal);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.ComandaOriginalId.Should().Be(comandaOriginalId);
        resultado.Value.DivisionExitosa.Should().BeTrue();
        resultado.Value.TotalComandasCreadas.Should().Be(2);

        // Verificar que se crearon las nuevas comandas
        VerificarCreacionNuevasComandas(2);

        // Verificar logging
        VerificarLoggingDivisionExitosa(comandaOriginalId);
    }

    [Fact]
    public async Task Handle_ConComandaInexistente_DeberiaRetornarError()
    {
        // Arrange
        var command = new DividirComandaCommand
        {
            ComandaOriginalId = Guid.NewGuid(),
            TipoDivision = TipoDivision.PorItems,
            MotivoDivision = "Test",
            DivisionItems = new List<DivisionItemsDto>()
        };

        ConfigurarMockComandasVacio();

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("comanda original especificada no existe");

        VerificarNoSeGuardaronCambios();
    }

    [Fact]
    public async Task Handle_ConDistribucionIncorrecta_DeberiaRetornarError()
    {
        // Arrange
        var comandaOriginalId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        
        var command = new DividirComandaCommand
        {
            ComandaOriginalId = comandaOriginalId,
            TipoDivision = TipoDivision.PorItems,
            MotivoDivision = "Test",
            MantenerComandaOriginal = false,
            DivisionItems = new List<DivisionItemsDto>
            {
                new DivisionItemsDto
                {
                    Items = new List<ItemDivisionDto>
                    {
                        new ItemDivisionDto { ItemId = itemId, Cantidad = 5 } // Más cantidad que la original
                    }
                }
            }
        };

        var comandaOriginal = CrearComandaConItems(comandaOriginalId, new[]
        {
            CrearItemComanda(itemId, cantidad: 3) // Solo 3 items originales
        });

        ConfigurarMockComandas(new[] { comandaOriginal });

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("cantidad distribuida del item");
        resultado.Error.Should().Contain("excede la cantidad original");

        VerificarNoSeGuardaronCambios();
    }

    [Fact]
    public async Task Handle_ConItemsSinDistribuir_DeberiaRetornarError()
    {
        // Arrange
        var comandaOriginalId = Guid.NewGuid();
        var itemId1 = Guid.NewGuid();
        var itemId2 = Guid.NewGuid();
        
        var command = new DividirComandaCommand
        {
            ComandaOriginalId = comandaOriginalId,
            TipoDivision = TipoDivision.PorItems,
            MotivoDivision = "Test",
            MantenerComandaOriginal = false,
            DivisionItems = new List<DivisionItemsDto>
            {
                new DivisionItemsDto
                {
                    Items = new List<ItemDivisionDto>
                    {
                        new ItemDivisionDto { ItemId = itemId1, Cantidad = 2 }
                        // itemId2 no está distribuido
                    }
                }
            }
        };

        var comandaOriginal = CrearComandaConItems(comandaOriginalId, new[]
        {
            CrearItemComanda(itemId1, cantidad: 2),
            CrearItemComanda(itemId2, cantidad: 1) // Este item no está distribuido
        });

        ConfigurarMockComandas(new[] { comandaOriginal });

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("no está distribuido en ninguna nueva comanda");

        VerificarNoSeGuardaronCambios();
    }

    [Theory]
    [InlineData(EstadoComanda.Creada, true)]
    [InlineData(EstadoComanda.EnProceso, true)]
    [InlineData(EstadoComanda.Finalizada, false)]
    [InlineData(EstadoComanda.Cancelada, false)]
    [InlineData(EstadoComanda.Dividida, false)]
    public async Task Handle_ConDiferentesEstadosComanda_DeberiaValidarCorrectamente(
        EstadoComanda estadoComanda, bool deberiaDividir)
    {
        // Arrange
        var comandaOriginalId = Guid.NewGuid();
        var command = new DividirComandaCommand
        {
            ComandaOriginalId = comandaOriginalId,
            TipoDivision = TipoDivision.PorItems,
            MotivoDivision = "Test estado",
            DivisionItems = new List<DivisionItemsDto>
            {
                new DivisionItemsDto { Items = new List<ItemDivisionDto>() }
            }
        };

        var comandaOriginal = CrearComandaConItems(comandaOriginalId);
        comandaOriginal.Estado = estadoComanda;

        if (deberiaDividir)
        {
            ConfigurarMocksParaDivisionExitosa(comandaOriginal);
        }
        else
        {
            ConfigurarMockComandas(new[] { comandaOriginal });
        }

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        if (deberiaDividir)
        {
            resultado.Succeeded.Should().BeTrue();
        }
        else
        {
            resultado.Succeeded.Should().BeFalse();
            resultado.Error.Should().Contain("no es divisible");
        }
    }

    [Fact]
    public async Task Handle_ConMantenerComandaOriginalTrue_NoDeberiaEliminarItemsOriginales()
    {
        // Arrange
        var comandaOriginalId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        
        var command = new DividirComandaCommand
        {
            ComandaOriginalId = comandaOriginalId,
            TipoDivision = TipoDivision.PorItems,
            MotivoDivision = "Mantener original",
            MantenerComandaOriginal = true, // Mantener comanda original
            DivisionItems = new List<DivisionItemsDto>
            {
                new DivisionItemsDto
                {
                    Items = new List<ItemDivisionDto>
                    {
                        new ItemDivisionDto { ItemId = itemId, Cantidad = 2 }
                    }
                }
            }
        };

        var comandaOriginal = CrearComandaConItems(comandaOriginalId, new[]
        {
            CrearItemComanda(itemId, cantidad: 5) // 5 items originales
        });

        ConfigurarMocksParaDivisionExitosa(comandaOriginal);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();

        // Verificar que los items originales se mantuvieron
        comandaOriginal.Items.First().Cantidad.Should().Be(5); // Sin cambios
        comandaOriginal.Observaciones.Should().Contain("Dividida el");
    }

    [Fact]
    public async Task Handle_ConDistribuirDescuentosTrue_DeberiaDistribuirDescuentosProporcionales()
    {
        // Arrange
        var comandaOriginalId = Guid.NewGuid();
        var command = new DividirComandaCommand
        {
            ComandaOriginalId = comandaOriginalId,
            TipoDivision = TipoDivision.PorItems,
            MotivoDivision = "Con descuentos",
            DistribuirDescuentos = true,
            MantenerComandaOriginal = false,
            DivisionItems = new List<DivisionItemsDto>
            {
                new DivisionItemsDto { Items = new List<ItemDivisionDto>() },
                new DivisionItemsDto { Items = new List<ItemDivisionDto>() }
            }
        };

        var comandaOriginal = CrearComandaConItems(comandaOriginalId);
        comandaOriginal.Descuentos = new List<DescuentoComanda>
        {
            new DescuentoComanda { Monto = 100m }
        };
        comandaOriginal.Subtotal = 1000m;

        ConfigurarMocksParaDivisionExitosa(comandaOriginal);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();

        // Verificar que se crearon descuentos proporcionales
        VerificarCreacionDescuentosProporcionales();
    }

    [Fact]
    public async Task Handle_ConDivisionCompleta_DeberiaMarcarComandaOriginalComoDividida()
    {
        // Arrange
        var comandaOriginalId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        
        var command = new DividirComandaCommand
        {
            ComandaOriginalId = comandaOriginalId,
            TipoDivision = TipoDivision.PorItems,
            MotivoDivision = "División completa",
            MantenerComandaOriginal = false,
            DivisionItems = new List<DivisionItemsDto>
            {
                new DivisionItemsDto
                {
                    Items = new List<ItemDivisionDto>
                    {
                        new ItemDivisionDto { ItemId = itemId, Cantidad = 3 } // Todos los items
                    }
                }
            }
        };

        var comandaOriginal = CrearComandaConItems(comandaOriginalId, new[]
        {
            CrearItemComanda(itemId, cantidad: 3)
        });

        ConfigurarMocksParaDivisionExitosa(comandaOriginal);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();

        // Verificar que la comanda original se marcó como dividida
        comandaOriginal.Estado.Should().Be(EstadoComanda.Dividida);
        comandaOriginal.FechaFinalizacion.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ConErrorEnTransaccion_DeberiaRevertirCambios()
    {
        // Arrange
        var command = new DividirComandaCommand
        {
            ComandaOriginalId = Guid.NewGuid(),
            TipoDivision = TipoDivision.PorItems,
            MotivoDivision = "Test error",
            DivisionItems = new List<DivisionItemsDto>()
        };

        var comandaOriginal = CrearComandaConItems(command.ComandaOriginalId);
        ConfigurarMockComandas(new[] { comandaOriginal });

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Error en base de datos"));

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("Error interno al dividir la comanda");

        // Verificar logging de error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al dividir comanda")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConGeneracionNumeroComanda_DeberiaAsignarNumerosUnicos()
    {
        // Arrange
        var command = new DividirComandaCommand
        {
            ComandaOriginalId = Guid.NewGuid(),
            TipoDivision = TipoDivision.PorItems,
            MotivoDivision = "Test números",
            DivisionItems = new List<DivisionItemsDto>
            {
                new DivisionItemsDto { Items = new List<ItemDivisionDto>() },
                new DivisionItemsDto { Items = new List<ItemDivisionDto>() }
            }
        };

        var comandaOriginal = CrearComandaConItems(command.ComandaOriginalId);
        ConfigurarMocksParaDivisionExitosa(comandaOriginal);

        _mockGeneradorNumero.SetupSequence(g => g.GenerarNumeroComandaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("CMD-001")
            .ReturnsAsync("CMD-002");

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();

        // Verificar que se llamó al generador de números las veces correctas
        _mockGeneradorNumero.Verify(
            g => g.GenerarNumeroComandaAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    #region Métodos de apoyo

    private void ConfigurarMocksBase()
    {
        _mockCurrentUserService.Setup(u => u.UserId)
            .Returns(Guid.NewGuid().ToString());

        var mockTransaction = new Mock<IDbContextTransaction>();
        _mockUnitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        _mockGeneradorNumero.Setup(g => g.GenerarNumeroComandaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("CMD-NEW");
    }

    private void ConfigurarMocksParaDivisionExitosa(Comanda comandaOriginal)
    {
        ConfigurarMockComandas(new[] { comandaOriginal });

        var mockItemsComandaSet = new Mock<DbSet<ItemComanda>>();
        _mockContext.Setup(c => c.ItemsComanda).Returns(mockItemsComandaSet.Object);

        var mockDescuentosSet = new Mock<DbSet<DescuentoComanda>>();
        _mockContext.Setup(c => c.DescuentosComanda).Returns(mockDescuentosSet.Object);

        var mockAuditoriaSet = new Mock<DbSet<RegistroAuditoria>>();
        _mockContext.Setup(c => c.RegistrosAuditoria).Returns(mockAuditoriaSet.Object);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
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

    private Comanda CrearComandaConItems(Guid id, IEnumerable<ItemComanda>? items = null)
    {
        return new Comanda
        {
            Id = id,
            NumeroComanda = "CMD-ORIGINAL",
            Estado = EstadoComanda.EnProceso,
            MesaId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            Items = items?.ToList() ?? new List<ItemComanda>
            {
                CrearItemComanda(Guid.NewGuid(), cantidad: 2),
                CrearItemComanda(Guid.NewGuid(), cantidad: 1)
            },
            Descuentos = new List<DescuentoComanda>(),
            Subtotal = 500m,
            Total = 500m
        };
    }

    private ItemComanda CrearItemComanda(Guid id, int cantidad = 1, decimal precio = 50m)
    {
        return new ItemComanda
        {
            Id = id,
            ProductoId = Guid.NewGuid(),
            Cantidad = cantidad,
            PrecioUnitario = precio,
            Estado = EstadoItemComanda.Pendiente,
            Producto = new Producto { Id = Guid.NewGuid(), Nombre = "Producto Test" }
        };
    }

    private void VerificarCreacionNuevasComandas(int cantidadEsperada)
    {
        _mockContext.Verify(
            c => c.Comandas.Add(It.IsAny<Comanda>()),
            Times.Exactly(cantidadEsperada));
    }

    private void VerificarCreacionDescuentosProporcionales()
    {
        _mockContext.Verify(
            c => c.DescuentosComanda.Add(It.Is<DescuentoComanda>(d => d.TipoDescuento == "Proporcional División")),
            Times.AtLeastOnce);
    }

    private void VerificarNoSeGuardaronCambios()
    {
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private void VerificarLoggingDivisionExitosa(Guid comandaId)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("División completada exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
} 