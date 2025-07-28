using Moq;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Commercial;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Services.Commercial;

public class ClientesServiceTests
{
    private readonly Mock<IApiService> _mockApiService;
    private readonly ClientesService _clientesService;

    public ClientesServiceTests()
    {
        _mockApiService = new Mock<IApiService>();
        _clientesService = new ClientesService(_mockApiService.Object);
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

        var apiResponse = ApiResponse<List<ClienteSummaryDto>>.SuccessResponse(clientes);
        _mockApiService.Setup(x => x.GetAsync<List<ClienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.ObtenerClientesAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        _mockApiService.Verify(x => x.GetAsync<List<ClienteSummaryDto>>("api/clientes?soloActivos=True", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerClientesAsync_WithApiError_ShouldReturnFailure()
    {
        // Arrange
        var errorResponse = ApiResponse<List<ClienteSummaryDto>>.ErrorResponse(new List<string> { "Error de API" }, "Error de API", 500);
        _mockApiService.Setup(x => x.GetAsync<List<ClienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(errorResponse);

        // Act
        var result = await _clientesService.ObtenerClientesAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de API", result.Error);
    }

    [Fact]
    public async Task ObtenerClienteAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var cliente = new ClienteDto { Id = clienteId, NombreCompleto = "Juan Pérez", Email = "juan@test.com" };
        var apiResponse = ApiResponse<ClienteDto>.SuccessResponse(cliente);
        
        _mockApiService.Setup(x => x.GetAsync<ClienteDto>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.ObtenerClienteAsync(clienteId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(clienteId, result.Data.Id);
        _mockApiService.Verify(x => x.GetAsync<ClienteDto>($"api/clientes/{clienteId}", It.IsAny<string>()), Times.Once);
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

        var apiResponse = ApiResponse<List<ClienteSummaryDto>>.SuccessResponse(clientes);
        _mockApiService.Setup(x => x.GetAsync<List<ClienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.BuscarClientesAsync(termino);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        _mockApiService.Verify(x => x.GetAsync<List<ClienteSummaryDto>>($"api/clientes/buscar?termino={termino}", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CrearClienteAsync_WithValidCliente_ShouldReturnSuccess()
    {
        // Arrange
        var cliente = new ClienteDto { NombreCompleto = "Nuevo Cliente", Email = "nuevo@test.com" };
        var clienteCreado = new ClienteDto { Id = Guid.NewGuid(), NombreCompleto = "Nuevo Cliente", Email = "nuevo@test.com" };
        var apiResponse = ApiResponse<ClienteDto>.SuccessResponse(clienteCreado);
        
        _mockApiService.Setup(x => x.PostAsync<ClienteDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.CrearClienteAsync(cliente);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("Nuevo Cliente", result.Data.NombreCompleto);
        _mockApiService.Verify(x => x.PostAsync<ClienteDto>("api/clientes", cliente, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ActualizarClienteAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var cliente = new ClienteDto { Id = clienteId, NombreCompleto = "Cliente Actualizado", Email = "actualizado@test.com" };
        var apiResponse = ApiResponse<ClienteDto>.SuccessResponse(cliente);
        
        _mockApiService.Setup(x => x.PutAsync<ClienteDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.ActualizarClienteAsync(clienteId, cliente);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("Cliente Actualizado", result.Data.NombreCompleto);
        _mockApiService.Verify(x => x.PutAsync<ClienteDto>($"api/clientes/{clienteId}", cliente, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task EliminarClienteAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var apiResponse = ApiResponse<bool>.SuccessResponse(true);
        
        _mockApiService.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.EliminarClienteAsync(clienteId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data);
        _mockApiService.Verify(x => x.DeleteAsync($"api/clientes/{clienteId}", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_ShouldReturnSuccess()
    {
        // Arrange
        var estadisticas = new EstadisticasClientesDto
        {
            TotalClientes = 100,
            ClientesActivos = 85,
            ClientesInactivos = 15,
            ClientesNuevosHoy = 10
        };

        var apiResponse = ApiResponse<EstadisticasClientesDto>.SuccessResponse(estadisticas);
        _mockApiService.Setup(x => x.GetAsync<EstadisticasClientesDto>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.ObtenerEstadisticasAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(100, result.Data.TotalClientes);
        Assert.Equal(85, result.Data.ClientesActivos);
        _mockApiService.Verify(x => x.GetAsync<EstadisticasClientesDto>("api/clientes/estadisticas", It.IsAny<string>()), Times.Once);
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

        var apiResponse = ApiResponse<List<ClienteSummaryDto>>.SuccessResponse(clientes);
        _mockApiService.Setup(x => x.GetAsync<List<ClienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.ObtenerClientesFrecuentesAsync(cantidad);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        _mockApiService.Verify(x => x.GetAsync<List<ClienteSummaryDto>>($"api/clientes/frecuentes?cantidad={cantidad}", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerClientesAsync_WithFiltro_ShouldReturnSuccess()
    {
        // Arrange
        var clientes = new List<ClienteSummaryDto>
        {
            new() { Id = Guid.NewGuid(), NombreCompleto = "Cliente Activo", Email = "activo@test.com" }
        };

        var apiResponse = ApiResponse<List<ClienteSummaryDto>>.SuccessResponse(clientes);
        _mockApiService.Setup(x => x.GetAsync<List<ClienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.ObtenerClientesAsync(soloActivos: false);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        _mockApiService.Verify(x => x.GetAsync<List<ClienteSummaryDto>>("api/clientes?soloActivos=False", It.IsAny<string>()), Times.Once);
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

        var apiResponse = ApiResponse<List<ClienteSummaryDto>>.SuccessResponse(clientes);
        _mockApiService.Setup(x => x.GetAsync<List<ClienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.ObtenerClientesPorSegmentoAsync(segmento);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        _mockApiService.Verify(x => x.GetAsync<List<ClienteSummaryDto>>($"api/clientes/segmento/{segmento}", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerClientesConTarjetaFidelizacionAsync_ShouldReturnSuccess()
    {
        // Arrange
        var clientes = new List<ClienteSummaryDto>
        {
            new() { Id = Guid.NewGuid(), NombreCompleto = "Cliente con Tarjeta", Email = "tarjeta@test.com" }
        };

        var apiResponse = ApiResponse<List<ClienteSummaryDto>>.SuccessResponse(clientes);
        _mockApiService.Setup(x => x.GetAsync<List<ClienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.ObtenerClientesConTarjetaFidelizacionAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        _mockApiService.Verify(x => x.GetAsync<List<ClienteSummaryDto>>("api/clientes/con-tarjeta-fidelizacion", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerClientesPorFechaRegistroAsync_WithValidDates_ShouldReturnSuccess()
    {
        // Arrange
        var fechaDesde = new DateTime(2024, 1, 1);
        var fechaHasta = new DateTime(2024, 12, 31);
        var clientes = new List<ClienteSummaryDto>
        {
            new() { Id = Guid.NewGuid(), NombreCompleto = "Cliente Registrado", Email = "registrado@test.com" }
        };

        var apiResponse = ApiResponse<List<ClienteSummaryDto>>.SuccessResponse(clientes);
        _mockApiService.Setup(x => x.GetAsync<List<ClienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.ObtenerClientesPorFechaRegistroAsync(fechaDesde, fechaHasta);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        _mockApiService.Verify(x => x.GetAsync<List<ClienteSummaryDto>>($"api/clientes/por-fecha-registro?fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DesactivarClienteAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var apiResponse = ApiResponse<bool>.SuccessResponse(true);
        
        _mockApiService.Setup(x => x.PostAsync<bool>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.DesactivarClienteAsync(clienteId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data);
        _mockApiService.Verify(x => x.PostAsync<bool>($"api/clientes/{clienteId}/desactivar", It.IsAny<object>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerHistorialComandasAsync_WithValidClienteId_ShouldReturnSuccess()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var comandas = new List<ComandaDto>
        {
            new() { Id = Guid.NewGuid(), Numero = "COM001", Estado = "Finalizada" },
            new() { Id = Guid.NewGuid(), Numero = "COM002", Estado = "Finalizada" }
        };
        
        var apiResponse = ApiResponse<List<ComandaDto>>.SuccessResponse(comandas);
        _mockApiService.Setup(x => x.GetAsync<List<ComandaDto>>(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(apiResponse);

        // Act
        var result = await _clientesService.ObtenerHistorialComandasAsync(clienteId);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        _mockApiService.Verify(x => x.GetAsync<List<ComandaDto>>($"api/clientes/{clienteId}/historial-comandas", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerClientesAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        _mockApiService.Setup(x => x.GetAsync<List<ClienteSummaryDto>>(It.IsAny<string>(), It.IsAny<string>()))
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
        _mockApiService.Setup(x => x.GetAsync<ClienteDto>(It.IsAny<string>(), It.IsAny<string>()))
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
        var cliente = new ClienteDto { NombreCompleto = "Nuevo Cliente", Email = "nuevo@test.com" };
        _mockApiService.Setup(x => x.PostAsync<ClienteDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
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
        var cliente = new ClienteDto { Id = clienteId, NombreCompleto = "Cliente Actualizado", Email = "actualizado@test.com" };
        _mockApiService.Setup(x => x.PutAsync<ClienteDto>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
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
        _mockApiService.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<string>()))
                      .ThrowsAsync(new Exception("Error de red"));

        // Act
        var result = await _clientesService.EliminarClienteAsync(clienteId);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error de red", result.Error);
    }
} 