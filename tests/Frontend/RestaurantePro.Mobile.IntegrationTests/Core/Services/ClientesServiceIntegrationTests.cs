using RestaurantePro.Mobile.Core.Services.Commercial;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Platform;
using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services.Clientes;

public class ClientesServiceIntegrationTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;
    private readonly HttpClient _client;
    private IClientesService _clientesService;
    private IAuthService _authService;

    public ClientesServiceIntegrationTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
        Setup();
    }

    private void Setup()
    {
        // Crear servicios móviles localmente para evitar conflictos con el backend
        var httpClient = _client;
        var apiService = new ApiService(httpClient);
        var authService = new AuthService(apiService, NullLogger<AuthService>.Instance, new FakeSecureStorageService());
        _clientesService = new ClientesService(apiService);
        _authService = authService;
    }

    [Fact]
    public async Task ObtenerClientesAsync_WithValidAuth_ShouldReturnClientes()
    {
        // Act
        var result = await _clientesService.ObtenerClientesAsync();

        // Assert
        Assert.True(result.Succeeded, $"Error: {result.Error}");
        Assert.NotNull(result.Data);
        Assert.IsType<List<ClienteSummaryDto>>(result.Data);
    }

    [Fact]
    public async Task ObtenerClienteAsync_WithValidId_ShouldReturnCliente()
    {
        // Arrange
        var clientesResult = await _clientesService.ObtenerClientesAsync();
        Assert.True(clientesResult.Succeeded);
        Assert.True(clientesResult.Data.Count > 0, "No hay clientes para testear");

        var clienteId = clientesResult.Data.First().Id;

        // Act
        var result = await _clientesService.ObtenerClienteAsync(clienteId);

        // Assert
        Assert.True(result.Succeeded, $"Error: {result.Error}");
        Assert.NotNull(result.Data);
        Assert.Equal(clienteId, result.Data.Id);
    }

    [Fact]
    public async Task ObtenerClienteAsync_WithInvalidId_ShouldReturnError()
    {
        // Arrange
        var invalidId = Guid.NewGuid(); // Usar un GUID válido pero inexistente

        // Act
        var result = await _clientesService.ObtenerClienteAsync(invalidId);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task BuscarClientesAsync_WithValidQuery_ShouldReturnFilteredResults()
    {
        // Arrange
        var searchQuery = "test";

        // Act
        var result = await _clientesService.BuscarClientesAsync(searchQuery);

        // Assert
        Assert.True(result.Succeeded, $"Error: {result.Error}");
        Assert.NotNull(result.Data);
        Assert.IsType<List<ClienteSummaryDto>>(result.Data);
    }

    [Fact]
    public async Task BuscarClientesAsync_WithEmptyQuery_ShouldReturnAllClientes()
    {
        // Arrange
        var searchQuery = "";

        // Act
        var result = await _clientesService.BuscarClientesAsync(searchQuery);

        // Assert
        Assert.True(result.Succeeded, $"Error: {result.Error}");
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ObtenerClientesFrecuentesAsync_ShouldReturnFrequentClientes()
    {
        // Act
        var result = await _clientesService.ObtenerClientesFrecuentesAsync();

        // Assert
        Assert.True(result.Succeeded, $"Error: {result.Error}");
        Assert.NotNull(result.Data);
        Assert.IsType<List<ClienteSummaryDto>>(result.Data);
    }

    [Fact]
    public async Task ObtenerHistorialComandasAsync_WithValidClienteId_ShouldReturnComandas()
    {
        // Arrange
        var clientesResult = await _clientesService.ObtenerClientesAsync();
        Assert.True(clientesResult.Succeeded);
        Assert.True(clientesResult.Data.Count > 0, "No hay clientes para testear");

        var clienteId = clientesResult.Data.First().Id;

        // Act
        var result = await _clientesService.ObtenerHistorialComandasAsync(clienteId);

        // Assert
        Assert.True(result.Succeeded, $"Error: {result.Error}");
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ObtenerHistorialComandasAsync_WithInvalidClienteId_ShouldReturnError()
    {
        // Arrange
        var invalidClienteId = Guid.NewGuid(); // Usar un GUID válido pero inexistente

        // Act
        var result = await _clientesService.ObtenerHistorialComandasAsync(invalidClienteId);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task ObtenerClientesConTarjetaFidelizacionAsync_ShouldReturnClientesWithLoyalty()
    {
        // Act
        var result = await _clientesService.ObtenerClientesConTarjetaFidelizacionAsync();

        // Assert
        Assert.True(result.Succeeded, $"Error: {result.Error}");
        Assert.NotNull(result.Data);
        Assert.IsType<List<ClienteSummaryDto>>(result.Data);
    }

    [Fact]
    public async Task Service_WithExpiredToken_ShouldHandleAuthError()
    {
        // Arrange - Forzar expiración de token
        await _authService.LogoutAsync();

        // Act
        var result = await _clientesService.ObtenerClientesAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.True(result.Error.Contains("401") || result.Error.Contains("Unauthorized") || result.Error.Contains("autenticación"));
    }
} 