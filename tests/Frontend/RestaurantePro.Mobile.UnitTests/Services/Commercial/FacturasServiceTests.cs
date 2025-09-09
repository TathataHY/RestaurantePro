using Moq;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Commercial;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Services.Commercial;

public class FacturasServiceTests
{
    private readonly Mock<IApiService> _mockApiService;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly FacturasService _facturasService;

    public FacturasServiceTests()
    {
        _mockApiService = new Mock<IApiService>();
        _mockAuthService = new Mock<IAuthService>();
        
        // Configurar el mock de autenticación para devolver un token válido
        _mockAuthService.Setup(x => x.GetTokenAsync())
                       .ReturnsAsync("test-token");
        
        _facturasService = new FacturasService(_mockApiService.Object, _mockAuthService.Object);
    }

    [Fact]
    public async Task ObtenerFacturasAsync_WithValidDate_ShouldReturnSuccess()
    {
        // Arrange
        var fecha = new DateTime(2024, 12, 15);
        var facturas = new List<FacturaDto>
        {
            new() { Id = Guid.NewGuid(), NumeroFactura = "FAC001", Total = 150.00m, Estado = "Pagada" },
            new() { Id = Guid.NewGuid(), NumeroFactura = "FAC002", Total = 200.00m, Estado = "Pendiente" }
        };
        
        var apiResponse = ApiResponse<List<FacturaDto>>.SuccessResponse(facturas);
        _mockApiService.Setup(x => x.GetAsync<List<FacturaDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _facturasService.ObtenerFacturasAsync(fecha);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        _mockApiService.Verify(x => x.GetAsync<List<FacturaDto>>($"api/comercial/facturas?fecha={fecha:yyyy-MM-dd}", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task BuscarFacturasAsync_WithValidSearch_ShouldReturnSuccess()
    {
        // Arrange
        var termino = "FAC001";
        var facturas = new List<FacturaDto>
        {
            new() { Id = Guid.NewGuid(), NumeroFactura = "FAC001", Total = 150.00m, Estado = "Pagada" }
        };
        
        var apiResponse = ApiResponse<List<FacturaDto>>.SuccessResponse(facturas);
        _mockApiService.Setup(x => x.GetAsync<List<FacturaDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _facturasService.BuscarFacturasAsync(termino, DateTime.Today);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        _mockApiService.Verify(x => x.GetAsync<List<FacturaDto>>($"api/comercial/facturas/buscar-por-termino?busqueda={termino}&fecha={DateTime.Today:yyyy-MM-dd}", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_WithValidDate_ShouldReturnSuccess()
    {
        // Arrange
        var fecha = new DateTime(2024, 12, 15);
        var estadisticas = new EstadisticasFacturasDto
        {
            TotalFacturas = 25,
            FacturasPagadas = 20,
            FacturasPendientes = 5
        };
        
        var apiResponse = ApiResponse<EstadisticasFacturasDto>.SuccessResponse(estadisticas);
        _mockApiService.Setup(x => x.GetAsync<EstadisticasFacturasDto>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _facturasService.ObtenerEstadisticasAsync(fecha);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(25, result.Data.TotalFacturas);
        Assert.Equal(20, result.Data.FacturasPagadas);
        _mockApiService.Verify(x => x.GetAsync<EstadisticasFacturasDto>($"api/comercial/facturas/estadisticas?fecha={fecha:yyyy-MM-dd}", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ImprimirFacturaAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var apiResponse = ApiResponse<bool>.SuccessResponse(true);
        
        _mockApiService.Setup(x => x.PostAsync<bool>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _facturasService.ImprimirFacturaAsync(facturaId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data);
        _mockApiService.Verify(x => x.PostAsync<bool>($"api/comercial/facturas/{facturaId}/imprimir", It.IsAny<object>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerFacturasPendientesAsync_ShouldReturnSuccess()
    {
        // Arrange
        var facturas = new List<FacturaDto>
        {
            new() { Id = Guid.NewGuid(), NumeroFactura = "FAC003", Total = 100.00m, Estado = "Pendiente" },
            new() { Id = Guid.NewGuid(), NumeroFactura = "FAC004", Total = 250.00m, Estado = "Pendiente" }
        };

        var apiResponse = ApiResponse<List<FacturaDto>>.SuccessResponse(facturas);
        _mockApiService.Setup(x => x.GetAsync<List<FacturaDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _facturasService.ObtenerFacturasPendientesAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.All(result.Data, f => Assert.Equal("Pendiente", f.Estado));
        _mockApiService.Verify(x => x.GetAsync<List<FacturaDto>>("api/comercial/facturas/pendientes", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarPagoAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var pagoDto = new RegistrarPagoDto
        {
            MetodoPago = "Efectivo",
            MontoPagado = 150.00m,
            ReferenciaPago = "REF001"
        };

        var apiResponse = ApiResponse<bool>.SuccessResponse(true);
        _mockApiService.Setup(x => x.PostAsync<bool>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _facturasService.RegistrarPagoAsync(facturaId, pagoDto);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data);
        _mockApiService.Verify(x => x.PostAsync<bool>($"api/comercial/facturas/{facturaId}/pagar", pagoDto, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task AnularFacturaAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var anulacionDto = new AnularFacturaDto
        {
            MotivoAnulacion = "Error en el pedido",
            Observaciones = "Cliente solicitó anulación"
        };

        var apiResponse = ApiResponse<bool>.SuccessResponse(true);
        _mockApiService.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _facturasService.AnularFacturaAsync(facturaId, anulacionDto);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data);
        _mockApiService.Verify(x => x.DeleteAsync($"api/comercial/facturas/{facturaId}", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DescargarFacturaPdfAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var pdfUrl = "https://api.example.com/facturas/FAC001.pdf";
        var apiResponse = ApiResponse<string>.SuccessResponse(pdfUrl);
        
        _mockApiService.Setup(x => x.GetAsync<string>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _facturasService.DescargarFacturaPdfAsync(facturaId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(pdfUrl, result.Data);
        _mockApiService.Verify(x => x.GetAsync<string>($"api/comercial/facturas/{facturaId}/pdf", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task EnviarFacturaPorEmailAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var email = "cliente@test.com";
        var mensaje = "Factura enviada exitosamente";
        var apiResponse = ApiResponse<string>.SuccessResponse(mensaje);
        
        _mockApiService.Setup(x => x.PostAsync<string>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _facturasService.EnviarFacturaPorEmailAsync(facturaId, email);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(mensaje, result.Data);
        _mockApiService.Verify(x => x.PostAsync<string>($"api/comercial/facturas/{facturaId}/enviar-email", It.Is<object>(data => data.ToString().Contains(email)), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerFacturasAsync_WithApiError_ShouldReturnFailure()
    {
        // Arrange
        var fecha = new DateTime(2024, 12, 15);
        var errorResponse = ApiResponse<List<FacturaDto>>.ErrorResponse(new List<string> { "Error de API" }, "Error de API", 500);
        _mockApiService.Setup(x => x.GetAsync<List<FacturaDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(errorResponse);

        // Act
        var result = await _facturasService.ObtenerFacturasAsync(fecha);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de API", result.Error);
    }

    [Fact]
    public async Task ObtenerFacturasAsync_ShouldReturnCancelled_WhenTokenIsCancelled()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var calls = 0;
        _mockApiService
            .Setup(x => x.GetAsync<List<FacturaDto>>(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => calls++)
            .ReturnsAsync(ApiResponse<List<FacturaDto>>.SuccessResponse(new List<FacturaDto>()));

        // Act
        var result = await _facturasService.ObtenerFacturasAsync(DateTime.Today, cts.Token);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("cancelada", result.Error, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task RegistrarPagoAsync_ShouldReturnCancelled_WhenTokenIsCancelled()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var calls = 0;
        _mockApiService
            .Setup(x => x.PostAsync<bool>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Callback(() => calls++)
            .ReturnsAsync(ApiResponse<bool>.SuccessResponse(true));

        // Act
        var result = await _facturasService.RegistrarPagoAsync(Guid.NewGuid(), new RegistrarPagoDto{ MetodoPago = "Efectivo", MontoPagado = 10 }, cts.Token);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("cancelada", result.Error, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task ObtenerFacturasAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var fecha = new DateTime(2024, 12, 15);
        _mockApiService.Setup(x => x.GetAsync<List<FacturaDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _facturasService.ObtenerFacturasAsync(fecha);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }

    [Fact]
    public async Task RegistrarPagoAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var pagoDto = new RegistrarPagoDto { MetodoPago = "Efectivo", MontoPagado = 150.00m };
        
        _mockApiService.Setup(x => x.PostAsync<bool>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _facturasService.RegistrarPagoAsync(facturaId, pagoDto);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }

    [Fact]
    public async Task BuscarFacturasAsync_WithEmptySearch_ShouldReturnSuccess()
    {
        // Arrange
        var termino = "";
        var facturas = new List<FacturaDto>();
        
        var apiResponse = ApiResponse<List<FacturaDto>>.SuccessResponse(facturas);
        _mockApiService.Setup(x => x.GetAsync<List<FacturaDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _facturasService.BuscarFacturasAsync(termino, DateTime.Today);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
        _mockApiService.Verify(x => x.GetAsync<List<FacturaDto>>($"api/comercial/facturas/buscar-por-termino?busqueda={termino}&fecha={DateTime.Today:yyyy-MM-dd}", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_WithNoData_ShouldReturnSuccess()
    {
        // Arrange
        var fecha = new DateTime(2024, 12, 15);
        var estadisticas = new EstadisticasFacturasDto
        {
            TotalFacturas = 0,
            FacturasPagadas = 0,
            FacturasPendientes = 0
        };
        
        var apiResponse = ApiResponse<EstadisticasFacturasDto>.SuccessResponse(estadisticas);
        _mockApiService.Setup(x => x.GetAsync<EstadisticasFacturasDto>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _facturasService.ObtenerEstadisticasAsync(fecha);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(0, result.Data.TotalFacturas);
        _mockApiService.Verify(x => x.GetAsync<EstadisticasFacturasDto>($"api/comercial/facturas/estadisticas?fecha={fecha:yyyy-MM-dd}", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerFacturaAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var factura = new FacturaDto { Id = facturaId, NumeroFactura = "FAC001", Total = 150.00m, Estado = "Pagada" };
        var apiResponse = ApiResponse<FacturaDto>.SuccessResponse(factura);
        
        _mockApiService.Setup(x => x.GetAsync<FacturaDto>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _facturasService.ObtenerFacturaAsync(facturaId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(facturaId, result.Data.Id);
        Assert.Equal("FAC001", result.Data.NumeroFactura);
        _mockApiService.Verify(x => x.GetAsync<FacturaDto>($"api/comercial/facturas/{facturaId}", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerFacturaAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        _mockApiService.Setup(x => x.GetAsync<FacturaDto>(It.IsAny<string>(), It.IsAny<string>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _facturasService.ObtenerFacturaAsync(facturaId);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }

    [Fact]
    public async Task RegistrarPagoAsync_WithApiError_ShouldReturnFailure()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var pagoDto = new RegistrarPagoDto { MetodoPago = "Efectivo", MontoPagado = 150.00m };
        var errorResponse = ApiResponse<bool>.ErrorResponse(new List<string> { "Error de pago" }, "Error de pago", 400);
        
        _mockApiService.Setup(x => x.PostAsync<bool>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                      .ReturnsAsync(errorResponse);

        // Act
        var result = await _facturasService.RegistrarPagoAsync(facturaId, pagoDto);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de pago", result.Error);
    }

    [Fact]
    public async Task AnularFacturaAsync_WithApiError_ShouldReturnFailure()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var anulacionDto = new AnularFacturaDto { MotivoAnulacion = "Error en el pedido" };
        var errorResponse = ApiResponse<bool>.ErrorResponse(new List<string> { "Error de anulación" }, "Error de anulación", 400);
        
        _mockApiService.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(errorResponse);

        // Act
        var result = await _facturasService.AnularFacturaAsync(facturaId, anulacionDto);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de anulación", result.Error);
    }

    [Fact]
    public async Task DescargarFacturaPdfAsync_WithApiError_ShouldReturnFailure()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var errorResponse = ApiResponse<string>.ErrorResponse(new List<string> { "PDF no disponible" }, "PDF no disponible", 404);
        
        _mockApiService.Setup(x => x.GetAsync<string>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(errorResponse);

        // Act
        var result = await _facturasService.DescargarFacturaPdfAsync(facturaId);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("PDF no disponible", result.Error);
    }

    [Fact]
    public async Task EnviarFacturaPorEmailAsync_WithApiError_ShouldReturnFailure()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var email = "cliente@test.com";
        var errorResponse = ApiResponse<string>.ErrorResponse(new List<string> { "Error al enviar email" }, "Error al enviar email", 500);
        
        _mockApiService.Setup(x => x.PostAsync<string>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                      .ReturnsAsync(errorResponse);

        // Act
        var result = await _facturasService.EnviarFacturaPorEmailAsync(facturaId, email);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error al enviar email", result.Error);
    }

    [Fact]
    public async Task ObtenerFacturasAsync_WithUnauthorized_ShouldPropagate401()
    {
        // Arrange
        var fecha = DateTime.Today;
        var errorResponse = ApiResponse<List<FacturaDto>>.ErrorResponse(new List<string> { "Unauthorized" }, "Unauthorized", 401);
        _mockApiService.Setup(x => x.GetAsync<List<FacturaDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(errorResponse);

        // Act
        var result = await _facturasService.ObtenerFacturasAsync(fecha);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(401, result.StatusCode);
        Assert.Contains("Unauthorized", result.Error);
    }

    [Fact]
    public async Task ObtenerFacturasAsync_WithForbidden_ShouldPropagate403()
    {
        // Arrange
        var fecha = DateTime.Today;
        var errorResponse = ApiResponse<List<FacturaDto>>.ErrorResponse(new List<string> { "Forbidden" }, "Forbidden", 403);
        _mockApiService.Setup(x => x.GetAsync<List<FacturaDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(errorResponse);

        // Act
        var result = await _facturasService.ObtenerFacturasAsync(fecha);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Forbidden", result.Error);
    }

    [Fact]
    public async Task ObtenerFacturasAsync_WithTooManyRequests_ShouldPropagate429()
    {
        // Arrange
        var fecha = DateTime.Today;
        var errorResponse = ApiResponse<List<FacturaDto>>.ErrorResponse(new List<string> { "Too Many Requests" }, "Too Many Requests", 429);
        _mockApiService.Setup(x => x.GetAsync<List<FacturaDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(errorResponse);

        // Act
        var result = await _facturasService.ObtenerFacturasAsync(fecha);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(429, result.StatusCode);
        Assert.Contains("Too Many Requests", result.Error);
    }

    [Fact]
    public async Task ObtenerFacturasAsync_WithEmptyBody_ShouldReturnEmptyList()
    {
        // Arrange
        var fecha = DateTime.Today;
        var apiResponse = ApiResponse<List<FacturaDto>>.SuccessResponse(new List<FacturaDto>());
        _mockApiService.Setup(x => x.GetAsync<List<FacturaDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _facturasService.ObtenerFacturasAsync(fecha);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }
} 
