using Moq;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Commercial;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Services.Commercial;

public class TarjetasFidelizacionServiceTests
{
    private readonly Mock<IApiService> _mockApiService;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly TarjetasFidelizacionService _tarjetasService;

    public TarjetasFidelizacionServiceTests()
    {
        _mockApiService = new Mock<IApiService>();
        _mockAuthService = new Mock<IAuthService>();
        
        // Configurar el mock de autenticación para devolver un token válido
        _mockAuthService.Setup(x => x.GetTokenAsync())
                       .ReturnsAsync("test-token");
        
        _tarjetasService = new TarjetasFidelizacionService(_mockApiService.Object, _mockAuthService.Object);
    }

    [Fact]
    public async Task BuscarTarjetaAsync_WithValidNumero_ShouldReturnSuccess()
    {
        // Arrange
        var numeroTarjeta = "123456789";
        var tarjeta = new TarjetaFidelizacionDto
        {
            Id = Guid.NewGuid(),
            NumeroTarjeta = numeroTarjeta,
            ClienteNombre = "Juan Pérez",
            PuntosDisponibles = 150,
            Estado = "Activa"
        };

        var tarjetas = new List<TarjetaFidelizacionDto> { tarjeta };
        var apiResponse = ApiResponse<List<TarjetaFidelizacionDto>>.SuccessResponse(tarjetas);
        _mockApiService.Setup(x => x.GetAsync<List<TarjetaFidelizacionDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.BuscarTarjetaAsync(numeroTarjeta);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(numeroTarjeta, result.Data.NumeroTarjeta);
        _mockApiService.Verify(x => x.GetAsync<List<TarjetaFidelizacionDto>>($"api/comercial/tarjetas-fidelizacion?pageSize=100", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActivarTarjetaAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var numeroTarjeta = "987654321";
        var nombreCliente = "María García";
        var tarjetaId = Guid.NewGuid();
        var tarjeta = new TarjetaFidelizacionDto
        {
            Id = tarjetaId,
            NumeroTarjeta = numeroTarjeta,
            ClienteNombre = nombreCliente,
            PuntosDisponibles = 0,
            Estado = "Activa"
        };

        // Mock para BuscarTarjetaAsync (primera llamada)
        var tarjetas = new List<TarjetaFidelizacionDto> { tarjeta };
        var buscarResponse = ApiResponse<List<TarjetaFidelizacionDto>>.SuccessResponse(tarjetas);
        
        // Mock para PostAsync (segunda llamada)
        var activarResponse = ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(tarjeta);
        
        _mockApiService.Setup(x => x.GetAsync<List<TarjetaFidelizacionDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(buscarResponse);
        _mockApiService.Setup(x => x.PostAsync<TarjetaFidelizacionDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(activarResponse);

        // Act
        var result = await _tarjetasService.ActivarTarjetaAsync(numeroTarjeta, nombreCliente);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(numeroTarjeta, result.Data.NumeroTarjeta);
        _mockApiService.Verify(x => x.GetAsync<List<TarjetaFidelizacionDto>>($"api/comercial/tarjetas-fidelizacion?pageSize=100", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockApiService.Verify(x => x.PostAsync<TarjetaFidelizacionDto>($"api/comercial/tarjetas-fidelizacion/{tarjetaId}/activar", It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerTarjetaPorCodigoAsync_WithValidCodigo_ShouldReturnSuccess()
    {
        // Arrange
        var codigo = "ABC123";
        var tarjeta = new TarjetaFidelizacionDto
        {
            Id = Guid.NewGuid(),
            NumeroTarjeta = codigo,
            ClienteNombre = "María García",
            PuntosDisponibles = 200,
            Estado = "Activa"
        };

        var tarjetas = new List<TarjetaFidelizacionDto> { tarjeta };
        var apiResponse = ApiResponse<List<TarjetaFidelizacionDto>>.SuccessResponse(tarjetas);
        _mockApiService.Setup(x => x.GetAsync<List<TarjetaFidelizacionDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.ObtenerTarjetaPorCodigoAsync(codigo);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(codigo, result.Data.NumeroTarjeta);
        _mockApiService.Verify(x => x.GetAsync<List<TarjetaFidelizacionDto>>($"api/comercial/tarjetas-fidelizacion?pageSize=100", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerTarjetaAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var tarjeta = new TarjetaFidelizacionDto
        {
            Id = tarjetaId,
            NumeroTarjeta = "123456789",
            ClienteNombre = "Carlos López",
            PuntosDisponibles = 300,
            Estado = "Activa"
        };

        var apiResponse = ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(tarjeta);
        _mockApiService.Setup(x => x.GetAsync<TarjetaFidelizacionDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.ObtenerTarjetaAsync(tarjetaId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(tarjetaId, result.Data.Id);
        _mockApiService.Verify(x => x.GetAsync<TarjetaFidelizacionDto>($"api/comercial/tarjetas-fidelizacion/{tarjetaId}", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerTransaccionesAsync_WithValidTarjetaId_ShouldReturnSuccess()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var historialPuntos = new List<HistorialPuntosDto>
        {
            new() { Id = Guid.NewGuid(), CodigoTarjeta = "123456", PuntosAcumulados = 50, PuntosCanjeados = 0, PuntosDisponibles = 50, FechaUltimaTransaccion = DateTime.Now }
        };

        var apiResponse = ApiResponse<List<HistorialPuntosDto>>.SuccessResponse(historialPuntos);
        _mockApiService.Setup(x => x.GetAsync<List<HistorialPuntosDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.ObtenerHistorialTransaccionesAsync(tarjetaId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.Count);
        _mockApiService.Verify(x => x.GetAsync<List<HistorialPuntosDto>>($"api/comercial/tarjetas-fidelizacion/{tarjetaId}/historial", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AcumularPuntosAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var montoCompra = 25.0m;
        var tarjeta = new TarjetaFidelizacionDto
        {
            Id = tarjetaId,
            NumeroTarjeta = "123456789",
            ClienteNombre = "Juan Pérez",
            PuntosDisponibles = 175,
            Estado = "Activa"
        };

        // Mock para la primera llamada (acumular puntos)
        var acumularResponse = ApiResponse<object>.SuccessResponse(new { });
        _mockApiService.Setup(x => x.PostAsync<object>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(acumularResponse);

        // Mock para la segunda llamada (obtener tarjeta actualizada)
        var tarjetaResponse = ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(tarjeta);
        _mockApiService.Setup(x => x.GetAsync<TarjetaFidelizacionDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(tarjetaResponse);

        // Act
        var result = await _tarjetasService.AcumularPuntosAsync(tarjetaId, montoCompra);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(175, result.Data.PuntosDisponibles);
        _mockApiService.Verify(x => x.PostAsync<object>($"api/comercial/tarjetas-fidelizacion/{tarjetaId}/puntos", It.Is<object>(data => data.ToString().Contains(montoCompra.ToString())), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockApiService.Verify(x => x.GetAsync<TarjetaFidelizacionDto>($"api/comercial/tarjetas-fidelizacion/{tarjetaId}", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CanjearPuntosAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var puntosACanjear = 30;
        var descuento = 10.0m;
        var tarjeta = new TarjetaFidelizacionDto
        {
            Id = tarjetaId,
            NumeroTarjeta = "123456789",
            ClienteNombre = "Juan Pérez",
            PuntosDisponibles = 120,
            Estado = "Activa"
        };

        // Mock para la primera llamada (canjear puntos)
        var canjearResponse = ApiResponse<object>.SuccessResponse(new { });
        _mockApiService.Setup(x => x.PostAsync<object>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(canjearResponse);

        // Mock para la segunda llamada (obtener tarjeta actualizada)
        var tarjetaResponse = ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(tarjeta);
        _mockApiService.Setup(x => x.GetAsync<TarjetaFidelizacionDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(tarjetaResponse);

        // Act
        var result = await _tarjetasService.CanjearPuntosAsync(tarjetaId, puntosACanjear, descuento);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(120, result.Data.PuntosDisponibles);
        _mockApiService.Verify(x => x.PostAsync<object>($"api/comercial/tarjetas-fidelizacion/{tarjetaId}/canjear", It.Is<object>(data => data.ToString().Contains(puntosACanjear.ToString())), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockApiService.Verify(x => x.GetAsync<TarjetaFidelizacionDto>($"api/comercial/tarjetas-fidelizacion/{tarjetaId}", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerTarjetasAsync_ShouldReturnSuccess()
    {
        // Arrange
        var tarjetas = new List<TarjetaFidelizacionDto>
        {
            new() { Id = Guid.NewGuid(), NumeroTarjeta = "123456789", Estado = "Activa" },
            new() { Id = Guid.NewGuid(), NumeroTarjeta = "987654321", Estado = "Activa" }
        };

        var apiResponse = ApiResponse<List<TarjetaFidelizacionDto>>.SuccessResponse(tarjetas);
        _mockApiService.Setup(x => x.GetAsync<List<TarjetaFidelizacionDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.ObtenerTarjetasAsync(new FiltroTarjetasFidelizacionDto());

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        _mockApiService.Verify(x => x.GetAsync<List<TarjetaFidelizacionDto>>("api/comercial/tarjetas-fidelizacion", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DesactivarTarjetaAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var apiResponse = ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(new TarjetaFidelizacionDto());
        
        _mockApiService.Setup(x => x.PostAsync<TarjetaFidelizacionDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.DesactivarTarjetaAsync(tarjetaId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data);
        _mockApiService.Verify(x => x.PostAsync<TarjetaFidelizacionDto>($"api/comercial/tarjetas-fidelizacion/{tarjetaId}/desactivar", It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerTarjetasActivasAsync_ShouldReturnSuccess()
    {
        // Arrange
        var tarjetas = new List<TarjetaFidelizacionDto>
        {
            new() { Id = Guid.NewGuid(), NumeroTarjeta = "123456789", PuntosDisponibles = 150, Estado = "Activa" }
        };

        var apiResponse = ApiResponse<List<TarjetaFidelizacionDto>>.SuccessResponse(tarjetas);
        _mockApiService.Setup(x => x.GetAsync<List<TarjetaFidelizacionDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.ObtenerTarjetasActivasAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        _mockApiService.Verify(x => x.GetAsync<List<TarjetaFidelizacionDto>>("api/comercial/tarjetas-fidelizacion", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerHistorialPuntosAsync_WithValidTarjetaId_ShouldReturnSuccess()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var historial = new HistorialPuntosDto
        {
            Id = tarjetaId,
            PuntosAcumulados = 500,
            PuntosCanjeados = 200,
            PuntosDisponibles = 300,
            Transacciones = new List<TransaccionPuntosDto>
            {
                new() { Id = Guid.NewGuid(), TipoTransaccion = "Acumulación", Puntos = 30, FechaTransaccion = DateTime.Now },
                new() { Id = Guid.NewGuid(), TipoTransaccion = "Canje", Puntos = -10, FechaTransaccion = DateTime.Now.AddDays(-1) }
            }
        };

        var historialList = new List<HistorialPuntosDto> { historial };
        var apiResponse = ApiResponse<List<HistorialPuntosDto>>.SuccessResponse(historialList);
        _mockApiService.Setup(x => x.GetAsync<List<HistorialPuntosDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.ObtenerHistorialPuntosAsync(tarjetaId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Transacciones.Count);
        _mockApiService.Verify(x => x.GetAsync<List<HistorialPuntosDto>>($"api/comercial/tarjetas-fidelizacion/{tarjetaId}/historial", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BloquearTarjetaAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var apiResponse = ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(new TarjetaFidelizacionDto());
        
        _mockApiService.Setup(x => x.PostAsync<TarjetaFidelizacionDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.BloquearTarjetaAsync(tarjetaId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data);
        _mockApiService.Verify(x => x.PostAsync<TarjetaFidelizacionDto>($"api/comercial/tarjetas-fidelizacion/{tarjetaId}/desactivar", It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerHistorialCompletoAsync_WithValidTarjetaId_ShouldReturnSuccess()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var historial = new HistorialPuntosDto
        {
            Id = tarjetaId,
            PuntosAcumulados = 500,
            PuntosCanjeados = 200,
            PuntosDisponibles = 300,
            Transacciones = new List<TransaccionPuntosDto>()
        };

        var historialList = new List<HistorialPuntosDto> { historial };
        var apiResponse = ApiResponse<List<HistorialPuntosDto>>.SuccessResponse(historialList);
        _mockApiService.Setup(x => x.GetAsync<List<HistorialPuntosDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.ObtenerHistorialPuntosAsync(tarjetaId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(tarjetaId, result.Data.Id);
        Assert.Equal(300, result.Data.PuntosDisponibles);
        _mockApiService.Verify(x => x.GetAsync<List<HistorialPuntosDto>>($"api/comercial/tarjetas-fidelizacion/{tarjetaId}/historial", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_WithValidTarjetaId_ShouldReturnSuccess()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var estadisticas = new EstadisticasTarjetaDto
        {
            TarjetaId = tarjetaId,
            TotalTransacciones = 25,
            PuntosAcumulados = 1000,
            PuntosCanjeados = 400
        };

        var apiResponse = ApiResponse<EstadisticasTarjetaDto>.SuccessResponse(estadisticas);
        _mockApiService.Setup(x => x.GetAsync<EstadisticasTarjetaDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.ObtenerEstadisticasAsync(tarjetaId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(tarjetaId, result.Data.TarjetaId);
        Assert.Equal(1000, result.Data.PuntosAcumulados);
        _mockApiService.Verify(x => x.GetAsync<EstadisticasTarjetaDto>($"api/comercial/tarjetas-fidelizacion/{tarjetaId}/estadisticas", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EliminarTarjetaAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var apiResponse = ApiResponse<bool>.SuccessResponse(true);
        
        _mockApiService.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.EliminarTarjetaAsync(tarjetaId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data);
        _mockApiService.Verify(x => x.DeleteAsync($"api/comercial/tarjetas-fidelizacion/{tarjetaId}", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BuscarTarjetaAsync_WithApiError_ShouldReturnFailure()
    {
        // Arrange
        var numeroTarjeta = "123456789";
        var errorResponse = ApiResponse<List<TarjetaFidelizacionDto>>.ErrorResponse(new List<string> { "Tarjeta no encontrada" }, "Tarjeta no encontrada", 404);
        _mockApiService.Setup(x => x.GetAsync<List<TarjetaFidelizacionDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(errorResponse);

        // Act
        var result = await _tarjetasService.BuscarTarjetaAsync(numeroTarjeta);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Tarjeta no encontrada", result.Error);
    }

    [Fact]
    public async Task BuscarTarjetaAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var numeroTarjeta = "123456789";
        _mockApiService.Setup(x => x.GetAsync<List<TarjetaFidelizacionDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _tarjetasService.BuscarTarjetaAsync(numeroTarjeta);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }

    [Fact]
    public async Task ActivarTarjetaAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var numeroTarjeta = "987654321";
        var nombreCliente = "María García";
        var tarjetaId = Guid.NewGuid();
        var tarjeta = new TarjetaFidelizacionDto
        {
            Id = tarjetaId,
            NumeroTarjeta = numeroTarjeta,
            ClienteNombre = nombreCliente,
            PuntosDisponibles = 0,
            Estado = "Inactiva"
        };

        // Mock para la primera llamada (BuscarTarjetaAsync) - exitosa
        var tarjetas = new List<TarjetaFidelizacionDto> { tarjeta };
        var buscarResponse = ApiResponse<List<TarjetaFidelizacionDto>>.SuccessResponse(tarjetas);
        _mockApiService.Setup(x => x.GetAsync<List<TarjetaFidelizacionDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(buscarResponse);

        // Mock para la segunda llamada (PostAsync) - lanza excepción
        _mockApiService.Setup(x => x.PostAsync<TarjetaFidelizacionDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _tarjetasService.ActivarTarjetaAsync(numeroTarjeta, nombreCliente);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error al activar tarjeta: Error de red", result.Error);
    }

    [Fact]
    public async Task AcumularPuntosAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var montoCompra = 25.0m;
        _mockApiService.Setup(x => x.PostAsync<object>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _tarjetasService.AcumularPuntosAsync(tarjetaId, montoCompra);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }

    [Fact]
    public async Task CanjearPuntosAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var puntosACanjear = 30;
        var descuento = 10.0m;
        _mockApiService.Setup(x => x.PostAsync<object>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _tarjetasService.CanjearPuntosAsync(tarjetaId, puntosACanjear, descuento);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }

    [Fact]
    public async Task EliminarTarjetaAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        _mockApiService.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _tarjetasService.EliminarTarjetaAsync(tarjetaId);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }

    // Tests para 401/403/429
    [Fact]
    public async Task BuscarTarjetaAsync_WithUnauthorized_ShouldPropagate401()
    {
        // Arrange
        var numeroTarjeta = "123456789";
        var apiResponse = ApiResponse<List<TarjetaFidelizacionDto>>.ErrorResponse(new List<string> { "Unauthorized" }, "Unauthorized", 401);
        _mockApiService.Setup(x => x.GetAsync<List<TarjetaFidelizacionDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.BuscarTarjetaAsync(numeroTarjeta);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(401, result.StatusCode);
        Assert.Contains("Unauthorized", result.Error);
    }

    [Fact]
    public async Task BuscarTarjetaAsync_WithForbidden_ShouldPropagate403()
    {
        // Arrange
        var numeroTarjeta = "123456789";
        var apiResponse = ApiResponse<List<TarjetaFidelizacionDto>>.ErrorResponse(new List<string> { "Forbidden" }, "Forbidden", 403);
        _mockApiService.Setup(x => x.GetAsync<List<TarjetaFidelizacionDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.BuscarTarjetaAsync(numeroTarjeta);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Forbidden", result.Error);
    }

    [Fact]
    public async Task BuscarTarjetaAsync_WithTooManyRequests_ShouldPropagate429()
    {
        // Arrange
        var numeroTarjeta = "123456789";
        var apiResponse = ApiResponse<List<TarjetaFidelizacionDto>>.ErrorResponse(new List<string> { "Too Many Requests" }, "Too Many Requests", 429);
        _mockApiService.Setup(x => x.GetAsync<List<TarjetaFidelizacionDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.BuscarTarjetaAsync(numeroTarjeta);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(429, result.StatusCode);
        Assert.Contains("Too Many Requests", result.Error);
    }

    // Tests para 204/empty body
    [Fact]
    public async Task BuscarTarjetaAsync_WithEmptyBody_ShouldReturnFailure()
    {
        // Arrange
        var numeroTarjeta = "123456789";
        var apiResponse = ApiResponse<List<TarjetaFidelizacionDto>>.SuccessResponse(new List<TarjetaFidelizacionDto>());
        _mockApiService.Setup(x => x.GetAsync<List<TarjetaFidelizacionDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.BuscarTarjetaAsync(numeroTarjeta);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Tarjeta no encontrada", result.Error);
    }

    // Tests para cancelación
    [Fact]
    public async Task BuscarTarjetaAsync_WhenCancelled_ShouldReturnCancelled()
    {
        // Arrange
        var numeroTarjeta = "123456789";
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel the token immediately

        // Act
        var result = await _tarjetasService.BuscarTarjetaAsync(numeroTarjeta, cts.Token);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Operación cancelada por el usuario", result.Error);
        _mockApiService.Verify(x => x.GetAsync<List<TarjetaFidelizacionDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ActivarTarjetaAsync_WhenCancelled_ShouldReturnCancelled()
    {
        // Arrange
        var numeroTarjeta = "123456789";
        var nombreCliente = "Juan Pérez";
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel the token immediately

        // Act
        var result = await _tarjetasService.ActivarTarjetaAsync(numeroTarjeta, nombreCliente, cts.Token);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Operación cancelada por el usuario", result.Error);
        _mockApiService.Verify(x => x.GetAsync<List<TarjetaFidelizacionDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
} 
