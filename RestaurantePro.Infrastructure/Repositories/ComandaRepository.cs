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
using RestaurantePro.Core.DTOs.Plato;
using RestaurantePro.Core.DTOs.Estadisticas;

namespace RestaurantePro.Infrastructure.Repositories
{
    public class ComandaRepository : BaseRepository<Comanda>, IComandaRepository
    {
        private readonly RestauranteContext _context;
        private readonly ILogger<ComandaRepository> _logger;

        public ComandaRepository(RestauranteContext context, ILogger<ComandaRepository> logger) : base(context)
        {
            _context = context;
            _logger = logger;
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

        public async Task<IEnumerable<Comanda>> GetComandasDelDia()
        {
            var hoy = DateTime.UtcNow.Date;
            return await _context.Comandas
                .Include(c => c.Detalles)
                .Include(c => c.Mesa)
                .Where(c => c.FechaHora.Date == hoy)
                .OrderByDescending(c => c.FechaHora)
                .ToListAsync();
        }

        public async Task<IEnumerable<VentasDiariasDto>> GetVentasDiariasAsync(DateTime fecha)
        {
            return await _context.Comandas
                .Where(c => c.CreatedAt.Date == fecha.Date)
                .GroupBy(c => c.CreatedAt.Hour)
                .Select(g => new VentasDiariasDto
                {
                    Hora = g.Key,
                    TotalVentas = g.Sum(c => c.Total),
                    CantidadComandas = g.Count()
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<PlatoPopularDto>> GetPlatosPopularesAsync(DateTime desde, DateTime hasta)
        {
            return await _context.ComandaDetalles
                .Where(cd => cd.Comanda.CreatedAt >= desde && cd.Comanda.CreatedAt <= hasta)
                .GroupBy(cd => cd.PlatoId)
                .Select(g => new PlatoPopularDto
                {
                    PlatoId = g.Key,
                    Cantidad = g.Sum(cd => cd.Cantidad),
                    Total = g.Sum(cd => cd.Subtotal)
                })
                .ToListAsync();
        }
    }
} 