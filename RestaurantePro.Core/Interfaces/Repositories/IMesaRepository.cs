using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Interfaces;

namespace RestaurantePro.Core.Interfaces.Repositories
{
    public interface IMesaRepository : IAsyncRepository<Mesa>
    {
        Task<IEnumerable<Mesa>> GetDisponiblesAsync();
        Task<Mesa> GetMesaWithComandasAsync(int mesaId);
        Task<bool> ExisteNumeroMesaAsync(string numero);
    }
}