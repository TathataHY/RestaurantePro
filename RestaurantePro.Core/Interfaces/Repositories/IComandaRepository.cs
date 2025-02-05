using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Interfaces;
using RestaurantePro.Core.DTOs.Plato;
using RestaurantePro.Core.DTOs.Estadisticas;

namespace RestaurantePro.Core.Interfaces.Repositories
{
    public interface IComandaRepository : IAsyncRepository<Comanda>
    {
        Task<IEnumerable<Comanda>> GetAllAsync();
        Task<IEnumerable<Comanda>> GetPendientesAsync();
        Task<IEnumerable<Comanda>> GetByMesaIdAsync(int mesaId);
        Task<Comanda> GetComandaWithDetallesAsync(int comandaId);
        Task<IEnumerable<VentasDiariasDto>> GetVentasDiariasAsync(DateTime fecha);
        Task<IEnumerable<PlatoPopularDto>> GetPlatosPopularesAsync(DateTime desde, DateTime hasta);
    }
}