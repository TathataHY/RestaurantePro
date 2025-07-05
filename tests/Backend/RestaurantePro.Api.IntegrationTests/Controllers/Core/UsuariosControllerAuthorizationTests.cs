using System.Net;
using FluentAssertions;
using RestaurantePro.Api.IntegrationTests.TestBase;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Core;

/// <summary>
/// Tests de autorización para UsuariosController
/// Verifica que los endpoints protegidos requieren autenticación
/// </summary>
public class UsuariosControllerAuthorizationTests : AuthorizationTestBase
{
    public UsuariosControllerAuthorizationTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetUsuarios_DebeRequerirAutenticacion()
    {
        await Endpoint_DebeRequerirAutenticacion("/api/core/usuarios");
    }

    [Fact]
    public async Task GetUsuario_DebeRequerirAutenticacion()
    {
        await Endpoint_DebeRequerirAutenticacion("/api/core/usuarios/11111111-1111-1111-1111-111111111111");
    }

    [Fact]
    public async Task CrearUsuario_DebeRequerirRolAdministrador()
    {
        await Endpoint_DebeRequerirRol("/api/core/usuarios", "Administrador", HttpMethod.Post);
    }

    [Fact]
    public async Task ActualizarUsuario_DebeRequerirAutenticacion()
    {
        await Endpoint_DebeRequerirAutenticacion("/api/core/usuarios/11111111-1111-1111-1111-111111111111", HttpMethod.Put);
    }

    [Fact]
    public async Task EliminarUsuario_DebeRequerirRolAdministrador()
    {
        await Endpoint_DebeRequerirRol("/api/core/usuarios/11111111-1111-1111-1111-111111111111", "Administrador", HttpMethod.Delete);
    }

    [Fact]
    public async Task ObtenerPerfilActual_DebeRequerirAutenticacion()
    {
        await Endpoint_DebeRequerirAutenticacion("/api/core/usuarios/perfil");
    }

    [Fact]
    public async Task CambiarRol_DebeRequerirRolAdministrador()
    {
        await Endpoint_DebeRequerirRol("/api/core/usuarios/11111111-1111-1111-1111-111111111111/cambiar-rol", "Administrador", HttpMethod.Post);
    }

    [Fact]
    public async Task ResetPassword_DebeRequerirRolAdministrador()
    {
        await Endpoint_DebeRequerirRol("/api/core/usuarios/11111111-1111-1111-1111-111111111111/reset-password", "Administrador", HttpMethod.Post);
    }
} 