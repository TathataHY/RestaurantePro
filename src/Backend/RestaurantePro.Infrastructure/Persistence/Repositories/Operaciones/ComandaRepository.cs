using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Operaciones
{
    public class ComandaRepository : Repository<Comanda>, IComandaRepository
    {
        public ComandaRepository(RestauranteProDbContext context) : base(context)
        {
        }

        public async Task<Comanda?> ObtenerPorIdAsync(Guid id, bool incluirItems = true, CancellationToken cancellationToken = default)
        {
            if (incluirItems)
            {
                return await _context.Set<Comanda>()
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            }
            
            return await _context.Set<Comanda>()
                .FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<IEnumerable<Comanda>> ObtenerPorEstadoAsync(EstadoComanda estado, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Comanda>()
                .Where(c => c.Estado == estado)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Comanda>> ObtenerPorMesaAsync(Guid mesaId, bool incluirItems = false, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Comanda>()
                .Where(c => c.MesaId == mesaId);
                
            if (incluirItems)
            {
                query = query.Include(c => c.Items);
            }
            
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Comanda>> ObtenerPorMeseroAsync(Guid meseroId, bool incluirItems = false, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Comanda>()
                .Where(c => c.MeseroId == meseroId);
                
            if (incluirItems)
            {
                query = query.Include(c => c.Items);
            }
            
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Comanda>> ObtenerPorClienteAsync(Guid clienteId, bool incluirItems = false, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Comanda>()
                .Where(c => c.ClienteId == clienteId);
                
            if (incluirItems)
            {
                query = query.Include(c => c.Items);
            }
            
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Comanda>> ObtenerPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, bool incluirItems = false, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Comanda>()
                .Where(c => c.FechaCreacion >= fechaInicio && c.FechaCreacion <= fechaFin);
                
            if (incluirItems)
            {
                query = query.Include(c => c.Items);
            }
            
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<ItemComanda?> ObtenerItemPorIdAsync(Guid itemId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ItemComanda>()
                .FindAsync(new object[] { itemId }, cancellationToken);
        }

        public async Task<IEnumerable<ItemComanda>> ObtenerItemsPorProductoAsync(Guid productoId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ItemComanda>()
                .Where(i => i.ProductoId == productoId)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<Comanda> Comandas, int Total)> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, bool incluirItems = false, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Comanda>().AsQueryable();
            
            if (incluirItems)
            {
                query = query.Include(c => c.Items);
            }
            
            var total = await query.CountAsync(cancellationToken);
            
            var comandas = await query
                .Skip(pagina * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync(cancellationToken);
                
            return (comandas, total);
        }

        public async Task<IEnumerable<Comanda>> ObtenerPorIngredienteAsync(Guid ingredienteId, CancellationToken cancellationToken = default)
        {
            // Necesitamos unir varias tablas para encontrar comandas que incluyan un ingrediente específico
            // Esta es una implementación simplificada que asume que hay una relación entre ItemComanda y los ingredientes
            return await _context.Set<Comanda>()
                .Include(c => c.Items)
                .Where(c => c.Items.Any(i => i.Ingredientes.Any(ing => ing.IngredienteId == ingredienteId)))
                .ToListAsync(cancellationToken);
        }

        public async Task<Dictionary<DateTime, int>> ObtenerEstadisticasPorPeriodoAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
        {
            var comandasPorDia = await _context.Set<Comanda>()
                .Where(c => c.FechaCreacion >= fechaInicio && c.FechaCreacion <= fechaFin)
                .GroupBy(c => c.FechaCreacion.Date)
                .Select(g => new { Fecha = g.Key, Cantidad = g.Count() })
                .ToListAsync(cancellationToken);
                
            return comandasPorDia.ToDictionary(x => x.Fecha, x => x.Cantidad);
        }

        public async Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
} 