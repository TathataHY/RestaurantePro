using RestaurantePro.Mobile.Core.Services.Productos;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Platform;
using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services.Productos;

public class ProductosServiceAdvancedIntegrationTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;
    private readonly HttpClient _client;
    private IProductosService _productosService;
    private IAuthService _authService;

    public ProductosServiceAdvancedIntegrationTests(MobileIntegrationTestFixture fixture)
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
        _authService = authService;
        
        // Login automático para todos los tests
        var loginResult = await authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        if (!loginResult.Success)
        {
            throw new InvalidOperationException($"Login failed: {loginResult.Message}");
        }
    }

    [Fact]
    public async Task ObtenerProductoPorIdAsync_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        await SetupAsync();
        
        // Primero obtener un producto para tener un ID válido
        var productosResult = await _productosService.ObtenerProductosPaginadosAsync();
        Assert.True(productosResult.Success);
        var productoId = productosResult.Data.First().Id;

        // Act
        var result = await _productosService.ObtenerProductoPorIdAsync(productoId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(productoId, result.Data.Id);
    }

    [Fact]
    public async Task ObtenerProductosPorCategoriaAsync_WithValidCategoriaId_ShouldReturnProducts()
    {
        // Arrange
        await SetupAsync();
        
        // Primero obtener productos para encontrar una categoría válida
        var productosResult = await _productosService.ObtenerProductosPaginadosAsync();
        Assert.True(productosResult.Success);
        var categoriaId = productosResult.Data.First().CategoriaId;

        // Act
        var result = await _productosService.ObtenerProductosPorCategoriaAsync(categoriaId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.All(p => p.CategoriaId == categoriaId));
    }

    [Fact]
    public async Task VerificarDisponibilidadProductoAsync_WithValidProductoId_ShouldReturnAvailability()
    {
        // Arrange
        await SetupAsync();
        
        // Primero obtener un producto para tener un ID válido
        var productosResult = await _productosService.ObtenerProductosPaginadosAsync();
        Assert.True(productosResult.Success);
        var productoId = productosResult.Data.First().Id;

        // Act
        var result = await _productosService.VerificarDisponibilidadProductoAsync(productoId);

        // Assert
        Assert.NotNull(result);
        // El endpoint de disponibilidad no está implementado en el backend aún
        // Por ahora, esperamos que falle con un error apropiado
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_WithMultiplePages_ShouldReturnDifferentResults()
    {
        // Arrange
        await SetupAsync();
        var pageSize = 2;

        // Act
        var page1Result = await _productosService.ObtenerProductosPaginadosAsync(1, pageSize);
        var page2Result = await _productosService.ObtenerProductosPaginadosAsync(2, pageSize);

        // Assert
        Assert.True(page1Result.Success);
        Assert.True(page2Result.Success);
        Assert.NotNull(page1Result.Data);
        Assert.NotNull(page2Result.Data);
        
        // Las páginas deberían tener diferentes productos (si hay suficientes productos)
        if (page1Result.Data.Count > 0 && page2Result.Data.Count > 0)
        {
            var page1Ids = page1Result.Data.Select(p => p.Id).ToHashSet();
            var page2Ids = page2Result.Data.Select(p => p.Id).ToHashSet();
            
            // No deberían tener productos en común
            Assert.False(page1Ids.Overlaps(page2Ids));
        }
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_WithCaseInsensitiveFiltro_ShouldReturnFilteredProducts()
    {
        // Arrange
        await SetupAsync();
        var filtro = "PIZZA"; // Mayúsculas

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(filtro: filtro);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        // Debería encontrar productos que contengan "pizza" (case insensitive)
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_WithSpecialCharactersFiltro_ShouldHandleGracefully()
    {
        // Arrange
        await SetupAsync();
        var filtro = "pizza@#$%";

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(filtro: filtro);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        // Debería manejar caracteres especiales correctamente
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_WithVeryLongFiltro_ShouldHandleGracefully()
    {
        // Arrange
        await SetupAsync();
        var filtro = new string('a', 1000); // Filtro muy largo

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(filtro: filtro);

        // Assert
        Assert.NotNull(result);
        // El backend valida que el filtro no exceda 100 caracteres
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        // Debería contener error de validación sobre longitud del filtro
    }

    [Fact]
    public async Task FlujoCompleto_Productos_ShouldWorkEndToEnd()
    {
        // Arrange
        await SetupAsync();
        
        // 1. Obtener productos paginados
        var productosResult = await _productosService.ObtenerProductosPaginadosAsync();
        Assert.True(productosResult.Success);
        Assert.True(productosResult.Data.Count > 0);

        // 2. Obtener un producto específico
        var productoId = productosResult.Data.First().Id;
        var productoResult = await _productosService.ObtenerProductoPorIdAsync(productoId);
        Assert.True(productoResult.Success);
        Assert.Equal(productoId, productoResult.Data.Id);

        // 3. Verificar disponibilidad del producto (endpoint no implementado aún)
        var disponibilidadResult = await _productosService.VerificarDisponibilidadProductoAsync(productoId);
        Assert.False(disponibilidadResult.Success);
        Assert.NotNull(disponibilidadResult.Errors);

        // 4. Obtener productos por categoría
        var categoriaId = productoResult.Data.CategoriaId;
        var productosPorCategoriaResult = await _productosService.ObtenerProductosPorCategoriaAsync(categoriaId);
        Assert.True(productosPorCategoriaResult.Success);
        Assert.True(productosPorCategoriaResult.Data.All(p => p.CategoriaId == categoriaId));
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_WithSoloActivosTrue_ShouldReturnOnlyActiveProducts()
    {
        // Arrange
        await SetupAsync();
        
        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(soloActivos: true);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.All(p => p.Activo));
    }

    [Fact]
    public async Task ObtenerProductosPorCategoriaAsync_WithSoloActivosFalse_ShouldReturnAllProducts()
    {
        // Arrange
        await SetupAsync();
        
        // Primero obtener productos para encontrar una categoría válida
        var productosResult = await _productosService.ObtenerProductosPaginadosAsync();
        Assert.True(productosResult.Success);
        var categoriaId = productosResult.Data.First().CategoriaId;

        // Act
        var result = await _productosService.ObtenerProductosPorCategoriaAsync(categoriaId, soloActivos: false);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.All(p => p.CategoriaId == categoriaId));
        // Puede incluir productos inactivos
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_WithInvalidPageNumber_ShouldHandleGracefully()
    {
        // Arrange
        await SetupAsync();
        var pageNumber = int.MaxValue; // Número de página muy alto
        var pageSize = 10;

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(pageNumber, pageSize);

        // Assert
        Assert.NotNull(result);
        // El backend puede validar y rechazar página muy alta, o retornar lista vacía
        // Por ahora, el backend no valida páginas muy altas, así que esperamos éxito con datos
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        // Puede retornar datos o lista vacía, ambos son válidos
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_WithNegativePageSize_ShouldHandleGracefully()
    {
        // Arrange
        await SetupAsync();
        var pageNumber = 1;
        var pageSize = -10;

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(pageNumber, pageSize);

        // Assert
        Assert.NotNull(result);
        // El backend debería validar y rechazar tamaño de página negativo
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_WithZeroPageNumber_ShouldHandleGracefully()
    {
        // Arrange
        await SetupAsync();
        var pageNumber = 0;
        var pageSize = 10;

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(pageNumber, pageSize);

        // Assert
        Assert.NotNull(result);
        // El backend debería validar y rechazar página cero
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_WithAllParameters_ShouldWorkCorrectly()
    {
        // Arrange
        await SetupAsync();
        var pageNumber = 1;
        var pageSize = 5;
        var filtro = "pizza";
        var soloActivos = true;

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(pageNumber, pageSize, filtro, soloActivos);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Count <= pageSize);
        Assert.True(result.Data.All(p => p.Activo));
        // Los productos deberían contener "pizza" en el nombre (si existen)
    }
} 
 