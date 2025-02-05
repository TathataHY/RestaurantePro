using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Enums;
using RestaurantePro.Core.Interfaces;
using RestaurantePro.Core.Interfaces.Repositories;
using RestaurantePro.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Infrastructure.Repositories
{
    public class MesaRepository : BaseRepository<Mesa>, IMesaRepository
    {
        private readonly RestauranteContext _context;
        private readonly ILogger<MesaRepository> _logger;

        public MesaRepository(RestauranteContext context, ILogger<MesaRepository> logger) : base(context)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Mesa>> GetDisponiblesAsync()
        {
            return await _context.Mesas
                .Where(m => m.Estado == EstadoMesa.Disponible && !m.IsDeleted)
                .ToListAsync();
        }

        public async Task<Mesa> GetMesaWithComandasAsync(int mesaId)
        {
            return await _context.Mesas
                .Include(m => m.Comandas)
                .FirstOrDefaultAsync(m => m.Id == mesaId && !m.IsDeleted);
        }

        public async Task<bool> ExisteNumeroMesaAsync(string numero)
        {
            return await _context.Mesas
                .AnyAsync(m => m.Numero == numero && !m.IsDeleted);
        }

        public async Task<Mesa> GetByNumeroAsync(string numero)
        {
            return await _context.Mesas
                .FirstOrDefaultAsync(m => m.Numero.Equals(numero) && !m.IsDeleted);
        }
    }
} 