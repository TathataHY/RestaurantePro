using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientesBajoStock;
using RestaurantePro.Application.Inventario.Ingredientes.DTOs;

namespace RestaurantePro.Application.UnitTests.Inventario.Ingredientes.Queries;

public class ObtenerIngredientesBajoStockHandlerTests
{
    private readonly Mock<IIngredienteRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ObtenerIngredientesBajoStockHandler>> _mockLogger;
    private readonly ObtenerIngredientesBajoStockHandler _handler;
    private readonly List<Ingrediente> _ingredientesBase;

    public ObtenerIngredientesBajoStockHandlerTests()
    {
        _mockRepository = new Mock<IIngredienteRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ObtenerIngredientesBajoStockHandler>>();
        _handler = new ObtenerIngredientesBajoStockHandler(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);

        // Dataset de prueba con diferentes niveles de stock
        _ingredientesBase = CrearIngredientesConDiferentesStocks();
        
        ConfigurarMockMapper();
    }

    [Fact]
    public async Task Handle_ConParametrosBasicos_DeberiaRetornarIngredientesBajoStock()
    {
        // Arrange
        var query = ObtenerIngredientesBajoStockQuery.CrearParaAlertasCriticas(); // 25% crítico
        
        _mockRepository.Setup(r => r.ObtenerConStockBajoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_ingredientesBase.Where(i => i.Stock < i.StockMinimo));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().NotBeEmpty();

        // Solo ingredientes con stock < 25% del mínimo deberían estar incluidos
        foreach (var dto in result.Value)
        {
            var porcentaje = (dto.StockActual / dto.StockMinimo) * 100;
            porcentaje.Should().BeLessThan(25);
        }

        _mockRepository.Verify(r => r.ObtenerConStockBajoAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConPorcentajeCritico50_DeberiaIncluirMasIngredientes()
    {
        // Arrange
        var query = ObtenerIngredientesBajoStockQuery.CrearParaReporteReposicion(); // 50% crítico
        
        _mockRepository.Setup(r => r.ObtenerConStockBajoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_ingredientesBase.Where(i => i.Stock < i.StockMinimo));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Con 50% crítico debería incluir más ingredientes que con 25%
        var query25 = ObtenerIngredientesBajoStockQuery.CrearParaAlertasCriticas();
        var result25 = await _handler.Handle(query25, CancellationToken.None);

        result.Value.Count.Should().BeGreaterThanOrEqualTo(result25.Value.Count);
    }

    [Fact]
    public async Task Handle_ConFiltroRotacionAlta_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        var query = ObtenerIngredientesBajoStockQuery.CrearParaAltaRotacion();
        
        _mockRepository.Setup(r => r.ObtenerConStockBajoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_ingredientesBase.Where(i => i.Stock < i.StockMinimo));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        // Verificar que se pasó al mapper solo ingredientes de alta rotación bajo stock
        _mockMapper.Verify(m => m.Map<List<IngredienteSummaryDto>>(
            It.Is<IEnumerable<Ingrediente>>(list => 
                list.All(i => i.Rotacion == RotacionIngrediente.Alta &&
                         (i.Stock / i.StockMinimo) * 100 < 30))), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConLimiteResultados5_DeberiaRespetarLimite()
    {
        // Arrange
        var query = new ObtenerIngredientesBajoStockQuery
        {
            LimiteResultados = 5,
            PorcentajeCritico = 80 // Muy alto para obtener muchos resultados
        };
        
        _mockRepository.Setup(r => r.ObtenerConStockBajoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_ingredientesBase.Where(i => i.Stock < i.StockMinimo));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Count.Should().BeLessThan(6);
    }

    [Fact]
    public async Task Handle_ConFactoryMethodCrearParaDashboard_DeberiaConfigurarCorrectamente()
    {
        // Arrange
        var query = ObtenerIngredientesBajoStockQuery.CrearParaDashboard();
        
        _mockRepository.Setup(r => r.ObtenerConStockBajoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_ingredientesBase.Where(i => i.Stock < i.StockMinimo));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        // Verificar configuración del factory method
        query.PorcentajeCritico.Should().Be(40);
        query.SoloActivos.Should().BeTrue();
        query.LimiteResultados.Should().Be(30);
        query.OrdenarPorPrioridad.Should().BeTrue();
        
        result.Value.Count.Should().BeLessThan(31);
    }

    [Fact]
    public async Task Handle_ConFiltrosMultiples_DeberiaAplicarTodos()
    {
        // Arrange
        var query = new ObtenerIngredientesBajoStockQuery
        {
            PorcentajeCritico = 40,
            FiltroRotacion = "Alta",
            FiltroTemporada = "Verano",
            IncluirBloqueados = false,
            SoloActivos = true,
            LimiteResultados = 10
        };
        
        _mockRepository.Setup(r => r.ObtenerConStockBajoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_ingredientesBase.Where(i => i.Stock < i.StockMinimo));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        // Verificar que se aplicaron múltiples filtros
        _mockMapper.Verify(m => m.Map<List<IngredienteSummaryDto>>(
            It.Is<IEnumerable<Ingrediente>>(list => 
                list.All(i => 
                    i.Rotacion == RotacionIngrediente.Alta &&
                    i.Temporada == TemporadaIngrediente.Verano &&
                    !i.BloqueadoControlCalidad &&
                    i.EstaActivo &&
                    (i.Stock / i.StockMinimo) * 100 < 40))), 
            Times.Once);
        
        result.Value.Count.Should().BeLessThan(11);
    }

    [Fact]
    public async Task Handle_ConExcepcionEnRepositorio_DeberiaRetornarError()
    {
        // Arrange
        var query = ObtenerIngredientesBajoStockQuery.CrearParaAlertasCriticas();
        
        _mockRepository.Setup(r => r.ObtenerConStockBajoAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno al obtener ingredientes bajo stock");
    }

    [Fact]
    public async Task Handle_SinIngredientesBajoStock_DeberiaRetornarListaVacia()
    {
        // Arrange
        var query = ObtenerIngredientesBajoStockQuery.CrearParaAlertasCriticas();
        
        _mockRepository.Setup(r => r.ObtenerConStockBajoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Ingrediente>()); // Sin ingredientes bajo stock

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ConOrdenamientoPorPrioridad_DeberiaOrdenarCorrectamente()
    {
        // Arrange
        var query = new ObtenerIngredientesBajoStockQuery
        {
            OrdenarPorPrioridad = true,
            PorcentajeCritico = 60
        };
        
        _mockRepository.Setup(r => r.ObtenerConStockBajoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_ingredientesBase.Where(i => i.Stock < i.StockMinimo));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        // Con el comportamiento actual (placeholder), debería ordenar por nombre
        result.Value.Should().BeInAscendingOrder(i => i.Nombre);
    }

    [Fact]
    public async Task Handle_ConStockMinimoZero_DeberiaExcluirIngrediente()
    {
        // Arrange
        var query = new ObtenerIngredientesBajoStockQuery { PorcentajeCritico = 50 };
        
        var ingredienteSinStockMinimo = CrearIngredienteMock("Sin Mínimo", 5m, 0m, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño);
        var ingredientesConStockCero = new List<Ingrediente> { ingredienteSinStockMinimo };
        
        _mockRepository.Setup(r => r.ObtenerConStockBajoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingredientesConStockCero);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeEmpty(); // No debería incluir ingredientes con stock mínimo = 0
    }

    // Métodos auxiliares
    private List<Ingrediente> CrearIngredientesConDiferentesStocks()
    {
        return new List<Ingrediente>
        {
            // Stock crítico (< 25% del mínimo)
            CrearIngredienteMock("Tomate Crítico", 2m, 10m, RotacionIngrediente.Alta, TemporadaIngrediente.Verano), // 20%
            CrearIngredienteMock("Lechuga Crítica", 1m, 5m, RotacionIngrediente.Alta, TemporadaIngrediente.TodoElAño), // 20%
            
            // Stock bajo (25-50% del mínimo)
            CrearIngredienteMock("Cebolla Baja", 3m, 8m, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño), // 37.5%
            CrearIngredienteMock("Zanahoria Baja", 4m, 10m, RotacionIngrediente.Media, TemporadaIngrediente.Invierno), // 40%
            
            // Stock medio (50-75% del mínimo)
            CrearIngredienteMock("Aceite Medio", 6m, 10m, RotacionIngrediente.Baja, TemporadaIngrediente.TodoElAño), // 60%
            CrearIngredienteMock("Sal Media", 7m, 10m, RotacionIngrediente.Baja, TemporadaIngrediente.TodoElAño), // 70%
            
            // Stock normal (> stock mínimo)
            CrearIngredienteMock("Pan Normal", 15m, 10m, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño), // 150%
            CrearIngredienteMock("Queso Normal", 20m, 8m, RotacionIngrediente.Alta, TemporadaIngrediente.TodoElAño) // 250%
        };
    }

    private Ingrediente CrearIngredienteMock(
        string nombre, 
        decimal stock, 
        decimal stockMinimo, 
        RotacionIngrediente rotacion,
        TemporadaIngrediente temporada)
    {
        return Ingrediente.Crear(
            Guid.NewGuid(),
            nombre,
            $"COD-{nombre.Replace(" ", "").ToUpper()}",
            $"Descripción de {nombre}",
            UnidadMedida.Kilogramo,
            stockMinimo,
            stock,
            rotacion,
            temporada);
    }

    private void ConfigurarMockMapper()
    {
        _mockMapper.Setup(m => m.Map<List<IngredienteSummaryDto>>(It.IsAny<IEnumerable<Ingrediente>>()))
            .Returns((IEnumerable<Ingrediente> ingredientes) =>
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
} 