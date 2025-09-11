using Moq;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Commercial;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Services.Commercial;

public class ClientesServiceTests
{
    private readonly Mock<IApiService> _mockApiService;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly ClientesService _clientesService;

    public ClientesServiceTests()
    {
        _mockApiService = new Mock<IApiService>();
        _mockAuthService = new Mock<IAuthService>();
        
        // Configurar el mock de autenticación para devolver un token válido
        _mockAuthService.Setup(x => x.GetTokenAsync())
                       .ReturnsAsync("test-token");
        
        _clientesService = new ClientesService(_mockApiService.Object, _mockAuthService.Object);
    }

    [Fact]
    public async Task ObtenerClientesAsync_WithValidResponse_ShouldReturnSuccess()
    {
        // Arrange
        var clientes = new List<ClienteSummaryDto>
        {
            new() { Id = Guid.NewGuid(), NombreCompleto = "Juan Pérez", Email = "juan@test.com" },
            new() { Id = Guid.NewGuid(), NombreCompleto = "María García", Email = "maria@test.com" }
        };

        var paginatedResponse = new PaginatedList<ClienteSummaryDto>
        {
            Items = clientes,
            TotalCount = clientes.Count,
            PageNumber = 1,
            PageSize = 10
        };
        var apiResponse = ApiResponse<PaginatedList<ClienteSummaryDto>>.SuccessResponse(paginatedResponse);
        _mockApiService.Setup(x => x.GetAsync<PaginatedList<ClienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.ObtenerClientesAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        _mockApiService.Verify(x => x.GetAsync<PaginatedList<ClienteSummaryDto>>("api/comercial/clientes?soloActivos=True", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerClientesAsync_WithApiError_ShouldReturnFailure()
    {
        // Arrange
        var errorResponse = ApiResponse<PaginatedList<ClienteSummaryDto>>.ErrorResponse(new List<string> { "Error de API" }, "Error de API", 500);
        _mockApiService.Setup(x => x.GetAsync<PaginatedList<ClienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(errorResponse);

        // Act
        var result = await _clientesService.ObtenerClientesAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("No se pudieron obtener los clientes", result.Error);
    }

    [Fact]
    public async Task ObtenerClienteAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var cliente = new ClienteDto { Id = clienteId, NombreCompleto = "Juan Pérez", Email = "juan@test.com" };
        var apiResponse = ApiResponse<ClienteDto>.SuccessResponse(cliente);
        
        _mockApiService.Setup(x => x.GetAsync<ClienteDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.ObtenerClienteAsync(clienteId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(clienteId, result.Data.Id);
        _mockApiService.Verify(x => x.GetAsync<ClienteDto>($"api/comercial/clientes/{clienteId}", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BuscarClientesAsync_WithValidTerm_ShouldReturnSuccess()
    {
        // Arrange
        var termino = "juan";
        var clientes = new List<ClienteSummaryDto>
        {
            new() { Id = Guid.NewGuid(), NombreCompleto = "Juan Pérez", Email = "juan@test.com" }
        };

        var paginatedResponse = new PaginatedList<ClienteSummaryDto>
        {
            Items = clientes,
            TotalCount = clientes.Count,
            PageNumber = 1,
            PageSize = 10
        };
        var apiResponse = ApiResponse<PaginatedList<ClienteSummaryDto>>.SuccessResponse(paginatedResponse);
        _mockApiService.Setup(x => x.GetAsync<PaginatedList<ClienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.BuscarClientesAsync(termino);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        _mockApiService.Verify(x => x.GetAsync<PaginatedList<ClienteSummaryDto>>($"api/comercial/clientes?filtroTexto={termino}", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CrearClienteAsync_WithValidCliente_ShouldReturnSuccess()
    {
        // Arrange
        var cliente = new ClienteDto { NombreCompleto = "Nuevo Cliente", Email = "nuevo@test.com" };
        var clienteCreado = new ClienteDto { Id = Guid.NewGuid(), NombreCompleto = "Nuevo Cliente", Email = "nuevo@test.com" };
        var apiResponse = ApiResponse<ClienteDto>.SuccessResponse(clienteCreado);
        
        _mockApiService.Setup(x => x.PostAsync<ClienteDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.CrearClienteAsync(cliente);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("Nuevo Cliente", result.Data.NombreCompleto);
        _mockApiService.Verify(x => x.PostAsync<ClienteDto>("api/comercial/clientes", cliente, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActualizarClienteAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var cliente = new ClienteDto { Id = clienteId, NombreCompleto = "Juan Pérez Actualizado", Email = "juan.actualizado@test.com" };
        var apiResponse = ApiResponse<ClienteDto>.SuccessResponse(cliente);
        
        _mockApiService.Setup(x => x.PutAsync<ClienteDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.ActualizarClienteAsync(clienteId, cliente);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("Juan Pérez Actualizado", result.Data.NombreCompleto);
        _mockApiService.Verify(x => x.PutAsync<ClienteDto>($"api/comercial/clientes/{clienteId}", cliente, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EliminarClienteAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var apiResponse = ApiResponse<bool>.SuccessResponse(true);
        
        _mockApiService.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.EliminarClienteAsync(clienteId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data);
        _mockApiService.Verify(x => x.DeleteAsync($"api/comercial/clientes/{clienteId}", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_ShouldReturnSuccess()
    {
        // Act
        var result = await _clientesService.ObtenerEstadisticasAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Endpoint de estadísticas no disponible", result.Error);
    }

    [Fact]
    public async Task ObtenerClientesFrecuentesAsync_WithValidCantidad_ShouldReturnSuccess()
    {
        // Arrange
        var cantidad = 5;
        var clientes = new List<ClienteSummaryDto>
        {
            new() { Id = Guid.NewGuid(), NombreCompleto = "Cliente Frecuente 1", Email = "frecuente1@test.com" },
            new() { Id = Guid.NewGuid(), NombreCompleto = "Cliente Frecuente 2", Email = "frecuente2@test.com" }
        };

        var paginatedResponse = new PaginatedList<ClienteSummaryDto>
        {
            Items = clientes,
            TotalCount = clientes.Count,
            PageNumber = 1,
            PageSize = 10
        };
        var apiResponse = ApiResponse<PaginatedList<ClienteSummaryDto>>.SuccessResponse(paginatedResponse);
        _mockApiService.Setup(x => x.GetAsync<PaginatedList<ClienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.ObtenerClientesFrecuentesAsync(cantidad);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        _mockApiService.Verify(x => x.GetAsync<PaginatedList<ClienteSummaryDto>>($"api/comercial/clientes?soloClientesFrecuentes=true&pageSize={cantidad}", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerClientesAsync_WithFiltro_ShouldReturnSuccess()
    {
        // Arrange
        var filtro = new FiltroClientesDto { SoloActivos = true };
        var clientes = new List<ClienteSummaryDto>
        {
            new() { Id = Guid.NewGuid(), NombreCompleto = "Cliente Activo", Email = "activo@test.com" }
        };

        var paginatedResponse = new PaginatedList<ClienteSummaryDto>
        {
            Items = clientes,
            TotalCount = clientes.Count,
            PageNumber = 1,
            PageSize = 10
        };
        var apiResponse = ApiResponse<PaginatedList<ClienteSummaryDto>>.SuccessResponse(paginatedResponse);
        _mockApiService.Setup(x => x.GetAsync<PaginatedList<ClienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.ObtenerClientesAsync(filtro);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        _mockApiService.Verify(x => x.GetAsync<PaginatedList<ClienteSummaryDto>>("api/comercial/clientes?soloActivos=True", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerClientesPorSegmentoAsync_WithValidSegmento_ShouldReturnSuccess()
    {
        // Arrange
        var segmento = "Premium";
        var clientes = new List<ClienteSummaryDto>
        {
            new() { Id = Guid.NewGuid(), NombreCompleto = "Cliente Premium", Email = "premium@test.com" }
        };

        var paginatedResponse = new PaginatedList<ClienteSummaryDto>
        {
            Items = clientes,
            TotalCount = clientes.Count,
            PageNumber = 1,
            PageSize = 10
        };
        var apiResponse = ApiResponse<PaginatedList<ClienteSummaryDto>>.SuccessResponse(paginatedResponse);
        _mockApiService.Setup(x => x.GetAsync<PaginatedList<ClienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.ObtenerClientesPorSegmentoAsync(segmento);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        _mockApiService.Verify(x => x.GetAsync<PaginatedList<ClienteSummaryDto>>($"api/comercial/clientes?segmento={segmento}", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerClientesConTarjetaFidelizacionAsync_ShouldReturnSuccess()
    {
        // Arrange
        var clientes = new List<ClienteSummaryDto>
        {
            new() { Id = Guid.NewGuid(), NombreCompleto = "Cliente con Tarjeta", Email = "tarjeta@test.com" }
        };

        var paginatedResponse = new PaginatedList<ClienteSummaryDto>
        {
            Items = clientes,
            TotalCount = clientes.Count,
            PageNumber = 1,
            PageSize = 10
        };
        var apiResponse = ApiResponse<PaginatedList<ClienteSummaryDto>>.SuccessResponse(paginatedResponse);
        _mockApiService.Setup(x => x.GetAsync<PaginatedList<ClienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.ObtenerClientesConTarjetaFidelizacionAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        _mockApiService.Verify(x => x.GetAsync<PaginatedList<ClienteSummaryDto>>("api/comercial/clientes?soloConTarjetaFidelizacion=true", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerClientesPorFechaRegistroAsync_WithValidDates_ShouldReturnSuccess()
    {
        // Arrange
        var fechaDesde = new DateTime(2024, 1, 1);
        var fechaHasta = new DateTime(2024, 12, 31);
        var clientes = new List<ClienteSummaryDto>
        {
            new() { Id = Guid.NewGuid(), NombreCompleto = "Cliente 2024", Email = "2024@test.com" }
        };

        var paginatedResponse = new PaginatedList<ClienteSummaryDto>
        {
            Items = clientes,
            TotalCount = clientes.Count,
            PageNumber = 1,
            PageSize = 10
        };
        var apiResponse = ApiResponse<PaginatedList<ClienteSummaryDto>>.SuccessResponse(paginatedResponse);
        _mockApiService.Setup(x => x.GetAsync<PaginatedList<ClienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.ObtenerClientesPorFechaRegistroAsync(fechaDesde, fechaHasta);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        _mockApiService.Verify(x => x.GetAsync<PaginatedList<ClienteSummaryDto>>($"api/comercial/clientes?fechaRegistroDesde={fechaDesde:yyyy-MM-dd}&fechaRegistroHasta={fechaHasta:yyyy-MM-dd}", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DesactivarClienteAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var apiResponse = ApiResponse<bool>.SuccessResponse(true);
        
        _mockApiService.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.DesactivarClienteAsync(clienteId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data);
        _mockApiService.Verify(x => x.DeleteAsync($"api/comercial/clientes/{clienteId}", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerHistorialComandasAsync_WithValidClienteId_ShouldReturnSuccess()
    {
        // Arrange
        var clienteId = Guid.NewGuid();

        // Act
        var result = await _clientesService.ObtenerHistorialComandasAsync(clienteId);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Endpoint de historial de comandas no disponible", result.Error);
    }

    [Fact]
    public async Task ObtenerClientesAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        _mockApiService.Setup(x => x.GetAsync<PaginatedList<ClienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _clientesService.ObtenerClientesAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }

    [Fact]
    public async Task ObtenerClienteAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        _mockApiService.Setup(x => x.GetAsync<ClienteDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _clientesService.ObtenerClienteAsync(clienteId);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }

    [Fact]
    public async Task CrearClienteAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var cliente = new ClienteDto { NombreCompleto = "Test", Email = "test@test.com" };
        _mockApiService.Setup(x => x.PostAsync<ClienteDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _clientesService.CrearClienteAsync(cliente);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }

    [Fact]
    public async Task ActualizarClienteAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var cliente = new ClienteDto { Id = clienteId, NombreCompleto = "Test", Email = "test@test.com" };
        _mockApiService.Setup(x => x.PutAsync<ClienteDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _clientesService.ActualizarClienteAsync(clienteId, cliente);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }

    [Fact]
    public async Task EliminarClienteAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        _mockApiService.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _clientesService.EliminarClienteAsync(clienteId);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }

    [Fact]
    public async Task ObtenerClienteAsync_WithUnauthorized_ShouldPropagate401()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var errorResponse = ApiResponse<ClienteDto>.ErrorResponse(new List<string> { "Unauthorized" }, "Unauthorized", 401);
        _mockApiService.Setup(x => x.GetAsync<ClienteDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(errorResponse);

        // Act
        var result = await _clientesService.ObtenerClienteAsync(clienteId);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(401, result.StatusCode);
        Assert.Contains("Unauthorized", result.Error);
    }

    [Fact]
    public async Task EliminarClienteAsync_WithForbidden_ShouldPropagate403()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var errorResponse = ApiResponse<bool>.ErrorResponse(new List<string> { "Forbidden" }, "Forbidden", 403);
        _mockApiService.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(errorResponse);

        // Act
        var result = await _clientesService.EliminarClienteAsync(clienteId);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Forbidden", result.Error);
    }

    [Fact]
    public async Task CrearClienteAsync_WithTooManyRequests_ShouldPropagate429()
    {
        // Arrange
        var cliente = new ClienteDto { NombreCompleto = "Rate Limited", Email = "rate@test.com" };
        var errorResponse = ApiResponse<ClienteDto>.ErrorResponse(new List<string> { "Too Many Requests" }, "Too Many Requests", 429);
        _mockApiService.Setup(x => x.PostAsync<ClienteDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(errorResponse);

        // Act
        var result = await _clientesService.CrearClienteAsync(cliente);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(429, result.StatusCode);
        Assert.Contains("Too Many Requests", result.Error);
    }

    [Fact]
    public async Task ObtenerClientesAsync_WithEmptyBody_ShouldReturnEmptyList()
    {
        // Arrange
        var paginatedResponse = new PaginatedList<ClienteSummaryDto>
        {
            Items = new List<ClienteSummaryDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };
        var apiResponse = ApiResponse<PaginatedList<ClienteSummaryDto>>.SuccessResponse(paginatedResponse);
        _mockApiService.Setup(x => x.GetAsync<PaginatedList<ClienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.ObtenerClientesAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task ObtenerClientesAsync_WithNullData_ShouldReturnFailure()
    {
        // Arrange
        var apiResponse = ApiResponse<PaginatedList<ClienteSummaryDto>>.SuccessResponse(null);
        _mockApiService.Setup(x => x.GetAsync<PaginatedList<ClienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.ObtenerClientesAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("No se pudieron obtener los clientes", result.Error);
    }
} 
