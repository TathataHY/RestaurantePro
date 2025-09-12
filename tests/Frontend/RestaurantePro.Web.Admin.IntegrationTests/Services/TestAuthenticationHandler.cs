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
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Para pruebas de integración, autenticar solo cuando hay un header de autorización
        // Esto permite que las pruebas de seguridad funcionen sin autenticación
        
        if (Request.Headers.ContainsKey("Authorization"))
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Email, "test@restaurantepro.com")
            };

            // Si hay un header X-Test-Role, usar solo ese rol específico
            if (Request.Headers.TryGetValue("X-Test-Role", out var testRole))
            {
                var role = testRole.ToString();
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            else
            {
                // Si no hay header específico, usar todos los roles (comportamiento por defecto)
                claims.Add(new Claim(ClaimTypes.Role, "Administrador"));
                claims.Add(new Claim(ClaimTypes.Role, "Gerente"));
                claims.Add(new Claim(ClaimTypes.Role, "Cajero"));
                claims.Add(new Claim(ClaimTypes.Role, "Mesero"));
                claims.Add(new Claim(ClaimTypes.Role, "Cocinero"));
                claims.Add(new Claim(ClaimTypes.Role, "EncargadoInventario"));
            }

            // Agregar permisos básicos
            claims.Add(new Claim("permission", "clientes.read"));
            claims.Add(new Claim("permission", "productos.read"));
            claims.Add(new Claim("permission", "categorias.read"));
            claims.Add(new Claim("permission", "usuarios.read"));

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        
        // Sin header de autorización, no autenticar (para tests de seguridad)
        return Task.FromResult(AuthenticateResult.NoResult());
    }
}
