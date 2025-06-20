using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RestaurantePro.Api.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Inventario;

/// <summary>
/// Tests de integración para ReportesInventarioController
/// Valida todos los endpoints REST del controlador de reportes de inventario
/// </summary>
[Collection("Sequential")]
public class ReportesInventarioControllerTests : ApiIntegrationTestBase, IDisposable
{
    private readonly TestWebApplicationFactory _factory;

    public ReportesInventarioControllerTests() : base(new TestWebApplicationFactory())
    {
        _factory = (TestWebApplicationFactory)Factory;
    }

    [Fact]
    public async Task GetInventarioGeneral_DebeRetornar501NotImplemented()
    {
        // Arrange
        var url = "/api/inventario/reportes/general";

        // Act
        var response = await HttpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        content.Should().Contain("Endpoint no implementado");
    }
    
    [Fact]
    public async Task GetAlertas_DebeRetornar501NotImplemented()
    {
        // Arrange
        var url = "/api/inventario/reportes/alertas";

        // Act
        var response = await HttpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        content.Should().Contain("Endpoint no implementado");
    }

    [Fact]
    public async Task GetAnalisisInventario_DebeRetornar501NotImplemented()
    {
        // Arrange
        var url = "/api/inventario/reportes/analisis";

        // Act
        var response = await HttpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        content.Should().Contain("Endpoint no implementado");
    }

    [Fact]
    public async Task GetRecomendacionesCompra_DebeRetornar501NotImplemented()
    {
        // Arrange
        var url = "/api/inventario/reportes/recomendaciones-compra";

        // Act
        var response = await HttpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        content.Should().Contain("Endpoint no implementado");
    }

    [Fact]
    public async Task RealizarInventarioFisico_DebeRetornar501NotImplemented()
    {
        // Arrange
        var url = "/api/inventario/reportes/inventario-fisico";
        var command = new { };
        
        // Act
        var response = await HttpClient.PostAsJsonAsync(url, command);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        content.Should().Contain("Endpoint no implementado");
    }

    [Fact]
    public async Task ExportarReporte_DebeRetornar501NotImplemented()
    {
        // Arrange
        var url = "/api/inventario/reportes/exportar";

        // Act
        var response = await HttpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        content.Should().Contain("Endpoint no implementado");
    }

    [Fact]
    public async Task GetValorTotalInventario_DebeRetornar501NotImplemented()
    {
        // Arrange
        var url = "/api/inventario/reportes/valor-total";

        // Act
        var response = await HttpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        content.Should().Contain("Endpoint no implementado");
    }

    public new void Dispose()
    {
        _factory?.Dispose();
        base.Dispose();
    }
} 