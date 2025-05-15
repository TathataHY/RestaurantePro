using Microsoft.AspNetCore.Authorization;

namespace RestaurantePro.Api.Extensions
{
    public static class AuthorizationExtensions
    {
        public static AuthorizationOptions AddRestauranteProPolicies(this AuthorizationOptions options)
        {
            options.AddPolicy("RequiereAdministrador", policy =>
                policy.RequireRole("Administrador"));

            options.AddPolicy("RequiereGerente", policy =>
                policy.RequireRole("Administrador", "Gerente"));

            options.AddPolicy("RequiereMesero", policy =>
                policy.RequireRole("Administrador", "Gerente", "Mesero"));

            options.AddPolicy("RequiereCocinero", policy =>
                policy.RequireRole("Administrador", "Gerente", "Cocinero"));

            return options;
        }
    }
} 