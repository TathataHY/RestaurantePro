using Moq;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Inventory;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Services.Inventory;

public class IngredientesServiceTests
{
    private readonly Mock<IApiService> _mockApiService;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly IngredientesService _ingredientesService;

    public IngredientesServiceTests()
    {
        _mockApiService = new Mock<IApiService>();
        _mockAuthService = new Mock<IAuthService>();
        
        // Configurar el mock de autenticación para devolver un token válido
        _mockAuthService.Setup(x => x.GetTokenAsync())
                       .ReturnsAsync("test-token");
        
        _ingredientesService = new IngredientesService(_mockApiService.Object, _mockAuthService.Object);
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
        _mockApiService.Setup(x => x.GetAsync<List<IngredienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _ingredientesService.ObtenerIngredientesAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        _mockApiService.Verify(x => x.GetAsync<List<IngredienteSummaryDto>>("api/inventario/ingredientes/lista?soloActivos=True", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerIngredientesAsync_WithApiError_ShouldReturnFailure()
    {
        // Arrange
        var errorResponse = ApiResponse<List<IngredienteSummaryDto>>.ErrorResponse(new List<string> { "Error de API" }, "Error de API", 500);
        _mockApiService.Setup(x => x.GetAsync<List<IngredienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
        
        _mockApiService.Setup(x => x.GetAsync<IngredienteDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _ingredientesService.ObtenerIngredienteAsync(ingredienteId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(ingredienteId, result.Data.Id);
        _mockApiService.Verify(x => x.GetAsync<IngredienteDto>($"api/inventario/ingredientes/{ingredienteId}", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
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
        _mockApiService.Setup(x => x.GetAsync<List<IngredienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _ingredientesService.BuscarIngredientesAsync(termino);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        _mockApiService.Verify(x => x.GetAsync<List<IngredienteSummaryDto>>($"api/inventario/ingredientes/buscar?termino={termino}", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CrearIngredienteAsync_WithValidIngrediente_ShouldReturnSuccess()
    {
        // Arrange
        var ingrediente = new IngredienteDto { Nombre = "Nuevo Ingrediente", StockActual = 100, UnidadMedida = "kg" };
        var ingredienteCreado = new IngredienteDto { Id = Guid.NewGuid(), Nombre = "Nuevo Ingrediente", StockActual = 100, UnidadMedida = "kg" };
        var apiResponse = ApiResponse<IngredienteDto>.SuccessResponse(ingredienteCreado);
        
        _mockApiService.Setup(x => x.PostAsync<IngredienteDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _ingredientesService.CrearIngredienteAsync(ingrediente);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("Nuevo Ingrediente", result.Data.Nombre);
        _mockApiService.Verify(x => x.PostAsync<IngredienteDto>("api/inventario/ingredientes", ingrediente, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActualizarIngredienteAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var ingrediente = new IngredienteDto { Id = ingredienteId, Nombre = "Ingrediente Actualizado", StockActual = 75, UnidadMedida = "kg" };
        var apiResponse = ApiResponse<IngredienteDto>.SuccessResponse(ingrediente);
        
        _mockApiService.Setup(x => x.PutAsync<IngredienteDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _ingredientesService.ActualizarIngredienteAsync(ingredienteId, ingrediente);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("Ingrediente Actualizado", result.Data.Nombre);
        _mockApiService.Verify(x => x.PutAsync<IngredienteDto>($"api/inventario/ingredientes/{ingredienteId}", ingrediente, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
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
        _mockApiService.Verify(x => x.DeleteAsync($"api/inventario/ingredientes/{ingredienteId}", It.IsAny<string>()), Times.Once);
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
        _mockApiService.Setup(x => x.GetAsync<EstadisticasIngredientesDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _ingredientesService.ObtenerEstadisticasAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(150, result.Data.TotalIngredientes);
        Assert.Equal(140, result.Data.IngredientesDisponibles);
        _mockApiService.Verify(x => x.GetAsync<EstadisticasIngredientesDto>("api/inventario/ingredientes/estadisticas", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
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
        _mockApiService.Setup(x => x.GetAsync<List<IngredienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _ingredientesService.ObtenerIngredientesBajoStockAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        _mockApiService.Verify(x => x.GetAsync<List<IngredienteSummaryDto>>("api/inventario/ingredientes/bajo-stock?stockMinimo=10", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
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
        _mockApiService.Setup(x => x.GetAsync<List<IngredienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _ingredientesService.ObtenerIngredientesAsync(soloActivos: false);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        _mockApiService.Verify(x => x.GetAsync<List<IngredienteSummaryDto>>("api/inventario/ingredientes/lista?soloActivos=False", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
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
        _mockApiService.Setup(x => x.GetAsync<List<MovimientoInventarioDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _ingredientesService.ObtenerMovimientosAsync(ingredienteId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        _mockApiService.Verify(x => x.GetAsync<List<MovimientoInventarioDto>>($"api/inventario/ingredientes/{ingredienteId}/movimientos", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerIngredientesAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        _mockApiService.Setup(x => x.GetAsync<List<IngredienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
        _mockApiService.Setup(x => x.GetAsync<IngredienteDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
        _mockApiService.Setup(x => x.PostAsync<IngredienteDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
        _mockApiService.Setup(x => x.PutAsync<IngredienteDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
        _mockApiService.Setup(x => x.GetAsync<List<IngredienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
        _mockApiService.Setup(x => x.GetAsync<EstadisticasIngredientesDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _ingredientesService.ObtenerEstadisticasAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }

    // Tests para 401/403/429
    [Fact]
    public async Task ObtenerIngredientesAsync_WithUnauthorized_ShouldPropagate401()
    {
        // Arrange
        var apiResponse = ApiResponse<List<IngredienteSummaryDto>>.ErrorResponse(new List<string> { "Unauthorized" }, "Unauthorized", 401);
        _mockApiService.Setup(x => x.GetAsync<List<IngredienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _ingredientesService.ObtenerIngredientesAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(401, result.StatusCode);
        Assert.Contains("Unauthorized", result.Error);
    }

    [Fact]
    public async Task ObtenerIngredientesAsync_WithForbidden_ShouldPropagate403()
    {
        // Arrange
        var apiResponse = ApiResponse<List<IngredienteSummaryDto>>.ErrorResponse(new List<string> { "Forbidden" }, "Forbidden", 403);
        _mockApiService.Setup(x => x.GetAsync<List<IngredienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _ingredientesService.ObtenerIngredientesAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Forbidden", result.Error);
    }

    [Fact]
    public async Task ObtenerIngredientesAsync_WithTooManyRequests_ShouldPropagate429()
    {
        // Arrange
        var apiResponse = ApiResponse<List<IngredienteSummaryDto>>.ErrorResponse(new List<string> { "Too Many Requests" }, "Too Many Requests", 429);
        _mockApiService.Setup(x => x.GetAsync<List<IngredienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _ingredientesService.ObtenerIngredientesAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(429, result.StatusCode);
        Assert.Contains("Too Many Requests", result.Error);
    }

    // Tests para 204/empty body
    [Fact]
    public async Task ObtenerIngredientesAsync_WithEmptyBody_ShouldReturnEmptyList()
    {
        // Arrange
        var apiResponse = ApiResponse<List<IngredienteSummaryDto>>.SuccessResponse(new List<IngredienteSummaryDto>());
        _mockApiService.Setup(x => x.GetAsync<List<IngredienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _ingredientesService.ObtenerIngredientesAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    // Tests para cancelación
    [Fact]
    public async Task ObtenerIngredientesAsync_WhenCancelled_ShouldReturnCancelled()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel the token immediately

        // Act
        var result = await _ingredientesService.ObtenerIngredientesAsync(cancellationToken: cts.Token);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Operación cancelada por el usuario", result.Error);
        _mockApiService.Verify(x => x.GetAsync<List<IngredienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuscarIngredientesAsync_WhenCancelled_ShouldReturnCancelled()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel the token immediately

        // Act
        var result = await _ingredientesService.BuscarIngredientesAsync("test", cancellationToken: cts.Token);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Operación cancelada por el usuario", result.Error);
        _mockApiService.Verify(x => x.GetAsync<List<IngredienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
} 
