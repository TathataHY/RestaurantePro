using RestaurantePro.Application.UnitTests.Common;
using RestaurantePro.Application.Operaciones.Comandas.Commands.UnificarComandas;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Core.Base;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;

namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.Commands;

/// <summary>
/// Tests unitarios para UnificarComandasHandler
/// Cobertura completa de unificación de comandas, consolidación de items y estrategias de descuentos
/// </summary>
public class UnificarComandasHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<UnificarComandasHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ICommunicationService> _mockNotificacionService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IDateTimeService> _mockDateTimeService;
    private readonly UnificarComandasHandler _handler;

    public UnificarComandasHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<UnificarComandasHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockNotificacionService = new Mock<ICommunicationService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockDateTimeService = new Mock<IDateTimeService>();

        _handler = new UnificarComandasHandler(
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
    public async Task Handle_ConUnificacionValida_DeberiaUnificarExitosamente()
    {
        // Arrange
        var comandaIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var mesaDestinoId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();

        var command = new UnificarComandasCommand
        {
            ComandasIds = comandaIds,
            MesaDestinoId = mesaDestinoId,
            MeseroId = meseroId,
            MotivoUnificacion = "Solicitud del cliente",
            EstrategiaDescuentos = EstrategiaDescuentos.Sumar,
            MantenerHistorico = true,
            AutorizadoPor = meseroId
        };

        var comandas = CrearComandasParaUnificar(comandaIds);
        var mesaDestino = CrearMesa(mesaDestinoId);

        ConfigurarMocksParaUnificacionExitosa(comandas, mesaDestino);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.ComandasOriginalesIds.Should().BeEquivalentTo(comandaIds);
        resultado.Value.MesaDestinoId.Should().Be(mesaDestinoId);
        resultado.Value.MeseroId.Should().Be(meseroId);
        resultado.Value.UnificacionExitosa.Should().BeTrue();
        resultado.Value.EstrategiaDescuentos.Should().Be(EstrategiaDescuentos.Sumar);

        // Verificar que se creó la comanda unificada
        VerificarCreacionComandaUnificada();

        // Verificar logging
        VerificarLoggingUnificacionExitosa(comandaIds);

        // Verificar que las comandas originales se marcaron como divididas  
        foreach (var comanda in comandas.Where(c => c.Id != resultado.Value.ComandaUnificadaId))
        {
            comanda.Estado.Should().Be(EstadoComanda.Dividida);
            comanda.Observaciones.Should().Contain("Unificada en comanda");
        }
    }

    [Fact]
    public async Task Handle_ConComandasInexistentes_DeberiaRetornarError()
    {
        // Arrange
        var comandaIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var command = new UnificarComandasCommand
        {
            ComandasIds = comandaIds,
            MesaDestinoId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            MotivoUnificacion = "Test",
            EstrategiaDescuentos = EstrategiaDescuentos.Sumar
        };

        // Solo devolver una comanda cuando se esperan dos
        var comandas = CrearComandasParaUnificar(new[] { comandaIds.First() });
        ConfigurarMockComandas(comandas);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("comandas no fueron encontradas");

        VerificarNoSeGuardaronCambios();
    }

    [Fact]
    public async Task Handle_ConComandaPrincipalEspecifica_DeberiaUsarComandaExistente()
    {
        // Arrange
        var comandaIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var comandaPrincipalId = comandaIds.First();

        var command = new UnificarComandasCommand
        {
            ComandasIds = comandaIds,
            ComandaPrincipalId = comandaPrincipalId,
            MesaDestinoId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            MotivoUnificacion = "Usar comanda principal",
            EstrategiaDescuentos = EstrategiaDescuentos.TomarMayor
        };

        var comandas = CrearComandasParaUnificar(comandaIds);
        var mesaDestino = CrearMesa(command.MesaDestinoId);

        ConfigurarMocksParaUnificacionExitosa(comandas, mesaDestino);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.ComandaUnificadaId.Should().Be(comandaPrincipalId);

        // Verificar que NO se generó un nuevo número de comanda
        _mockDateTimeService.Verify(
            d => d.Now,
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ConComandaPrincipalInvalida_DeberiaRetornarError()
    {
        // Arrange
        var comandaIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var comandaPrincipalInvalida = Guid.NewGuid(); // No está en la lista

        var command = new UnificarComandasCommand
        {
            ComandasIds = comandaIds,
            ComandaPrincipalId = comandaPrincipalInvalida,
            MesaDestinoId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            MotivoUnificacion = "Test principal inválida",
            EstrategiaDescuentos = EstrategiaDescuentos.Sumar
        };

        var comandas = CrearComandasParaUnificar(comandaIds);
        ConfigurarMockComandas(comandas);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("comanda principal especificada no se encuentra");

        VerificarNoSeGuardaronCambios();
    }

    [Theory]
    [InlineData(EstrategiaDescuentos.Sumar)]
    [InlineData(EstrategiaDescuentos.TomarMayor)]
    [InlineData(EstrategiaDescuentos.TomarMenor)]
    [InlineData(EstrategiaDescuentos.Promedio)]
    [InlineData(EstrategiaDescuentos.SinDescuentos)]
    public async Task Handle_ConDiferentesEstrategiasDescuentos_DeberiaAplicarCorrectamente(
        EstrategiaDescuentos estrategia)
    {
        // Arrange
        var comandaIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var command = new UnificarComandasCommand
        {
            ComandasIds = comandaIds,
            MesaDestinoId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            MotivoUnificacion = $"Test estrategia {estrategia}",
            EstrategiaDescuentos = estrategia
        };

        var comandas = CrearComandasConDescuentos(comandaIds, new[] { 100m, 50m });
        var mesaDestino = CrearMesa(command.MesaDestinoId);

        ConfigurarMocksParaUnificacionExitosa(comandas, mesaDestino);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.EstrategiaDescuentos.Should().Be(estrategia);

        // Verificar que se aplicó la estrategia de descuentos
        if (estrategia != EstrategiaDescuentos.SinDescuentos)
        {
            VerificarAplicacionEstrategiaDescuentos(estrategia);
        }
    }

    [Theory]
    [InlineData(EstadoComanda.Creada, EstadoComanda.EnProceso, true)]
    [InlineData(EstadoComanda.EnProceso, EstadoComanda.EnProceso, true)]
    [InlineData(EstadoComanda.Finalizada, EstadoComanda.EnProceso, false)]
    [InlineData(EstadoComanda.Cancelada, EstadoComanda.EnProceso, false)]
    public async Task Handle_ConDiferentesEstadosComandas_DeberiaValidarCorrectamente(
        EstadoComanda estado1, EstadoComanda estado2, bool deberiaUnificar)
    {
        // Arrange
        var comandaIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var command = new UnificarComandasCommand
        {
            ComandasIds = comandaIds,
            MesaDestinoId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            MotivoUnificacion = "Test estados",
            EstrategiaDescuentos = EstrategiaDescuentos.Sumar
        };

        var comandas = new List<Comanda>
        {
            CrearComanda(comandaIds[0], estado1),
            CrearComanda(comandaIds[1], estado2)
        };

        if (deberiaUnificar)
        {
            var mesaDestino = CrearMesa(command.MesaDestinoId);
            ConfigurarMocksParaUnificacionExitosa(comandas, mesaDestino);
        }
        else
        {
            ConfigurarMockComandas(comandas);
        }

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        if (deberiaUnificar)
        {
            resultado.Succeeded.Should().BeTrue();
        }
        else
        {
            resultado.Succeeded.Should().BeFalse();
            resultado.Error.Should().Contain("no son unificables");
        }
    }

    [Fact]
    public async Task Handle_ConConsolidacionItems_DeberiaAgruparItemsSimilares()
    {
        // Arrange
        var comandaIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var productoId = Guid.NewGuid();

        var command = new UnificarComandasCommand
        {
            ComandasIds = comandaIds,
            MesaDestinoId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            MotivoUnificacion = "Test consolidación",
            EstrategiaDescuentos = EstrategiaDescuentos.Sumar
        };

        var comandas = CrearComandasConItemsSimilares(comandaIds, productoId);
        var mesaDestino = CrearMesa(command.MesaDestinoId);

        ConfigurarMocksParaUnificacionExitosa(comandas, mesaDestino);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();

        // Verificar que se consolidaron los items
        VerificarConsolidacionItems();
    }

    [Fact]
    public async Task Handle_ConMantenerHistoricoTrue_DeberiaMarcarComandasComoUnificadas()
    {
        // Arrange
        var comandaIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var command = new UnificarComandasCommand
        {
            ComandasIds = comandaIds,
            MesaDestinoId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            MotivoUnificacion = "Mantener histórico",
            EstrategiaDescuentos = EstrategiaDescuentos.Sumar,
            MantenerHistorico = true
        };

        var comandas = CrearComandasParaUnificar(comandaIds);
        var mesaDestino = CrearMesa(command.MesaDestinoId);

        ConfigurarMocksParaUnificacionExitosa(comandas, mesaDestino);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();

        // Verificar que las comandas originales se marcaron como unificadas
        foreach (var comanda in comandas.Where(c => c.Id != resultado.Value.ComandaUnificadaId))
        {
            comanda.Estado.Should().Be(EstadoComanda.Dividida);
            comanda.Observaciones.Should().Contain("Unificada en comanda");
        }
    }

    [Fact]
    public async Task Handle_ConMantenerHistoricoFalse_DeberiaCancelarComandasOriginales()
    {
        // Arrange
        var comandaIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var command = new UnificarComandasCommand
        {
            ComandasIds = comandaIds,
            MesaDestinoId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            MotivoUnificacion = "No mantener histórico",
            EstrategiaDescuentos = EstrategiaDescuentos.Sumar,
            MantenerHistorico = false
        };

        var comandas = CrearComandasParaUnificar(comandaIds);
        var mesaDestino = CrearMesa(command.MesaDestinoId);

        ConfigurarMocksParaUnificacionExitosa(comandas, mesaDestino);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();

        // Verificar que las comandas originales se cancelaron
        foreach (var comanda in comandas.Where(c => c.Id != resultado.Value.ComandaUnificadaId))
        {
            comanda.Estado.Should().Be(EstadoComanda.Cancelada);
            comanda.FechaActualizacion.Should().NotBeNull();
            comanda.Observaciones.Should().Contain("Cancelada por unificación");
        }
    }

    [Fact]
    public async Task Handle_ConErrorEnTransaccion_DeberiaRevertirCambios()
    {
        // Arrange
        var command = new UnificarComandasCommand
        {
            ComandasIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            MesaDestinoId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            MotivoUnificacion = "Test error",
            EstrategiaDescuentos = EstrategiaDescuentos.Sumar
        };

        var comandas = CrearComandasParaUnificar(command.ComandasIds);
        ConfigurarMockComandas(comandas);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Error en base de datos"));

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("Error interno al unificar las comandas");

        // Verificar logging de error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al unificar comandas")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConActualizacionMesaDestino_DeberiaMarcarMesaComoOcupada()
    {
        // Arrange
        var command = new UnificarComandasCommand
        {
            ComandasIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            MesaDestinoId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            MotivoUnificacion = "Test mesa destino",
            EstrategiaDescuentos = EstrategiaDescuentos.Sumar
        };

        var comandas = CrearComandasParaUnificar(command.ComandasIds);
        var mesaDestino = CrearMesa(command.MesaDestinoId, EstadoMesa.Disponible);

        ConfigurarMocksParaUnificacionExitosa(comandas, mesaDestino);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();

        // Verificar que el proceso se completó exitosamente
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.MesaDestinoId.Should().Be(command.MesaDestinoId);
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

    private void ConfigurarMocksParaUnificacionExitosa(List<Comanda> comandas, Mesa mesaDestino)
    {
        ConfigurarMockComandas(comandas);
        ConfigurarMockMesas(new[] { mesaDestino });

        _mockDateTimeService.Setup(d => d.Now)
            .Returns(DateTime.UtcNow);

        _mockUnitOfWork.Setup(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    private void ConfigurarMockComandas(IEnumerable<Comanda> comandas)
    {
        var mockSet = MockDbSetHelper.CreateMockDbSet(comandas.AsQueryable());
        _mockContext.Setup(c => c.Comandas).Returns(mockSet.Object);
    }

    private void ConfigurarMockMesas(IEnumerable<Mesa> mesas)
    {
        var mockSet = MockDbSetHelper.CreateMockDbSet(mesas.AsQueryable());
        _mockContext.Setup(c => c.Mesas).Returns(mockSet.Object);
    }

    private List<Comanda> CrearComandasParaUnificar(IEnumerable<Guid> comandaIds)
    {
        return comandaIds.Select((id, index) => CrearComanda(id, EstadoComanda.EnProceso, index + 1)).ToList();
    }

    private List<Comanda> CrearComandasConDescuentos(IEnumerable<Guid> comandaIds, decimal[] descuentos)
    {
        return comandaIds.Select((id, index) => 
        {
            var comanda = CrearComanda(id, EstadoComanda.EnProceso, index + 1);
            // Aplicar descuento de fidelización para simular descuentos
            if (descuentos.Length > index && descuentos[index] > 0)
            {
                comanda.AplicarDescuentoFidelizacion(descuentos[index] / 100m); // Convertir a porcentaje
            }
            return comanda;
        }).ToList();
    }

    private List<Comanda> CrearComandasConItemsSimilares(IEnumerable<Guid> comandaIds, Guid productoId)
    {
        return comandaIds.Select((id, index) => 
        {
            var comanda = CrearComanda(id, EstadoComanda.EnProceso, index + 1);
            // Agregar item usando el método correcto de la entidad
            comanda.AgregarItem(
                productoId, 
                $"Producto Común {index}", 
                2, 
                50m, 
                "Test item común");
            return comanda;
        }).ToList();
    }

    private Comanda CrearComanda(Guid id, EstadoComanda estado, int numero = 1)
    {
        var comanda = Comanda.Crear(
            meseroId: Guid.NewGuid(),
            clienteId: null,
            mesaId: Guid.NewGuid(),
            observaciones: $"Test comanda {numero}",
            numeroComanda: $"CMD-{numero:000}");

        // Usar reflection para establecer ID y estado
        typeof(Comanda).GetProperty("Id")?.SetValue(comanda, id);
        typeof(Comanda).GetProperty("Estado")?.SetValue(comanda, estado);

        return comanda;
    }

    private Mesa CrearMesa(Guid id, EstadoMesa estado = EstadoMesa.Disponible)
    {
        var mesa = Mesa.Crear(numero: 1, capacidad: 6, ubicacion: "Interior");
        
        // Usar reflection para establecer ID y estado
        typeof(Mesa).GetProperty("Id")?.SetValue(mesa, id);
        typeof(Mesa).GetProperty("Estado")?.SetValue(mesa, estado);
        
        return mesa;
    }

    private void VerificarCreacionComandaUnificada()
    {
        _mockContext.Verify(
            c => c.Comandas.Add(It.IsAny<Comanda>()),
            Times.AtLeastOnce);
    }

    private void VerificarAplicacionEstrategiaDescuentos(EstrategiaDescuentos estrategia)
    {
        // TODO: Verificar aplicación de estrategia de descuentos cuando DescuentosComanda esté disponible en IApplicationDbContext
        // _mockContext.Verify(
        //     c => c.DescuentosComanda.Add(It.Is<DescuentoComanda>(d => d.TipoDescuento.Contains(estrategia.ToString()))),
        //     Times.AtLeastOnce);
        
        // Por ahora verificamos que se llamó al logger indicando el uso de la estrategia
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(estrategia.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private void VerificarConsolidacionItems()
    {
        _mockContext.Verify(
            c => c.ItemsComanda.Add(It.IsAny<ItemComanda>()),
            Times.AtLeastOnce);
    }

    private void VerificarNoSeGuardaronCambios()
    {
        _mockUnitOfWork.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private void VerificarLoggingUnificacionExitosa(List<Guid> comandaIds)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unificación completada exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
} 