using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IPromocionesApiService
{
    Task<PaginatedList<PromocionDto>?> ObtenerPromocionesAsync(int pagina, int tamanoPagina);
    Task<PromocionDto?> ObtenerPromocionPorIdAsync(Guid id);
    Task<PromocionDto?> CrearPromocionAsync(CrearPromocionRequest request);
    Task<PromocionDto?> ActualizarPromocionAsync(ActualizarPromocionRequest request);
    Task<bool> EliminarPromocionAsync(Guid id);
    Task<bool> CambiarEstadoPromocionAsync(Guid id, bool activa);
    Task<PromocionEstadisticasDto?> ObtenerEstadisticasAsync();
    Task<ApiResponse<bool>?> ToggleActivarPromocionAsync(Guid id, bool activar);
}
