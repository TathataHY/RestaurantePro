using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Interfaces;

namespace RestaurantePro.Core.Interfaces.Repositories
{
    public interface IPlatoRepository : IAsyncRepository<Plato>
    {
        Task<IEnumerable<Plato>> GetDisponiblesAsync();
        Task<IEnumerable<Plato>> GetByCategoria(string categoria);
        Task<bool> ActualizarStockAsync(int platoId, int cantidad);
    }
}