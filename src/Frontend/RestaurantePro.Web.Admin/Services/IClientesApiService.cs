using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IClientesApiService
{
    Task<List<ClienteDto>> ObtenerClientesAsync();
    Task<ClienteDto?> ObtenerClientePorIdAsync(Guid id);
    Task<ClienteDto?> CrearClienteAsync(CrearClienteRequest request);
    Task<ClienteDto?> ActualizarClienteAsync(Guid id, ActualizarClienteRequest request);
    Task<bool> EliminarClienteAsync(Guid id);
    Task<bool> CambiarEstadoClienteAsync(Guid id, bool activo);
    Task<bool> ValidarEmailAsync(string email, Guid? clienteIdExcluir = null);
    Task<bool> ToggleActivarClienteAsync(Guid id);
}
