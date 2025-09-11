using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.IntegrationTests.Components;

/// <summary>
/// Pruebas de integración para el componente ClienteForm
/// </summary>
public class ClienteFormIntegrationTests : BaseIntegrationTest
{
    public ClienteFormIntegrationTests(WebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ClienteForm_ConDatosValidos_DeberiaCrearCliente()
    {
        // Arrange
        var nuevoCliente = new CrearClienteRequest
        {
            Nombre = "Juan",
            Apellidos = "Pérez",
            Email = "juan.perez@test.com",
            Telefono = "+1234567890",
            FechaNacimiento = DateTime.Today.AddYears(-30),
            Ciudad = "Lima",
            Pais = "Perú",
            AceptaMarketing = true,
            AceptaTerminos = true
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/comercial/clientes", nuevoCliente);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var clienteCreado = await response.Content.ReadFromJsonAsync<ApiResponse<ClienteDto>>();
        clienteCreado.Should().NotBeNull();
        clienteCreado!.Success.Should().BeTrue();
        clienteCreado.Data.Should().NotBeNull();
        clienteCreado.Data!.Nombre.Should().Be("Juan");
        clienteCreado.Data.Apellidos.Should().Be("Pérez");
        clienteCreado.Data.Email.Should().Be("juan.perez@test.com");
    }

    [Fact]
    public async Task ClienteForm_ConEmailDuplicado_DeberiaRetornarError()
    {
        // Arrange
        var cliente1 = new CrearClienteRequest
        {
            Nombre = "Cliente",
            Apellidos = "Uno",
            Email = "duplicado@test.com",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            AceptaTerminos = true
        };

        var cliente2 = new CrearClienteRequest
        {
            Nombre = "Cliente",
            Apellidos = "Dos",
            Email = "duplicado@test.com", // Mismo email
            FechaNacimiento = DateTime.Today.AddYears(-25),
            AceptaTerminos = true
        };

        // Act
        await Client.PostAsJsonAsync("/api/comercial/clientes", cliente1);
        var response = await Client.PostAsJsonAsync("/api/comercial/clientes", cliente2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ClienteForm_ConDatosInvalidos_DeberiaRetornarError()
    {
        // Arrange
        var clienteInvalido = new CrearClienteRequest
        {
            Nombre = "", // Nombre vacío
            Apellidos = "", // Apellidos vacíos
            Email = "email-invalido", // Email inválido
            FechaNacimiento = DateTime.Today.AddYears(1), // Fecha futura
            AceptaTerminos = false // No acepta términos
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/comercial/clientes", clienteInvalido);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ClienteForm_ValidarEmail_ConEmailExistente_DeberiaRetornarTrue()
    {
        // Arrange
        var cliente = new CrearClienteRequest
        {
            Nombre = "Test",
            Apellidos = "Email",
            Email = "test.email@test.com",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            AceptaTerminos = true
        };

        await Client.PostAsJsonAsync("/api/comercial/clientes", cliente);

        // Act
        var response = await Client.GetAsync($"/api/comercial/clientes/validar-email?email={Uri.EscapeDataString("test.email@test.com")}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var validacion = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        validacion.Should().NotBeNull();
        validacion!.Data.Should().BeTrue();
    }

    [Fact]
    public async Task ClienteForm_ValidarEmail_ConEmailNoExistente_DeberiaRetornarFalse()
    {
        // Act
        var response = await Client.GetAsync($"/api/comercial/clientes/validar-email?email={Uri.EscapeDataString("noexiste@test.com")}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var validacion = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        validacion.Should().NotBeNull();
        validacion!.Data.Should().BeFalse();
    }
}
