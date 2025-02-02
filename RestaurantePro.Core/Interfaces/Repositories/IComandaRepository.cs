using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Interfaces;

namespace RestaurantePro.Core.Interfaces.Repositories
{
    public interface IComandaRepository : IAsyncRepository<Comanda>
    {
        Task<IEnumerable<Comanda>> GetAllAsync();
        Task<IEnumerable<Comanda>> GetPendientesAsync();
        Task<IEnumerable<Comanda>> GetByMesaIdAsync(int mesaId);
        Task<Comanda> GetComandaWithDetallesAsync(int comandaId);
    }
}