using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IMesasApiService
{
    Task<PaginatedList<MesaDto>?> ObtenerAsync(string? estado = null, string? ubicacion = null, int? capacidadMinima = null, int pageNumber = 1, int pageSize = 10);
    Task<MesaDto?> ObtenerPorIdAsync(Guid id);
    Task<MesaDto?> CrearAsync(CrearMesaRequest dto);
    Task<MesaDto?> ActualizarAsync(Guid id, ActualizarMesaRequest dto);
    Task<bool> EliminarAsync(Guid id);
    
    // Métodos adicionales para compatibilidad
    Task<List<MesaDto>> ObtenerMesasAsync();
    Task<MesaDto?> ObtenerMesaPorIdAsync(Guid id);
    Task<MesaDto?> CrearMesaAsync(CrearMesaRequest request);
    Task<MesaDto?> ActualizarMesaAsync(Guid id, ActualizarMesaRequest request);
    Task<bool> EliminarMesaAsync(Guid id);
    Task<bool> CambiarEstadoMesaAsync(Guid id, string estado);
}
