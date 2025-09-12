using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Web.Admin.IntegrationTests.Services;

/// <summary>
/// Implementación mock de ICurrentUserService para pruebas de integración
/// </summary>
public class TestCurrentUserService : ICurrentUserService
{
    public string UserId => "test-user-id";
    public string UserName => "test-user";
    public string Email => "test@restaurantepro.com";
    public bool IsAuthenticated => true;
    public IEnumerable<string> Roles => new[] { "Administrador" };
    public string Rol => "Administrador";
    
    public bool IsInRole(string role)
    {
        return role == "Administrador";
    }
}