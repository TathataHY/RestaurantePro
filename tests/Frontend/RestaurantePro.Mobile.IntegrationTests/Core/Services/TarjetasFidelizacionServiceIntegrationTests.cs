using RestaurantePro.Mobile.Core.Services.Commercial;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Platform;
using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services.TarjetasFidelizacion;

public class TarjetasFidelizacionServiceIntegrationTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;
    private readonly HttpClient _client;
    private ITarjetasFidelizacionService _tarjetasFidelizacionService;
    private IAuthService _authService;

    public TarjetasFidelizacionServiceIntegrationTests(MobileIntegrationTestFixture fixture)
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
        _tarjetasFidelizacionService = new TarjetasFidelizacionService(apiService);
        _authService = authService;
    }

    [Fact]
    public async Task ObtenerTarjetasActivasAsync_WithValidAuth_ShouldReturnTarjetas()
    {
        // Act
        var result = await _tarjetasFidelizacionService.ObtenerTarjetasActivasAsync();

        // Assert
        Assert.True(result.Succeeded, $"Error: {result.Error}");
        Assert.NotNull(result.Data);
        Assert.IsType<List<TarjetaFidelizacionDto>>(result.Data);
    }

    [Fact]
    public async Task ObtenerTarjetaAsync_WithValidId_ShouldReturnTarjeta()
    {
        // Arrange
        var tarjetasResult = await _tarjetasFidelizacionService.ObtenerTarjetasActivasAsync();
        Assert.True(tarjetasResult.Succeeded);
        Assert.True(tarjetasResult.Data.Count > 0, "No hay tarjetas para testear");

        var tarjetaId = tarjetasResult.Data.First().Id;

        // Act
        var result = await _tarjetasFidelizacionService.ObtenerTarjetaAsync(tarjetaId);

        // Assert
        Assert.True(result.Succeeded, $"Error: {result.Error}");
        Assert.NotNull(result.Data);
        Assert.Equal(tarjetaId, result.Data.Id);
    }

    [Fact]
    public async Task ObtenerTarjetaAsync_WithInvalidId_ShouldReturnError()
    {
        // Arrange
        var invalidId = Guid.NewGuid(); // Usar un GUID válido pero inexistente

        // Act
        var result = await _tarjetasFidelizacionService.ObtenerTarjetaAsync(invalidId);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
    }



    [Fact]
    public async Task BuscarTarjetaAsync_WithValidNumber_ShouldReturnTarjeta()
    {
        // Arrange
        var tarjetasResult = await _tarjetasFidelizacionService.ObtenerTarjetasActivasAsync();
        Assert.True(tarjetasResult.Succeeded);
        Assert.True(tarjetasResult.Data.Count > 0, "No hay tarjetas para testear");

        var numeroTarjeta = tarjetasResult.Data.First().NumeroTarjeta;

        // Act
        var result = await _tarjetasFidelizacionService.BuscarTarjetaAsync(numeroTarjeta);

        // Assert
        Assert.True(result.Succeeded, $"Error: {result.Error}");
        Assert.NotNull(result.Data);
        Assert.Equal(numeroTarjeta, result.Data.NumeroTarjeta);
    }

    [Fact]
    public async Task BuscarTarjetaAsync_WithInvalidNumber_ShouldReturnError()
    {
        // Arrange
        var invalidNumber = "999999999";

        // Act
        var result = await _tarjetasFidelizacionService.BuscarTarjetaAsync(invalidNumber);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task AcumularPuntosAsync_WithValidData_ShouldUpdatePuntos()
    {
        // Arrange
        var tarjetasResult = await _tarjetasFidelizacionService.ObtenerTarjetasActivasAsync();
        Assert.True(tarjetasResult.Succeeded);
        Assert.True(tarjetasResult.Data.Count > 0, "No hay tarjetas para testear");

        var tarjeta = tarjetasResult.Data.First();
        var puntosOriginales = tarjeta.PuntosDisponibles;
        var montoCompra = 100.0m; // Monto para acumular puntos

        // Act
        var result = await _tarjetasFidelizacionService.AcumularPuntosAsync(tarjeta.Id, montoCompra);

        // Assert
        Assert.True(result.Succeeded, $"Error: {result.Error}");
        Assert.NotNull(result.Data);
        Assert.True(result.Data.PuntosDisponibles >= puntosOriginales, "Los puntos deberían haberse incrementado");
    }

    [Fact]
    public async Task AcumularPuntosAsync_WithInvalidTarjetaId_ShouldReturnError()
    {
        // Arrange
        var invalidTarjetaId = Guid.NewGuid();
        var montoCompra = 100.0m;

        // Act
        var result = await _tarjetasFidelizacionService.AcumularPuntosAsync(invalidTarjetaId, montoCompra);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task CanjearPuntosAsync_WithValidData_ShouldUpdatePuntos()
    {
        // Arrange
        var tarjetasResult = await _tarjetasFidelizacionService.ObtenerTarjetasActivasAsync();
        Assert.True(tarjetasResult.Succeeded);
        Assert.True(tarjetasResult.Data.Count > 0, "No hay tarjetas para testear");

        var tarjeta = tarjetasResult.Data.First();
        var puntosOriginales = tarjeta.PuntosDisponibles;
        var puntosCanjear = Math.Min(5, puntosOriginales); // Asegurar que no exceda los puntos disponibles
        var descuento = 10.0m; // Descuento en pesos

        // Act
        var result = await _tarjetasFidelizacionService.CanjearPuntosAsync(tarjeta.Id, puntosCanjear, descuento);

        // Assert
        Assert.True(result.Succeeded, $"Error: {result.Error}");
        Assert.NotNull(result.Data);
        Assert.True(result.Data.PuntosDisponibles <= puntosOriginales, "Los puntos deberían haberse reducido");
    }

    [Fact]
    public async Task CanjearPuntosAsync_WithInsufficientPuntos_ShouldReturnError()
    {
        // Arrange
        var tarjetasResult = await _tarjetasFidelizacionService.ObtenerTarjetasActivasAsync();
        Assert.True(tarjetasResult.Succeeded);
        Assert.True(tarjetasResult.Data.Count > 0, "No hay tarjetas para testear");

        var tarjeta = tarjetasResult.Data.First();
        var puntosExcesivos = tarjeta.PuntosDisponibles + 1000; // Más puntos de los disponibles
        var descuento = 10.0m;

        // Act
        var result = await _tarjetasFidelizacionService.CanjearPuntosAsync(tarjeta.Id, puntosExcesivos, descuento);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task ObtenerHistorialTransaccionesAsync_WithValidTarjetaId_ShouldReturnTransacciones()
    {
        // Arrange
        var tarjetasResult = await _tarjetasFidelizacionService.ObtenerTarjetasActivasAsync();
        Assert.True(tarjetasResult.Succeeded);
        Assert.True(tarjetasResult.Data.Count > 0, "No hay tarjetas para testear");

        var tarjetaId = tarjetasResult.Data.First().Id;

        // Act
        var result = await _tarjetasFidelizacionService.ObtenerHistorialTransaccionesAsync(tarjetaId);

        // Assert
        Assert.True(result.Succeeded, $"Error: {result.Error}");
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ObtenerHistorialTransaccionesAsync_WithInvalidTarjetaId_ShouldReturnError()
    {
        // Arrange
        var invalidTarjetaId = Guid.NewGuid();

        // Act
        var result = await _tarjetasFidelizacionService.ObtenerHistorialTransaccionesAsync(invalidTarjetaId);

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task ObtenerTarjetasActivasAsync_ShouldReturnActiveTarjetas()
    {
        // Act
        var result = await _tarjetasFidelizacionService.ObtenerTarjetasActivasAsync();

        // Assert
        Assert.True(result.Succeeded, $"Error: {result.Error}");
        Assert.NotNull(result.Data);
        Assert.IsType<List<TarjetaFidelizacionDto>>(result.Data);
    }

    [Fact]
    public async Task Service_WithExpiredToken_ShouldHandleAuthError()
    {
        // Arrange - Forzar expiración de token
        await _authService.LogoutAsync();

        // Act
        var result = await _tarjetasFidelizacionService.ObtenerTarjetasActivasAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.True(result.Error.Contains("401") || result.Error.Contains("Unauthorized") || result.Error.Contains("autenticación"));
    }
} 