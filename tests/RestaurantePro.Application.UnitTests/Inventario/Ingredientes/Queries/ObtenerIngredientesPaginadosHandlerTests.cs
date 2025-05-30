using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientesPaginados;
using RestaurantePro.Application.Inventario.Ingredientes.DTOs;
using RestaurantePro.Application.Common.DTOs;

namespace RestaurantePro.Application.UnitTests.Inventario.Ingredientes.Queries;

public class ObtenerIngredientesPaginadosHandlerTests
{
    private readonly Mock<IIngredienteRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ObtenerIngredientesPaginadosHandler>> _mockLogger;
    private readonly ObtenerIngredientesPaginadosHandler _handler;
    private readonly List<Ingrediente> _ingredientesBase;

    public ObtenerIngredientesPaginadosHandlerTests()
    {
        _mockRepository = new Mock<IIngredienteRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ObtenerIngredientesPaginadosHandler>>();
        _handler = new ObtenerIngredientesPaginadosHandler(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);

        // Dataset de prueba diverso
        _ingredientesBase = CrearIngredientesBasePrueba();
        
        ConfigurarMockMapper();
    }

    [Fact]
    public async Task Handle_ConParametrosBasicos_DeberiaRetornarPaginaValida()
    {
        // Arrange
        var query = ObtenerIngredientesPaginadosQuery.Crear(pageNumber: 1, pageSize: 5);
        
        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_ingredientesBase);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(5);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(5);
        result.Value.TotalCount.Should().Be(6); // Solo activos por defecto, excluye "Pan Viejo" inactivo
        result.Value.TotalPages.Should().Be((int)Math.Ceiling(6 / 5.0));

