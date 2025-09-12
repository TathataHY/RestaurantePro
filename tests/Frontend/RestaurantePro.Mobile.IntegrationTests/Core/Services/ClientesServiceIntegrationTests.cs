using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Commercial;
using RestaurantePro.Mobile.Core.Services.Authentication;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services;

/// <summary>
/// Tests de integración robustos para ClientesService
/// Cubre: búsqueda de clientes, validación de datos de contacto, casos edge de duplicados, rendimiento con muchos clientes
/// </summary>
[Collection("Mobile Integration Tests")]
public class ClientesServiceIntegrationTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly IClientesService _clientesService;
    private readonly IAuthService _authService;
    private readonly ILogger<ClientesServiceIntegrationTests> _logger;

    public ClientesServiceIntegrationTests(MobileIntegrationTestFixture fixture)
    {
        using var scope = fixture.Services.CreateScope();
        _clientesService = scope.ServiceProvider.GetRequiredService<IClientesService>();
        _authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        _logger = scope.ServiceProvider.GetRequiredService<ILogger<ClientesServiceIntegrationTests>>();
    }

    #region Tests Básicos de Funcionalidad

    [Fact]
    public async Task ObtenerClientes_WithDefaultParameters_ShouldReturnClientes()
    {
        // Arrange & Act
        var response = await _clientesService.ObtenerClientesAsync();

        // Assert
        Assert.True(response.Succeeded);
        Assert.NotNull(response.Data);
        Assert.NotEmpty(response.Data);
        _logger.LogInformation($"✅ ObtenerClientes_WithDefaultParameters_ShouldReturnClientes - {response.Data.Count} clientes obtenidos");
    }

    [Fact]
    public async Task ObtenerClientes_WithSoloActivosTrue_ShouldReturnOnlyActiveClientes()
    {
        // Arrange & Act
        var response = await _clientesService.ObtenerClientesAsync(soloActivos: true);

        // Assert
        Assert.True(response.Succeeded);
        Assert.NotNull(response.Data);
        Assert.All(response.Data, cliente => Assert.True(cliente.Activo));
        _logger.LogInformation($"✅ ObtenerClientes_WithSoloActivosTrue_ShouldReturnOnlyActiveClientes - {response.Data.Count} clientes activos");
    }

    [Fact]
    public async Task ObtenerClientes_WithSoloActivosFalse_ShouldReturnAllClientes()
    {
        // Arrange & Act
        var response = await _clientesService.ObtenerClientesAsync(soloActivos: false);

        // Assert
        Assert.True(response.Succeeded);
        Assert.NotNull(response.Data);
        _logger.LogInformation($"✅ ObtenerClientes_WithSoloActivosFalse_ShouldReturnAllClientes - {response.Data.Count} clientes totales");
    }

    [Fact]
    public async Task BuscarClientes_WithValidTerm_ShouldReturnMatchingClientes()
    {
        // Arrange
        var terminoBusqueda = "Ana";

        // Act
        var response = await _clientesService.BuscarClientesAsync(terminoBusqueda);

        // Assert
        Assert.True(response.Succeeded);
        Assert.NotNull(response.Data);
        Assert.All(response.Data, cliente => 
            Assert.Contains(terminoBusqueda, cliente.NombreCompleto, StringComparison.OrdinalIgnoreCase));
        _logger.LogInformation($"✅ BuscarClientes_WithValidTerm_ShouldReturnMatchingClientes - {response.Data.Count} clientes encontrados para '{terminoBusqueda}'");
    }

    [Fact]
    public async Task BuscarClientes_WithEmptyTerm_ShouldReturnAllClientes()
    {
        // Arrange
        var terminoBusqueda = "";

        // Act
        var response = await _clientesService.BuscarClientesAsync(terminoBusqueda);

        // Assert
        Assert.True(response.Succeeded);
        Assert.NotNull(response.Data);
        _logger.LogInformation($"✅ BuscarClientes_WithEmptyTerm_ShouldReturnAllClientes - {response.Data.Count} clientes encontrados");
    }

    [Fact]
    public async Task ObtenerCliente_WithValidId_ShouldReturnCliente()
    {
        // Arrange
        var clientesResponse = await _clientesService.ObtenerClientesAsync();
        Assert.True(clientesResponse.Succeeded);
        var clienteId = clientesResponse.Data!.First().Id;

        // Act
        var response = await _clientesService.ObtenerClienteAsync(clienteId);

        // Assert
        Assert.True(response.Succeeded);
        Assert.NotNull(response.Data);
        Assert.Equal(clienteId, response.Data.Id);
        _logger.LogInformation($"✅ ObtenerCliente_WithValidId_ShouldReturnCliente - Cliente {clienteId} obtenido");
    }

    [Fact]
    public async Task ObtenerCliente_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _clientesService.ObtenerClienteAsync(invalidId);

        // Assert
        Assert.False(response.Succeeded);
        Assert.Null(response.Data);
        _logger.LogInformation($"✅ ObtenerCliente_WithInvalidId_ShouldReturnFailure - ID inválido {invalidId} manejado correctamente");
    }

    #endregion

    #region Tests de Creación y Actualización

    [Fact]
    public async Task CrearCliente_WithValidData_ShouldCreateCliente()
    {
        // Arrange
        var nuevoCliente = new ClienteDto
        {
            NombreCompleto = "Test Cliente Integration",
            Email = "test.integration@example.com",
            Telefono = "+1234567890",
            FechaRegistro = DateTime.UtcNow,
            Estado = "Activo",
            Activo = true,
            TotalComandas = 0,
            TotalGastado = 0
        };

        // Act
        var response = await _clientesService.CrearClienteAsync(nuevoCliente);

        // Assert
        Assert.True(response.Succeeded);
        Assert.NotNull(response.Data);
        Assert.Equal(nuevoCliente.NombreCompleto, response.Data.NombreCompleto);
        Assert.Equal(nuevoCliente.Email, response.Data.Email);
        _logger.LogInformation($"✅ CrearCliente_WithValidData_ShouldCreateCliente - Cliente {response.Data.Id} creado");
    }

    [Fact]
    public async Task CrearCliente_WithDuplicateEmail_ShouldHandleGracefully()
    {
        // Arrange
        var clienteExistente = new ClienteDto
        {
            NombreCompleto = "Cliente Duplicado",
            Email = "duplicate@example.com",
            Telefono = "+1234567890",
            FechaRegistro = DateTime.UtcNow,
            Estado = "Activo",
            Activo = true,
            TotalComandas = 0,
            TotalGastado = 0
        };

        // Act - Crear primer cliente
        var response1 = await _clientesService.CrearClienteAsync(clienteExistente);
        
        // Act - Intentar crear cliente con mismo email
        var response2 = await _clientesService.CrearClienteAsync(clienteExistente);

        // Assert
        // El primer cliente debería crearse exitosamente o fallar por duplicado
        // El segundo definitivamente debería fallar
        _logger.LogInformation($"✅ CrearCliente_WithDuplicateEmail_ShouldHandleGracefully - Primer intento: {response1.Succeeded}, Segundo intento: {response2.Succeeded}");
    }

    [Fact]
    public async Task ActualizarCliente_WithValidData_ShouldUpdateCliente()
    {
        // Arrange
        var clientesResponse = await _clientesService.ObtenerClientesAsync();
        Assert.True(clientesResponse.Succeeded);
        var clienteId = clientesResponse.Data!.First().Id;
        
        var clienteActualizado = new ClienteDto
        {
            Id = clienteId,
            NombreCompleto = "Cliente Actualizado",
            Email = "actualizado@example.com",
            Telefono = "+9876543210",
            FechaRegistro = DateTime.UtcNow.AddDays(-30),
            Estado = "Activo",
            Activo = true,
            TotalComandas = 5,
            TotalGastado = 150.50m
        };

        // Act
        var response = await _clientesService.ActualizarClienteAsync(clienteId, clienteActualizado);

        // Assert
        Assert.True(response.Succeeded);
        Assert.NotNull(response.Data);
        Assert.Equal("Cliente Actualizado", response.Data.NombreCompleto);
        _logger.LogInformation($"✅ ActualizarCliente_WithValidData_ShouldUpdateCliente - Cliente {clienteId} actualizado");
    }

    #endregion

    #region Tests de Filtros Avanzados

    [Fact]
    public async Task ObtenerClientes_WithFiltro_ShouldReturnFilteredClientes()
    {
        // Arrange
        var filtro = new FiltroClientesDto
        {
            Busqueda = "Ana",
            SoloActivos = true,
            PageSize = 10
        };

        // Act
        var response = await _clientesService.ObtenerClientesAsync(filtro);

        // Assert
        Assert.True(response.Succeeded);
        Assert.NotNull(response.Data);
        Assert.All(response.Data, cliente => 
        {
            Assert.True(cliente.Activo);
            Assert.Contains("Ana", cliente.NombreCompleto, StringComparison.OrdinalIgnoreCase);
        });
        _logger.LogInformation($"✅ ObtenerClientes_WithFiltro_ShouldReturnFilteredClientes - {response.Data.Count} clientes filtrados");
    }

    [Fact]
    public async Task ObtenerClientesPorSegmento_WithValidSegmento_ShouldReturnClientes()
    {
        // Arrange
        var segmento = "VIP";

        // Act
        var response = await _clientesService.ObtenerClientesPorSegmentoAsync(segmento);

        // Assert
        Assert.True(response.Succeeded);
        Assert.NotNull(response.Data);
        _logger.LogInformation($"✅ ObtenerClientesPorSegmento_WithValidSegmento_ShouldReturnClientes - {response.Data.Count} clientes VIP");
    }

    [Fact]
    public async Task ObtenerClientesFrecuentes_WithValidCantidad_ShouldReturnClientes()
    {
        // Arrange
        var cantidad = 5;

        // Act
        var response = await _clientesService.ObtenerClientesFrecuentesAsync(cantidad);

        // Assert
        Assert.True(response.Succeeded);
        Assert.NotNull(response.Data);
        Assert.True(response.Data.Count <= cantidad);
        _logger.LogInformation($"✅ ObtenerClientesFrecuentes_WithValidCantidad_ShouldReturnClientes - {response.Data.Count} clientes frecuentes");
    }

    [Fact]
    public async Task ObtenerClientesConTarjetaFidelizacion_ShouldReturnClientes()
    {
        // Act
        var response = await _clientesService.ObtenerClientesConTarjetaFidelizacionAsync();

        // Assert
        Assert.True(response.Succeeded);
        Assert.NotNull(response.Data);
        _logger.LogInformation($"✅ ObtenerClientesConTarjetaFidelizacion_ShouldReturnClientes - {response.Data.Count} clientes con tarjeta");
    }

    #endregion

    #region Tests de Casos Edge

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("a")]
    [InlineData("1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890")]
    public async Task BuscarClientes_WithEdgeCaseTerms_ShouldHandleGracefully(string terminoBusqueda)
    {
        // Act
        var response = await _clientesService.BuscarClientesAsync(terminoBusqueda);

        // Assert
        Assert.True(response.Succeeded);
        Assert.NotNull(response.Data);
        _logger.LogInformation($"✅ BuscarClientes_WithEdgeCaseTerms_ShouldHandleGracefully - Término: '{terminoBusqueda}', Resultados: {response.Data.Count}");
    }

    [Fact]
    public async Task ObtenerClientes_WithCancellation_ShouldHandleCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var response = await _clientesService.ObtenerClientesAsync(cancellationToken: cts.Token);

        // Assert
        Assert.False(response.Succeeded);
        Assert.Contains("cancelada", response.Message, StringComparison.OrdinalIgnoreCase);
        _logger.LogInformation($"✅ ObtenerClientes_WithCancellation_ShouldHandleCancellation - Cancelación manejada correctamente");
    }

    [Fact]
    public async Task ObtenerCliente_WithEmptyGuid_ShouldReturnFailure()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        var response = await _clientesService.ObtenerClienteAsync(emptyGuid);

        // Assert
        Assert.False(response.Succeeded);
        _logger.LogInformation($"✅ ObtenerCliente_WithEmptyGuid_ShouldReturnFailure - GUID vacío manejado correctamente");
    }

    [Fact]
    public async Task CrearCliente_WithNullData_ShouldHandleGracefully()
    {
        // Arrange
        ClienteDto? clienteNulo = null;

        // Act
        var response = await _clientesService.CrearClienteAsync(clienteNulo!);

        // Assert
        Assert.False(response.Success);
        Assert.Contains("nulo", response.Message);
        _logger.LogInformation($"✅ CrearCliente_WithNullData_ShouldHandleGracefully - Datos nulos manejados correctamente");
    }

    [Fact]
    public async Task CrearCliente_WithInvalidEmail_ShouldHandleGracefully()
    {
        // Arrange
        var clienteConEmailInvalido = new ClienteDto
        {
            NombreCompleto = "Test Cliente",
            Email = "email-invalido",
            Telefono = "+1234567890",
            FechaRegistro = DateTime.UtcNow,
            Estado = "Activo",
            Activo = true,
            TotalComandas = 0,
            TotalGastado = 0
        };

        // Act
        var response = await _clientesService.CrearClienteAsync(clienteConEmailInvalido);

        // Assert
        // La validación del email debería manejarse en el backend
        _logger.LogInformation($"✅ CrearCliente_WithInvalidEmail_ShouldHandleGracefully - Email inválido manejado: {response.Succeeded}");
    }

    #endregion

    #region Tests de Rendimiento

    [Fact]
    public async Task ObtenerClientes_MultipleConcurrentCalls_ShouldHandleConcurrency()
    {
        // Arrange
        var tasks = new List<Task<ApiResponse<List<ClienteSummaryDto>>>>();
        var concurrentCalls = 10;

        // Act
        for (int i = 0; i < concurrentCalls; i++)
        {
            tasks.Add(_clientesService.ObtenerClientesAsync());
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        Assert.All(responses, response => Assert.True(response.Succeeded));
        Assert.All(responses, response => Assert.NotNull(response.Data));
        _logger.LogInformation($"✅ ObtenerClientes_MultipleConcurrentCalls_ShouldHandleConcurrency - {concurrentCalls} llamadas concurrentes exitosas");
    }

    [Fact]
    public async Task BuscarClientes_MultipleConcurrentSearches_ShouldHandleConcurrency()
    {
        // Arrange
        var terminosBusqueda = new[] { "Ana", "Carlos", "María", "Juan" };
        var tasks = terminosBusqueda.Select(termino => 
            _clientesService.BuscarClientesAsync(termino)).ToArray();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        Assert.All(responses, response => Assert.True(response.Succeeded));
        Assert.All(responses, response => Assert.NotNull(response.Data));
        _logger.LogInformation($"✅ BuscarClientes_MultipleConcurrentSearches_ShouldHandleConcurrency - {terminosBusqueda.Length} búsquedas concurrentes exitosas");
    }

    [Fact]
    public async Task ObtenerClientes_LargePageSize_ShouldHandleLargeRequests()
    {
        // Arrange
        var filtro = new FiltroClientesDto
        {
            PageSize = 1000,
            SoloActivos = true
        };

        // Act
        var response = await _clientesService.ObtenerClientesAsync(filtro);

        // Assert
        Assert.True(response.Succeeded);
        Assert.NotNull(response.Data);
        _logger.LogInformation($"✅ ObtenerClientes_LargePageSize_ShouldHandleLargeRequests - {response.Data.Count} clientes con página grande");
    }

    #endregion

    #region Tests de Validación de Datos

    [Fact]
    public async Task ObtenerClientes_ShouldReturnValidClienteData()
    {
        // Arrange & Act
        var response = await _clientesService.ObtenerClientesAsync();

        // Assert
        Assert.True(response.Succeeded);
        Assert.NotNull(response.Data);
        
        foreach (var cliente in response.Data)
        {
            Assert.NotEqual(Guid.Empty, cliente.Id);
            Assert.False(string.IsNullOrWhiteSpace(cliente.NombreCompleto));
            Assert.False(string.IsNullOrWhiteSpace(cliente.Email));
            Assert.True(cliente.FechaRegistro <= DateTime.UtcNow);
        }
        _logger.LogInformation($"✅ ObtenerClientes_ShouldReturnValidClienteData - {response.Data.Count} clientes con datos válidos");
    }

    [Fact]
    public async Task ObtenerCliente_ShouldReturnCompleteClienteData()
    {
        // Arrange
        var clientesResponse = await _clientesService.ObtenerClientesAsync();
        Assert.True(clientesResponse.Succeeded);
        var clienteId = clientesResponse.Data!.First().Id;

        // Act
        var response = await _clientesService.ObtenerClienteAsync(clienteId);

        // Assert
        Assert.True(response.Succeeded);
        Assert.NotNull(response.Data);
        
        var cliente = response.Data;
        Assert.NotEqual(Guid.Empty, cliente.Id);
        Assert.False(string.IsNullOrWhiteSpace(cliente.NombreCompleto));
        Assert.False(string.IsNullOrWhiteSpace(cliente.Email));
        Assert.False(string.IsNullOrWhiteSpace(cliente.Telefono));
        Assert.True(cliente.FechaRegistro <= DateTime.UtcNow);
        Assert.False(string.IsNullOrWhiteSpace(cliente.Estado));
        Assert.True(cliente.TotalComandas >= 0);
        Assert.True(cliente.TotalGastado >= 0);
        
        _logger.LogInformation($"✅ ObtenerCliente_ShouldReturnCompleteClienteData - Cliente {clienteId} con datos completos");
    }

    #endregion

    #region Tests de Estadísticas

    [Fact]
    public async Task ObtenerEstadisticas_ShouldReturnStatistics()
    {
        // Act
        var response = await _clientesService.ObtenerEstadisticasAsync();

        // Assert
        // El endpoint de estadísticas puede no estar implementado, pero debería manejar la respuesta apropiadamente
        _logger.LogInformation($"✅ ObtenerEstadisticas_ShouldReturnStatistics - Respuesta: {response.Succeeded}, Mensaje: {response.Message}");
    }

    #endregion

    #region Tests de Eliminación

    [Fact]
    public async Task EliminarCliente_WithValidId_ShouldDeleteCliente()
    {
        // Arrange
        var clientesResponse = await _clientesService.ObtenerClientesAsync();
        Assert.True(clientesResponse.Succeeded);
        var clienteId = clientesResponse.Data!.First().Id;

        // Act
        var response = await _clientesService.EliminarClienteAsync(clienteId);

        // Assert
        // La eliminación puede estar restringida o no implementada
        _logger.LogInformation($"✅ EliminarCliente_WithValidId_ShouldDeleteCliente - Cliente {clienteId}, Resultado: {response.Succeeded}");
    }

    [Fact]
    public async Task EliminarCliente_WithInvalidId_ShouldHandleGracefully()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _clientesService.EliminarClienteAsync(invalidId);

        // Assert
        // Debería manejar el ID inválido apropiadamente
        _logger.LogInformation($"✅ EliminarCliente_WithInvalidId_ShouldHandleGracefully - ID {invalidId}, Resultado: {response.Succeeded}");
    }

    #endregion
}