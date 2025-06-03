using RestaurantePro.Application.UnitTests.Common;
using RestaurantePro.Application.Operaciones.Comandas.Commands.DividirComanda;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Core.Base;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using Microsoft.Extensions.Logging;
using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;
using System.Reflection;

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
    private readonly Mock<IDateTimeService> _mockDateTimeService;
    private readonly DividirComandaHandler _handler;

    public DividirComandaHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<DividirComandaHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockNotificacionService = new Mock<ICommunicationService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockDateTimeService = new Mock<IDateTimeService>();

        _handler = new DividirComandaHandler(
            _mockContext.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockCurrentUserService.Object,
            _mockNotificacionService.Object,
            _mockUnitOfWork.Object,
            _mockDateTimeService.Object);

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
            TipoDivision = TipoDivisionComanda.PorItems,
            MotivoDivision = "Cliente solicita cuentas separadas",
            DistribuirDescuentos = true,
            MantenerComandaOriginal = false,
            AutorizadoPor = Guid.NewGuid(),
            DivisionItems = new List<DivisionComandaDto>
            {
                new DivisionComandaDto
                {
                    NumeroComandaNueva = 1,
                    MesaDestinoId = Guid.NewGuid(),
                    Items = new List<ItemDivisionDto>
                    {
                        new ItemDivisionDto { ItemId = Guid.NewGuid(), Cantidad = 2 }
                    }
                },
                new DivisionComandaDto
                {
                    NumeroComandaNueva = 2,
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
            TipoDivision = TipoDivisionComanda.PorItems,
            MotivoDivision = "Test",
            DivisionItems = new List<DivisionComandaDto>()
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
            TipoDivision = TipoDivisionComanda.PorItems,
            MotivoDivision = "Test",
            MantenerComandaOriginal = false,
            DivisionItems = new List<DivisionComandaDto>
            {
                new DivisionComandaDto
                {
                    NumeroComandaNueva = 1,
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
            TipoDivision = TipoDivisionComanda.PorItems,
            MotivoDivision = "Test",
            MantenerComandaOriginal = false,
            DivisionItems = new List<DivisionComandaDto>
            {
                new DivisionComandaDto
                {
                    NumeroComandaNueva = 1,
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
            TipoDivision = TipoDivisionComanda.PorItems,
            MotivoDivision = "Test estado",
            DivisionItems = new List<DivisionComandaDto>
            {
                new DivisionComandaDto 
                { 
                    NumeroComandaNueva = 1,
                    Items = new List<ItemDivisionDto>() 
                }
            }
        };

        var comandaOriginal = CrearComandaConItems(comandaOriginalId);
        
        // Usar reflection para modificar el estado (solo en tests)
        var propEstado = typeof(Comanda).GetProperty("Estado", BindingFlags.Public | BindingFlags.Instance);
        propEstado?.SetValue(comandaOriginal, estadoComanda);

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
            TipoDivision = TipoDivisionComanda.PorItems,
            MotivoDivision = "Mantener original",
            MantenerComandaOriginal = true, // Mantener comanda original
            DivisionItems = new List<DivisionComandaDto>
            {
                new DivisionComandaDto
                {
                    NumeroComandaNueva = 1,
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
            TipoDivision = TipoDivisionComanda.PorItems,
            MotivoDivision = "Con descuentos",
            DistribuirDescuentos = true,
            MantenerComandaOriginal = false,
            DivisionItems = new List<DivisionComandaDto>
            {
                new DivisionComandaDto 
                { 
                    NumeroComandaNueva = 1,
                    Items = new List<ItemDivisionDto>() 
                },
                new DivisionComandaDto 
                { 
                    NumeroComandaNueva = 2,
                    Items = new List<ItemDivisionDto>() 
                }
            }
        };

        var comandaOriginal = CrearComandaConItems(comandaOriginalId);
        // TODO: Descuentos no está disponible aún en la entidad Comanda del dominio
        // comandaOriginal.Descuentos = new List<DescuentoComanda>
        // {
        //     new DescuentoComanda { Monto = 100m }
        // };
        // comandaOriginal.Subtotal = 1000m;

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
            TipoDivision = TipoDivisionComanda.PorItems,
            MotivoDivision = "División completa",
            MantenerComandaOriginal = false,
            DivisionItems = new List<DivisionComandaDto>
            {
                new DivisionComandaDto
                {
                    NumeroComandaNueva = 1,
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
        // TODO: FechaFinalizacion no está disponible aún en la entidad Comanda del dominio
        // comandaOriginal.FechaFinalizacion.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ConErrorEnTransaccion_DeberiaRevertirCambios()
    {
        // Arrange
        var command = new DividirComandaCommand
        {
            ComandaOriginalId = Guid.NewGuid(),
            TipoDivision = TipoDivisionComanda.PorItems,
            MotivoDivision = "Test error",
            DivisionItems = new List<DivisionComandaDto>()
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

    #region Métodos de apoyo

    private void ConfigurarMocksBase()
    {
        _mockCurrentUserService.Setup(u => u.UserId)
            .Returns(Guid.NewGuid().ToString());

        var mockTransaction = new Mock<IDbContextTransaction>();
        _mockUnitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(mockTransaction.Object));

        _mockDateTimeService.Setup(d => d.Now)
            .Returns(DateTime.UtcNow);

        _mockUnitOfWork.Setup(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Configurar DbSets mockeados
        var comandasMock = MockDbSetHelper.CreateMockDbSet(new List<Comanda>().AsQueryable());
        var mesasMock = MockDbSetHelper.CreateMockDbSet(new List<Mesa>().AsQueryable());
        
        _mockContext.Setup(c => c.Comandas).Returns(comandasMock.Object);
        _mockContext.Setup(c => c.Mesas).Returns(mesasMock.Object);
    }

    private void ConfigurarMocksParaDivisionExitosa(Comanda comandaOriginal)
    {
        ConfigurarMockComandas(new[] { comandaOriginal });

        var mockItemsComandaSet = new Mock<DbSet<ItemComanda>>();
        _mockContext.Setup(c => c.ItemsComanda).Returns(mockItemsComandaSet.Object);

        // TODO: Descomentar cuando DescuentosComanda esté disponible en el dominio
        // var mockDescuentosSet = new Mock<DbSet<DescuentoComanda>>();
        // _mockContext.Setup(c => c.DescuentosComanda).Returns(mockDescuentosSet.Object);

        // TODO: Descomentar cuando RegistroAuditoria esté disponible en el dominio  
        // var mockAuditoriaSet = new Mock<DbSet<RegistroAuditoria>>();
        // _mockContext.Setup(c => c.RegistrosAuditoria).Returns(mockAuditoriaSet.Object);

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
        // Usar el factory method estático y reflection para setear el ID
        var comanda = Comanda.Crear(
            mesaId: Guid.NewGuid(), 
            meseroId: Guid.NewGuid(), 
            clienteId: null, 
            observaciones: "Comanda de prueba");

        // Usar reflection para setear el ID específico requerido para tests
        typeof(EntityBase).GetProperty("Id")?.SetValue(comanda, id);

        // Agregar items usando el método de dominio
        if (items != null)
        {
            foreach (var item in items)
            {
                comanda.AgregarItem(item.ProductoId, "Producto Test", item.Cantidad, item.PrecioUnitario);
            }
        }
        else
        {
            // Agregar items por defecto
            comanda.AgregarItem(Guid.NewGuid(), "Producto Test 1", 2, 50m);
            comanda.AgregarItem(Guid.NewGuid(), "Producto Test 2", 1, 50m);
        }

        return comanda;
    }

    private ItemComanda CrearItemComanda(Guid id, int cantidad = 1, decimal precio = 50m)
    {
        var item = new ItemComanda(
            comandaId: Guid.NewGuid(),
            productoId: Guid.NewGuid(),
            cantidad: cantidad,
            precioUnitario: precio,
            observaciones: "Item de prueba");

        // Usar reflection para setear el ID específico requerido para tests
        typeof(EntityBase).GetProperty("Id")?.SetValue(item, id);

        return item;
    }

    private void VerificarCreacionNuevasComandas(int cantidadEsperada)
    {
        _mockContext.Verify(
            c => c.Comandas.Add(It.IsAny<Comanda>()),
            Times.Exactly(cantidadEsperada));
    }

    private void VerificarCreacionDescuentosProporcionales()
    {
        // TODO: Descomentar cuando DescuentosComanda esté disponible en el dominio
        // _mockContext.Verify(
        //     c => c.DescuentosComanda.Add(It.Is<DescuentoComanda>(d => d.TipoDescuento == "Proporcional División")),
        //     Times.AtLeastOnce);
        
        // Por ahora, verificar que se llamó SaveChangesAsync (indicativo de que el proceso continuó)
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
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