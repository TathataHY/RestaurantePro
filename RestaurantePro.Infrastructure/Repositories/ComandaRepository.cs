using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Enums;
using RestaurantePro.Core.Interfaces;
using RestaurantePro.Core.Interfaces.Repositories;
using RestaurantePro.Infrastructure.Data;

namespace RestaurantePro.Infrastructure.Repositories
{
    public class ComandaRepository : BaseRepository<Comanda>, IComandaRepository
    {
        public ComandaRepository(RestauranteContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Comanda>> GetPendientesAsync()
        {
            return await _context.Comandas
                .Where(c => c.Estado == EstadoComanda.Pendiente && !c.IsDeleted)
                .Include(c => c.Mesa)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comanda>> GetByMesaIdAsync(int mesaId)
        {
            return await _context.Comandas
                .Where(c => c.MesaId == mesaId && !c.IsDeleted)
                .Include(c => c.Detalles)
                .ToListAsync();
        }

        public async Task<Comanda> GetComandaWithDetallesAsync(int comandaId)
        {
            return await _context.Comandas
                .Include(c => c.Mesa)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Plato)
                .FirstOrDefaultAsync(c => c.Id == comandaId && !c.IsDeleted);
        }

        public async Task<IEnumerable<Comanda>> GetAllAsync()
        {
            return await _context.Comandas
                .Include(c => c.Mesa)
                .Include(c => c.Mesero)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Plato)
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.FechaHora)
                .ToListAsync();
        }
    }
} 