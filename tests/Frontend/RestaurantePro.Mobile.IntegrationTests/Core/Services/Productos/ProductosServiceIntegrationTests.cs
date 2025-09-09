using RestaurantePro.Mobile.Core.Services.Productos;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Platform;
using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services.Productos;

public class ProductosServiceIntegrationTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;
    private readonly HttpClient _client;
    private IProductosService _productosService;

    public ProductosServiceIntegrationTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }

    private async Task SetupAsync()
    {
        // Crear servicios móviles localmente para evitar conflictos con el backend
        var httpClient = _client;
        var apiService = new ApiService(httpClient);
        var authService = new AuthService(apiService, NullLogger<AuthService>.Instance, new FakeSecureStorageService(), new FakeNavigationService());
        _productosService = new ProductosService(apiService, authService);
        
        // Login automático para todos los tests
        var loginResult = await authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        if (!loginResult.Success)
        {
            throw new InvalidOperationException($"Login failed: {loginResult.Message}");
        }
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_ShouldReturnProducts()
    {
        // Arrange
        await SetupAsync();

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Count > 0);
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_WithPagination_ShouldReturnPaginatedResults()
    {
        // Arrange
        await SetupAsync();
        var pageNumber = 1;
        var pageSize = 5;

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(pageNumber, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Count <= pageSize);
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_WithFiltro_ShouldReturnFilteredProducts()
    {
        // Arrange
        await SetupAsync();
        var filtro = "pizza"; // Buscar productos que contengan "pizza"

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(filtro: filtro);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        // Puede estar vacío si no hay productos con "pizza" en el nombre
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_WithSoloActivosFalse_ShouldReturnAllProducts()
    {
        // Arrange
        await SetupAsync();

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(soloActivos: false);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        // Puede incluir productos inactivos
    }

    [Fact]
    public async Task ObtenerProductoPorIdAsync_WithInvalidId_ShouldReturnError()
    {
        // Arrange
        await SetupAsync();
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _productosService.ObtenerProductoPorIdAsync(invalidId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    public async Task ObtenerProductosPorCategoriaAsync_WithInvalidCategoriaId_ShouldReturnError()
    {
        // Arrange
        await SetupAsync();
        var invalidCategoriaId = Guid.NewGuid();

        // Act
        var result = await _productosService.ObtenerProductosPorCategoriaAsync(invalidCategoriaId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    public async Task VerificarDisponibilidadProductoAsync_WithInvalidProductoId_ShouldReturnError()
    {
        // Arrange
        await SetupAsync();
        var invalidProductoId = Guid.NewGuid();

        // Act
        var result = await _productosService.VerificarDisponibilidadProductoAsync(invalidProductoId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_WithNegativePageNumber_ShouldHandleGracefully()
    {
        // Arrange
        await SetupAsync();
        var pageNumber = -1;
        var pageSize = 10;

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(pageNumber, pageSize);

        // Assert
        Assert.NotNull(result);
        // El backend debería validar y rechazar página negativa
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_WithZeroPageSize_ShouldHandleGracefully()
    {
        // Arrange
        await SetupAsync();
        var pageNumber = 1;
        var pageSize = 0;

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(pageNumber, pageSize);

        // Assert
        Assert.NotNull(result);
        // El backend debería validar y rechazar tamaño de página cero
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_WithVeryLargePageSize_ShouldHandleGracefully()
    {
        // Arrange
        await SetupAsync();
        var pageNumber = 1;
        var pageSize = 1000;

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(pageNumber, pageSize);

        // Assert
        Assert.NotNull(result);
        // El backend debería validar y rechazar tamaño de página muy grande
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_WithEmptyFiltro_ShouldReturnAllProducts()
    {
        // Arrange
        await SetupAsync();
        var filtro = "";

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(filtro: filtro);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        // Debería retornar todos los productos cuando el filtro está vacío
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_WithNullFiltro_ShouldReturnAllProducts()
    {
        // Arrange
        await SetupAsync();
        string filtro = null;

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(filtro: filtro);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        // Debería retornar todos los productos cuando el filtro es nulo
    }
} 
 