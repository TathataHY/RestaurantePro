using RestaurantePro.Core.DTOs.Comanda;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Interfaces.Repositories;

namespace RestaurantePro.Core.Interfaces.Services
{
    public interface IComandaService
    {
        Task<ComandaDto> GetByIdAsync(int id);
        Task<IEnumerable<ComandaDto>> GetAllAsync();
        Task<IEnumerable<ComandaDto>> GetPendientesAsync();
        Task<ComandaDto> CreateAsync(ComandaCreateDto comandaDto);
        Task<ComandaDto> UpdateAsync(int id, ComandaUpdateDto comandaDto);
        Task<bool> DeleteAsync(int id);
        Task<ComandaDto> AddDetalleAsync(int comandaId, ComandaDetalleCreateDto detalleDto);
    }
}