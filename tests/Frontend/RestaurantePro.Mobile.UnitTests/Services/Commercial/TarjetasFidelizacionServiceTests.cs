using Moq;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Commercial;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Services.Commercial;

public class TarjetasFidelizacionServiceTests
{
    private readonly Mock<IApiService> _mockApiService;
    private readonly TarjetasFidelizacionService _tarjetasService;

    public TarjetasFidelizacionServiceTests()
    {
        _mockApiService = new Mock<IApiService>();
        _tarjetasService = new TarjetasFidelizacionService(_mockApiService.Object);
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

        var apiResponse = ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(tarjeta);
        _mockApiService.Setup(x => x.GetAsync<TarjetaFidelizacionDto>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.BuscarTarjetaAsync(numeroTarjeta);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(numeroTarjeta, result.Data.NumeroTarjeta);
        _mockApiService.Verify(x => x.GetAsync<TarjetaFidelizacionDto>($"api/tarjetas-fidelizacion/buscar?numero={numeroTarjeta}", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ActivarTarjetaAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var numeroTarjeta = "987654321";
        var nombreCliente = "María García";
        var tarjeta = new TarjetaFidelizacionDto
        {
            Id = Guid.NewGuid(),
            NumeroTarjeta = numeroTarjeta,
            ClienteNombre = nombreCliente,
            PuntosDisponibles = 0,
            Estado = "Activa"
        };

        var apiResponse = ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(tarjeta);
        _mockApiService.Setup(x => x.PostAsync<TarjetaFidelizacionDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.ActivarTarjetaAsync(numeroTarjeta, nombreCliente);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(numeroTarjeta, result.Data.NumeroTarjeta);
        _mockApiService.Verify(x => x.PostAsync<TarjetaFidelizacionDto>("api/tarjetas-fidelizacion/activar", It.Is<object>(data => data.ToString().Contains(numeroTarjeta)), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerTarjetaPorCodigoAsync_WithValidCodigo_ShouldReturnSuccess()
    {
        // Arrange
        var codigo = "ABC123";
        var tarjeta = new TarjetaFidelizacionDto
        {
            Id = Guid.NewGuid(),
            CodigoTarjeta = codigo,
            ClienteNombre = "María García",
            PuntosDisponibles = 200,
            Estado = "Activa"
        };

        var apiResponse = ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(tarjeta);
        _mockApiService.Setup(x => x.GetAsync<TarjetaFidelizacionDto>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.ObtenerTarjetaPorCodigoAsync(codigo);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(codigo, result.Data.CodigoTarjeta);
        _mockApiService.Verify(x => x.GetAsync<TarjetaFidelizacionDto>($"api/tarjetas-fidelizacion/codigo/{codigo}", It.IsAny<string>()), Times.Once);
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
        _mockApiService.Setup(x => x.GetAsync<TarjetaFidelizacionDto>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.ObtenerTarjetaAsync(tarjetaId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(tarjetaId, result.Data.Id);
        _mockApiService.Verify(x => x.GetAsync<TarjetaFidelizacionDto>($"api/tarjetas-fidelizacion/{tarjetaId}", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerTransaccionesAsync_WithValidTarjetaId_ShouldReturnSuccess()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var transacciones = new List<TransaccionPuntosDto>
        {
            new() { Id = Guid.NewGuid(), TipoTransaccion = "Acumulación", Puntos = 50, FechaTransaccion = DateTime.Now },
            new() { Id = Guid.NewGuid(), TipoTransaccion = "Canje", Puntos = -20, FechaTransaccion = DateTime.Now.AddDays(-1) }
        };

        var apiResponse = ApiResponse<List<TransaccionPuntosDto>>.SuccessResponse(transacciones);
        _mockApiService.Setup(x => x.GetAsync<List<TransaccionPuntosDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.ObtenerHistorialTransaccionesAsync(tarjetaId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        _mockApiService.Verify(x => x.GetAsync<List<TransaccionPuntosDto>>($"api/tarjetas-fidelizacion/{tarjetaId}/transacciones", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task AcumularPuntosAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var puntos = 25;
        var tarjeta = new TarjetaFidelizacionDto
        {
            Id = tarjetaId,
            NumeroTarjeta = "123456789",
            ClienteNombre = "Juan Pérez",
            PuntosDisponibles = 175,
            Estado = "Activa"
        };

        var apiResponse = ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(tarjeta);
        _mockApiService.Setup(x => x.PostAsync<TarjetaFidelizacionDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.AcumularPuntosAsync(tarjetaId, puntos);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(175, result.Data.PuntosDisponibles);
        _mockApiService.Verify(x => x.PostAsync<TarjetaFidelizacionDto>($"api/tarjetas-fidelizacion/{tarjetaId}/acumular-puntos", It.Is<object>(data => data.ToString().Contains(puntos.ToString())), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CanjearPuntosAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var puntos = 30;
        var tarjeta = new TarjetaFidelizacionDto
        {
            Id = tarjetaId,
            NumeroTarjeta = "123456789",
            ClienteNombre = "Juan Pérez",
            PuntosDisponibles = 120,
            Estado = "Activa"
        };

        var apiResponse = ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(tarjeta);
        _mockApiService.Setup(x => x.PostAsync<TarjetaFidelizacionDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.CanjearPuntosAsync(tarjetaId, puntos, 10.0m);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(120, result.Data.PuntosDisponibles);
        _mockApiService.Verify(x => x.PostAsync<TarjetaFidelizacionDto>($"api/tarjetas-fidelizacion/{tarjetaId}/canjear-puntos", It.Is<object>(data => data.ToString().Contains(puntos.ToString())), It.IsAny<string>()), Times.Once);
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
        _mockApiService.Setup(x => x.PostAsync<List<TarjetaFidelizacionDto>>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.ObtenerTarjetasAsync(new FiltroTarjetasFidelizacionDto());

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        _mockApiService.Verify(x => x.PostAsync<List<TarjetaFidelizacionDto>>("api/tarjetas-fidelizacion/buscar", It.IsAny<object>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DesactivarTarjetaAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var apiResponse = ApiResponse<bool>.SuccessResponse(true);
        
        _mockApiService.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.DesactivarTarjetaAsync(tarjetaId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data);
        _mockApiService.Verify(x => x.DeleteAsync($"api/tarjetas-fidelizacion/{tarjetaId}", It.IsAny<string>()), Times.Once);
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
        _mockApiService.Setup(x => x.GetAsync<List<TarjetaFidelizacionDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.ObtenerTarjetasActivasAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        _mockApiService.Verify(x => x.GetAsync<List<TarjetaFidelizacionDto>>("api/tarjetas-fidelizacion/activas", It.IsAny<string>()), Times.Once);
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

        var apiResponse = ApiResponse<HistorialPuntosDto>.SuccessResponse(historial);
        _mockApiService.Setup(x => x.GetAsync<HistorialPuntosDto>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.ObtenerHistorialPuntosAsync(tarjetaId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Transacciones.Count);
        _mockApiService.Verify(x => x.GetAsync<HistorialPuntosDto>($"api/tarjetas-fidelizacion/{tarjetaId}/historial-puntos", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task BloquearTarjetaAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var apiResponse = ApiResponse<bool>.SuccessResponse(true);
        
        _mockApiService.Setup(x => x.PostAsync<bool>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.BloquearTarjetaAsync(tarjetaId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data);
        _mockApiService.Verify(x => x.PostAsync<bool>($"api/tarjetas-fidelizacion/{tarjetaId}/bloquear", It.IsAny<object>(), It.IsAny<string>()), Times.Once);
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

        var apiResponse = ApiResponse<HistorialPuntosDto>.SuccessResponse(historial);
        _mockApiService.Setup(x => x.GetAsync<HistorialPuntosDto>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.ObtenerHistorialPuntosAsync(tarjetaId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(tarjetaId, result.Data.Id);
        Assert.Equal(300, result.Data.PuntosDisponibles);
        _mockApiService.Verify(x => x.GetAsync<HistorialPuntosDto>($"api/tarjetas-fidelizacion/{tarjetaId}/historial-puntos", It.IsAny<string>()), Times.Once);
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
        _mockApiService.Setup(x => x.GetAsync<EstadisticasTarjetaDto>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.ObtenerEstadisticasAsync(tarjetaId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(tarjetaId, result.Data.TarjetaId);
        Assert.Equal(1000, result.Data.PuntosAcumulados);
        _mockApiService.Verify(x => x.GetAsync<EstadisticasTarjetaDto>($"api/tarjetas-fidelizacion/{tarjetaId}/estadisticas", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task EliminarTarjetaAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var apiResponse = ApiResponse<bool>.SuccessResponse(true);
        
        _mockApiService.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _tarjetasService.EliminarTarjetaAsync(tarjetaId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data);
        _mockApiService.Verify(x => x.DeleteAsync($"api/tarjetas-fidelizacion/{tarjetaId}", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task BuscarTarjetaAsync_WithApiError_ShouldReturnFailure()
    {
        // Arrange
        var numeroTarjeta = "123456789";
        var errorResponse = ApiResponse<TarjetaFidelizacionDto>.ErrorResponse(new List<string> { "Tarjeta no encontrada" }, "Tarjeta no encontrada", 404);
        _mockApiService.Setup(x => x.GetAsync<TarjetaFidelizacionDto>(It.IsAny<string>(), It.IsAny<string>()))
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
        _mockApiService.Setup(x => x.GetAsync<TarjetaFidelizacionDto>(It.IsAny<string>(), It.IsAny<string>()))
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
        _mockApiService.Setup(x => x.PostAsync<TarjetaFidelizacionDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _tarjetasService.ActivarTarjetaAsync(numeroTarjeta, nombreCliente);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }

    [Fact]
    public async Task AcumularPuntosAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var puntos = 25;
        _mockApiService.Setup(x => x.PostAsync<TarjetaFidelizacionDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _tarjetasService.AcumularPuntosAsync(tarjetaId, puntos);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }

    [Fact]
    public async Task CanjearPuntosAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var puntos = 30;
        _mockApiService.Setup(x => x.PostAsync<TarjetaFidelizacionDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _tarjetasService.CanjearPuntosAsync(tarjetaId, puntos, 10.0m);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }

    [Fact]
    public async Task EliminarTarjetaAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        _mockApiService.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<string>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _tarjetasService.EliminarTarjetaAsync(tarjetaId);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }
} 