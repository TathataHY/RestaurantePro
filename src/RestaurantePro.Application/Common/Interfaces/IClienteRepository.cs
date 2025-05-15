using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantePro.Domain.Comercial.Clientes.Entities;

namespace RestaurantePro.Application.Common.Interfaces
{
    public interface IClienteRepository
    {
        Task<Cliente> GetByIdAsync(int id);
        Task<IEnumerable<Cliente>> GetAllAsync();
        Task<IEnumerable<Cliente>> GetActivosAsync();
        Task<Cliente> GetByEmailAsync(string email);
        Task<Cliente> AddAsync(Cliente cliente);
        Task UpdateAsync(Cliente cliente);
        Task DeleteAsync(int id);
    }
} 