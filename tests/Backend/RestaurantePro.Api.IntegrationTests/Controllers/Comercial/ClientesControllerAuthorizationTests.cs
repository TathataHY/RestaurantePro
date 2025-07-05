using System.Net;
using FluentAssertions;
using RestaurantePro.Api.IntegrationTests.TestBase;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Comercial;

/// <summary>
/// Tests de autorización para ClientesController
/// Verifica que los endpoints protegidos requieren autenticación
/// </summary>
public class ClientesControllerAuthorizationTests : AuthorizationTestBase
{
    public ClientesControllerAuthorizationTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetClientes_DebeRequerirAutenticacion()
    {
        await Endpoint_DebeRequerirAutenticacion("/api/comercial/clientes");
    }

    [Fact]
    public async Task GetCliente_DebeRequerirAutenticacion()
    {
        await Endpoint_DebeRequerirAutenticacion("/api/comercial/clientes/11111111-1111-1111-1111-111111111111");
    }

    [Fact]
    public async Task CrearCliente_DebeRequerirAutenticacion()
    {
        await Endpoint_DebeRequerirAutenticacion("/api/comercial/clientes", HttpMethod.Post);
    }

    [Fact]
    public async Task ActualizarCliente_DebeRequerirAutenticacion()
    {
        await Endpoint_DebeRequerirAutenticacion("/api/comercial/clientes/11111111-1111-1111-1111-111111111111", HttpMethod.Put);
    }

    [Fact]
    public async Task EliminarCliente_DebeRequerirAutenticacion()
    {
        await Endpoint_DebeRequerirAutenticacion("/api/comercial/clientes/11111111-1111-1111-1111-111111111111", HttpMethod.Delete);
    }
} 