        _mockRepository.Verify(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConFiltroTexto_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        var query = ObtenerIngredientesPaginadosQuery.CrearConFiltro("tomate");
        
        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_ingredientesBase);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1); // Solo "Tomate" debería coincidir
        
        _mockMapper.Verify(m => m.Map<List<IngredienteSummaryDto>>(
            It.Is<List<Ingrediente>>(list => list.Count == 1 && 
                                          list.First().Nombre.ToLower().Contains("tomate"))), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConSoloBajoStock_DeberiaFiltrarIngredientesBajoMinimo()
    {
        // Arrange
        var query = ObtenerIngredientesPaginadosQuery.CrearParaBajoStock();
        
        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_ingredientesBase);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        // Deberían aparecer solo ingredientes con stock < stock mínimo
        _mockMapper.Verify(m => m.Map<List<IngredienteSummaryDto>>(
            It.Is<List<Ingrediente>>(list => list.All(i => i.Stock < i.StockMinimo))), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConFiltroRotacion_DeberiaFiltrarPorRotacion()
    {
        // Arrange
        var query = ObtenerIngredientesPaginadosQuery.CrearParaAltaRotacion();
        
        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_ingredientesBase);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        _mockMapper.Verify(m => m.Map<List<IngredienteSummaryDto>>(
            It.Is<List<Ingrediente>>(list => list.All(i => i.Rotacion == RotacionIngrediente.Alta))), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConSoloConStock_DeberiaFiltrarIngredientesConStock()
    {
        // Arrange
        var query = new ObtenerIngredientesPaginadosQuery { SoloConStock = true };
        
        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_ingredientesBase);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        _mockMapper.Verify(m => m.Map<List<IngredienteSummaryDto>>(
            It.Is<List<Ingrediente>>(list => list.All(i => i.Stock > 0))), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConOrdenamientoPorNombre_DeberiaOrdenarCorrectamente()
    {
        // Arrange
        var query = new ObtenerIngredientesPaginadosQuery 
        { 
            OrdenarPor = "nombre", 
            DireccionOrden = "Asc" 
        };
        
        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_ingredientesBase);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        // Verificar que se pasó una lista ordenada al mapper
        _mockMapper.Verify(m => m.Map<List<IngredienteSummaryDto>>(
            It.Is<List<Ingrediente>>(list => EstaOrdenadoPorNombreAsc(list))), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConOrdenamientoPorStockDesc_DeberiaOrdenarPorStockDescendente()
    {
        // Arrange
        var query = new ObtenerIngredientesPaginadosQuery 
        { 
            OrdenarPor = "stockactual", 
            DireccionOrden = "Desc" 
        };
        
        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_ingredientesBase);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        _mockMapper.Verify(m => m.Map<List<IngredienteSummaryDto>>(
            It.Is<List<Ingrediente>>(list => EstaOrdenadoPorStockDesc(list))), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConPaginaVacia_DeberiaRetornarListaVacia()
    {
        // Arrange
        var query = new ObtenerIngredientesPaginadosQuery { PageNumber = 10, PageSize = 5 }; // Página que no existe
        
        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_ingredientesBase);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(6); // Solo activos por defecto, excluye "Pan Viejo" inactivo
        result.Value.PageNumber.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ConFiltrosMultiples_DeberiaAplicarTodosFiltros()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();
        var query = new ObtenerIngredientesPaginadosQuery
        {
            FiltroTexto = "a",
            SoloActivos = true,
            SoloConStock = true,
            StockMinimo = 5m,
            CostoMinimo = 1000m,
            ProveedorPrincipalId = proveedorId
        };
        
        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_ingredientesBase);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        // Verificar que se aplicaron múltiples filtros
        _mockMapper.Verify(m => m.Map<List<IngredienteSummaryDto>>(
            It.Is<List<Ingrediente>>(list => list.All(i => 
                i.Nombre.ToLower().Contains("a") &&
                i.EstaActivo &&
                i.Stock > 0 &&
                i.Stock >= 5m &&
                i.CostoPromedio >= 1000m))), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConIngredientesInactivos_DeberiaExcluirlosPorDefecto()
    {
        // Arrange
        var query = new ObtenerIngredientesPaginadosQuery(); // SoloActivos = true por defecto
        
        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_ingredientesBase);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        _mockMapper.Verify(m => m.Map<List<IngredienteSummaryDto>>(
            It.Is<List<Ingrediente>>(list => list.All(i => i.EstaActivo))), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConErrorEnRepositorio_DeberiaRetornarError()
    {
        // Arrange
        var query = ObtenerIngredientesPaginadosQuery.Crear();
        
        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno al obtener ingredientes");
    }

    [Fact]
    public async Task Handle_ConListaVacia_DeberiaRetornarPaginacionVacia()
    {
        // Arrange
        var query = ObtenerIngredientesPaginadosQuery.Crear();
        
        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Ingrediente>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ConRangosDeCosto_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        var query = new ObtenerIngredientesPaginadosQuery
        {
            CostoMinimo = 1500m,
            CostoMaximo = 2500m
        };
        
        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_ingredientesBase);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        _mockMapper.Verify(m => m.Map<List<IngredienteSummaryDto>>(
            It.Is<List<Ingrediente>>(list => list.All(i => 
                i.CostoPromedio >= 1500m && i.CostoPromedio <= 2500m))), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConFiltroTemporada_DeberiaFiltrarPorTemporada()
    {
        // Arrange
        var query = new ObtenerIngredientesPaginadosQuery
        {
            FiltroTemporada = "Verano"
        };
        
        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_ingredientesBase);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        _mockMapper.Verify(m => m.Map<List<IngredienteSummaryDto>>(
            It.Is<List<Ingrediente>>(list => list.All(i => i.Temporada == TemporadaIngrediente.Verano))), 
            Times.Once);
    }

    // Métodos auxiliares
    private List<Ingrediente> CrearIngredientesBasePrueba()
    {
        return new List<Ingrediente>
        {
            CrearIngredienteMock("Tomate", "TOM001", 10m, 15m, 2000m, RotacionIngrediente.Alta, TemporadaIngrediente.Verano, true),
            CrearIngredienteMock("Lechuga", "LEC001", 3m, 5m, 1500m, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño, true),
            CrearIngredienteMock("Carne", "CAR001", 0m, 2m, 5000m, RotacionIngrediente.Alta, TemporadaIngrediente.TodoElAño, true),
            CrearIngredienteMock("Aceite", "ACE001", 50m, 20m, 800m, RotacionIngrediente.Baja, TemporadaIngrediente.TodoElAño, true),
            CrearIngredienteMock("Pan Viejo", "PAN001", 5m, 10m, 1200m, RotacionIngrediente.Media, TemporadaIngrediente.Invierno, false),
            CrearIngredienteMock("Albahaca", "ALB001", 8m, 5m, 3000m, RotacionIngrediente.Alta, TemporadaIngrediente.Verano, true),
            CrearIngredienteMock("Queso", "QUE001", 15m, 8m, 4500m, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño, true)
        };
    }

    private Ingrediente CrearIngredienteMock(
        string nombre, 
        string codigo, 
        decimal stock, 
        decimal stockMinimo, 
        decimal costo,
        RotacionIngrediente rotacion,
        TemporadaIngrediente temporada,
        bool activo)
    {
        var ingrediente = Ingrediente.Crear(
            Guid.NewGuid(),
            nombre,
            codigo,
            "Descripción de " + nombre,
            UnidadMedida.Kilogramo,
            stockMinimo,
            stock,
            rotacion,
            temporada);

        if (!activo)
        {
            ingrediente.Desactivar();
        }

        return ingrediente;
    }

    private void ConfigurarMockMapper()
    {
        _mockMapper.Setup(m => m.Map<List<IngredienteSummaryDto>>(It.IsAny<List<Ingrediente>>()))
            .Returns((List<Ingrediente> ingredientes) =>
                ingredientes.Select(i => new IngredienteSummaryDto
                {
                    Id = i.Id,
                    Nombre = i.Nombre,
                    Codigo = i.Codigo,
                    StockActual = i.Stock,
                    StockMinimo = i.StockMinimo,
                    CostoUnitario = i.CostoPromedio,
                    Activo = i.EstaActivo
                }).ToList());
    }

    private bool EstaOrdenadoPorNombreAsc(List<Ingrediente> lista)
    {
        for (int i = 1; i < lista.Count; i++)
        {
            if (string.Compare(lista[i - 1].Nombre, lista[i].Nombre, StringComparison.OrdinalIgnoreCase) > 0)
                return false;
        }
        return true;
    }

    private bool EstaOrdenadoPorStockDesc(List<Ingrediente> lista)
    {
        for (int i = 1; i < lista.Count; i++)
        {
            if (lista[i - 1].Stock < lista[i].Stock)
                return false;
        }
        return true;
    }
} 