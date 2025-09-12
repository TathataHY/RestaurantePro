using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IPreparacionesApiService
{
    Task<List<PreparacionDto>> ObtenerPreparacionesAsync();
    Task<PreparacionDto?> ObtenerPreparacionPorIdAsync(Guid id);
    Task<PreparacionDto?> CrearPreparacionAsync(CrearPreparacionRequest request);
    Task<PreparacionDto?> ActualizarPreparacionAsync(Guid id, ActualizarPreparacionRequest request);
    Task<bool> EliminarPreparacionAsync(Guid id);
    Task<bool> CambiarEstadoPreparacionAsync(Guid id, EstadoPreparacion estado);
}
