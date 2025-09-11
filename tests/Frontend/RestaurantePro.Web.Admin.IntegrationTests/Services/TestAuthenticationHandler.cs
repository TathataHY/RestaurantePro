using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace RestaurantePro.Web.Admin.IntegrationTests.Services;

/// <summary>
/// Handler de autenticación para pruebas que simula un usuario autenticado
/// </summary>
public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Crear un usuario de prueba con roles administrativos
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Email, "test@restaurantepro.com"),
            new Claim(ClaimTypes.Role, "Administrador"),
            new Claim(ClaimTypes.Role, "Gerente"),
            new Claim(ClaimTypes.Role, "Cajero"),
            new Claim(ClaimTypes.Role, "Mesero"),
            new Claim(ClaimTypes.Role, "Cocinero"),
            new Claim(ClaimTypes.Role, "EncargadoInventario"),
            // Agregar permisos específicos para clientes
            new Claim("permission", "clientes.read"),
            new Claim("permission", "clientes.create"),
            new Claim("permission", "clientes.update"),
            new Claim("permission", "clientes.delete")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
