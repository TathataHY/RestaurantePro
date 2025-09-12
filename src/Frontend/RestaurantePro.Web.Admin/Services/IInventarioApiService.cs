using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IInventarioApiService
{
    Task<List<InventarioDto>> ObtenerInventarioAsync();
    Task<InventarioDto?> ObtenerInventarioPorIdAsync(Guid id);
    Task<InventarioDto?> CrearInventarioAsync(InventarioDto inventario);
    Task<InventarioDto?> ActualizarInventarioAsync(Guid id, InventarioDto inventario);
    Task<bool> EliminarInventarioAsync(Guid id);
    Task<List<AlertaInventarioDto>> ObtenerAlertasInventarioAsync();
}
