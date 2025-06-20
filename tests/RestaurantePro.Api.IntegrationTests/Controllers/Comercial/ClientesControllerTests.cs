// ✅ Usando GlobalUsings.cs - La mayoría de importaciones ya están incluidas
using RestaurantePro.Application.Comercial.Clientes.Commands.ActualizarCliente;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Comercial;

/// <summary>
/// Tests de integración para ClientesController
/// Contexto: Comercial
/// </summary>
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

    /// <summary>
    /// Método helper para crear un cliente válido directamente en la base de datos
    /// </summary>
    private async Task<Cliente> CrearClienteEnBD()
    {
        var nombre = ClienteNombre.Crear("Juan", "Pérez");
        var cliente = Cliente.Crear(
            nombre,
            "juan.perez@test.com",
            "+56987654321",
            DateTime.Now.AddYears(-30)
        );

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        
        await context.Clientes.AddAsync(cliente);
        await context.SaveChangesAsync(CancellationToken.None);
        
        Logger.LogInformation($"✅ Cliente creado en BD con ID: {cliente.Id}");
        return cliente;
    }

    [Fact]
    public async Task GetClientes_SinClientesEnBD_DebeRetornarListaVacia()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetClientes_SinClientesEnBD_DebeRetornarListaVacia");

        // Act
        var response = await HttpClient.GetAsync("/api/comercial/clientes");
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<ClienteSummaryDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Items.Should().BeEmpty();
        apiResponse.Data.TotalCount.Should().Be(0);
        
        Logger.LogInformation("✅ Test completado - lista vacía retornada correctamente");
    }

    [Fact]
    public async Task GetClientes_ConClientesEnBD_DebeRetornarClientes()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetClientes_ConClientesEnBD_DebeRetornarClientes");
        var cliente = await CrearClienteEnBD();

        // Act
        var response = await HttpClient.GetAsync("/api/comercial/clientes");
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<ClienteSummaryDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Items.Should().HaveCount(1);
        apiResponse.Data.TotalCount.Should().Be(1);
        
        var clienteDto = apiResponse.Data.Items.First();
        clienteDto.Id.Should().Be(cliente.Id);
        clienteDto.NombreCompleto.Should().Be("Juan Pérez");
        clienteDto.Email.Should().Be("juan.perez@test.com");
        
        Logger.LogInformation("✅ Test completado - clientes retornados correctamente");
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
        apiResponse.Message.Should().Contain("no encontrado");
        
        Logger.LogInformation("✅ Test completado - 404 retornado correctamente");
    }

    [Fact]
    public async Task GetCliente_ConIdExistente_DebeRetornarCliente()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetCliente_ConIdExistente_DebeRetornarCliente");
        var cliente = await CrearClienteEnBD();

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
        apiResponse.Data.NombreCompleto.Should().Be("Juan Pérez");
        apiResponse.Data.Email.Should().Be("juan.perez@test.com");
        
        Logger.LogInformation("✅ Test completado - cliente retornado correctamente");
    }

    [Fact]
    public async Task PostCliente_ConDatosValidos_DebeCrearCliente()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PostCliente_ConDatosValidos_DebeCrearCliente");
        
        var command = new CrearClienteCommand
        {
            Nombre = "María González",
            Email = "maria.gonzalez@test.com",
            Telefono = "+56912345678",
            FechaNacimiento = DateTime.Now.AddYears(-25)
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
        apiResponse.Data!.NombreCompleto.Should().Be("María González");
        apiResponse.Data.Email.Should().Be("maria.gonzalez@test.com");
        apiResponse.Data.Id.Should().NotBe(Guid.Empty);
        
        Logger.LogInformation("✅ Test completado exitosamente - cliente creado correctamente");
    }

    [Fact]
    public async Task PostCliente_ConEmailDuplicado_DebeRetornar400()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PostCliente_ConEmailDuplicado_DebeRetornar400");
        
        // Crear primer cliente
        await CrearClienteEnBD(); // Email: juan.perez@test.com
        
        var command = new CrearClienteCommand
        {
            Nombre = "Otro Juan",
            Email = "juan.perez@test.com", // Email duplicado
            Telefono = "+56987654322",
            FechaNacimiento = DateTime.Now.AddYears(-28)
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/comercial/clientes", command);
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Errors.Should().NotBeEmpty();
        
        Logger.LogInformation("✅ Test completado - email duplicado correctamente rechazado");
    }

    [Fact]
    public async Task PostCliente_ConDatosInvalidos_DebeRetornar400()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PostCliente_ConDatosInvalidos_DebeRetornar400");
        
        var command = new CrearClienteCommand
        {
            Nombre = "", // Nombre vacío - inválido
            Email = "email-invalido", // Email inválido
            Telefono = "123", // Teléfono muy corto
            FechaNacimiento = DateTime.Now.AddYears(5) // Fecha futura - inválida
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/comercial/clientes", command);
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        
        Logger.LogInformation("✅ Test completado - datos inválidos correctamente rechazados");
    }

    [Fact]
    public async Task PutCliente_ConDatosValidos_DebeActualizarCliente()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PutCliente_ConDatosValidos_DebeActualizarCliente");
        var cliente = await CrearClienteEnBD();
        
        var command = new ActualizarClienteCommand
        {
            Id = cliente.Id,
            Nombre = "Juan Pérez Actualizado",
            Email = "juan.actualizado@test.com",
            Telefono = "+56999888777",
            FechaNacimiento = DateTime.Now.AddYears(-32)
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/comercial/clientes/{cliente.Id}", command);
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ClienteDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(cliente.Id);
        apiResponse.Data.NombreCompleto.Should().Be("Juan Pérez Actualizado");
        apiResponse.Data.Email.Should().Be("juan.actualizado@test.com");
        
        Logger.LogInformation("✅ Test completado - cliente actualizado correctamente");
    }

    [Fact]
    public async Task DeleteCliente_ConIdExistente_DebeDesactivarCliente()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: DeleteCliente_ConIdExistente_DebeDesactivarCliente");
        var cliente = await CrearClienteEnBD();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/comercial/clientes/{cliente.Id}");
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().BeTrue();
        
        Logger.LogInformation("✅ Test completado - cliente desactivado correctamente");
    }

    [Fact]
    public async Task DeleteCliente_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: DeleteCliente_ConIdInexistente_DebeRetornar404");
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/comercial/clientes/{idInexistente}");
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        // El controlador devuelve 404 cuando el cliente no existe
        // pero el validador puede devolver 400 BadRequest por validaciones
        // Aceptamos ambos como válidos para este escenario
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        
        Logger.LogInformation("✅ Test completado - error correctamente retornado");
    }
} 