using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IClientesApiService
{
    // Métodos básicos
    Task<List<ClienteDto>> ObtenerClientesAsync();
    Task<PaginatedList<ClienteDto>?> ObtenerClientesAsync(ClienteFiltrosDto filtros);
    Task<ClienteDto?> ObtenerClienteAsync(Guid id);
    Task<ClienteDto?> ObtenerClientePorIdAsync(Guid id);
    Task<ClienteDto?> CrearClienteAsync(CrearClienteRequest request);
    Task<ClienteDto?> ActualizarClienteAsync(Guid id, ActualizarClienteRequest request);
    Task<bool> EliminarClienteAsync(Guid id);
    Task<bool> CambiarEstadoClienteAsync(Guid id, bool activo);
    Task<bool> ValidarEmailAsync(string email, Guid? clienteIdExcluir = null);
    Task<bool> ToggleActivarClienteAsync(Guid id);
    
    // Estadísticas y reportes
    Task<ClienteEstadisticasDto?> ObtenerEstadisticasAsync();
    Task<ApiResponse<byte[]>?> ExportarClientesAsync(ClienteFiltrosDto filtros, string formato = "Excel");
    
    // Métodos auxiliares para filtros
    Task<List<string>?> ObtenerSegmentosAsync();
    Task<List<string>?> ObtenerCiudadesAsync();
}
