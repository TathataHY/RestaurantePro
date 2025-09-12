using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IMesasApiService
{
    Task<List<MesaDto>> ObtenerMesasAsync();
    Task<MesaDto?> ObtenerMesaPorIdAsync(Guid id);
    Task<MesaDto?> CrearMesaAsync(CrearMesaRequest request);
    Task<MesaDto?> ActualizarMesaAsync(Guid id, ActualizarMesaRequest request);
    Task<bool> EliminarMesaAsync(Guid id);
    Task<bool> CambiarEstadoMesaAsync(Guid id, string estado);
}
