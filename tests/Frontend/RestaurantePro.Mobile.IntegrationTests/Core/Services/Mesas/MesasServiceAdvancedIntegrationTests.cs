using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Platform;
using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services.Mesas;

public class MesasServiceAdvancedIntegrationTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;
    private readonly HttpClient _client;
    private IMesasService _mesasService;
    private IAuthService _authService;

    public MesasServiceAdvancedIntegrationTests(MobileIntegrationTestFixture fixture)
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
        _mesasService = new MesasService(apiService, authService);
        _authService = authService;
    }

    private async Task SetupAsync()
    {
        // Hacer login automático para todos los tests
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        if (!loginResult.Success)
        {
            throw new InvalidOperationException($"Login falló: {loginResult.Message}");
        }
    }

    private async Task DisposeAsync()
    {
        // Limpiar estado entre tests
        await _authService.LogoutAsync();
    }

    [Fact]
    public async Task ObtenerMesaAsync_WithValidId_ShouldReturnMesa()
    {
        // Arrange - Login automático
        await SetupAsync();
        
        // Obtener una mesa para tener un ID válido
        var mesasResult = await _mesasService.ObtenerMesasAsync();
        Assert.True(mesasResult.Success);
        var mesaId = mesasResult.Data.First().Id;

        // Act
        var result = await _mesasService.ObtenerMesaAsync(mesaId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(mesaId, result.Data.Id);
    }

    [Fact]
    public async Task ObtenerMesaAsync_WithInvalidId_ShouldReturnError()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _mesasService.ObtenerMesaAsync(invalidId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    public async Task ObtenerMesasAsync_WithCapacidadMinimaFilter_ShouldReturnFilteredMesas()
    {
        // Arrange - Login automático
        await SetupAsync();
        
        var capacidadMinima = 4;

        // Act
        var result = await _mesasService.ObtenerMesasAsync(capacidadMinima: capacidadMinima);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.All(m => m.Capacidad >= capacidadMinima));
    }

    [Fact]
    public async Task ObtenerMesasAsync_WithUbicacionFilter_ShouldReturnFilteredMesas()
    {
        // Arrange - Login automático
        await SetupAsync();
        
        var ubicacion = "Interior";

        // Act
        var result = await _mesasService.ObtenerMesasAsync(ubicacion: ubicacion);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.All(m => m.Zona == ubicacion));
    }

    [Fact]
    public async Task ObtenerMesasDisponiblesAsync_WithCapacidadMinima_ShouldReturnFilteredAvailableMesas()
    {
        // Arrange - Login automático
        await SetupAsync();
        
        var capacidadMinima = 2;

        // Act
        var result = await _mesasService.ObtenerMesasDisponiblesAsync(capacidadMinima: capacidadMinima);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.All(m => m.Estado == "Disponible" && m.Capacidad >= capacidadMinima));
    }

    [Fact]
    public async Task ObtenerEstadoOcupacionAsync_ShouldReturnValidStatistics()
    {
        // Arrange - Login automático
        await SetupAsync();
        
        // Act
        var result = await _mesasService.ObtenerEstadoOcupacionAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.TotalMesas > 0);
        Assert.True(result.Data.Estadisticas.MesasDisponibles >= 0);
        Assert.True(result.Data.Estadisticas.MesasOcupadas >= 0);
        Assert.True(result.Data.Estadisticas.MesasReservadas >= 0);
        
        // Verificar que las estadísticas son coherentes
        var sumaEstadisticas = result.Data.Estadisticas.MesasDisponibles + 
                              result.Data.Estadisticas.MesasOcupadas + 
                              result.Data.Estadisticas.MesasReservadas;
        
        // La suma debe ser menor o igual al total (puede haber mesas en otros estados)
        Assert.True(sumaEstadisticas <= result.Data.TotalMesas, 
            $"Suma de estadísticas ({sumaEstadisticas}) debe ser menor o igual al total ({result.Data.TotalMesas})");
        
        // Verificar que al menos hay una mesa disponible
        Assert.True(result.Data.Estadisticas.MesasDisponibles > 0, 
            "Debe haber al menos una mesa disponible");
    }

    [Fact]
    public async Task FlujoCompleto_Mesas_ShouldWorkEndToEnd()
    {
        // Arrange - Login automático
        await SetupAsync();
        
        // 1. Obtener estado inicial
        var estadoInicial = await _mesasService.ObtenerEstadoOcupacionAsync();
        Assert.True(estadoInicial.Success);

        // 2. Obtener mesas disponibles
        var mesasDisponibles = await _mesasService.ObtenerMesasDisponiblesAsync();
        Assert.True(mesasDisponibles.Success);
        Assert.True(mesasDisponibles.Data.Count > 0);

        // 3. Obtener una mesa específica
        var mesaId = mesasDisponibles.Data.First().Id;
        var mesa = await _mesasService.ObtenerMesaAsync(mesaId);
        Assert.True(mesa.Success);
        Assert.Equal(mesaId, mesa.Data.Id);

        // 4. Verificar que la mesa está disponible
        Assert.Equal("Disponible", mesa.Data.Estado);
    }

    [Fact]
    public async Task ObtenerMesasAsync_WithMultipleFilters_ShouldReturnFilteredMesas()
    {
        // Arrange - Login automático
        await SetupAsync();
        
        var estado = "Disponible";
        var ubicacion = "Interior";
        var capacidadMinima = 2;

        // Act
        var result = await _mesasService.ObtenerMesasAsync(
            estado: estado, 
            ubicacion: ubicacion, 
            capacidadMinima: capacidadMinima);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.All(m => 
            m.Estado == estado && 
            m.Zona == ubicacion && 
            m.Capacidad >= capacidadMinima));
    }

    [Fact]
    public async Task ObtenerMesasAsync_WithInvalidEstado_ShouldReturnEmptyOrError()
    {
        // Arrange
        var estadoInvalido = "EstadoInexistente";

        // Act
        var result = await _mesasService.ObtenerMesasAsync(estado: estadoInvalido);

        // Assert
        Assert.NotNull(result);
        // Puede retornar lista vacía o error, ambos son válidos
        if (result.Success)
        {
            Assert.True(result.Data.Count == 0);
        }
        else
        {
            Assert.NotNull(result.Errors);
        }
    }

    [Fact]
    public async Task ObtenerMesasAsync_WithInvalidUbicacion_ShouldReturnEmptyOrError()
    {
        // Arrange
        var ubicacionInvalida = "UbicacionInexistente";

        // Act
        var result = await _mesasService.ObtenerMesasAsync(ubicacion: ubicacionInvalida);

        // Assert
        Assert.NotNull(result);
        // Puede retornar lista vacía o error, ambos son válidos
        if (result.Success)
        {
            Assert.True(result.Data.Count == 0);
        }
        else
        {
            Assert.NotNull(result.Errors);
        }
    }

    [Fact]
    public async Task ObtenerMesasAsync_WithNegativeCapacidadMinima_ShouldHandleGracefully()
    {
        // Arrange
        var capacidadNegativa = -1;

        // Act
        var result = await _mesasService.ObtenerMesasAsync(capacidadMinima: capacidadNegativa);

        // Assert
        Assert.NotNull(result);
        // El backend valida y rechaza valores negativos
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    public async Task ObtenerMesasAsync_WithVeryHighCapacidadMinima_ShouldReturnEmptyOrError()
    {
        // Arrange
        var capacidadMuyAlta = 100;

        // Act
        var result = await _mesasService.ObtenerMesasAsync(capacidadMinima: capacidadMuyAlta);

        // Assert
        Assert.NotNull(result);
        // Debería retornar lista vacía o error para capacidades muy altas
        if (result.Success)
        {
            Assert.True(result.Data.Count == 0);
        }
        else
        {
            Assert.NotNull(result.Errors);
        }
    }
} 
 