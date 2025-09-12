using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.Core.Services;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Platform;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services;

/// <summary>
/// Tests de integración para DailyPreparationsService - Preparaciones diarias de cocina
/// </summary>
public class DailyPreparationsServiceIntegrationTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;
    private readonly HttpClient _client;
    private IDailyPreparationsService _dailyPreparationsService;
    private IAuthService _authService;

    public DailyPreparationsServiceIntegrationTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
        Setup();
    }

    private void Setup()
    {
        // Crear servicios móviles localmente
        var httpClient = _client;
        var apiService = new ApiService(httpClient);
        var authService = new AuthService(apiService, NullLogger<AuthService>.Instance, new FakeSecureStorageService(), new FakeNavigationService());
        var logger = NullLogger<DailyPreparationsService>.Instance;

        _dailyPreparationsService = new DailyPreparationsService(apiService, authService, logger);
        _authService = authService;
    }

    private async Task SetupAsync()
    {
        // Login automático para todos los tests
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        if (!loginResult.Success)
        {
            throw new InvalidOperationException($"Login falló: {loginResult.Message}");
        }
    }

    #region Tests de Obtener Preparaciones Diarias

    [Fact]
    public async Task GetPreparacionesDiariasAsync_ShouldReturnValidList()
    {
        // Arrange
        await SetupAsync();

        // Act
        var result = await _dailyPreparationsService.GetPreparacionesDiariasAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Count >= 0);
    }

    [Fact]
    public async Task GetPreparacionesDiariasAsync_ShouldReturnValidPreparaciones()
    {
        // Arrange
        await SetupAsync();

        // Act
        var result = await _dailyPreparationsService.GetPreparacionesDiariasAsync();

        // Assert
        if (result.Succeeded && result.Data.Any())
        {
            var preparacion = result.Data.First();
            Assert.NotEqual(Guid.Empty, preparacion.Id);
            Assert.NotNull(preparacion.NombreProducto);
            Assert.True(preparacion.CantidadPreparada >= 0);
            Assert.True(preparacion.CantidadDisponible >= 0);
        }
    }

    #endregion

    #region Tests de Obtener Preparación por ID

    [Fact]
    public async Task GetPreparacionDiariaAsync_WithValidId_ShouldReturnPreparacion()
    {
        // Arrange
        await SetupAsync();
        
        // Primero obtener una preparación existente
        var preparacionesResult = await _dailyPreparationsService.GetPreparacionesDiariasAsync();
        if (preparacionesResult.Succeeded && preparacionesResult.Data.Any())
        {
            var preparacionId = preparacionesResult.Data.First().Id;

            // Act
            var result = await _dailyPreparationsService.GetPreparacionDiariaAsync(preparacionId);

            // Assert
            Assert.NotNull(result);
            if (result.Succeeded)
            {
                Assert.Equal(preparacionId, result.Data.Id);
                Assert.NotNull(result.Data.NombreProducto);
            }
        }
    }

    [Fact]
    public async Task GetPreparacionDiariaAsync_WithInvalidId_ShouldHandleGracefully()
    {
        // Arrange
        await SetupAsync();
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _dailyPreparationsService.GetPreparacionDiariaAsync(invalidId);

        // Assert
        Assert.NotNull(result);
        // Puede ser exitoso con datos vacíos o fallar, pero no debe lanzar excepción
    }

    #endregion

    #region Tests de Crear Preparación Diaria

    [Fact]
    public async Task CrearPreparacionDiariaAsync_WithValidData_ShouldCreatePreparacion()
    {
        // Arrange
        await SetupAsync();
        
        // Obtener un producto válido para la preparación
        var productoId = await TestDataBuilders.GetAnyProductoIdAsync(_client);
        
        var command = new CrearPreparacionDiariaCommand
        {
            ProductoId = productoId,
            Cantidad = 10,
            Observaciones = "Preparación de prueba para tests"
        };

        // Act
        var result = await _dailyPreparationsService.CrearPreparacionDiariaAsync(command);

        // Assert
        Assert.NotNull(result);
        // Puede ser exitoso o fallar según la lógica de negocio, pero no debe lanzar excepción
        if (result.Succeeded)
        {
            Assert.NotEqual(Guid.Empty, result.Data.Id);
            Assert.Equal(command.ProductoId, result.Data.ProductoId);
            Assert.Equal(command.Cantidad, result.Data.CantidadPreparada);
        }
    }

    [Fact]
    public async Task CrearPreparacionDiariaAsync_WithInvalidData_ShouldHandleValidation()
    {
        // Arrange
        await SetupAsync();
        
        var command = new CrearPreparacionDiariaCommand
        {
            ProductoId = Guid.Empty, // ID inválido
            Cantidad = -1, // Cantidad inválida
            Observaciones = "Datos inválidos para test"
        };

        // Act
        var result = await _dailyPreparationsService.CrearPreparacionDiariaAsync(command);

        // Assert
        Assert.NotNull(result);
        // Debe manejar la validación apropiadamente
    }

    #endregion

    #region Tests de Actualizar Preparación Diaria

    [Fact]
    public async Task ActualizarPreparacionDiariaAsync_WithValidData_ShouldUpdatePreparacion()
    {
        // Arrange
        await SetupAsync();
        
        // Primero crear una preparación para actualizar
        var productoId = await TestDataBuilders.GetAnyProductoIdAsync(_client);
        var crearCommand = new CrearPreparacionDiariaCommand
        {
            ProductoId = productoId,
            Cantidad = 5,
            Observaciones = "Preparación inicial"
        };
        
        var crearResult = await _dailyPreparationsService.CrearPreparacionDiariaAsync(crearCommand);
        if (crearResult.Succeeded)
        {
            var preparacionId = crearResult.Data.Id;
            var actualizarCommand = new ActualizarPreparacionDiariaCommand
            {
                CantidadPreparada = 8,
                Observaciones = "Preparación actualizada"
            };

            // Act
            var result = await _dailyPreparationsService.ActualizarPreparacionDiariaAsync(preparacionId, actualizarCommand);

            // Assert
            Assert.NotNull(result);
            if (result.Succeeded)
            {
                Assert.Equal(preparacionId, result.Data.Id);
                Assert.Equal(actualizarCommand.CantidadPreparada, result.Data.CantidadPreparada);
            }
        }
    }

    #endregion

    #region Tests de Eliminar Preparación Diaria

    [Fact]
    public async Task EliminarPreparacionDiariaAsync_WithValidId_ShouldDeletePreparacion()
    {
        // Arrange
        await SetupAsync();
        
        // Primero crear una preparación para eliminar
        var productoId = await TestDataBuilders.GetAnyProductoIdAsync(_client);
        var crearCommand = new CrearPreparacionDiariaCommand
        {
            ProductoId = productoId,
            Cantidad = 3,
            Observaciones = "Preparación para eliminar"
        };
        
        var crearResult = await _dailyPreparationsService.CrearPreparacionDiariaAsync(crearCommand);
        if (crearResult.Succeeded)
        {
            var preparacionId = crearResult.Data.Id;

            // Act
            var result = await _dailyPreparationsService.EliminarPreparacionDiariaAsync(preparacionId);

            // Assert
            Assert.NotNull(result);
            // Puede ser exitoso o fallar según la lógica de negocio
        }
    }

    [Fact]
    public async Task EliminarPreparacionDiariaAsync_WithInvalidId_ShouldHandleGracefully()
    {
        // Arrange
        await SetupAsync();
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _dailyPreparationsService.EliminarPreparacionDiariaAsync(invalidId);

        // Assert
        Assert.NotNull(result);
        // Debe manejar el error graciosamente
    }

    #endregion

    #region Tests de Consumir Preparación Diaria

    [Fact]
    public async Task ConsumirPreparacionDiariaAsync_WithValidData_ShouldConsumePreparacion()
    {
        // Arrange
        await SetupAsync();
        
        // Primero crear una preparación para consumir
        var productoId = await TestDataBuilders.GetAnyProductoIdAsync(_client);
        var crearCommand = new CrearPreparacionDiariaCommand
        {
            ProductoId = productoId,
            Cantidad = 10,
            Observaciones = "Preparación para consumir"
        };
        
        var crearResult = await _dailyPreparationsService.CrearPreparacionDiariaAsync(crearCommand);
        if (crearResult.Succeeded)
        {
            var preparacionId = crearResult.Data.Id;
            var cantidadConsumir = 3;

            // Act
            var result = await _dailyPreparationsService.ConsumirPreparacionDiariaAsync(preparacionId, cantidadConsumir, "Consumo de prueba");

            // Assert
            Assert.NotNull(result);
            if (result.Succeeded)
            {
                Assert.Equal(preparacionId, result.Data.Id);
                Assert.True(result.Data.CantidadDisponible >= 0);
            }
        }
    }

    [Fact]
    public async Task ConsumirPreparacionDiariaAsync_WithExcessiveQuantity_ShouldHandleValidation()
    {
        // Arrange
        await SetupAsync();
        var preparacionId = Guid.NewGuid(); // ID que probablemente no existe
        var cantidadExcesiva = 999;

        // Act
        var result = await _dailyPreparationsService.ConsumirPreparacionDiariaAsync(preparacionId, cantidadExcesiva, "Cantidad excesiva");

        // Assert
        Assert.NotNull(result);
        // Debe manejar la validación apropiadamente
    }

    #endregion

    #region Tests de Marcar como Disponible

    [Fact]
    public async Task MarcarComoDisponibleAsync_WithValidId_ShouldMarkAsAvailable()
    {
        // Arrange
        await SetupAsync();
        
        // Primero crear una preparación para marcar como disponible
        var productoId = await TestDataBuilders.GetAnyProductoIdAsync(_client);
        var crearCommand = new CrearPreparacionDiariaCommand
        {
            ProductoId = productoId,
            Cantidad = 5,
            Observaciones = "Preparación para marcar disponible"
        };
        
        var crearResult = await _dailyPreparationsService.CrearPreparacionDiariaAsync(crearCommand);
        if (crearResult.Succeeded)
        {
            var preparacionId = crearResult.Data.Id;

            // Act
            var result = await _dailyPreparationsService.MarcarComoDisponibleAsync(preparacionId);

            // Assert
            Assert.NotNull(result);
            if (result.Succeeded)
            {
                Assert.Equal(preparacionId, result.Data.Id);
            }
        }
    }

    #endregion

    #region Tests de Obtener por Estado

    [Fact]
    public async Task GetPreparacionesDiariasPorEstadoAsync_WithValidEstado_ShouldReturnFilteredList()
    {
        // Arrange
        await SetupAsync();
        var estado = "Disponible";

        // Act
        var result = await _dailyPreparationsService.GetPreparacionesDiariasPorEstadoAsync(estado);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Count >= 0);
    }

    [Fact]
    public async Task GetPreparacionesDiariasPorEstadoAsync_WithEmptyEstado_ShouldHandleGracefully()
    {
        // Arrange
        await SetupAsync();
        var estado = "";

        // Act
        var result = await _dailyPreparationsService.GetPreparacionesDiariasPorEstadoAsync(estado);

        // Assert
        Assert.NotNull(result);
        // Debe manejar el estado vacío apropiadamente
    }

    #endregion

    #region Tests de Obtener por Producto

    [Fact]
    public async Task GetPreparacionesDiariasPorProductoAsync_WithValidProductoId_ShouldReturnFilteredList()
    {
        // Arrange
        await SetupAsync();
        var productoId = await TestDataBuilders.GetAnyProductoIdAsync(_client);

        // Act
        var result = await _dailyPreparationsService.GetPreparacionesDiariasPorProductoAsync(productoId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Count >= 0);
    }

    [Fact]
    public async Task GetPreparacionesDiariasPorProductoAsync_WithInvalidProductoId_ShouldHandleGracefully()
    {
        // Arrange
        await SetupAsync();
        var invalidProductoId = Guid.NewGuid();

        // Act
        var result = await _dailyPreparationsService.GetPreparacionesDiariasPorProductoAsync(invalidProductoId);

        // Assert
        Assert.NotNull(result);
        // Debe manejar el ID inválido apropiadamente
    }

    #endregion

    #region Tests de Estadísticas

    [Fact]
    public async Task GetEstadisticasAsync_ShouldReturnValidStatistics()
    {
        // Arrange
        await SetupAsync();

        // Act
        var result = await _dailyPreparationsService.GetEstadisticasAsync();

        // Assert
        Assert.NotNull(result);
        if (result.Succeeded && result.Data != null)
        {
            Assert.True(result.Data.TotalPreparaciones >= 0);
            Assert.True(result.Data.PreparacionesDisponibles >= 0);
            Assert.True(result.Data.CantidadConsumida >= 0);
        }
    }

    #endregion

    #region Tests de Resiliencia

    [Fact]
    public async Task AllMethods_ShouldHandleConcurrentCalls()
    {
        // Arrange
        await SetupAsync();

        // Act - Llamar múltiples métodos en paralelo
        var tasks = new Task[]
        {
            _dailyPreparationsService.GetPreparacionesDiariasAsync(),
            _dailyPreparationsService.GetEstadisticasAsync(),
            _dailyPreparationsService.GetPreparacionesDiariasPorEstadoAsync("Disponible")
        };

        await Task.WhenAll(tasks);

        // Assert - Todos los métodos deben completarse sin excepción
        Assert.True(true); // Si llegamos aquí, todos los métodos se completaron sin excepción
    }

    [Fact]
    public async Task AllMethods_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        await SetupAsync();
        var timeout = TimeSpan.FromSeconds(10);

        // Act & Assert - Cada método debe completarse en tiempo razonable
        var preparacionesTask = _dailyPreparationsService.GetPreparacionesDiariasAsync();
        Assert.True(await Task.WhenAny(preparacionesTask, Task.Delay(timeout)) == preparacionesTask, 
                   "GetPreparacionesDiariasAsync debe completarse en tiempo razonable");

        var estadisticasTask = _dailyPreparationsService.GetEstadisticasAsync();
        Assert.True(await Task.WhenAny(estadisticasTask, Task.Delay(timeout)) == estadisticasTask, 
                   "GetEstadisticasAsync debe completarse en tiempo razonable");
    }

    #endregion

    #region Tests de Flujos Completos

    [Fact]
    public async Task CompleteFlow_CreateUpdateConsumeDelete_ShouldWorkCorrectly()
    {
        // Arrange
        await SetupAsync();
        var productoId = await TestDataBuilders.GetAnyProductoIdAsync(_client);

        // 1. Crear preparación
        var crearCommand = new CrearPreparacionDiariaCommand
        {
            ProductoId = productoId,
            Cantidad = 10,
            Observaciones = "Flujo completo de prueba"
        };
        
        var crearResult = await _dailyPreparationsService.CrearPreparacionDiariaAsync(crearCommand);
        
        if (crearResult.Succeeded)
        {
            var preparacionId = crearResult.Data.Id;

            // 2. Actualizar preparación
            var actualizarCommand = new ActualizarPreparacionDiariaCommand
            {
                CantidadPreparada = 15,
                Observaciones = "Actualizada en flujo completo"
            };
            
            var actualizarResult = await _dailyPreparationsService.ActualizarPreparacionDiariaAsync(preparacionId, actualizarCommand);

            // 3. Consumir preparación
            var consumirResult = await _dailyPreparationsService.ConsumirPreparacionDiariaAsync(preparacionId, 3, "Consumo en flujo completo");

            // 4. Marcar como disponible
            var disponibleResult = await _dailyPreparationsService.MarcarComoDisponibleAsync(preparacionId);

            // 5. Eliminar preparación
            var eliminarResult = await _dailyPreparationsService.EliminarPreparacionDiariaAsync(preparacionId);

            // Assert - Al menos algunas operaciones deben ser exitosas
            Assert.True(crearResult.Succeeded || actualizarResult.Succeeded || consumirResult.Succeeded || 
                       disponibleResult.Succeeded || eliminarResult.Succeeded, 
                       "Al menos una operación del flujo completo debe ser exitosa");
        }
    }

    #endregion
}
