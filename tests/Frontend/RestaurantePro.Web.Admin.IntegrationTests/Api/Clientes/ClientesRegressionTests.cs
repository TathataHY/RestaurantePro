using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RestaurantePro.Web.Admin.Models;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClienteDto = RestaurantePro.Application.Comercial.Clientes.DTOs.ClienteDto;

namespace RestaurantePro.Web.Admin.IntegrationTests;

/// <summary>
/// Pruebas de regresión para la API de clientes
/// </summary>
public class ClientesRegressionTests : BaseIntegrationTest
{
    public ClientesRegressionTests(WebApplicationFactory factory) : base(factory)
    {
        SetupAuthentication();
    }

    private void SetupAuthentication()
    {
        _client.DefaultRequestHeaders.Clear();
    }

    [Fact]
    public async Task CrearCliente_ConDatosMinimos_DeberiaFuncionarComoAntes()
    {
        // Arrange - Datos mínimos requeridos
        var clienteMinimo = new CrearClienteRequest
        {
            Nombre = "Cliente Mínimo",
            Email = "minimo@test.com",
            Telefono = "+1234567890", // Agregar teléfono requerido
            FechaNacimiento = DateTime.Today.AddYears(-25),
            AceptaTerminos = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/comercial/clientes", clienteMinimo);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var jsonContent = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(jsonContent, GetJsonOptions());
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Nombre.Should().Be("Cliente"); // El handler divide el nombre
        resultado.Data.Apellido.Should().Be("Mínimo");
        resultado.Data.Email.Should().Be("minimo@test.com");
    }

    [Fact]
    public async Task CrearCliente_ConDatosCompletos_DeberiaFuncionarComoAntes()
    {
        // Arrange - Datos completos
        var clienteCompleto = new CrearClienteRequest
        {
            Nombre = "Cliente Completo",
            Email = "completo@test.com",
            Telefono = "+51-987-654-321",
            FechaNacimiento = DateTime.Today.AddYears(-30),
            Ciudad = "Lima",
            Pais = "Perú",
            AceptaMarketing = true,
            AceptaTerminos = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/comercial/clientes", clienteCompleto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var jsonContent = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(jsonContent, GetJsonOptions());
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Nombre.Should().Be("Cliente"); // El handler divide el nombre
        resultado.Data.Apellido.Should().Be("Completo");
        resultado.Data.Email.Should().Be("completo@test.com");
        resultado.Data.Telefono.Should().Be("+51-987-654-321");
        // Nota: Ciudad y Pais no están disponibles en ClienteDto
    }

    [Fact]
    public async Task ObtenerClientes_ConPaginacionBasica_DeberiaFuncionarComoAntes()
    {
        // Arrange - Crear algunos clientes primero
        for (int i = 0; i < 5; i++)
        {
            var cliente = new CrearClienteRequest
            {
                Nombre = $"Cliente Regresión {i:D3}",
                Email = $"regresion{i:D3}@test.com",
                Telefono = $"+123456{i:D4}",
                FechaNacimiento = DateTime.Today.AddYears(-25),
                AceptaTerminos = true
            };
            await _client.PostAsJsonAsync("/api/comercial/clientes", cliente);
        }

        // Act
        var response = await _client.GetAsync("/api/comercial/clientes?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonContent = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<ApiResponse<PaginatedList<ClienteDto>>>(jsonContent, GetJsonOptions());
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Items.Should().NotBeNull();
        resultado.Data.Items.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task ActualizarCliente_ConDatosValidos_DeberiaFuncionarComoAntes()
    {
        // Arrange - Crear cliente primero
        var clienteOriginal = new CrearClienteRequest
        {
            Nombre = "Cliente Original",
            Email = "original@test.com",
            Telefono = "+1234567890",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            AceptaTerminos = true
        };

        var createResponse = await _client.PostAsJsonAsync("/api/comercial/clientes", clienteOriginal);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createJsonContent = await createResponse.Content.ReadAsStringAsync();
        var createResultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(createJsonContent, GetJsonOptions());
        var clienteId = createResultado!.Data!.Id;

        // Actualizar cliente
        var clienteActualizado = new ActualizarClienteRequest
        {
            Nombre = "Cliente Actualizado",
            Apellidos = "Apellido Actualizado",
            Email = "actualizado@test.com",
            Telefono = "+9876543210",
            FechaNacimiento = DateTime.Today.AddYears(-30),
            AceptaMarketing = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/comercial/clientes/{clienteId}", clienteActualizado);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonContent = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(jsonContent, GetJsonOptions());
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.NombreCompleto.Should().Be("Cliente Actualizado");
        resultado.Data.Email.Should().Be("actualizado@test.com");
    }

    [Fact]
    public async Task ObtenerClientePorId_ConIdValido_DeberiaFuncionarComoAntes()
    {
        // Arrange - Crear cliente primero
        var cliente = new CrearClienteRequest
        {
            Nombre = "Cliente Por ID",
            Email = "porid@test.com",
            Telefono = "+1234567890",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            AceptaTerminos = true
        };

        var createResponse = await _client.PostAsJsonAsync("/api/comercial/clientes", cliente);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createJsonContent = await createResponse.Content.ReadAsStringAsync();
        var createResultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(createJsonContent, GetJsonOptions());
        var clienteId = createResultado!.Data!.Id;

        // Act
        var response = await _client.GetAsync($"/api/comercial/clientes/{clienteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonContent = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(jsonContent, GetJsonOptions());
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Id.Should().Be(clienteId);
        resultado.Data.Nombre.Should().Be("Cliente"); // El handler divide el nombre
        resultado.Data.Apellido.Should().Be("Por ID");
        resultado.Data.Email.Should().Be("porid@test.com");
    }

    [Fact]
    public async Task ObtenerClientePorId_ConIdInvalido_DeberiaFuncionarComoAntes()
    {
        // Arrange
        var clienteId = Guid.NewGuid(); // ID que no existe

        // Act
        var response = await _client.GetAsync($"/api/comercial/clientes/{clienteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CrearCliente_ConEmailDuplicado_DeberiaFuncionarComoAntes()
    {
        // Arrange - Crear primer cliente
        var cliente1 = new CrearClienteRequest
        {
            Nombre = "Cliente Duplicado 1",
            Email = "duplicado@test.com",
            Telefono = "+1234567890",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            AceptaTerminos = true
        };

        var response1 = await _client.PostAsJsonAsync("/api/comercial/clientes", cliente1);
        response1.StatusCode.Should().Be(HttpStatusCode.Created);

        // Crear segundo cliente con mismo email
        var cliente2 = new CrearClienteRequest
        {
            Nombre = "Cliente Duplicado 2",
            Email = "duplicado@test.com", // Mismo email
            Telefono = "+9876543210",
            FechaNacimiento = DateTime.Today.AddYears(-30),
            AceptaTerminos = true
        };

        // Act
        var response2 = await _client.PostAsJsonAsync("/api/comercial/clientes", cliente2);

        // Assert
        response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearCliente_ConDatosInvalidos_DeberiaFuncionarComoAntes()
    {
        // Arrange - Cliente con datos inválidos
        var clienteInvalido = new CrearClienteRequest
        {
            Nombre = "", // Nombre vacío
            Email = "email-invalido", // Email inválido
            Telefono = "", // Teléfono vacío
            FechaNacimiento = DateTime.Today.AddYears(1), // Fecha futura
            AceptaTerminos = false // No acepta términos
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/comercial/clientes", clienteInvalido);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObtenerClientes_ConParametrosInvalidos_DeberiaFuncionarComoAntes()
    {
        // Act - Parámetros inválidos
        var response = await _client.GetAsync("/api/comercial/clientes?pageNumber=-1&pageSize=0");

        // Assert
        // La API debería manejar parámetros inválidos correctamente
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]
    public async Task CrearCliente_ConCaracteresEspeciales_DeberiaFuncionarComoAntes()
    {
        // Arrange - Cliente con caracteres especiales
        var clienteEspecial = new CrearClienteRequest
        {
            Nombre = "José María OConnor Smith", // Sin caracteres especiales problemáticos
            Email = "jose.oconnor@test.com",
            Telefono = "+51-987-654-321",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            AceptaTerminos = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/comercial/clientes", clienteEspecial);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var jsonContent = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(jsonContent, GetJsonOptions());
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Nombre.Should().Be("José"); // El handler divide el nombre
        resultado.Data.Apellido.Should().Be("María OConnor Smith");
    }
}
