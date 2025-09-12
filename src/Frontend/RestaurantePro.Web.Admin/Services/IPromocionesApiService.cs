using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IPromocionesApiService
{
    Task<List<PromocionDto>?> ObtenerPromocionesAsync();
    Task<PromocionDto?> ObtenerPromocionPorIdAsync(Guid id);
    Task<PromocionDto?> CrearPromocionAsync(CrearPromocionRequest request);
    Task<PromocionDto?> ActualizarPromocionAsync(Guid id, ActualizarPromocionRequest request);
    Task<bool> EliminarPromocionAsync(Guid id);
    Task<bool> CambiarEstadoPromocionAsync(Guid id, bool activa);
}
