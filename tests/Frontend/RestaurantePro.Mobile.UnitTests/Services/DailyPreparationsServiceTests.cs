using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Common;
using RestaurantePro.Mobile.Core.Services;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;

namespace RestaurantePro.Mobile.UnitTests.Services;

public class DailyPreparationsServiceTests
{
    private readonly Mock<IApiService> _mockApiService;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<ILogger<DailyPreparationsService>> _mockLogger;
    private readonly DailyPreparationsService _dailyPreparationsService;

    public DailyPreparationsServiceTests()
    {
        _mockApiService = new Mock<IApiService>();
        _mockAuthService = new Mock<IAuthService>();
        _mockLogger = new Mock<ILogger<DailyPreparationsService>>();
        
        _dailyPreparationsService = new DailyPreparationsService(
            _mockApiService.Object,
            _mockAuthService.Object,
            _mockLogger.Object);
    }

    #region GetPreparacionesDiariasAsync Tests

    [Fact]
    public async Task GetPreparacionesDiariasAsync_WithValidResponse_ShouldReturnSuccess()
    {
        // Arrange
        var token = "test-token";
        var preparaciones = new List<PreparacionDiariaDto>
        {
            CreatePreparacionDiariaDto("11111111-1111-1111-1111-111111111111", "Pizza Margherita", 10),
            CreatePreparacionDiariaDto("22222222-2222-2222-2222-222222222222", "Pasta Carbonara", 5)
        };

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockApiService
            .Setup(x => x.GetAsync<List<PreparacionDiariaDto>>("api/operaciones/preparaciones-diarias", token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<PreparacionDiariaDto>>.SuccessResponse(preparaciones, "Success"));

        // Act
        var result = await _dailyPreparationsService.GetPreparacionesDiariasAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal("Pizza Margherita", result.Data[0].NombreProducto);
        Assert.Equal("Pasta Carbonara", result.Data[1].NombreProducto);
    }

    [Fact]
    public async Task GetPreparacionesDiariasAsync_WithFailedResponse_ShouldReturnFailure()
    {
        // Arrange
        var token = "test-token";
        var errorMessage = "Error del servidor";

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockApiService
            .Setup(x => x.GetAsync<List<PreparacionDiariaDto>>("api/operaciones/preparaciones-diarias", token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<PreparacionDiariaDto>>.ErrorResponse(errorMessage));

        // Act
        var result = await _dailyPreparationsService.GetPreparacionesDiariasAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(errorMessage, result.Error);
    }

    [Fact]
    public async Task GetPreparacionesDiariasAsync_WithException_ShouldReturnFailure()
    {
        // Arrange
        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _dailyPreparationsService.GetPreparacionesDiariasAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error: Test exception", result.Error);
    }

    #endregion

    #region GetPreparacionDiariaAsync Tests

    [Fact]
    public async Task GetPreparacionDiariaAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var token = "test-token";
        var preparacion = CreatePreparacionDiariaDto(id.ToString(), "Pizza Margherita", 10);

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockApiService
            .Setup(x => x.GetAsync<PreparacionDiariaDto>($"api/operaciones/preparaciones-diarias/{id}", token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PreparacionDiariaDto>.SuccessResponse(preparacion, "Success"));

        // Act
        var result = await _dailyPreparationsService.GetPreparacionDiariaAsync(id);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(id, result.Data.Id);
        Assert.Equal("Pizza Margherita", result.Data.NombreProducto);
    }

    [Fact]
    public async Task GetPreparacionDiariaAsync_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var token = "test-token";
        var errorMessage = "Preparación no encontrada";

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockApiService
            .Setup(x => x.GetAsync<PreparacionDiariaDto>($"api/operaciones/preparaciones-diarias/{id}", token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PreparacionDiariaDto>.ErrorResponse(errorMessage));

        // Act
        var result = await _dailyPreparationsService.GetPreparacionDiariaAsync(id);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(errorMessage, result.Error);
    }

    #endregion

    #region CrearPreparacionDiariaAsync Tests

    [Fact]
    public async Task CrearPreparacionDiariaAsync_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var token = "test-token";
        var command = new CrearPreparacionDiariaCommand
        {
            ProductoId = Guid.NewGuid(),
            Cantidad = 15,
            ChefId = Guid.NewGuid(),
            FechaVencimiento = DateTime.Now.AddDays(1),
            Observaciones = "Preparación especial"
        };

        var preparacion = CreatePreparacionDiariaDto(Guid.NewGuid().ToString(), "Producto Test", command.Cantidad);

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockApiService
            .Setup(x => x.PostAsync<PreparacionDiariaDto>("api/operaciones/preparaciones-diarias", command, token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PreparacionDiariaDto>.SuccessResponse(preparacion, "Success"));

        // Act
        var result = await _dailyPreparationsService.CrearPreparacionDiariaAsync(command);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(command.Cantidad, result.Data.CantidadPreparada);
    }

    [Fact]
    public async Task CrearPreparacionDiariaAsync_WithInvalidCommand_ShouldReturnFailure()
    {
        // Arrange
        var token = "test-token";
        var command = new CrearPreparacionDiariaCommand
        {
            ProductoId = Guid.Empty,
            Cantidad = 0,
            ChefId = Guid.Empty,
            FechaVencimiento = DateTime.Now.AddDays(-1)
        };

        var errorMessage = "Datos inválidos";

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockApiService
            .Setup(x => x.PostAsync<PreparacionDiariaDto>("api/operaciones/preparaciones-diarias", command, token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PreparacionDiariaDto>.ErrorResponse(errorMessage));

        // Act
        var result = await _dailyPreparationsService.CrearPreparacionDiariaAsync(command);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(errorMessage, result.Error);
    }

    #endregion

    #region ActualizarPreparacionDiariaAsync Tests

    [Fact]
    public async Task ActualizarPreparacionDiariaAsync_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var token = "test-token";
        var command = new ActualizarPreparacionDiariaCommand
        {
            ProductoId = Guid.NewGuid(),
            CantidadPreparada = 20,
            CantidadDisponible = 15,
            ChefId = Guid.NewGuid(),
            FechaVencimiento = DateTime.Now.AddDays(1),
            Observaciones = "Actualización"
        };

        var preparacion = CreatePreparacionDiariaDto(id.ToString(), "Producto Actualizado", command.CantidadPreparada);

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockApiService
            .Setup(x => x.PutAsync<PreparacionDiariaDto>($"api/operaciones/preparaciones-diarias/{id}", command, token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PreparacionDiariaDto>.SuccessResponse(preparacion, "Success"));

        // Act
        var result = await _dailyPreparationsService.ActualizarPreparacionDiariaAsync(id, command);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(command.CantidadPreparada, result.Data.CantidadPreparada);
    }

    [Fact]
    public async Task ActualizarPreparacionDiariaAsync_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var token = "test-token";
        var command = new ActualizarPreparacionDiariaCommand
        {
            ProductoId = Guid.NewGuid(),
            CantidadPreparada = 10,
            CantidadDisponible = 8,
            ChefId = Guid.NewGuid(),
            FechaVencimiento = DateTime.Now.AddDays(1)
        };

        var errorMessage = "Preparación no encontrada";

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockApiService
            .Setup(x => x.PutAsync<PreparacionDiariaDto>($"api/operaciones/preparaciones-diarias/{id}", command, token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PreparacionDiariaDto>.ErrorResponse(errorMessage));

        // Act
        var result = await _dailyPreparationsService.ActualizarPreparacionDiariaAsync(id, command);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(errorMessage, result.Error);
    }

    #endregion

    #region EliminarPreparacionDiariaAsync Tests

    [Fact]
    public async Task EliminarPreparacionDiariaAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var token = "test-token";

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockApiService
            .Setup(x => x.DeleteAsync($"api/operaciones/preparaciones-diarias/{id}", token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<bool>.SuccessResponse(true, "Success"));

        // Act
        var result = await _dailyPreparationsService.EliminarPreparacionDiariaAsync(id);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task EliminarPreparacionDiariaAsync_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var token = "test-token";
        var errorMessage = "Preparación no encontrada";

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockApiService
            .Setup(x => x.DeleteAsync($"api/operaciones/preparaciones-diarias/{id}", token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<bool>.ErrorResponse(errorMessage));

        // Act
        var result = await _dailyPreparationsService.EliminarPreparacionDiariaAsync(id);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(errorMessage, result.Error);
    }

    #endregion

    #region ConsumirPreparacionDiariaAsync Tests

    [Fact]
    public async Task ConsumirPreparacionDiariaAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var token = "test-token";
        var cantidad = 5;
        var observaciones = "Consumo para comanda";

        var preparacion = CreatePreparacionDiariaDto(id.ToString(), "Producto Test", 10);

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockApiService
            .Setup(x => x.PostAsync<PreparacionDiariaDto>($"api/operaciones/preparaciones-diarias/{id}/consumir", It.IsAny<object>(), token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PreparacionDiariaDto>.SuccessResponse(preparacion, "Success"));

        // Act
        var result = await _dailyPreparationsService.ConsumirPreparacionDiariaAsync(id, cantidad, observaciones);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ConsumirPreparacionDiariaAsync_WithInvalidCantidad_ShouldReturnFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var token = "test-token";
        var cantidad = 0;
        var errorMessage = "Cantidad inválida";

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockApiService
            .Setup(x => x.PostAsync<PreparacionDiariaDto>($"api/operaciones/preparaciones-diarias/{id}/consumir", It.IsAny<object>(), token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PreparacionDiariaDto>.ErrorResponse(errorMessage));

        // Act
        var result = await _dailyPreparationsService.ConsumirPreparacionDiariaAsync(id, cantidad);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(errorMessage, result.Error);
    }

    #endregion

    #region MarcarComoDisponibleAsync Tests

    [Fact]
    public async Task MarcarComoDisponibleAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var token = "test-token";
        var preparacion = CreatePreparacionDiariaDto(id.ToString(), "Producto Test", 10);

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockApiService
            .Setup(x => x.PostAsync<PreparacionDiariaDto>($"api/operaciones/preparaciones-diarias/{id}/disponible", It.IsAny<object>(), token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PreparacionDiariaDto>.SuccessResponse(preparacion, "Success"));

        // Act
        var result = await _dailyPreparationsService.MarcarComoDisponibleAsync(id);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task MarcarComoDisponibleAsync_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var token = "test-token";
        var errorMessage = "Preparación no encontrada";

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockApiService
            .Setup(x => x.PostAsync<PreparacionDiariaDto>($"api/operaciones/preparaciones-diarias/{id}/disponible", It.IsAny<object>(), token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<PreparacionDiariaDto>.ErrorResponse(errorMessage));

        // Act
        var result = await _dailyPreparationsService.MarcarComoDisponibleAsync(id);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(errorMessage, result.Error);
    }

    #endregion

    #region GetPreparacionesDiariasPorEstadoAsync Tests

    [Fact]
    public async Task GetPreparacionesDiariasPorEstadoAsync_WithValidEstado_ShouldReturnSuccess()
    {
        // Arrange
        var estado = "Disponible";
        var token = "test-token";
        var preparaciones = new List<PreparacionDiariaDto>
        {
            CreatePreparacionDiariaDto("11111111-1111-1111-1111-111111111111", "Pizza Margherita", 10)
        };

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockApiService
            .Setup(x => x.GetAsync<List<PreparacionDiariaDto>>($"api/operaciones/preparaciones-diarias/por-estado?estado={estado}", token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<PreparacionDiariaDto>>.SuccessResponse(preparaciones, "Success"));

        // Act
        var result = await _dailyPreparationsService.GetPreparacionesDiariasPorEstadoAsync(estado);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
    }

    #endregion

    #region GetPreparacionesDiariasPorProductoAsync Tests

    [Fact]
    public async Task GetPreparacionesDiariasPorProductoAsync_WithValidProductoId_ShouldReturnSuccess()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var token = "test-token";
        var preparaciones = new List<PreparacionDiariaDto>
        {
            CreatePreparacionDiariaDto("11111111-1111-1111-1111-111111111111", "Producto Test", 10)
        };

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockApiService
            .Setup(x => x.GetAsync<List<PreparacionDiariaDto>>($"api/operaciones/preparaciones-diarias/por-producto/{productoId}", token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<PreparacionDiariaDto>>.SuccessResponse(preparaciones, "Success"));

        // Act
        var result = await _dailyPreparationsService.GetPreparacionesDiariasPorProductoAsync(productoId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
    }

    #endregion

    #region GetEstadisticasAsync Tests

    [Fact]
    public async Task GetEstadisticasAsync_WithValidResponse_ShouldReturnSuccess()
    {
        // Arrange
        var token = "test-token";
        var estadisticas = new EstadisticasPreparacionesDiariasDto
        {
            TotalPreparaciones = 10,
            PreparacionesDisponibles = 7,
            CantidadConsumida = 3,
            PreparacionesVencidas = 0
        };

        _mockAuthService
            .Setup(x => x.GetTokenAsync())
            .ReturnsAsync(token);

        _mockApiService
            .Setup(x => x.GetAsync<EstadisticasPreparacionesDiariasDto>("api/operaciones/preparaciones-diarias/estadisticas", token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<EstadisticasPreparacionesDiariasDto>.SuccessResponse(estadisticas, "Success"));

        // Act
        var result = await _dailyPreparationsService.GetEstadisticasAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(10, result.Data.TotalPreparaciones);
        Assert.Equal(7, result.Data.PreparacionesDisponibles);
    }

    #endregion

    #region Helper Methods

    private PreparacionDiariaDto CreatePreparacionDiariaDto(string id, string productoNombre, int cantidad)
    {
        return new PreparacionDiariaDto
        {
            Id = Guid.Parse(id),
            ProductoId = Guid.NewGuid(),
            NombreProducto = productoNombre,
            CantidadPreparada = cantidad,
            CantidadDisponible = cantidad,
            Estado = "Disponible",
            FechaPreparacion = DateTime.Now,
            FechaVencimiento = DateTime.Now.AddDays(1),
            ChefId = Guid.NewGuid(),
            NombreChef = "Chef Test",
            Observaciones = "Preparación de prueba"
        };
    }

    #endregion
}
