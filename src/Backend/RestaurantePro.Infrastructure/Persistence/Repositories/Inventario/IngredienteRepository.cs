using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Specifications;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Inventario
{
    public class IngredienteRepository : Repository<Ingrediente>, IIngredienteRepository
    {
        private new readonly RestauranteProDbContext _dbContext;

        public IngredienteRepository(RestauranteProDbContext dbContext, ILogger<IngredienteRepository> logger) 
            : base(dbContext, logger)
        {
            _dbContext = dbContext;
        }

        public override async Task<Ingrediente?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(i => i.Id == id && !i.EstaEliminado, cancellationToken);
        }

        public async Task<Ingrediente?> ObtenerPorIdAsync(Guid id, bool incluirMovimientos = false, CancellationToken cancellationToken = default)
        {
            if (incluirMovimientos)
            {
                return await _dbSet
                    .Include(i => i.MovimientosNavegacion)
                    .FirstOrDefaultAsync(i => i.Id == id && !i.EstaEliminado, cancellationToken);
            }
            
            return await ObtenerPorIdAsync(id, cancellationToken);
        }

        public async Task<Ingrediente?> ObtenerPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(i => i.Codigo == codigo && !i.EstaEliminado, cancellationToken);
        }

        public async Task<IEnumerable<Ingrediente>> ObtenerPorCategoriaAsync(Guid categoriaId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(i => !i.EstaEliminado)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Ingrediente>> ObtenerPorProveedorAsync(Guid proveedorId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(i => i.ProveedorPrincipalId == proveedorId && !i.EstaEliminado)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Ingrediente>> ObtenerPorStockMinimoAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(i => i.Stock <= i.StockMinimo && i.EstaActivo)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Ingrediente>> ObtenerBloqueadosPorCalidadAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(i => i.ProveedorPrincipal)
                .Where(i => i.BloqueadoControlCalidad && i.EstaActivo)
                .ToListAsync(cancellationToken);
        }

        public new async Task<(IEnumerable<Ingrediente> Items, int Total)> ObtenerPaginadoAsync(
            int pagina, 
            int elementosPorPagina, 
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            
            // Aplicar filtros
            query = query.Where(i => !i.EstaEliminado);
            
            // Contar total
            var total = await query.CountAsync(cancellationToken);
            
            // Paginar resultados
            var ingredientes = await query
                .OrderBy(i => i.Nombre)
                .Skip(pagina * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync(cancellationToken);
            
            return (Items: ingredientes, Total: total);
        }
        
        // Implementación explícita para la interfaz específica
        async Task<(IEnumerable<Ingrediente> Ingredientes, int Total)> IIngredienteRepository.ObtenerPaginadoAsync(
            int pagina, 
            int elementosPorPagina, 
            CancellationToken cancellationToken)
        {
            var result = await ObtenerPaginadoAsync(pagina, elementosPorPagina, cancellationToken);
            return (Ingredientes: result.Items, Total: result.Total);
        }

        public override async Task<Ingrediente> AgregarAsync(Ingrediente entity, CancellationToken cancellationToken = default)
        {
            var result = await _dbSet.AddAsync(entity, cancellationToken);
            return result.Entity;
        }

        public override async Task<IEnumerable<Ingrediente>> AgregarRangoAsync(IEnumerable<Ingrediente> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
            return entities;
        }

        public async Task<IEnumerable<Ingrediente>> BuscarPorNombreAsync(string nombre, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(i => i.Nombre.Contains(nombre) && !i.EstaEliminado)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistePorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(i => i.Codigo == codigo && !i.EstaEliminado, cancellationToken);
        }

        public async Task<IEnumerable<Ingrediente>> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Ingrediente>()
                .Where(i => i.Nombre.Contains(nombre))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Ingrediente>> ObtenerConStockBajoAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Ingrediente>()
                .Where(i => i.Stock < i.StockMinimo && i.EstaActivo)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Ingrediente>> ObtenerPorRotacionAsync(RotacionIngrediente rotacion, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Ingrediente>()
                .Where(i => i.Rotacion == rotacion)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Ingrediente>> ObtenerPorTemporadaAsync(TemporadaIngrediente temporada, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Ingrediente>()
                .Where(i => i.Temporada == temporada)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Ingrediente>> ObtenerPorEstadoActivoAsync(bool activos, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Ingrediente>()
                .Where(i => i.EstaActivo == activos)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Ingrediente>> ObtenerIngredientesPorProductoAsync(Guid productoId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Ingrediente>()
                .Where(i => !i.EstaEliminado)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Ingrediente>> ObtenerIngredientesConStockBajoAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Ingrediente>()
                .Where(i => i.Stock < i.StockMinimo && i.EstaActivo)
                .OrderBy(i => i.Stock / (decimal)i.StockMinimo) // Ordenamos por porcentaje respecto al mínimo
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Ingrediente>> BuscarPorCategoriaAsync(string categoria, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Ingrediente>()
                .Where(i => !i.EstaEliminado)
                .OrderBy(i => i.Nombre)
                .ToListAsync(cancellationToken);
        }

        public override Task ActualizarAsync(Ingrediente entity, CancellationToken cancellationToken = default)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        public override Task EliminarAsync(Ingrediente entity, CancellationToken cancellationToken = default)
        {
            _dbContext.Set<Ingrediente>().Remove(entity);
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        public override async Task EliminarPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await ObtenerPorIdAsync(id, cancellationToken: cancellationToken);
            if (entity != null)
            {
                await EliminarAsync(entity, cancellationToken);
            }
        }

        public new Task<IEnumerable<Ingrediente>> BuscarAsync(
            Func<Ingrediente, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbContext.Set<Ingrediente>().Where(predicado).ToList();
            return Task.FromResult<IEnumerable<Ingrediente>>(result);
        }

        public new Task<bool> ExisteAsync(
            Func<Ingrediente, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbContext.Set<Ingrediente>().Any(predicado);
            return Task.FromResult(result);
        }

        public new Task<int> ContarAsync(
            Func<Ingrediente, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbContext.Set<Ingrediente>().Count(predicado);
            return Task.FromResult(result);
        }

        public new Task<Ingrediente?> PrimeroODefaultAsync(
            Func<Ingrediente, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbContext.Set<Ingrediente>().FirstOrDefault(predicado);
            return Task.FromResult(result);
        }

        public new async Task<IEnumerable<Ingrediente>> ObtenerPorSpecAsync(
            ISpecification<Ingrediente> spec, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<Ingrediente>().AsQueryable();
            var resultados = SpecificationEvaluator.GetQuery<Ingrediente>(query, spec);
            return await resultados.ToListAsync(cancellationToken);
        }

        public new async Task<int> ContarPorSpecAsync(
            ISpecification<Ingrediente> spec, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<Ingrediente>().AsQueryable();
            var resultados = SpecificationEvaluator.GetQuery<Ingrediente>(query, spec);
            return await resultados.CountAsync(cancellationToken);
        }

        public new async Task<Ingrediente?> PrimeroODefaultPorSpecAsync(
            ISpecification<Ingrediente> spec, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<Ingrediente>().AsQueryable();
            var resultados = SpecificationEvaluator.GetQuery<Ingrediente>(query, spec);
            return await resultados.FirstOrDefaultAsync(cancellationToken);
        }

        public override Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
} 