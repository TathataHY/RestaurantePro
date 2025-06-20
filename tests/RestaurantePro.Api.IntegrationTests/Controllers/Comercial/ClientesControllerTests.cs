using Microsoft.Extensions.Logging;
using RestaurantePro.Api.IntegrationTests.TestBase;
using System.Net;
using System.Linq;
using AutoMapper;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Comercial;

[Collection("Sequential")]
public class ClientesControllerTests : ApiIntegrationTestBase, IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    
    public ClientesControllerTests() : base(new TestWebApplicationFactory())
    {
        _factory = (TestWebApplicationFactory)Factory;
    }
    
    public new void Dispose()
    {
        _factory?.Dispose();
        base.Dispose();
    }

    [Fact]
    public async Task GetClientes_SinClientesEnBD_DebeRetornarListaVacia()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetClientes_SinClientesEnBD_DebeRetornarListaVacia");

        // Act
        var response = await HttpClient.GetAsync("/api/comercial/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await ExecuteAndDeserializeAsync<PaginatedList<ClienteSummaryDto>>(
            client => client.GetAsync("/api/comercial/clientes"));
        
        VerificarRespuestaExitosa(response, apiResponse);
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Items.Should().BeEmpty();
        apiResponse.Data.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetClientes_ConClientesEnBD_DebeRetornarClientes()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetClientes_ConClientesEnBD_DebeRetornarClientes");
        var cliente = await CrearClientePrueba();
        
        // Act  
        var response = await HttpClient.GetAsync("/api/comercial/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await ExecuteAndDeserializeAsync<PaginatedList<ClienteSummaryDto>>(
            client => client.GetAsync("/api/comercial/clientes"));
        
        VerificarRespuestaExitosa(response, apiResponse);
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Items.Should().HaveCount(1);
        apiResponse.Data.TotalCount.Should().Be(1);
        
        var clienteDto = apiResponse.Data.Items.First();
        clienteDto.Id.Should().Be(cliente.Id);
        clienteDto.NombreCompleto.Should().Be("Cliente Test");
        clienteDto.Email.Should().Be("test@example.com");
        
        Logger.LogInformation("✅ Test completado exitosamente - cliente retornado correctamente");
    }

    [Fact]
    public async Task GetCliente_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetCliente_ConIdInexistente_DebeRetornar404");
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/api/comercial/clientes/{idInexistente}");
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetCliente_ConIdExistente_DebeRetornarCliente()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetCliente_ConIdExistente_DebeRetornarCliente");
        
        var cliente = await CrearClientePrueba();

        // Act
        var response = await HttpClient.GetAsync($"/api/comercial/clientes/{cliente.Id}");
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ClienteDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(cliente.Id);
        apiResponse.Data.Nombre.Should().Be("Cliente");
        apiResponse.Data.Apellido.Should().Be("Test");
    }

    [Fact]
    public async Task PostCliente_ConDatosValidos_DebeCrearCliente()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PostCliente_ConDatosValidos_DebeCrearCliente");
        
        var command = new CrearClienteCommand
        {
            Nombre = "María García",
            Email = "maria.garcia@test.com",
            Telefono = "123456789",
            FechaNacimiento = new DateTime(1990, 5, 15)
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/comercial/clientes", command);
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ClienteDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.NombreCompleto.Should().Be("María García");
        apiResponse.Data.Email.Should().Be("maria.garcia@test.com");
        apiResponse.Data.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task DeleteCliente_ConIdExistente_DebeDesactivarCliente()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: DeleteCliente_ConIdExistente_DebeDesactivarCliente");
        
        var cliente = await CrearClientePrueba();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/comercial/clientes/{cliente.Id}");
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();

        // Verificar que el cliente ya no aparece en la lista activa
        var getResponse = await HttpClient.GetAsync("/api/comercial/clientes");
        var getApiResponse = await getResponse.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<ClienteSummaryDto>>>();
        getApiResponse!.Data!.Items.Should().BeEmpty();
    }


} 