using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerAnalisisInventario;
using RestaurantePro.Application.UnitTests.Common;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Domain.Inventario.Ingredientes;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;
using RestaurantePro.Domain.Core.SharedKernel;
using AutoMapper;

namespace RestaurantePro.Application.UnitTests.Inventario.Queries;

/// <summary>
/// Tests para ObtenerAnalisisInventarioHandler
/// Valida generación completa de análisis de inventario con métricas, alertas y recomendaciones
/// </summary>
public class ObtenerAnalisisInventarioHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<ObtenerAnalisisInventarioHandler>> _loggerMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private Mock<DbSet<Ingrediente>> _ingredientesDbSetMock;
    private Mock<DbSet<MovimientoInventario>> _movimientosDbSetMock;
    private readonly ObtenerAnalisisInventarioHandler _handler;

    public ObtenerAnalisisInventarioHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<ObtenerAnalisisInventarioHandler>>();
        _dateTimeServiceMock = new Mock<IDateTimeService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        // Mock para IInventarioServiceFacade necesario por el constructor real
        var inventarioServiceFacadeMock = new Mock<IInventarioServiceFacade>();

        // Setup date time service
        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);

        // Setup current user service
        _currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());

        // Constructor correcto con todos los 6 parámetros
        _handler = new ObtenerAnalisisInventarioHandler(
            _contextMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _dateTimeServiceMock.Object,
            inventarioServiceFacadeMock.Object,
            _currentUserServiceMock.Object);
    }

    #region Tests de Factory Methods del Query

    [Fact]
    public void CrearAnalisisDiario_ConParametrosValidos_DeberiaCrearQueryCorrectamente()
    {
        // Arrange
        var fecha = DateTime.Today.AddDays(-1);

        // Act
        var query = ObtenerAnalisisInventarioQuery.CrearAnalisisDiario(fecha, true, true);

        // Assert
        Assert.Equal(fecha, query.FechaInicio);
        Assert.Equal(fecha, query.FechaFin);
        Assert.True(query.IncluirTendencias);
        Assert.True(query.IncluirRecomendaciones);
        Assert.Equal("Completo", query.NivelDetalle);
    }

    [Fact]
    public void CrearAnalisisSemanal_ConParametros_DeberiaConfigurarRangoSemanal()
    {
        // Arrange
        var fechaInicio = DateTime.Today.AddDays(-7);

        // Act
        var query = ObtenerAnalisisInventarioQuery.CrearAnalisisSemanal(fechaInicio, "Completo");

        // Assert
        Assert.Equal(fechaInicio, query.FechaInicio);
        Assert.Equal(fechaInicio.AddDays(7), query.FechaFin);
        Assert.True(query.IncluirTendencias);
        Assert.True(query.IncluirRecomendaciones);
        Assert.Equal("Completo", query.NivelDetalle);
    }

    [Fact]
    public void CrearAnalisisCriticos_ConParametros_DeberiaEnfocarseEnCriticos()
    {
        // Arrange & Act
        var query = ObtenerAnalisisInventarioQuery.CrearAnalisisCriticos(null, true);

        // Assert
        Assert.True(query.SoloCriticos);
        Assert.True(query.SoloAlertaStock);
        Assert.Equal("Completo", query.NivelDetalle);
        Assert.True(query.IncluirTendencias);
    }

    #endregion

    #region Tests de Escenarios Exitosos

    [Fact]
    public async Task Handle_AnalisisCompletoConIA_DeberiaRetornarAnalisisInteligente()
    {
        // Arrange
        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaInicio = DateTime.Today.AddDays(-30),
            FechaFin = DateTime.Today,
            NivelDetalle = "Completo",
            IncluirTendencias = true,
            IncluirRecomendaciones = true,
            UsuarioId = Guid.NewGuid()
        };

        SetupMockData();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Value.ResumenExecutivo.TotalIngredientes.Should().BeGreaterThan(0);
        result.Value.AnalisisIngredientes.Should().NotBeEmpty();
        result.Value.AnalisisCategorias.Should().NotBeEmpty();
        result.Value.Alertas.Should().NotBeNull(); // Corregido: era AlertasInventario
        result.Value.Recomendaciones.Should().NotBeNull(); // Corregido: era RecomendacionesCompra
        result.Value.MetricasEficiencia.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_AnalisisConPrediccionesML_DeberiaIncluirPrediccionesIA()
    {
        // Arrange
        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaInicio = DateTime.Today.AddDays(-30),
            FechaFin = DateTime.Today,
            NivelDetalle = "Completo",
            IncluirTendencias = true,
            UsuarioId = Guid.NewGuid()
        };

        SetupMockData();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Value.Predicciones.Should().NotBeNull();
        result.Value.Recomendaciones.Should().NotBeEmpty(); // Corregido: era RecomendacionesCompra
    }

    [Fact]
    public async Task Handle_AnalisisIngredientesCriticos_DeberiaIdentificarCriticos()
    {
        // Arrange
        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaInicio = DateTime.Today,
            FechaFin = DateTime.Today,
            NivelDetalle = "Críticos",
            SoloCriticos = true,
            SoloAlertaStock = true,
            UsuarioId = Guid.NewGuid()
        };

        SetupMockDataCriticos();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Value.AnalisisIngredientes.Should().NotBeEmpty();
        result.Value.Alertas.Should().Contain(a => a.Prioridad == "Alta"); // Corregido: usamos string en lugar de enum incompatible
    }

    [Fact]
    public async Task Handle_AnalisisFinanciero_DeberiaIncluirCostosDetallados()
    {
        // Arrange
        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaInicio = DateTime.Today.AddDays(-30),
            FechaFin = DateTime.Today,
            NivelDetalle = "Financiero",
            IncluirRecomendaciones = true,
            UsuarioId = Guid.NewGuid()
        };

        SetupMockData();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Value.AnalisisFinanciero.Should().NotBeNull();
        result.Value.ResumenExecutivo.ValorTotalInventario.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_AnalisisBasico_DeberiaRetornarSoloEsencial()
    {
        // Arrange
        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaInicio = DateTime.Today,
            FechaFin = DateTime.Today,
            NivelDetalle = "Básico",
            IncluirTendencias = false,
            IncluirRecomendaciones = false,
            UsuarioId = Guid.NewGuid()
        };

        SetupMockData();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        if (!result.Succeeded)
        {
            // Log error for debugging
            throw new Exception($"Handler failed with error: {result.Error}");
        }
        result.Succeeded.Should().BeTrue();
        result.Value.ResumenExecutivo.Should().NotBeNull();
    }

    #endregion

    #region Tests de Validaciones de Negocio

    [Fact]
    public async Task Handle_FechasInvalidas_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaInicio = DateTime.Today,
            FechaFin = DateTime.Today.AddDays(-5),
            NivelDetalle = "Completo",
            UsuarioId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("fecha de inicio no puede ser posterior");
    }

    [Fact]
    public async Task Handle_RangoMuyAmplio_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaInicio = DateTime.Today.AddYears(-2),
            FechaFin = DateTime.Today,
            NivelDetalle = "Completo",
            UsuarioId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("período de análisis no puede exceder");
    }

    #endregion

    #region Helper Methods

    private void SetupMockData()
    {
        var ingredientes = new List<Ingrediente>
        {
            CreateMockIngrediente(Guid.NewGuid(), "Tomate", 100, 20, 150m),
            CreateMockIngrediente(Guid.NewGuid(), "Cebolla", 50, 10, 80m),
            CreateMockIngrediente(Guid.NewGuid(), "Aceite", 25, 5, 120m)
        };

        var movimientos = new List<MovimientoInventario>
        {
            CreateMockMovimiento(Guid.NewGuid(), ingredientes.First().Id, TipoMovimientoInventario.Ingreso, 50),
            CreateMockMovimiento(Guid.NewGuid(), ingredientes.First().Id, TipoMovimientoInventario.Egreso, 30)
        };

        // Usar MockDbSetHelper para configurar correctamente los mocks async
        _ingredientesDbSetMock = MockDbSetHelper.CreateMockDbSet(ingredientes.AsQueryable());
        _movimientosDbSetMock = MockDbSetHelper.CreateMockDbSet(movimientos.AsQueryable());

        _contextMock.Setup(c => c.Ingredientes).Returns(_ingredientesDbSetMock.Object);
        _contextMock.Setup(c => c.MovimientosInventario).Returns(_movimientosDbSetMock.Object);
    }

    private void SetupMockDataCriticos()
    {
        var ingredientesCriticos = new List<Ingrediente>
        {
            CreateMockIngrediente(Guid.NewGuid(), "Ingrediente Crítico", 5, 20, 200m) // Stock bajo
        };

        var movimientos = new List<MovimientoInventario>();

        // Usar MockDbSetHelper para configurar correctamente los mocks async
        _ingredientesDbSetMock = MockDbSetHelper.CreateMockDbSet(ingredientesCriticos.AsQueryable());
        _movimientosDbSetMock = MockDbSetHelper.CreateMockDbSet(movimientos.AsQueryable());

        _contextMock.Setup(c => c.Ingredientes).Returns(_ingredientesDbSetMock.Object);
        _contextMock.Setup(c => c.MovimientosInventario).Returns(_movimientosDbSetMock.Object);
    }

    private static Ingrediente CreateMockIngrediente(Guid id, string nombre, decimal stock, decimal stockMinimo, decimal costo)
    {
        var ingrediente = Ingrediente.Crear(
            id,
            nombre,
            $"COD-{nombre.ToUpper()}",
            $"Descripción de {nombre}",
            UnidadMedida.Kilogramo,
            stockMinimo,
            stock,
            RotacionIngrediente.Media,
            TemporadaIngrediente.TodoElAño);

        // Establecer el costo promedio usando reflection
        typeof(Ingrediente).GetProperty("CostoPromedio")?.SetValue(ingrediente, costo);

        return ingrediente;
    }

    private static MovimientoInventario CreateMockMovimiento(Guid id, Guid ingredienteId, TipoMovimientoInventario tipo, decimal cantidad)
    {
        // Usar métodos de factory correctos según el tipo
        MovimientoInventario movimiento;
        if (tipo == TipoMovimientoInventario.Ingreso || tipo == TipoMovimientoInventario.Entrada || tipo == TipoMovimientoInventario.Incremento)
        {
            movimiento = MovimientoInventario.CrearIngreso(
                ingredienteId,
                cantidad,
                "Movimiento de prueba");
        }
        else
        {
            movimiento = MovimientoInventario.CrearEgreso(
                ingredienteId,
                cantidad,
                "Movimiento de prueba");
        }

        typeof(EntityBase).GetProperty("Id")?.SetValue(movimiento, id);
        return movimiento;
    }

    #endregion
} 