using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Interfaces;
using RestaurantePro.Core.Interfaces.Repositories;
using RestaurantePro.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Infrastructure.Repositories
{
    public class PlatoRepository : BaseRepository<Plato>, IPlatoRepository
    {
        private readonly RestauranteContext _context;
        private readonly ILogger<PlatoRepository> _logger;

        public PlatoRepository(RestauranteContext context, ILogger<PlatoRepository> logger) : base(context)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Plato>> GetDisponiblesAsync()
        {
            return await _context.Platos
                .Where(p => p.Disponible && p.Stock > 0 && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Plato>> GetByCategoria(string categoria)
        {
            return await _context.Platos
                .Where(p => p.Categoria == categoria && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> ActualizarStockAsync(int platoId, int cantidad)
        {
            var plato = await _context.Platos.FindAsync(platoId);
            if (plato == null || plato.Stock < cantidad)
                return false;

            plato.Stock -= cantidad;
            plato.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 