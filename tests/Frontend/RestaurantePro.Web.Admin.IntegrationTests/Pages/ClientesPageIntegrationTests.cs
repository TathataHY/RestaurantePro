using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace RestaurantePro.Web.Admin.IntegrationTests.Pages;

/// <summary>
/// Pruebas de integración para la página de clientes de la web administrativa
/// </summary>
public class ClientesPageIntegrationTests : BaseIntegrationTest
{
    public ClientesPageIntegrationTests(WebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ClientesPage_DeberiaCargarCorrectamente()
    {
        // Act
        var response = await Client.GetAsync("/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Gestión de Clientes");
    }

    [Fact]
    public async Task ClientesPage_ConUsuarioNoAutenticado_DeberiaRedirigirALogin()
    {
        // Arrange - Crear un cliente sin autenticación
        using var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("/login");
    }

    [Fact]
    public async Task ClientesPage_DeberiaMostrarEstadisticas()
    {
        // Act
        var response = await Client.GetAsync("/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Total Clientes");
        content.Should().Contain("Activos");
        content.Should().Contain("Frecuentes");
    }

    [Fact]
    public async Task ClientesPage_DeberiaMostrarFiltros()
    {
        // Act
        var response = await Client.GetAsync("/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Filtros de búsqueda");
    }

    [Fact]
    public async Task ClientesPage_DeberiaMostrarBotonNuevoCliente()
    {
        // Act
        var response = await Client.GetAsync("/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Nuevo Cliente");
    }
}
