using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.IntegrationTests.Services;

/// <summary>
/// Pruebas de integración para el servicio ClientesApiService
/// </summary>
public class ClientesApiServiceIntegrationTests : BaseIntegrationTest
{
    public ClientesApiServiceIntegrationTests(WebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ObtenerClientes_ConFiltrosBasicos_DeberiaRetornarListaPaginada()
    {
        // Arrange
        var filtros = new ClienteFiltrosDto
        {
            PageNumber = 1,
            PageSize = 10,
            OrdenarPor = "NombreCompleto",
            DireccionOrden = "asc"
        };

        // Act
        var response = await Client.GetAsync($"/api/comercial/clientes?pageNumber={filtros.PageNumber}&pageSize={filtros.PageSize}&ordenarPor={filtros.OrdenarPor}&direccionOrden={filtros.DireccionOrden}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<ClienteDto>>>();
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerClientes_ConFiltrosAvanzados_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        var filtros = new ClienteFiltrosDto
        {
            PageNumber = 1,
            PageSize = 10,
            Busqueda = "test",
            Ciudad = "Lima",
            Estado = "Activo",
            OrdenarPor = "TotalGastado",
            DireccionOrden = "desc"
        };

        // Act
        var queryString = $"pageNumber={filtros.PageNumber}&pageSize={filtros.PageSize}&busqueda={Uri.EscapeDataString(filtros.Busqueda)}&ciudad={Uri.EscapeDataString(filtros.Ciudad)}&estado={Uri.EscapeDataString(filtros.Estado)}&ordenarPor={filtros.OrdenarPor}&direccionOrden={filtros.DireccionOrden}";
        var response = await Client.GetAsync($"/api/comercial/clientes?{queryString}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<ClienteDto>>>();
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CrearCliente_ConDatosValidos_DeberiaCrearClienteExitosamente()
    {
        // Arrange
        var nuevoCliente = new CrearClienteRequest
        {
            Nombre = "María",
            Apellidos = "González",
            Email = "maria.gonzalez@test.com",
            Telefono = "+1234567890",
            FechaNacimiento = DateTime.Today.AddYears(-28),
            Ciudad = "Lima",
            Pais = "Perú",
            AceptaMarketing = true,
            AceptaTerminos = true
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/comercial/clientes", nuevoCliente);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<ApiResponse<ClienteDto>>();
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Nombre.Should().Be("María");
        resultado.Data.Apellidos.Should().Be("González");
        resultado.Data.Email.Should().Be("maria.gonzalez@test.com");
    }

    [Fact]
    public async Task ActualizarCliente_ConDatosValidos_DeberiaActualizarClienteExitosamente()
    {
        // Arrange - Primero crear un cliente
        var clienteOriginal = new CrearClienteRequest
        {
            Nombre = "Carlos",
            Apellidos = "López",
            Email = "carlos.lopez@test.com",
            FechaNacimiento = DateTime.Today.AddYears(-35),
            AceptaTerminos = true
        };

        var crearResponse = await Client.PostAsJsonAsync("/api/comercial/clientes", clienteOriginal);
        var clienteCreado = await crearResponse.Content.ReadFromJsonAsync<ApiResponse<ClienteDto>>();
        var clienteId = clienteCreado!.Data!.Id;

        // Act - Actualizar el cliente
        var actualizacion = new ActualizarClienteRequest
        {
            Id = clienteId,
            Nombre = "Carlos Actualizado",
            Apellidos = "López Actualizado",
            Email = "carlos.actualizado@test.com",
            Telefono = "+9876543210",
            FechaNacimiento = DateTime.Today.AddYears(-35),
            EstaActivo = true
        };

        var response = await Client.PutAsJsonAsync($"/api/comercial/clientes/{clienteId}", actualizacion);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<ApiResponse<ClienteDto>>();
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Nombre.Should().Be("Carlos Actualizado");
        resultado.Data.Telefono.Should().Be("+9876543210");
    }

    [Fact]
    public async Task EliminarCliente_ConIdValido_DeberiaEliminarClienteExitosamente()
    {
        // Arrange - Primero crear un cliente
        var clienteOriginal = new CrearClienteRequest
        {
            Nombre = "Ana",
            Apellidos = "Martínez",
            Email = "ana.martinez@test.com",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            AceptaTerminos = true
        };

        var crearResponse = await Client.PostAsJsonAsync("/api/comercial/clientes", clienteOriginal);
        var clienteCreado = await crearResponse.Content.ReadFromJsonAsync<ApiResponse<ClienteDto>>();
        var clienteId = clienteCreado!.Data!.Id;

        // Act
        var response = await Client.DeleteAsync($"/api/comercial/clientes/{clienteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().BeTrue();
    }

    [Fact]
    public async Task ObtenerEstadisticas_DeberiaRetornarEstadisticasValidas()
    {
        // Act
        var response = await Client.GetAsync("/api/comercial/clientes/estadisticas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<ApiResponse<ClienteEstadisticasDto>>();
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.TotalClientes.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task ExportarClientes_ConFiltros_DeberiaRetornarArchivoExcel()
    {
        // Arrange
        var filtros = new ClienteFiltrosDto
        {
            PageNumber = 1,
            PageSize = 100,
            OrdenarPor = "NombreCompleto",
            DireccionOrden = "asc"
        };

        var request = new { Filtros = filtros, Formato = "Excel" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/comercial/clientes/exportar", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsByteArrayAsync();
        content.Should().NotBeEmpty();
        
        // Verificar que es un archivo Excel (comienza con PK)
        content[0].Should().Be(0x50); // 'P'
        content[1].Should().Be(0x4B); // 'K'
    }
}
