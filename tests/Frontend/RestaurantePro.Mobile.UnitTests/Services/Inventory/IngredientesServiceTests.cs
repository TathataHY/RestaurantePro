using Moq;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Inventory;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Services.Inventory;

public class IngredientesServiceTests
{
    private readonly Mock<IApiService> _mockApiService;
    private readonly IngredientesService _ingredientesService;

    public IngredientesServiceTests()
    {
        _mockApiService = new Mock<IApiService>();
        _ingredientesService = new IngredientesService(_mockApiService.Object);
    }

    [Fact]
    public async Task ObtenerIngredientesAsync_WithValidResponse_ShouldReturnSuccess()
    {
        // Arrange
        var ingredientes = new List<IngredienteSummaryDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Tomate", StockActual = 50, UnidadMedida = "kg" },
            new() { Id = Guid.NewGuid(), Nombre = "Cebolla", StockActual = 30, UnidadMedida = "kg" }
        };

        var apiResponse = ApiResponse<List<IngredienteSummaryDto>>.SuccessResponse(ingredientes);
        _mockApiService.Setup(x => x.GetAsync<List<IngredienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _ingredientesService.ObtenerIngredientesAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        _mockApiService.Verify(x => x.GetAsync<List<IngredienteSummaryDto>>("api/ingredientes?soloActivos=True", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerIngredientesAsync_WithApiError_ShouldReturnFailure()
    {
        // Arrange
        var errorResponse = ApiResponse<List<IngredienteSummaryDto>>.ErrorResponse(new List<string> { "Error de API" }, "Error de API", 500);
        _mockApiService.Setup(x => x.GetAsync<List<IngredienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(errorResponse);

        // Act
        var result = await _ingredientesService.ObtenerIngredientesAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de API", result.Error);
    }

    [Fact]
    public async Task ObtenerIngredienteAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var ingrediente = new IngredienteDto { Id = ingredienteId, Nombre = "Tomate", StockActual = 50, UnidadMedida = "kg" };
        var apiResponse = ApiResponse<IngredienteDto>.SuccessResponse(ingrediente);
        
        _mockApiService.Setup(x => x.GetAsync<IngredienteDto>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _ingredientesService.ObtenerIngredienteAsync(ingredienteId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(ingredienteId, result.Data.Id);
        _mockApiService.Verify(x => x.GetAsync<IngredienteDto>($"api/ingredientes/{ingredienteId}", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task BuscarIngredientesAsync_WithValidTerm_ShouldReturnSuccess()
    {
        // Arrange
        var termino = "tomate";
        var ingredientes = new List<IngredienteSummaryDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Tomate Cherry", StockActual = 20, UnidadMedida = "kg" }
        };

        var apiResponse = ApiResponse<List<IngredienteSummaryDto>>.SuccessResponse(ingredientes);
        _mockApiService.Setup(x => x.GetAsync<List<IngredienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _ingredientesService.BuscarIngredientesAsync(termino);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        _mockApiService.Verify(x => x.GetAsync<List<IngredienteSummaryDto>>($"api/ingredientes/buscar?termino={termino}", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CrearIngredienteAsync_WithValidIngrediente_ShouldReturnSuccess()
    {
        // Arrange
        var ingrediente = new IngredienteDto { Nombre = "Nuevo Ingrediente", StockActual = 100, UnidadMedida = "kg" };
        var ingredienteCreado = new IngredienteDto { Id = Guid.NewGuid(), Nombre = "Nuevo Ingrediente", StockActual = 100, UnidadMedida = "kg" };
        var apiResponse = ApiResponse<IngredienteDto>.SuccessResponse(ingredienteCreado);
        
        _mockApiService.Setup(x => x.PostAsync<IngredienteDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _ingredientesService.CrearIngredienteAsync(ingrediente);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("Nuevo Ingrediente", result.Data.Nombre);
        _mockApiService.Verify(x => x.PostAsync<IngredienteDto>("api/ingredientes", ingrediente, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ActualizarIngredienteAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var ingrediente = new IngredienteDto { Id = ingredienteId, Nombre = "Ingrediente Actualizado", StockActual = 75, UnidadMedida = "kg" };
        var apiResponse = ApiResponse<IngredienteDto>.SuccessResponse(ingrediente);
        
        _mockApiService.Setup(x => x.PutAsync<IngredienteDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _ingredientesService.ActualizarIngredienteAsync(ingredienteId, ingrediente);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("Ingrediente Actualizado", result.Data.Nombre);
        _mockApiService.Verify(x => x.PutAsync<IngredienteDto>($"api/ingredientes/{ingredienteId}", ingrediente, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task EliminarIngredienteAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var apiResponse = ApiResponse<bool>.SuccessResponse(true);
        
        _mockApiService.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _ingredientesService.EliminarIngredienteAsync(ingredienteId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data);
        _mockApiService.Verify(x => x.DeleteAsync($"api/ingredientes/{ingredienteId}", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_ShouldReturnSuccess()
    {
        // Arrange
        var estadisticas = new EstadisticasIngredientesDto
        {
            TotalIngredientes = 150,
            IngredientesDisponibles = 140,
            IngredientesAgotados = 10,
            IngredientesBajoStock = 15,
            ValorTotalInventario = 5000.00m
        };

        var apiResponse = ApiResponse<EstadisticasIngredientesDto>.SuccessResponse(estadisticas);
        _mockApiService.Setup(x => x.GetAsync<EstadisticasIngredientesDto>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _ingredientesService.ObtenerEstadisticasAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(150, result.Data.TotalIngredientes);
        Assert.Equal(140, result.Data.IngredientesDisponibles);
        _mockApiService.Verify(x => x.GetAsync<EstadisticasIngredientesDto>("api/ingredientes/estadisticas", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerIngredientesBajoStockAsync_ShouldReturnSuccess()
    {
        // Arrange
        var ingredientes = new List<IngredienteSummaryDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Ingrediente Bajo Stock", StockActual = 5, UnidadMedida = "kg" }
        };

        var apiResponse = ApiResponse<List<IngredienteSummaryDto>>.SuccessResponse(ingredientes);
        _mockApiService.Setup(x => x.GetAsync<List<IngredienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _ingredientesService.ObtenerIngredientesBajoStockAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        _mockApiService.Verify(x => x.GetAsync<List<IngredienteSummaryDto>>("api/ingredientes/bajo-stock?stockMinimo=10", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerIngredientesAsync_WithFiltro_ShouldReturnSuccess()
    {
        // Arrange
        var ingredientes = new List<IngredienteSummaryDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Ingrediente Activo", StockActual = 50, UnidadMedida = "kg" }
        };

        var apiResponse = ApiResponse<List<IngredienteSummaryDto>>.SuccessResponse(ingredientes);
        _mockApiService.Setup(x => x.GetAsync<List<IngredienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _ingredientesService.ObtenerIngredientesAsync(soloActivos: false);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        _mockApiService.Verify(x => x.GetAsync<List<IngredienteSummaryDto>>("api/ingredientes?soloActivos=False", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerMovimientosAsync_WithValidIngredienteId_ShouldReturnSuccess()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var movimientos = new List<MovimientoInventarioDto>
        {
            new() { Id = Guid.NewGuid(), TipoMovimiento = "Entrada", Cantidad = 100, FechaMovimiento = DateTime.Now },
            new() { Id = Guid.NewGuid(), TipoMovimiento = "Salida", Cantidad = -50, FechaMovimiento = DateTime.Now.AddDays(-1) }
        };

        var apiResponse = ApiResponse<List<MovimientoInventarioDto>>.SuccessResponse(movimientos);
        _mockApiService.Setup(x => x.GetAsync<List<MovimientoInventarioDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _ingredientesService.ObtenerMovimientosAsync(ingredienteId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        _mockApiService.Verify(x => x.GetAsync<List<MovimientoInventarioDto>>($"api/ingredientes/{ingredienteId}/movimientos", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerIngredientesAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        _mockApiService.Setup(x => x.GetAsync<List<IngredienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _ingredientesService.ObtenerIngredientesAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }

    [Fact]
    public async Task ObtenerIngredienteAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        _mockApiService.Setup(x => x.GetAsync<IngredienteDto>(It.IsAny<string>(), It.IsAny<string>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _ingredientesService.ObtenerIngredienteAsync(ingredienteId);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }

    [Fact]
    public async Task CrearIngredienteAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var ingrediente = new IngredienteDto { Nombre = "Nuevo Ingrediente", StockActual = 100, UnidadMedida = "kg" };
        _mockApiService.Setup(x => x.PostAsync<IngredienteDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _ingredientesService.CrearIngredienteAsync(ingrediente);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }

    [Fact]
    public async Task ActualizarIngredienteAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var ingrediente = new IngredienteDto { Id = ingredienteId, Nombre = "Ingrediente Actualizado", StockActual = 75, UnidadMedida = "kg" };
        _mockApiService.Setup(x => x.PutAsync<IngredienteDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _ingredientesService.ActualizarIngredienteAsync(ingredienteId, ingrediente);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }

    [Fact]
    public async Task EliminarIngredienteAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        _mockApiService.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<string>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _ingredientesService.EliminarIngredienteAsync(ingredienteId);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }

    [Fact]
    public async Task BuscarIngredientesAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var termino = "tomate";
        _mockApiService.Setup(x => x.GetAsync<List<IngredienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _ingredientesService.BuscarIngredientesAsync(termino);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        _mockApiService.Setup(x => x.GetAsync<EstadisticasIngredientesDto>(It.IsAny<string>(), It.IsAny<string>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _ingredientesService.ObtenerEstadisticasAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }
} 