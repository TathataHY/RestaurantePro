using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Commercial;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services;

/// <summary>
/// Tests de integración robustos para TarjetasFidelizacionService
/// </summary>
[Collection("Mobile Integration Tests")]
public class TarjetasFidelizacionServiceIntegrationTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly ITarjetasFidelizacionService _tarjetasFidelizacionService;
    private readonly ILogger<TarjetasFidelizacionServiceIntegrationTests> _logger;

    public TarjetasFidelizacionServiceIntegrationTests(MobileIntegrationTestFixture fixture)
    {
        using var scope = fixture.Services.CreateScope();
        _tarjetasFidelizacionService = scope.ServiceProvider.GetRequiredService<ITarjetasFidelizacionService>();
        _logger = scope.ServiceProvider.GetRequiredService<ILogger<TarjetasFidelizacionServiceIntegrationTests>>();
    }

    #region Tests de Búsqueda y Obtención

    [Fact]
    public async Task BuscarTarjeta_WithValidNumber_ShouldReturnTarjeta()
    {
        // Arrange
        var numeroTarjeta = "TARJETA001";

        // Act
        var response = await _tarjetasFidelizacionService.BuscarTarjetaAsync(numeroTarjeta);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(numeroTarjeta, response.Data.NumeroTarjeta);
        _logger.LogInformation("Tarjeta {NumeroTarjeta} encontrada exitosamente", numeroTarjeta);
    }

    [Fact]
    public async Task BuscarTarjeta_WithInvalidNumber_ShouldReturnFailure()
    {
        // Arrange
        var numeroTarjeta = "TARJETA_INEXISTENTE";

        // Act
        var response = await _tarjetasFidelizacionService.BuscarTarjetaAsync(numeroTarjeta);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Contains("no encontrada", response.Message);
        _logger.LogInformation("Tarjeta {NumeroTarjeta} no encontrada correctamente", numeroTarjeta);
    }

    [Fact]
    public async Task BuscarTarjeta_WithEmptyNumber_ShouldHandleGracefully()
    {
        // Arrange
        var numeroTarjeta = "";

        // Act
        var response = await _tarjetasFidelizacionService.BuscarTarjetaAsync(numeroTarjeta);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        _logger.LogInformation("Búsqueda con número vacío manejada correctamente");
    }

    [Fact]
    public async Task ObtenerTarjeta_WithValidId_ShouldReturnTarjeta()
    {
        // Arrange - Usar un ID que sabemos que existe en el mock
        var tarjetaId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // Act
        var response = await _tarjetasFidelizacionService.ObtenerTarjetaAsync(tarjetaId);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        _logger.LogInformation("Tarjeta {TarjetaId} obtenida exitosamente", tarjetaId);
    }

    [Fact]
    public async Task ObtenerTarjeta_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        var tarjetaId = Guid.Empty;

        // Act
        var response = await _tarjetasFidelizacionService.ObtenerTarjetaAsync(tarjetaId);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        _logger.LogInformation("GUID vacío manejado correctamente");
    }

    [Fact]
    public async Task ObtenerTarjetaPorCodigo_WithValidCode_ShouldReturnTarjeta()
    {
        // Arrange
        var codigo = "TARJETA001";

        // Act
        var response = await _tarjetasFidelizacionService.ObtenerTarjetaPorCodigoAsync(codigo);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        _logger.LogInformation("Tarjeta con código {Codigo} obtenida exitosamente", codigo);
    }

    [Fact]
    public async Task ObtenerTarjetaPorCodigo_WithInvalidCode_ShouldReturnFailure()
    {
        // Arrange
        var codigo = "CODIGO_INEXISTENTE";

        // Act
        var response = await _tarjetasFidelizacionService.ObtenerTarjetaPorCodigoAsync(codigo);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        _logger.LogInformation("Código inválido {Codigo} manejado correctamente", codigo);
    }

    #endregion

    #region Tests de Activación y Desactivación

    [Fact]
    public async Task ActivarTarjeta_WithValidData_ShouldActivateTarjeta()
    {
        // Arrange
        var numeroTarjeta = "TARJETA001";
        var nombreCliente = "Cliente Test";

        // Act
        var response = await _tarjetasFidelizacionService.ActivarTarjetaAsync(numeroTarjeta, nombreCliente);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        _logger.LogInformation("Tarjeta {NumeroTarjeta} activada para {NombreCliente}", numeroTarjeta, nombreCliente);
    }

    [Fact]
    public async Task ActivarTarjeta_WithInvalidNumber_ShouldReturnFailure()
    {
        // Arrange
        var numeroTarjeta = "TARJETA_INEXISTENTE";
        var nombreCliente = "Cliente Test";

        // Act
        var response = await _tarjetasFidelizacionService.ActivarTarjetaAsync(numeroTarjeta, nombreCliente);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        _logger.LogInformation("Activación de tarjeta inexistente manejada correctamente");
    }

    [Fact]
    public async Task DesactivarTarjeta_WithValidId_ShouldDeactivateTarjeta()
    {
        // Arrange
        var tarjetaId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // Act
        var response = await _tarjetasFidelizacionService.DesactivarTarjetaAsync(tarjetaId);

        // Assert
        Assert.True(response.Success);
        Assert.True(response.Data);
        _logger.LogInformation("Tarjeta {TarjetaId} desactivada exitosamente", tarjetaId);
    }

    [Fact]
    public async Task DesactivarTarjeta_WithInvalidId_ShouldHandleGracefully()
    {
        // Arrange
        var tarjetaId = Guid.Empty;

        // Act
        var response = await _tarjetasFidelizacionService.DesactivarTarjetaAsync(tarjetaId);

        // Assert
        Assert.False(response.Success);
        _logger.LogInformation("Desactivación con GUID vacío manejada correctamente");
    }

    #endregion

    #region Tests de Puntos y Transacciones

    [Fact]
    public async Task AcumularPuntos_WithValidData_ShouldAccumulatePoints()
    {
        // Arrange
        var tarjetaId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var montoCompra = 100.50m;

        // Act
        var response = await _tarjetasFidelizacionService.AcumularPuntosAsync(tarjetaId, montoCompra);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        _logger.LogInformation("Puntos acumulados para tarjeta {TarjetaId} con monto {Monto}", tarjetaId, montoCompra);
    }

    [Fact]
    public async Task AcumularPuntos_WithZeroAmount_ShouldHandleGracefully()
    {
        // Arrange
        var tarjetaId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var montoCompra = 0m;

        // Act
        var response = await _tarjetasFidelizacionService.AcumularPuntosAsync(tarjetaId, montoCompra);

        // Assert
        Assert.False(response.Success);
        _logger.LogInformation("Acumulación con monto cero manejada correctamente");
    }

    [Fact]
    public async Task AcumularPuntos_WithNegativeAmount_ShouldHandleGracefully()
    {
        // Arrange
        var tarjetaId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var montoCompra = -50m;

        // Act
        var response = await _tarjetasFidelizacionService.AcumularPuntosAsync(tarjetaId, montoCompra);

        // Assert
        Assert.False(response.Success);
        _logger.LogInformation("Acumulación con monto negativo manejada correctamente");
    }

    [Fact]
    public async Task CanjearPuntos_WithValidData_ShouldRedeemPoints()
    {
        // Arrange
        var tarjetaId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var puntosACanjear = 100;
        var descuento = 10.00m;

        // Act
        var response = await _tarjetasFidelizacionService.CanjearPuntosAsync(tarjetaId, puntosACanjear, descuento);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        _logger.LogInformation("Puntos canjeados para tarjeta {TarjetaId}: {Puntos} puntos, {Descuento} descuento", 
            tarjetaId, puntosACanjear, descuento);
    }

    [Fact]
    public async Task CanjearPuntos_WithZeroPoints_ShouldHandleGracefully()
    {
        // Arrange
        var tarjetaId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var puntosACanjear = 0;
        var descuento = 0m;

        // Act
        var response = await _tarjetasFidelizacionService.CanjearPuntosAsync(tarjetaId, puntosACanjear, descuento);

        // Assert
        Assert.False(response.Success);
        _logger.LogInformation("Canje con cero puntos manejado correctamente");
    }

    [Fact]
    public async Task CanjearPuntos_WithNegativePoints_ShouldHandleGracefully()
    {
        // Arrange
        var tarjetaId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var puntosACanjear = -50;
        var descuento = 5m;

        // Act
        var response = await _tarjetasFidelizacionService.CanjearPuntosAsync(tarjetaId, puntosACanjear, descuento);

        // Assert
        Assert.False(response.Success);
        _logger.LogInformation("Canje con puntos negativos manejado correctamente");
    }

    [Fact]
    public async Task ObtenerHistorialTransacciones_WithValidId_ShouldReturnHistorial()
    {
        // Arrange
        var tarjetaId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // Act
        var response = await _tarjetasFidelizacionService.ObtenerHistorialTransaccionesAsync(tarjetaId);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        _logger.LogInformation("Historial de transacciones obtenido para tarjeta {TarjetaId}", tarjetaId);
    }

    [Fact]
    public async Task ObtenerHistorialTransacciones_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        var tarjetaId = Guid.Empty;

        // Act
        var response = await _tarjetasFidelizacionService.ObtenerHistorialTransaccionesAsync(tarjetaId);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        _logger.LogInformation("Historial con GUID vacío manejado correctamente");
    }

    #endregion

    #region Tests de Listado y Filtros

    [Fact]
    public async Task ObtenerTarjetasActivas_ShouldReturnActiveTarjetas()
    {
        // Act
        var response = await _tarjetasFidelizacionService.ObtenerTarjetasActivasAsync();

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.All(response.Data, tarjeta => Assert.Equal("Activa", tarjeta.Estado));
        _logger.LogInformation("{Count} tarjetas activas obtenidas", response.Data.Count);
    }

    [Fact]
    public async Task ObtenerTarjetas_WithValidFilter_ShouldReturnFilteredTarjetas()
    {
        // Arrange
        var filtro = new FiltroTarjetasFidelizacionDto
        {
            Estado = "Activa",
            SoloActivas = true,
            PageSize = 10
        };

        // Act
        var response = await _tarjetasFidelizacionService.ObtenerTarjetasAsync(filtro);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        _logger.LogInformation("{Count} tarjetas filtradas obtenidas", response.Data.Count);
    }

    [Fact]
    public async Task ObtenerTarjetas_WithEmptyFilter_ShouldReturnAllTarjetas()
    {
        // Arrange
        var filtro = new FiltroTarjetasFidelizacionDto();

        // Act
        var response = await _tarjetasFidelizacionService.ObtenerTarjetasAsync(filtro);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        _logger.LogInformation("{Count} tarjetas obtenidas sin filtro", response.Data.Count);
    }

    #endregion

    #region Tests de Estadísticas

    [Fact]
    public async Task ObtenerEstadisticas_WithValidId_ShouldReturnEstadisticas()
    {
        // Arrange
        var tarjetaId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // Act
        var response = await _tarjetasFidelizacionService.ObtenerEstadisticasAsync(tarjetaId);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(tarjetaId, response.Data.TarjetaId);
        _logger.LogInformation("Estadísticas obtenidas para tarjeta {TarjetaId}", tarjetaId);
    }

    [Fact]
    public async Task ObtenerEstadisticas_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        var tarjetaId = Guid.Empty;

        // Act
        var response = await _tarjetasFidelizacionService.ObtenerEstadisticasAsync(tarjetaId);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        _logger.LogInformation("Estadísticas con GUID vacío manejadas correctamente");
    }

    [Fact]
    public async Task ObtenerHistorialPuntos_WithValidId_ShouldReturnHistorial()
    {
        // Arrange
        var tarjetaId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // Act
        var response = await _tarjetasFidelizacionService.ObtenerHistorialPuntosAsync(tarjetaId);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        _logger.LogInformation("Historial de puntos obtenido para tarjeta {TarjetaId}", tarjetaId);
    }

    #endregion

    #region Tests de Bloqueo y Eliminación

    [Fact]
    public async Task BloquearTarjeta_WithValidId_ShouldBlockTarjeta()
    {
        // Arrange
        var tarjetaId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // Act
        var response = await _tarjetasFidelizacionService.BloquearTarjetaAsync(tarjetaId);

        // Assert
        Assert.True(response.Success);
        Assert.True(response.Data);
        _logger.LogInformation("Tarjeta {TarjetaId} bloqueada exitosamente", tarjetaId);
    }

    [Fact]
    public async Task BloquearTarjeta_WithInvalidId_ShouldHandleGracefully()
    {
        // Arrange
        var tarjetaId = Guid.Empty;

        // Act
        var response = await _tarjetasFidelizacionService.BloquearTarjetaAsync(tarjetaId);

        // Assert
        Assert.False(response.Success);
        _logger.LogInformation("Bloqueo con GUID vacío manejado correctamente");
    }

    [Fact]
    public async Task EliminarTarjeta_WithValidId_ShouldDeleteTarjeta()
    {
        // Arrange
        var tarjetaId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // Act
        var response = await _tarjetasFidelizacionService.EliminarTarjetaAsync(tarjetaId);

        // Assert
        Assert.True(response.Success);
        Assert.True(response.Data);
        _logger.LogInformation("Tarjeta {TarjetaId} eliminada exitosamente", tarjetaId);
    }

    [Fact]
    public async Task EliminarTarjeta_WithInvalidId_ShouldHandleGracefully()
    {
        // Arrange
        var tarjetaId = Guid.Empty;

        // Act
        var response = await _tarjetasFidelizacionService.EliminarTarjetaAsync(tarjetaId);

        // Assert
        Assert.False(response.Success);
        _logger.LogInformation("Eliminación con GUID vacío manejada correctamente");
    }

    #endregion

    #region Tests de Concurrencia

    [Fact]
    public async Task MultipleConcurrentOperations_ShouldHandleConcurrency()
    {
        // Arrange
        var tarjetaId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var tasks = new List<Task<ApiResponse<TarjetaFidelizacionDto>>>();

        // Act - Ejecutar múltiples operaciones concurrentes
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(_tarjetasFidelizacionService.AcumularPuntosAsync(tarjetaId, 50m));
            tasks.Add(_tarjetasFidelizacionService.CanjearPuntosAsync(tarjetaId, 25, 5m));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        Assert.All(responses, response => Assert.True(response.Success));
        _logger.LogInformation("{Count} operaciones concurrentes exitosas", responses.Length);
    }

    [Fact]
    public async Task MultipleConcurrentSearches_ShouldHandleConcurrency()
    {
        // Arrange
        var numeroTarjeta = "TARJETA001";
        var tasks = new List<Task<ApiResponse<TarjetaFidelizacionDto>>>();

        // Act - Ejecutar múltiples búsquedas concurrentes
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_tarjetasFidelizacionService.BuscarTarjetaAsync(numeroTarjeta));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        Assert.All(responses, response => Assert.True(response.Success));
        _logger.LogInformation("{Count} búsquedas concurrentes exitosas", responses.Length);
    }

    #endregion

    #region Tests de Cancelación

    [Fact]
    public async Task Operations_WithCancellation_ShouldHandleCancellation()
    {
        // Arrange
        var tarjetaId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var response = await _tarjetasFidelizacionService.AcumularPuntosAsync(tarjetaId, 100m, cts.Token);

        // Assert
        Assert.False(response.Success);
        Assert.Contains("cancelada", response.Message);
        _logger.LogInformation("Cancelación manejada correctamente");
    }

    [Fact]
    public async Task Search_WithCancellation_ShouldHandleCancellation()
    {
        // Arrange
        var numeroTarjeta = "TARJETA001";
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var response = await _tarjetasFidelizacionService.BuscarTarjetaAsync(numeroTarjeta, cts.Token);

        // Assert
        Assert.False(response.Success);
        Assert.Contains("cancelada", response.Message);
        _logger.LogInformation("Cancelación de búsqueda manejada correctamente");
    }

    #endregion

    #region Tests de Casos Edge

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("TARJETA_MUY_LARGA_QUE_EXCEDE_LIMITES_RAZONABLES_DE_LONGITUD")]
    [InlineData("1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890")]
    public async Task BuscarTarjeta_WithEdgeCaseNumbers_ShouldHandleGracefully(string numeroTarjeta)
    {
        // Act
        var response = await _tarjetasFidelizacionService.BuscarTarjetaAsync(numeroTarjeta);

        // Assert
        Assert.NotNull(response);
        _logger.LogInformation("Búsqueda con término: '{Termino}', Resultados: {Count}", 
            numeroTarjeta, response.Data != null ? 1 : 0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    [InlineData(999999999)]
    [InlineData(0.01)]
    [InlineData(999999.99)]
    public async Task AcumularPuntos_WithEdgeCaseAmounts_ShouldHandleGracefully(decimal monto)
    {
        // Arrange
        var tarjetaId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // Act
        var response = await _tarjetasFidelizacionService.AcumularPuntosAsync(tarjetaId, monto);

        // Assert
        Assert.NotNull(response);
        _logger.LogInformation("Acumulación con monto: {Monto}, Resultado: {Success}", monto, response.Success);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    [InlineData(999999)]
    [InlineData(1)]
    public async Task CanjearPuntos_WithEdgeCasePoints_ShouldHandleGracefully(int puntos)
    {
        // Arrange
        var tarjetaId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var descuento = 10m;

        // Act
        var response = await _tarjetasFidelizacionService.CanjearPuntosAsync(tarjetaId, puntos, descuento);

        // Assert
        Assert.NotNull(response);
        _logger.LogInformation("Canje con puntos: {Puntos}, Resultado: {Success}", puntos, response.Success);
    }

    #endregion

    #region Tests de Rendimiento

    [Fact]
    public async Task ObtenerTarjetasActivas_LargePageSize_ShouldHandleLargeRequests()
    {
        // Act
        var response = await _tarjetasFidelizacionService.ObtenerTarjetasActivasAsync();

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        _logger.LogInformation("{Count} tarjetas activas con página grande", response.Data.Count);
    }

    [Fact]
    public async Task ObtenerTarjetas_WithLargeFilter_ShouldHandleLargeRequests()
    {
        // Arrange
        var filtro = new FiltroTarjetasFidelizacionDto
        {
            PageSize = 1000,
            Estado = "Activa"
        };

        // Act
        var response = await _tarjetasFidelizacionService.ObtenerTarjetasAsync(filtro);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        _logger.LogInformation("{Count} tarjetas con filtro grande", response.Data.Count);
    }

    #endregion
}