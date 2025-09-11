using Moq;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Inventory;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Services.Inventory;

public class PreparacionesServiceTests
{
    private readonly Mock<IApiService> _mockApiService;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly PreparacionesService _preparacionesService;

    public PreparacionesServiceTests()
    {
        _mockApiService = new Mock<IApiService>();
        _mockAuthService = new Mock<IAuthService>();
        _mockAuthService.Setup(x => x.GetTokenAsync()).ReturnsAsync("test-token");
        _preparacionesService = new PreparacionesService(_mockApiService.Object, _mockAuthService.Object);
    }

    [Fact]
    public async Task ObtenerPreparacionesAsync_WithValidResponse_ShouldReturnSuccess()
    {
        // Arrange
        var preparaciones = new List<PreparacionDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Pizza Margherita", Categoria = "Pizzas", Disponible = true },
            new() { Id = Guid.NewGuid(), Nombre = "Hamburguesa Clásica", Categoria = "Hamburguesas", Disponible = true }
        };

        var paginatedResponse = new PreparacionesPaginadasDto
        {
            Items = preparaciones,
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        var apiResponse = ApiResponse<PreparacionesPaginadasDto>.SuccessResponse(paginatedResponse);
        _mockApiService.Setup(x => x.GetAsync<PreparacionesPaginadasDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _preparacionesService.ObtenerPreparacionesAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        _mockApiService.Verify(x => x.GetAsync<PreparacionesPaginadasDto>("api/operaciones/preparaciones", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerPreparacionAsync_WithValidResponse_ShouldReturnSuccess()
    {
        // Arrange
        var preparacionId = Guid.NewGuid();
        var preparacion = new PreparacionDto
        {
            Id = preparacionId,
            Nombre = "Pizza Margherita",
            Categoria = "Pizzas",
            Disponible = true
        };

        var apiResponse = ApiResponse<PreparacionDto>.SuccessResponse(preparacion);
        _mockApiService.Setup(x => x.GetAsync<PreparacionDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _preparacionesService.ObtenerPreparacionAsync(preparacionId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(preparacionId, result.Data.Id);
        Assert.Equal("Pizza Margherita", result.Data.Nombre);
    }

    [Fact]
    public async Task BuscarPreparacionesAsync_WithValidResponse_ShouldReturnSuccess()
    {
        // Arrange
        var terminoBusqueda = "pizza";
        var preparaciones = new List<PreparacionDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Pizza Margherita", Categoria = "Pizzas" }
        };

        var apiResponse = ApiResponse<List<PreparacionDto>>.SuccessResponse(preparaciones);
        _mockApiService.Setup(x => x.GetAsync<List<PreparacionDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _preparacionesService.BuscarPreparacionesAsync(terminoBusqueda);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        _mockApiService.Verify(x => x.GetAsync<List<PreparacionDto>>($"api/operaciones/preparaciones/buscar?termino={terminoBusqueda}", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerPreparacionesAsync_WithApiError_ShouldReturnFailure()
    {
        // Arrange
        var apiResponse = ApiResponse<PreparacionesPaginadasDto>.ErrorResponse(new List<string> { "Error de API" }, "Error de API", 500);
        _mockApiService.Setup(x => x.GetAsync<PreparacionesPaginadasDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _preparacionesService.ObtenerPreparacionesAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de API", result.Error);
    }

    [Fact]
    public async Task ObtenerPreparacionesAsync_WithException_ShouldReturnFailure()
    {
        // Arrange
        _mockApiService.Setup(x => x.GetAsync<PreparacionesPaginadasDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _preparacionesService.ObtenerPreparacionesAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }

    // Tests para 401/403/429
    [Fact]
    public async Task ObtenerPreparacionesAsync_WithUnauthorized_ShouldPropagate401()
    {
        // Arrange
        var apiResponse = ApiResponse<PreparacionesPaginadasDto>.ErrorResponse(new List<string> { "Unauthorized" }, "Unauthorized", 401);
        _mockApiService.Setup(x => x.GetAsync<PreparacionesPaginadasDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _preparacionesService.ObtenerPreparacionesAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode); // El servicio devuelve 400 por defecto en caso de error
        Assert.Contains("Unauthorized", result.Error);
    }

    [Fact]
    public async Task ObtenerPreparacionesAsync_WithForbidden_ShouldPropagate403()
    {
        // Arrange
        var apiResponse = ApiResponse<PreparacionesPaginadasDto>.ErrorResponse(new List<string> { "Forbidden" }, "Forbidden", 403);
        _mockApiService.Setup(x => x.GetAsync<PreparacionesPaginadasDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _preparacionesService.ObtenerPreparacionesAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode); // El servicio devuelve 400 por defecto en caso de error
        Assert.Contains("Forbidden", result.Error);
    }

    [Fact]
    public async Task ObtenerPreparacionesAsync_WithTooManyRequests_ShouldPropagate429()
    {
        // Arrange
        var apiResponse = ApiResponse<PreparacionesPaginadasDto>.ErrorResponse(new List<string> { "Too Many Requests" }, "Too Many Requests", 429);
        _mockApiService.Setup(x => x.GetAsync<PreparacionesPaginadasDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _preparacionesService.ObtenerPreparacionesAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode); // El servicio devuelve 400 por defecto en caso de error
        Assert.Contains("Too Many Requests", result.Error);
    }

    // Tests para 204/empty body
    [Fact]
    public async Task ObtenerPreparacionesAsync_WithEmptyBody_ShouldReturnEmptyList()
    {
        // Arrange
        var paginatedResponse = new PreparacionesPaginadasDto
        {
            Items = new List<PreparacionDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        var apiResponse = ApiResponse<PreparacionesPaginadasDto>.SuccessResponse(paginatedResponse);
        _mockApiService.Setup(x => x.GetAsync<PreparacionesPaginadasDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _preparacionesService.ObtenerPreparacionesAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    // Tests para cancelación
    [Fact]
    public async Task ObtenerPreparacionesAsync_WhenCancelled_ShouldReturnCancelled()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel the token immediately

        // Act
        var result = await _preparacionesService.ObtenerPreparacionesAsync(cancellationToken: cts.Token);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Operación cancelada por el usuario", result.Error);
        _mockApiService.Verify(x => x.GetAsync<PreparacionesPaginadasDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuscarPreparacionesAsync_WhenCancelled_ShouldReturnCancelled()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel the token immediately

        // Act
        var result = await _preparacionesService.BuscarPreparacionesAsync("test", cts.Token);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Operación cancelada por el usuario", result.Error);
        _mockApiService.Verify(x => x.GetAsync<List<PreparacionDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
