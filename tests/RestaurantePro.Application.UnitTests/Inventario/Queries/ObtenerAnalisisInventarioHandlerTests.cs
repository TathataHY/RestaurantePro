using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerAnalisisInventario;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Application.UnitTests.Inventario.Queries;

/// <summary>
/// Tests para ObtenerAnalisisInventarioQueryHandler
/// Valida la lógica de obtención de análisis de inventario
/// </summary>
public class ObtenerAnalisisInventarioHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly ObtenerAnalisisInventarioQueryHandler _handler;

    public ObtenerAnalisisInventarioHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _dateTimeServiceMock = new Mock<IDateTimeService>();
        _handler = new ObtenerAnalisisInventarioQueryHandler(
            _contextMock.Object,
            _dateTimeServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_DebeRetornarAnalisisExitoso()
    {
        // Arrange
        var fechaDesde = DateTime.Today.AddDays(-30);
        var fechaHasta = DateTime.Today;
        var usuarioId = Guid.NewGuid();
        var nivelDetalle = "Completo";

        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            UsuarioId = usuarioId,
            NivelDetalle = nivelDetalle,
            Categorias = new List<string> { "Granos", "Verduras" },
            IncluirTendencias = true,
            IncluirRecomendaciones = true
        };

        // Configurar mocks
        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_DebeRetornarAnalisisConDatosReales()
    {
        // Arrange
        var fechaDesde = DateTime.Today.AddDays(-30);
        var fechaHasta = DateTime.Today;
        var usuarioId = Guid.NewGuid();

        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            UsuarioId = usuarioId,
            NivelDetalle = "Completo"
        };

        // Crear ingredientes de prueba
        var ingrediente1 = Ingrediente.Crear("Arroz", "ARZ-01", "Arroz blanco", UnidadMedida.Kilogramo, 10, 50);
        var ingrediente2 = Ingrediente.Crear("Frijol", "FRJ-01", "Frijol negro", UnidadMedida.Kilogramo, 5, 20);
        var ingrediente3 = Ingrediente.Crear("Aceite", "ACE-01", "Aceite de oliva", UnidadMedida.Litro, 2, 8);

        // Crear movimientos de prueba
        var movimiento1 = MovimientoInventario.CrearIngreso(ingrediente1.Id, 10, "Compra inicial");
        var movimiento2 = MovimientoInventario.CrearEgreso(ingrediente1.Id, 5, "Uso en cocina");
        var movimiento3 = MovimientoInventario.CrearIngreso(ingrediente2.Id, 15, "Compra inicial");

        var ingredientes = new List<Ingrediente> { ingrediente1, ingrediente2, ingrediente3 };
        var movimientos = new List<MovimientoInventario> { movimiento1, movimiento2, movimiento3 };

        // Configurar mocks
        _contextMock.Setup(x => x.Ingredientes).Returns(MockDbSetHelper.CreateMockDbSet(ingredientes.AsQueryable()));
        _contextMock.Setup(x => x.MovimientosInventario).Returns(MockDbSetHelper.CreateMockDbSet(movimientos.AsQueryable()));
        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.FechaGeneracion.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_DebeRetornarAnalisisSinDatos()
    {
        // Arrange
        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaDesde = DateTime.Today.AddDays(-30),
            FechaHasta = DateTime.Today,
            UsuarioId = Guid.NewGuid(),
            NivelDetalle = "Completo"
        };

        // Configurar mocks con datos vacíos
        _contextMock.Setup(x => x.Ingredientes).Returns(MockDbSetHelper.CreateMockDbSet(new List<Ingrediente>().AsQueryable()));
        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.FechaGeneracion.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_DebeRetornarAnalisisConFiltros()
    {
        // Arrange
        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaDesde = DateTime.Today.AddDays(-7),
            FechaHasta = DateTime.Today,
            UsuarioId = Guid.NewGuid(),
            NivelDetalle = "Básico",
            Categorias = new List<string> { "Granos" }
        };

        // Crear ingredientes de prueba
        var ingrediente1 = Ingrediente.Crear("Arroz", "ARZ-01", "Arroz blanco", UnidadMedida.Kilogramo, 10, 50);
        var ingrediente2 = Ingrediente.Crear("Frijol", "FRJ-01", "Frijol negro", UnidadMedida.Kilogramo, 5, 20);

        var ingredientes = new List<Ingrediente> { ingrediente1, ingrediente2 };

        // Configurar mocks
        _contextMock.Setup(x => x.Ingredientes).Returns(MockDbSetHelper.CreateMockDbSet(ingredientes.AsQueryable()));
        _contextMock.Setup(x => x.MovimientosInventario).Returns(MockDbSetHelper.CreateMockDbSet(new List<MovimientoInventario>().AsQueryable()));
        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_DebeRetornarAnalisisConTendencias()
    {
        // Arrange
        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaDesde = DateTime.Today.AddDays(-30),
            FechaHasta = DateTime.Today,
            UsuarioId = Guid.NewGuid(),
            NivelDetalle = "Completo",
            IncluirTendencias = true
        };

        // Crear ingredientes y movimientos de prueba
        var ingrediente1 = Ingrediente.Crear("Arroz", "ARZ-01", "Arroz blanco", UnidadMedida.Kilogramo, 10, 50);
        var movimiento1 = MovimientoInventario.CrearIngreso(ingrediente1.Id, 10, "Compra inicial");
        var movimiento2 = MovimientoInventario.CrearEgreso(ingrediente1.Id, 5, "Uso en cocina");

        var ingredientes = new List<Ingrediente> { ingrediente1 };
        var movimientos = new List<MovimientoInventario> { movimiento1, movimiento2 };

        // Configurar mocks
        _contextMock.Setup(x => x.Ingredientes).Returns(MockDbSetHelper.CreateMockDbSet(ingredientes.AsQueryable()));
        _contextMock.Setup(x => x.MovimientosInventario).Returns(MockDbSetHelper.CreateMockDbSet(movimientos.AsQueryable()));
        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_DebeRetornarAnalisisConRecomendaciones()
    {
        // Arrange
        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaDesde = DateTime.Today.AddDays(-30),
            FechaHasta = DateTime.Today,
            UsuarioId = Guid.NewGuid(),
            NivelDetalle = "Completo",
            IncluirRecomendaciones = true
        };

        // Crear ingredientes de prueba con stock bajo
        var ingrediente1 = Ingrediente.Crear("Arroz", "ARZ-01", "Arroz blanco", UnidadMedida.Kilogramo, 10, 5); // Stock bajo
        var ingrediente2 = Ingrediente.Crear("Frijol", "FRJ-01", "Frijol negro", UnidadMedida.Kilogramo, 5, 20);

        var ingredientes = new List<Ingrediente> { ingrediente1, ingrediente2 };

        // Configurar mocks
        _contextMock.Setup(x => x.Ingredientes).Returns(MockDbSetHelper.CreateMockDbSet(ingredientes.AsQueryable()));
        _contextMock.Setup(x => x.MovimientosInventario).Returns(MockDbSetHelper.CreateMockDbSet(new List<MovimientoInventario>().AsQueryable()));
        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_DebeRetornarAnalisisFinanciero()
    {
        // Arrange
        var query = new ObtenerAnalisisInventarioQuery
        {
            FechaDesde = DateTime.Today.AddDays(-30),
            FechaHasta = DateTime.Today,
            UsuarioId = Guid.NewGuid(),
            NivelDetalle = "Financiero"
        };

        // Crear ingredientes de prueba con costos
        var ingrediente1 = Ingrediente.Crear("Arroz", "ARZ-01", "Arroz blanco", UnidadMedida.Kilogramo, 10, 50);
        ingrediente1.ActualizarCostoPromedio(2.5m);
        
        var ingrediente2 = Ingrediente.Crear("Frijol", "FRJ-01", "Frijol negro", UnidadMedida.Kilogramo, 5, 20);
        ingrediente2.ActualizarCostoPromedio(3.0m);

        var ingredientes = new List<Ingrediente> { ingrediente1, ingrediente2 };

        // Configurar mocks
        _contextMock.Setup(x => x.Ingredientes).Returns(MockDbSetHelper.CreateMockDbSet(ingredientes.AsQueryable()));
        _contextMock.Setup(x => x.MovimientosInventario).Returns(MockDbSetHelper.CreateMockDbSet(new List<MovimientoInventario>().AsQueryable()));
        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
} 