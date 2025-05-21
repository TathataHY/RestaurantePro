using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Inventario
{
    public class IngredienteRepository : Repository<Ingrediente>, IIngredienteRepository
    {
        public IngredienteRepository(RestauranteProDbContext context) : base(context)
        {
        }

        public async Task<Ingrediente?> ObtenerPorIdAsync(Guid id, bool incluirMovimientos = false, CancellationToken cancellationToken = default)
        {
            if (incluirMovimientos)
            {
                return await _context.Set<Ingrediente>()
                    .Include(i => i.Movimientos)
                    .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            }
            
            return await _context.Set<Ingrediente>()
                .FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<IEnumerable<Ingrediente>> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Ingrediente>()
                .Where(i => i.Nombre.Contains(nombre))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Ingrediente>> ObtenerConStockBajoAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<Ingrediente>()
                .Where(i => i.Stock < i.StockMinimo && i.EstaActivo)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Ingrediente>> ObtenerPorRotacionAsync(RotacionIngrediente rotacion, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Ingrediente>()
                .Where(i => i.Rotacion == rotacion)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Ingrediente>> ObtenerPorTemporadaAsync(TemporadaIngrediente temporada, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Ingrediente>()
                .Where(i => i.Temporada == temporada)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Ingrediente>> ObtenerPorEstadoActivoAsync(bool activos, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Ingrediente>()
                .Where(i => i.EstaActivo == activos)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Ingrediente>> ObtenerPorProveedorAsync(Guid proveedorId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Ingrediente>()
                .Where(i => i.ProveedorPrincipalId == proveedorId)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<Ingrediente> Ingredientes, int Total)> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Ingrediente>().AsQueryable();
            
            var total = await query.CountAsync(cancellationToken);
            
            var ingredientes = await query
                .Skip(pagina * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync(cancellationToken);
                
            return (ingredientes, total);
        }

        public async Task<IEnumerable<Ingrediente>> ObtenerBloqueadosPorCalidadAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<Ingrediente>()
                .Where(i => i.BloqueadoControlCalidad)
                .ToListAsync(cancellationToken);
        }
    }
} 