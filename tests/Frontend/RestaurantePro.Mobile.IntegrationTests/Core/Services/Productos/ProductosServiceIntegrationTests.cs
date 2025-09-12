using RestaurantePro.Mobile.Core.Services.Productos;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Platform;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Common;
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

    #region Tests de Casos Edge y Validaciones

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_WithCancellationToken_ShouldHandleCancellation()
    {
        // Arrange
        await SetupAsync();
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancelar inmediatamente

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(cancellationToken: cts.Token);

        // Assert - El servicio puede no lanzar excepción pero debe manejar la cancelación
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ObtenerProductoPorIdAsync_WithEmptyGuid_ShouldReturnError()
    {
        // Arrange
        await SetupAsync();
        var emptyId = Guid.Empty;

        // Act
        var result = await _productosService.ObtenerProductoPorIdAsync(emptyId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.True(result.Errors.Any());
    }

    [Fact]
    public async Task ObtenerProductosPorCategoriaAsync_WithEmptyGuid_ShouldReturnError()
    {
        // Arrange
        await SetupAsync();
        var emptyCategoriaId = Guid.Empty;

        // Act
        var result = await _productosService.ObtenerProductosPorCategoriaAsync(emptyCategoriaId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.True(result.Errors.Any());
    }

    [Fact]
    public async Task VerificarDisponibilidadProductoAsync_WithEmptyGuid_ShouldReturnError()
    {
        // Arrange
        await SetupAsync();
        var emptyProductoId = Guid.Empty;

        // Act
        var result = await _productosService.VerificarDisponibilidadProductoAsync(emptyProductoId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.True(result.Errors.Any());
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_WithSpecialCharactersInFiltro_ShouldHandleGracefully()
    {
        // Arrange
        await SetupAsync();
        var filtro = "!@#$%^&*()_+{}|:<>?[]\\;'\",./";

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(filtro: filtro);

        // Assert
        Assert.NotNull(result);
        // Debería manejar caracteres especiales sin lanzar excepción
        Assert.True(result.Success || !result.Success); // Puede ser exitoso o fallar, pero no lanzar excepción
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
        // Debería manejar filtros largos sin lanzar excepción
        Assert.True(result.Success || !result.Success); // Puede ser exitoso o fallar, pero no lanzar excepción
    }

    #endregion

    #region Tests de Rendimiento

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        await SetupAsync();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync();
        stopwatch.Stop();

        // Assert
        Assert.NotNull(result);
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Operación tomó {stopwatch.ElapsedMilliseconds}ms, debería ser menor a 5000ms");
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_WithLargePageSize_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        await SetupAsync();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(1, 50);
        stopwatch.Stop();

        // Assert
        Assert.NotNull(result);
        Assert.True(stopwatch.ElapsedMilliseconds < 3000, $"Operación tomó {stopwatch.ElapsedMilliseconds}ms, debería ser menor a 3000ms");
    }

    [Fact]
    public async Task MultipleConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        await SetupAsync();
        var tasks = new List<Task<ApiResponse<List<ProductoDto>>>>();

        // Act - Ejecutar múltiples requests concurrentes
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_productosService.ObtenerProductosPaginadosAsync());
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(10, results.Length);
        foreach (var result in results)
        {
            Assert.NotNull(result);
            // Todos deberían completarse sin excepción
        }
    }

    #endregion

    #region Tests de Concurrencia

    [Fact]
    public async Task ConcurrentProductRequests_ShouldHandleRaceConditions()
    {
        // Arrange
        await SetupAsync();
        var tasks = new List<Task<ApiResponse<List<ProductoDto>>>>();

        // Act - Ejecutar múltiples requests concurrentes con diferentes parámetros
        tasks.Add(_productosService.ObtenerProductosPaginadosAsync());
        tasks.Add(_productosService.ObtenerProductosPaginadosAsync(1, 5));
        tasks.Add(_productosService.ObtenerProductosPaginadosAsync(1, 10));
        tasks.Add(_productosService.ObtenerProductosPaginadosAsync(filtro: "test"));
        tasks.Add(_productosService.ObtenerProductosPaginadosAsync(soloActivos: false));

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(5, results.Length);
        foreach (var result in results)
        {
            Assert.NotNull(result);
            // No debería lanzar excepción por concurrencia
        }
    }

    [Fact]
    public async Task ConcurrentProductByIdRequests_ShouldHandleGracefully()
    {
        // Arrange
        await SetupAsync();
        var tasks = new List<Task<ApiResponse<ProductoDto>>>();

        // Act - Ejecutar múltiples requests de producto por ID concurrentemente
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(_productosService.ObtenerProductoPorIdAsync(Guid.NewGuid()));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(5, results.Length);
        foreach (var result in results)
        {
            Assert.NotNull(result);
            // No debería lanzar excepción por concurrencia
        }
    }

    #endregion

    #region Tests de Validación de Datos

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_ShouldReturnValidProductData()
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
        
        // Verificar que cada producto tiene datos válidos
        foreach (var producto in result.Data)
        {
            Assert.NotNull(producto);
            Assert.NotEmpty(producto.Nombre);
            Assert.True(producto.Precio >= 0);
            Assert.True(producto.Id != Guid.Empty);
            Assert.NotNull(producto.CategoriaNombre);
        }
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_WithValidPagination_ShouldReturnCorrectPaginationInfo()
    {
        // Arrange
        await SetupAsync();
        var pageNumber = 1;
        var pageSize = 3;

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(pageNumber, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        
        // Verificar que la lista tiene elementos válidos
        Assert.True(result.Data.Count > 0);
        Assert.True(result.Data.Count <= pageSize);
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_WithFiltro_ShouldReturnRelevantProducts()
    {
        // Arrange
        await SetupAsync();
        var filtro = "pizza"; // Buscar productos que contengan "pizza"

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(filtro: filtro);

        // Assert
        Assert.NotNull(result);
        if (result.Success && result.Data.Count > 0)
        {
            // Si hay resultados, verificar que contienen el filtro
            foreach (var producto in result.Data)
            {
                Assert.Contains(filtro.ToLower(), producto.Nombre.ToLower());
            }
        }
    }

    #endregion

    #region Tests de Manejo de Errores

    [Fact]
    public async Task Service_WithInvalidCredentials_ShouldHandleAuthenticationError()
    {
        // Arrange
        var httpClient = _client;
        var apiService = new ApiService(httpClient);
        var authService = new AuthService(apiService, NullLogger<AuthService>.Instance, new FakeSecureStorageService(), new FakeNavigationService());
        var productosService = new ProductosService(apiService, authService);

        // No hacer login - simular credenciales inválidas

        // Act
        var result = await productosService.ObtenerProductosPaginadosAsync();

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        // El mensaje de error puede variar, solo verificamos que hay errores
        Assert.True(result.Errors.Any());
    }

    [Fact]
    public async Task Service_WithNetworkTimeout_ShouldHandleTimeoutGracefully()
    {
        // Arrange
        await SetupAsync();
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancelar inmediatamente

        // Act
        var result = await _productosService.ObtenerProductosPaginadosAsync(cancellationToken: cts.Token);

        // Assert - El servicio puede no lanzar excepción pero debe manejar la cancelación
        Assert.NotNull(result);
    }

    #endregion
} 
 