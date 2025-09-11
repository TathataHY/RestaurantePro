using Moq;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Categorias;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Services.Categorias;

public class CategoriasServiceTests
{
    private readonly Mock<IApiService> _mockApiService;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly CategoriasService _categoriasService;

    public CategoriasServiceTests()
    {
        _mockApiService = new Mock<IApiService>();
        _mockAuthService = new Mock<IAuthService>();
        _mockAuthService.Setup(x => x.GetTokenAsync()).ReturnsAsync("test-token");
        _categoriasService = new CategoriasService(_mockApiService.Object, _mockAuthService.Object);
    }

    [Fact]
    public async Task ObtenerCategoriasAsync_WithValidResponse_ShouldReturnSuccess()
    {
        // Arrange
        var categorias = new List<CategoriaProductoDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Entradas", Descripcion = "Platos de entrada" },
            new() { Id = Guid.NewGuid(), Nombre = "Platos Principales", Descripcion = "Platos principales" },
            new() { Id = Guid.NewGuid(), Nombre = "Postres", Descripcion = "Postres y dulces" }
        };

        var apiResponse = ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias);
        _mockApiService.Setup(x => x.GetAsync<List<CategoriaProductoDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _categoriasService.ObtenerCategoriasAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Count);
        _mockApiService.Verify(x => x.GetAsync<List<CategoriaProductoDto>>("api/core/categorias?soloActivas=True", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerCategoriasAsync_WithInactivas_ShouldReturnSuccess()
    {
        // Arrange
        var categorias = new List<CategoriaProductoDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Entradas" },
            new() { Id = Guid.NewGuid(), Nombre = "Categoría Inactiva" }
        };

        var apiResponse = ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias);
        _mockApiService.Setup(x => x.GetAsync<List<CategoriaProductoDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _categoriasService.ObtenerCategoriasAsync(soloActivas: false);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        _mockApiService.Verify(x => x.GetAsync<List<CategoriaProductoDto>>("api/core/categorias?soloActivas=False", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerCategoriaAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var categoria = new CategoriaProductoDto
        {
            Id = categoriaId,
            Nombre = "Entradas",
            Descripcion = "Platos de entrada deliciosos",
            CantidadProductos = 15
        };

        var apiResponse = ApiResponse<CategoriaProductoDto>.SuccessResponse(categoria);
        _mockApiService.Setup(x => x.GetAsync<CategoriaProductoDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _categoriasService.ObtenerCategoriaAsync(categoriaId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(categoriaId, result.Data.Id);
        Assert.Equal("Entradas", result.Data.Nombre);
        Assert.Equal(15, result.Data.CantidadProductos);
        _mockApiService.Verify(x => x.GetAsync<CategoriaProductoDto>($"api/core/categorias/{categoriaId}", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerProductosPorCategoriaAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var productos = new List<ProductoDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Ensalada César", Precio = 12.50m, CategoriaId = categoriaId },
            new() { Id = Guid.NewGuid(), Nombre = "Sopa del Día", Precio = 8.00m, CategoriaId = categoriaId },
            new() { Id = Guid.NewGuid(), Nombre = "Bruschetta", Precio = 10.00m, CategoriaId = categoriaId }
        };

        var apiResponse = ApiResponse<List<ProductoDto>>.SuccessResponse(productos);
        _mockApiService.Setup(x => x.GetAsync<List<ProductoDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _categoriasService.ObtenerProductosPorCategoriaAsync(categoriaId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Count);
        Assert.All(result.Data, p => Assert.Equal(categoriaId, p.CategoriaId));
        _mockApiService.Verify(x => x.GetAsync<List<ProductoDto>>($"api/core/productos/categoria/{categoriaId}?soloActivos=True", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerProductosPorCategoriaAsync_WithInactivos_ShouldReturnSuccess()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var productos = new List<ProductoDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Producto Activo", CategoriaId = categoriaId },
            new() { Id = Guid.NewGuid(), Nombre = "Producto Inactivo", CategoriaId = categoriaId }
        };

        var apiResponse = ApiResponse<List<ProductoDto>>.SuccessResponse(productos);
        _mockApiService.Setup(x => x.GetAsync<List<ProductoDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _categoriasService.ObtenerProductosPorCategoriaAsync(categoriaId, soloActivos: false);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        _mockApiService.Verify(x => x.GetAsync<List<ProductoDto>>($"api/core/productos/categoria/{categoriaId}?soloActivos=False", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerCategoriasActivasAsync_ShouldReturnSuccess()
    {
        // Arrange
        var categorias = new List<CategoriaProductoDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Entradas", CantidadProductos = 10 },
            new() { Id = Guid.NewGuid(), Nombre = "Platos Principales", CantidadProductos = 25 },
            new() { Id = Guid.NewGuid(), Nombre = "Postres", CantidadProductos = 8 }
        };

        var apiResponse = ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias);
        _mockApiService.Setup(x => x.GetAsync<List<CategoriaProductoDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _categoriasService.ObtenerCategoriasActivasAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Count);
        _mockApiService.Verify(x => x.GetAsync<List<CategoriaProductoDto>>("api/core/categorias?soloActivas=true&ocultarVacias=true", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BuscarCategoriasAsync_WithValidName_ShouldReturnSuccess()
    {
        // Arrange
        var nombre = "entrada";
        var categorias = new List<CategoriaProductoDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Entradas", Descripcion = "Platos de entrada" },
            new() { Id = Guid.NewGuid(), Nombre = "Entrada Especial", Descripcion = "Entrada del chef" }
        };

        var apiResponse = ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias);
        _mockApiService.Setup(x => x.GetAsync<List<CategoriaProductoDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _categoriasService.BuscarCategoriasAsync(nombre);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.All(result.Data, c => Assert.Contains(nombre.ToLower(), c.Nombre.ToLower()));
        _mockApiService.Verify(x => x.GetAsync<List<CategoriaProductoDto>>($"api/core/categorias?nombre={nombre}", "test-token", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerCategoriasAsync_WithApiError_ShouldReturnFailure()
    {
        // Arrange
        var errorResponse = ApiResponse<List<CategoriaProductoDto>>.ErrorResponse(new List<string> { "Error de API" }, "Error de API", 500);
        _mockApiService.Setup(x => x.GetAsync<List<CategoriaProductoDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(errorResponse);

        // Act
        var result = await _categoriasService.ObtenerCategoriasAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de API", result.Error);
    }

    [Fact]
    public async Task ObtenerCategoriaAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        
        _mockApiService.Setup(x => x.GetAsync<CategoriaProductoDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _categoriasService.ObtenerCategoriaAsync(categoriaId);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error al obtener categoría", result.Error);
        Assert.Contains("Error al obtener categoría", result.Error);
    }

    [Fact]
    public async Task ObtenerProductosPorCategoriaAsync_WithEmptyResult_ShouldReturnSuccess()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var productos = new List<ProductoDto>();

        var apiResponse = ApiResponse<List<ProductoDto>>.SuccessResponse(productos);
        _mockApiService.Setup(x => x.GetAsync<List<ProductoDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _categoriasService.ObtenerProductosPorCategoriaAsync(categoriaId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
        _mockApiService.Verify(x => x.GetAsync<List<ProductoDto>>($"api/core/productos/categoria/{categoriaId}?soloActivos=True", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BuscarCategoriasAsync_WithEmptySearch_ShouldReturnSuccess()
    {
        // Arrange
        var nombre = "";
        var categorias = new List<CategoriaProductoDto>();

        var apiResponse = ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias);
        _mockApiService.Setup(x => x.GetAsync<List<CategoriaProductoDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _categoriasService.BuscarCategoriasAsync(nombre);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
        _mockApiService.Verify(x => x.GetAsync<List<CategoriaProductoDto>>($"api/core/categorias?nombre={nombre}", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerCategoriasActivasAsync_WithNoData_ShouldReturnSuccess()
    {
        // Arrange
        var categorias = new List<CategoriaProductoDto>();

        var apiResponse = ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(categorias);
        _mockApiService.Setup(x => x.GetAsync<List<CategoriaProductoDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _categoriasService.ObtenerCategoriasActivasAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
        _mockApiService.Verify(x => x.GetAsync<List<CategoriaProductoDto>>("api/core/categorias?soloActivas=true&ocultarVacias=true", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerCategoriaAsync_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var errorResponse = ApiResponse<CategoriaProductoDto>.ErrorResponse(new List<string> { "Categoría no encontrada" }, "Categoría no encontrada", 404);
        _mockApiService.Setup(x => x.GetAsync<CategoriaProductoDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(errorResponse);

        // Act
        var result = await _categoriasService.ObtenerCategoriaAsync(categoriaId);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Categoría no encontrada", result.Error);
    }

    // Tests para 401/403/429
    [Fact]
    public async Task ObtenerCategoriasAsync_WithUnauthorized_ShouldPropagate401()
    {
        // Arrange
        var apiResponse = ApiResponse<List<CategoriaProductoDto>>.ErrorResponse(new List<string> { "Unauthorized" }, "Unauthorized", 401);
        _mockApiService.Setup(x => x.GetAsync<List<CategoriaProductoDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _categoriasService.ObtenerCategoriasAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(401, result.StatusCode);
        Assert.Contains("Unauthorized", result.Error);
    }

    [Fact]
    public async Task ObtenerCategoriasAsync_WithForbidden_ShouldPropagate403()
    {
        // Arrange
        var apiResponse = ApiResponse<List<CategoriaProductoDto>>.ErrorResponse(new List<string> { "Forbidden" }, "Forbidden", 403);
        _mockApiService.Setup(x => x.GetAsync<List<CategoriaProductoDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _categoriasService.ObtenerCategoriasAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Forbidden", result.Error);
    }

    [Fact]
    public async Task ObtenerCategoriasAsync_WithTooManyRequests_ShouldPropagate429()
    {
        // Arrange
        var apiResponse = ApiResponse<List<CategoriaProductoDto>>.ErrorResponse(new List<string> { "Too Many Requests" }, "Too Many Requests", 429);
        _mockApiService.Setup(x => x.GetAsync<List<CategoriaProductoDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _categoriasService.ObtenerCategoriasAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(429, result.StatusCode);
        Assert.Contains("Too Many Requests", result.Error);
    }

    // Tests para 204/empty body
    [Fact]
    public async Task ObtenerCategoriasAsync_WithEmptyBody_ShouldReturnEmptyList()
    {
        // Arrange
        var apiResponse = ApiResponse<List<CategoriaProductoDto>>.SuccessResponse(new List<CategoriaProductoDto>());
        _mockApiService.Setup(x => x.GetAsync<List<CategoriaProductoDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _categoriasService.ObtenerCategoriasAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    // Tests para cancelación
    [Fact]
    public async Task ObtenerCategoriasAsync_WhenCancelled_ShouldReturnCancelled()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel the token immediately

        // Act
        var result = await _categoriasService.ObtenerCategoriasAsync(cancellationToken: cts.Token);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Operación cancelada por el usuario", result.Error);
        _mockApiService.Verify(x => x.GetAsync<List<CategoriaProductoDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuscarCategoriasAsync_WhenCancelled_ShouldReturnCancelled()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel the token immediately

        // Act
        var result = await _categoriasService.BuscarCategoriasAsync("test", cts.Token);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Operación cancelada por el usuario", result.Error);
        _mockApiService.Verify(x => x.GetAsync<List<CategoriaProductoDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
} 
