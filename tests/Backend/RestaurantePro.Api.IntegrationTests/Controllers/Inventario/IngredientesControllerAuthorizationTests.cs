using System.Net;
using FluentAssertions;
using RestaurantePro.Api.IntegrationTests.TestBase;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Inventario;

/// <summary>
/// Tests de autorización para IngredientesController
/// Verifica que los endpoints protegidos requieren autenticación
/// </summary>
public class IngredientesControllerAuthorizationTests : AuthorizationTestBase
{
    public IngredientesControllerAuthorizationTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ObtenerIngredientes_DebeRequerirAutenticacion()
    {
        await Endpoint_DebeRequerirAutenticacion("/api/inventario/ingredientes");
    }

    [Fact]
    public async Task ObtenerIngredientePorId_DebeRequerirAutenticacion()
    {
        await Endpoint_DebeRequerirAutenticacion("/api/inventario/ingredientes/11111111-1111-1111-1111-111111111111");
    }

    [Fact]
    public async Task CrearIngrediente_DebeRequerirAutenticacion()
    {
        await Endpoint_DebeRequerirAutenticacion("/api/inventario/ingredientes", HttpMethod.Post);
    }

    [Fact]
    public async Task ActualizarIngrediente_DebeRequerirAutenticacion()
    {
        await Endpoint_DebeRequerirAutenticacion("/api/inventario/ingredientes/11111111-1111-1111-1111-111111111111", HttpMethod.Put);
    }

    [Fact]
    public async Task EliminarIngrediente_DebeRequerirAutenticacion()
    {
        await Endpoint_DebeRequerirAutenticacion("/api/inventario/ingredientes/11111111-1111-1111-1111-111111111111", HttpMethod.Delete);
    }

    [Fact]
    public async Task ObtenerMovimientosDeIngrediente_DebeRequerirAutenticacion()
    {
        await Endpoint_DebeRequerirAutenticacion("/api/inventario/ingredientes/11111111-1111-1111-1111-111111111111/movimientos");
    }

    [Fact]
    public async Task RegistrarMovimiento_DebeRequerirAutenticacion()
    {
        await Endpoint_DebeRequerirAutenticacion("/api/inventario/ingredientes/11111111-1111-1111-1111-111111111111/movimientos", HttpMethod.Post);
    }

    [Fact]
    public async Task ObtenerIngredientesBajoStock_DebeRequerirAutenticacion()
    {
        await Endpoint_DebeRequerirAutenticacion("/api/inventario/ingredientes/bajo-stock");
    }
} 