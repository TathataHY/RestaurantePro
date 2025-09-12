using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IComandasApiService
{
    Task<List<ComandaDto>> ObtenerComandasAsync();
    Task<ComandaDto?> ObtenerComandaPorIdAsync(Guid id);
    Task<ComandaDto?> CrearComandaAsync(CrearComandaRequest request);
    Task<ComandaDto?> ActualizarComandaAsync(Guid id, ActualizarComandaRequest request);
    Task<bool> EliminarComandaAsync(Guid id);
    Task<bool> CambiarEstadoComandaAsync(Guid id, string estado);
}
