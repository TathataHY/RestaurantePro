using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IConfiguracionApiService
{
    Task<ConfiguracionDto?> ObtenerConfiguracionAsync();
    Task<bool> ActualizarConfiguracionAsync(ConfiguracionDto configuracion);
    Task<bool> RestablecerConfiguracionAsync();
    Task<List<ConfiguracionDto>?> ObtenerPorCategoriaAsync(string categoria);
    Task<bool> ActualizarParametroAsync(ActualizarConfiguracionRequest request);
    Task<ConfiguracionNotificacionesDto?> ObtenerConfiguracionNotificacionesAsync();
    Task<bool> ActualizarConfiguracionNotificacionesAsync(ConfiguracionNotificacionesDto configuracion);
    Task<ConfiguracionFidelizacionDto?> ObtenerConfiguracionFidelizacionAsync();
    Task<bool> ActualizarConfiguracionFidelizacionAsync(ConfiguracionFidelizacionDto configuracion);
    Task<object?> ObtenerEstadisticasAsync();
    Task<bool> ResetearConfiguracionAsync(string categoria);
}
