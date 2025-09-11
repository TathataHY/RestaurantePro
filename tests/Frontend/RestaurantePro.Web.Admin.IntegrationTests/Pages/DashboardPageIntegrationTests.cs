using FluentAssertions;
using System.Net;

namespace RestaurantePro.Web.Admin.IntegrationTests.Pages;

/// <summary>
/// Pruebas de integración para la página del dashboard de la web administrativa
/// </summary>
public class DashboardPageIntegrationTests : BaseIntegrationTest
{
    public DashboardPageIntegrationTests(WebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DashboardPage_DeberiaCargarCorrectamente()
    {
        // Act
        var response = await Client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Dashboard");
    }

    [Fact]
    public async Task DashboardPage_DeberiaMostrarMetricas()
    {
        // Act
        var response = await Client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Ventas");
        content.Should().Contain("Mesas");
        content.Should().Contain("Comandas");
    }

    [Fact]
    public async Task DashboardPage_DeberiaMostrarProductosMasVendidos()
    {
        // Act
        var response = await Client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Productos Más Vendidos");
    }

    [Fact]
    public async Task DashboardPage_ConUsuarioNoAutenticado_DeberiaRedirigirALogin()
    {
        // Arrange - Crear un cliente sin autenticación
        using var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("/login");
    }
}
