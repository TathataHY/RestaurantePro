using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IConfiguracionApiService
{
    Task<ConfiguracionDto?> ObtenerConfiguracionAsync();
    Task<bool> ActualizarConfiguracionAsync(ConfiguracionDto configuracion);
    Task<bool> RestablecerConfiguracionAsync();
}
