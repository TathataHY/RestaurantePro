using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Inventory;
using RestaurantePro.Mobile.IntegrationTests.TestBase;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services;

/// <summary>
/// Tests de integración para IngredientesService
/// </summary>
public class IngredientesServiceIntegrationTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly IIngredientesService _ingredientesService;
    private readonly ILogger<IngredientesServiceIntegrationTests> _logger;

    public IngredientesServiceIntegrationTests(MobileIntegrationTestFixture fixture)
    {
        using var scope = fixture.Services.CreateScope();
        _ingredientesService = scope.ServiceProvider.GetRequiredService<IIngredientesService>();
        _logger = scope.ServiceProvider.GetRequiredService<ILogger<IngredientesServiceIntegrationTests>>();
    }

    #region Tests de Funcionalidad Básica

    [Fact]
    public async Task ObtenerIngredientes_WithDefaultParameters_ShouldReturnIngredientes()
    {
        // Act
        var response = await _ingredientesService.ObtenerIngredientesAsync();

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.NotEmpty(response.Data);
        _logger.LogInformation("Ingredientes obtenidos: {Count}", response.Data.Count);
    }

    [Fact]
    public async Task ObtenerIngredientes_WithSoloActivosTrue_ShouldReturnOnlyActiveIngredientes()
    {
        // Act
        var response = await _ingredientesService.ObtenerIngredientesAsync(soloActivos: true);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.All(response.Data, ingrediente => Assert.True(ingrediente.Disponible));
        _logger.LogInformation("Ingredientes activos obtenidos: {Count}", response.Data.Count);
    }

    [Fact]
    public async Task BuscarIngredientes_WithValidTerm_ShouldReturnMatchingIngredientes()
    {
        // Arrange
        var terminoBusqueda = "Tomate";

        // Act
        var response = await _ingredientesService.BuscarIngredientesAsync(terminoBusqueda);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        _logger.LogInformation("Búsqueda con término '{Termino}': {Count} resultados", terminoBusqueda, response.Data.Count);
    }

    [Fact]
    public async Task BuscarIngredientes_WithEmptyTerm_ShouldReturnAllIngredientes()
    {
        // Arrange
        var terminoBusqueda = "";

        // Act
        var response = await _ingredientesService.BuscarIngredientesAsync(terminoBusqueda);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        _logger.LogInformation("Búsqueda con término vacío: {Count} resultados", response.Data.Count);
    }

    [Fact]
    public async Task ObtenerIngredientesBajoStock_WithDefaultStockMinimo_ShouldReturnLowStockIngredientes()
    {
        // Act
        var response = await _ingredientesService.ObtenerIngredientesBajoStockAsync();

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        _logger.LogInformation("Ingredientes bajo stock obtenidos: {Count}", response.Data.Count);
    }

    [Fact]
    public async Task ObtenerIngredientesBajoStock_WithCustomStockMinimo_ShouldReturnFilteredIngredientes()
    {
        // Arrange
        var stockMinimo = 5;

        // Act
        var response = await _ingredientesService.ObtenerIngredientesBajoStockAsync(stockMinimo);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.All(response.Data, ingrediente => Assert.True(ingrediente.StockActual <= stockMinimo));
        _logger.LogInformation("Ingredientes bajo stock {StockMinimo}: {Count} resultados", stockMinimo, response.Data.Count);
    }

    [Fact]
    public async Task ObtenerIngrediente_WithValidId_ShouldReturnIngrediente()
    {
        // Arrange - Usar un ID que sabemos que existe en el mock
        var ingredienteId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // Act
        var response = await _ingredientesService.ObtenerIngredienteAsync(ingredienteId);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(ingredienteId, response.Data.Id);
        _logger.LogInformation("Ingrediente {IngredienteId} obtenido exitosamente", ingredienteId);
    }

    [Fact]
    public async Task ObtenerEstadisticas_ShouldReturnValidEstadisticas()
    {
        // Act
        var response = await _ingredientesService.ObtenerEstadisticasAsync();

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.True(response.Data.TotalIngredientes >= 0);
        Assert.True(response.Data.IngredientesDisponibles >= 0);
        Assert.True(response.Data.IngredientesBajoStock >= 0);
        Assert.True(response.Data.IngredientesAgotados >= 0);
        _logger.LogInformation("Estadísticas obtenidas - Total: {Total}, Disponibles: {Disponibles}, Bajo Stock: {BajoStock}", 
            response.Data.TotalIngredientes, response.Data.IngredientesDisponibles, response.Data.IngredientesBajoStock);
    }

    #endregion

    #region Tests de Creación y Actualización

    [Fact]
    public async Task CrearIngrediente_WithValidData_ShouldCreateIngrediente()
    {
        // Arrange
        var nuevoIngrediente = new IngredienteDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Ingrediente Test",
            Descripcion = "Descripción del ingrediente test",
            StockActual = 100,
            StockMinimo = 10,
            UnidadMedida = "kg",
            PrecioUnitario = 15.50m,
            Categoria = "Vegetales",
            Disponible = true,
            FechaCreacion = DateTime.Now,
            Proveedor = "Proveedor Test"
        };

        // Act
        var response = await _ingredientesService.CrearIngredienteAsync(nuevoIngrediente);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(nuevoIngrediente.Nombre, response.Data.Nombre);
        _logger.LogInformation("Ingrediente '{Nombre}' creado exitosamente", nuevoIngrediente.Nombre);
    }

    [Fact]
    public async Task ActualizarIngrediente_WithValidData_ShouldUpdateIngrediente()
    {
        // Arrange
        var ingredienteId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var ingredienteActualizado = new IngredienteDto
        {
            Id = ingredienteId,
            Nombre = "Ingrediente Actualizado",
            Descripcion = "Descripción actualizada",
            StockActual = 150,
            StockMinimo = 15,
            UnidadMedida = "litros",
            PrecioUnitario = 20.00m,
            Categoria = "Lácteos",
            Disponible = true,
            FechaCreacion = DateTime.Now.AddDays(-30),
            Proveedor = "Proveedor Actualizado"
        };

        // Act
        var response = await _ingredientesService.ActualizarIngredienteAsync(ingredienteId, ingredienteActualizado);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(ingredienteActualizado.Nombre, response.Data.Nombre);
        _logger.LogInformation("Ingrediente {IngredienteId} actualizado exitosamente", ingredienteId);
    }

    [Fact]
    public async Task EliminarIngrediente_WithValidId_ShouldDeleteIngrediente()
    {
        // Arrange
        var ingredienteId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // Act
        var response = await _ingredientesService.EliminarIngredienteAsync(ingredienteId);

        // Assert
        Assert.True(response.Success);
        Assert.True(response.Data);
        _logger.LogInformation("Ingrediente {IngredienteId} eliminado exitosamente", ingredienteId);
    }

    #endregion

    #region Tests de Casos Edge

    [Fact]
    public async Task ObtenerIngrediente_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        var ingredienteId = Guid.Empty;

        // Act
        var response = await _ingredientesService.ObtenerIngredienteAsync(ingredienteId);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        _logger.LogInformation("Ingrediente con GUID vacío manejado correctamente");
    }

    [Fact]
    public async Task BuscarIngredientes_WithSpecialCharacters_ShouldHandleGracefully()
    {
        // Arrange
        var terminoBusqueda = "!@#$%^&*()";

        // Act
        var response = await _ingredientesService.BuscarIngredientesAsync(terminoBusqueda);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        _logger.LogInformation("Búsqueda con caracteres especiales manejada correctamente");
    }

    [Fact]
    public async Task BuscarIngredientes_WithVeryLongTerm_ShouldHandleGracefully()
    {
        // Arrange
        var terminoBusqueda = new string('A', 1000);

        // Act
        var response = await _ingredientesService.BuscarIngredientesAsync(terminoBusqueda);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        _logger.LogInformation("Búsqueda con término muy largo manejada correctamente");
    }

    [Fact]
    public async Task ObtenerIngredientesBajoStock_WithNegativeStockMinimo_ShouldHandleGracefully()
    {
        // Arrange
        var stockMinimo = -10;

        // Act
        var response = await _ingredientesService.ObtenerIngredientesBajoStockAsync(stockMinimo);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        _logger.LogInformation("Stock mínimo negativo manejado correctamente");
    }

    [Fact]
    public async Task CrearIngrediente_WithNullData_ShouldHandleGracefully()
    {
        // Act
        var response = await _ingredientesService.CrearIngredienteAsync(null!);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        _logger.LogInformation("Creación con datos nulos manejada correctamente");
    }

    [Fact]
    public async Task ActualizarIngrediente_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        var ingredienteId = Guid.Empty;
        var ingrediente = new IngredienteDto { Id = ingredienteId, Nombre = "Test" };

        // Act
        var response = await _ingredientesService.ActualizarIngredienteAsync(ingredienteId, ingrediente);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        _logger.LogInformation("Actualización con GUID vacío manejada correctamente");
    }

    [Fact]
    public async Task EliminarIngrediente_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        var ingredienteId = Guid.Empty;

        // Act
        var response = await _ingredientesService.EliminarIngredienteAsync(ingredienteId);

        // Assert
        Assert.False(response.Success);
        Assert.False(response.Data);
        _logger.LogInformation("Eliminación con GUID vacío manejada correctamente");
    }

    #endregion

    #region Tests de Reportes

    [Fact]
    public async Task GenerarReporteValoracion_WithValidFiltro_ShouldReturnReporte()
    {
        // Arrange
        var filtro = new FiltroIngredientesDto
        {
            Busqueda = "Tomate",
            SoloDisponibles = true
        };

        // Act
        var response = await _ingredientesService.GenerarReporteValoracionAsync(filtro);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        _logger.LogInformation("Reporte de valoración generado: {Count} elementos", response.Data.Count);
    }

    [Fact]
    public async Task GenerarReporteValoracion_WithEmptyFiltro_ShouldReturnAllReportes()
    {
        // Arrange
        var filtro = new FiltroIngredientesDto();

        // Act
        var response = await _ingredientesService.GenerarReporteValoracionAsync(filtro);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        _logger.LogInformation("Reporte de valoración con filtro vacío: {Count} elementos", response.Data.Count);
    }

    [Fact]
    public async Task ObtenerMovimientos_WithValidIngredienteId_ShouldReturnMovimientos()
    {
        // Arrange
        var ingredienteId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // Act
        var response = await _ingredientesService.ObtenerMovimientosAsync(ingredienteId);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        _logger.LogInformation("Movimientos obtenidos para ingrediente {IngredienteId}: {Count}", ingredienteId, response.Data.Count);
    }

    [Fact]
    public async Task ObtenerMovimientos_WithInvalidIngredienteId_ShouldReturnFailure()
    {
        // Arrange
        var ingredienteId = Guid.Empty;

        // Act
        var response = await _ingredientesService.ObtenerMovimientosAsync(ingredienteId);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        _logger.LogInformation("Movimientos con GUID vacío manejados correctamente");
    }

    #endregion

    #region Tests de Concurrencia

    [Fact]
    public async Task MultipleConcurrentOperations_ShouldHandleConcurrency()
    {
        // Arrange
        var tasks = new List<Task<ApiResponse<List<IngredienteSummaryDto>>>>();

        // Act - Ejecutar múltiples operaciones concurrentes
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_ingredientesService.ObtenerIngredientesAsync());
            tasks.Add(_ingredientesService.BuscarIngredientesAsync($"Ingrediente{i}"));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.All(results, result => Assert.True(result.Success));
        _logger.LogInformation("10 operaciones concurrentes exitosas");
    }

    [Fact]
    public async Task ConcurrentSearchAndUpdate_ShouldHandleConcurrency()
    {
        // Arrange
        var searchTask = _ingredientesService.BuscarIngredientesAsync("Tomate");
        var statsTask = _ingredientesService.ObtenerEstadisticasAsync();
        var lowStockTask = _ingredientesService.ObtenerIngredientesBajoStockAsync();

        // Act
        await Task.WhenAll(searchTask, statsTask, lowStockTask);
        var searchResult = await searchTask;
        var statsResult = await statsTask;
        var lowStockResult = await lowStockTask;

        // Assert
        Assert.True(searchResult.Success); // Search
        Assert.True(statsResult.Success); // Stats
        Assert.True(lowStockResult.Success); // Low stock
        _logger.LogInformation("Operaciones concurrentes de búsqueda y estadísticas exitosas");
    }

    #endregion

    #region Tests de Cancelación

    [Fact]
    public async Task ObtenerIngredientes_WithCancellation_ShouldHandleCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var response = await _ingredientesService.ObtenerIngredientesAsync(cancellationToken: cts.Token);

        // Assert
        Assert.False(response.Success);
        _logger.LogInformation("Cancelación manejada correctamente");
    }

    [Fact]
    public async Task BuscarIngredientes_WithCancellation_ShouldHandleCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var response = await _ingredientesService.BuscarIngredientesAsync("test", cancellationToken: cts.Token);

        // Assert
        Assert.False(response.Success);
        _logger.LogInformation("Cancelación de búsqueda manejada correctamente");
    }

    #endregion

    #region Tests de Rendimiento

    [Fact]
    public async Task ObtenerIngredientes_WithLargeDataset_ShouldPerformWell()
    {
        // Arrange
        var startTime = DateTime.Now;

        // Act
        var response = await _ingredientesService.ObtenerIngredientesAsync();

        // Assert
        var duration = DateTime.Now - startTime;
        Assert.True(response.Success);
        Assert.True(duration.TotalSeconds < 5, $"Operación tomó {duration.TotalSeconds} segundos");
        _logger.LogInformation("Rendimiento con dataset grande: {Duration}ms", duration.TotalMilliseconds);
    }

    [Fact]
    public async Task BuscarIngredientes_WithComplexQuery_ShouldPerformWell()
    {
        // Arrange
        var startTime = DateTime.Now;
        var terminoComplejo = "ingrediente con nombre muy largo y caracteres especiales !@#$%";

        // Act
        var response = await _ingredientesService.BuscarIngredientesAsync(terminoComplejo);

        // Assert
        var duration = DateTime.Now - startTime;
        Assert.True(response.Success);
        Assert.True(duration.TotalSeconds < 3, $"Búsqueda compleja tomó {duration.TotalSeconds} segundos");
        _logger.LogInformation("Rendimiento de búsqueda compleja: {Duration}ms", duration.TotalMilliseconds);
    }

    #endregion

    #region Tests de Validación de Datos

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    [InlineData("Ingrediente con nombre muy largo que excede límites razonables de longitud")]
    public async Task BuscarIngredientes_WithVariousTerms_ShouldHandleAll(string termino)
    {
        // Act
        var response = await _ingredientesService.BuscarIngredientesAsync(termino);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        _logger.LogInformation("Búsqueda con término: '{Termino}', Resultados: {Count}", termino, response.Data.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public async Task ObtenerIngredientesBajoStock_WithVariousStockMinimos_ShouldHandleAll(int stockMinimo)
    {
        // Act
        var response = await _ingredientesService.ObtenerIngredientesBajoStockAsync(stockMinimo);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        _logger.LogInformation("Stock mínimo: {StockMinimo}, Resultados: {Count}", stockMinimo, response.Data.Count);
    }

    #endregion
}