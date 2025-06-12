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
using RestaurantePro.Infrastructure.Persistence.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Inventario
{
    public class IngredienteRepository : Repository<Ingrediente>, IIngredienteRepository
    {
        private readonly RestauranteProDbContext _dbContext;

        public IngredienteRepository(RestauranteProDbContext dbContext, ILogger<IngredienteRepository> logger)
            : base(dbContext, logger)
        {
            _dbContext = dbContext;
        }

        public async Task<Ingrediente?> ObtenerPorIdAsync(Guid id, bool incluirMovimientos = false, CancellationToken cancellationToken = default)
        {
            if (incluirMovimientos)
            {
                return await _dbContext.Set<Ingrediente>()
                    .Include(i => i.Movimientos)
                    .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            }
            
            return await _dbContext.Set<Ingrediente>()
                .FindAsync(new object[] { id }, cancellationToken);
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

        public async Task<IEnumerable<Ingrediente>> ObtenerPorProveedorAsync(Guid proveedorId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Ingrediente>()
                .Where(i => i.ProveedorPrincipalId == proveedorId)
                .ToListAsync(cancellationToken);
        }

        public new async Task<(IEnumerable<Ingrediente> Ingredientes, int Total)> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<Ingrediente>().AsQueryable();
            
            var total = await query.CountAsync(cancellationToken);
            
            var ingredientes = await query
                .Skip(pagina * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync(cancellationToken);

            return (ingredientes, total);
        }

        public async Task<IEnumerable<Ingrediente>> ObtenerBloqueadosPorCalidadAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Ingrediente>()
                .Where(i => i.BloqueadoControlCalidad)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Ingrediente>> ObtenerIngredientesPorProductoAsync(Guid productoId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Ingrediente>()
                .Include(i => i.ProductoIngredientes)
                .Where(i => i.ProductoIngredientes.Any(pi => pi.ProductoId == productoId))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Ingrediente>> ObtenerIngredientesConStockBajoAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Ingrediente>()
                .Where(i => i.Stock < i.StockMinimo && i.EstaActivo)
                .OrderBy(i => i.Stock / (double)i.StockMinimo) // Ordenamos por porcentaje respecto al mínimo
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Ingrediente>> BuscarPorCategoriaAsync(string categoria, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Ingrediente>()
                .Where(i => i.Categoria.ToLower() == categoria.ToLower())
                .OrderBy(i => i.Nombre)
                .ToListAsync(cancellationToken);
        }

        public override Task<Ingrediente> AgregarAsync(Ingrediente entity, CancellationToken cancellationToken = default)
        {
            _dbContext.Set<Ingrediente>().Add(entity);
            _dbContext.SaveChangesAsync(cancellationToken);
            return Task.FromResult(entity);
        }

        public override Task<IEnumerable<Ingrediente>> AgregarRangoAsync(IEnumerable<Ingrediente> entities, CancellationToken cancellationToken = default)
        {
            _dbContext.Set<Ingrediente>().AddRange(entities);
            _dbContext.SaveChangesAsync(cancellationToken);
            return Task.FromResult(entities);
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

        public Task<IEnumerable<Ingrediente>> BuscarAsync(
            Func<Ingrediente, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbContext.Set<Ingrediente>().Where(predicado).ToList();
            return Task.FromResult<IEnumerable<Ingrediente>>(result);
        }

        public Task<bool> ExisteAsync(
            Func<Ingrediente, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbContext.Set<Ingrediente>().Any(predicado);
            return Task.FromResult(result);
        }

        public Task<int> ContarAsync(
            Func<Ingrediente, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbContext.Set<Ingrediente>().Count(predicado);
            return Task.FromResult(result);
        }

        public Task<Ingrediente?> PrimeroODefaultAsync(
            Func<Ingrediente, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbContext.Set<Ingrediente>().FirstOrDefault(predicado);
            return Task.FromResult(result);
        }

        public async Task<IEnumerable<Ingrediente>> ObtenerPorSpecAsync(
            ISpecification<Ingrediente> spec, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(spec).ToListAsync(cancellationToken);
        }

        public async Task<int> ContarPorSpecAsync(
            ISpecification<Ingrediente> spec, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(spec).CountAsync(cancellationToken);
        }

        public async Task<Ingrediente?> PrimeroODefaultPorSpecAsync(
            ISpecification<Ingrediente> spec, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(spec).FirstOrDefaultAsync(cancellationToken);
        }

        public override Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        private IQueryable<Ingrediente> ApplySpecification(ISpecification<Ingrediente> spec)
        {
            return SpecificationEvaluator<Ingrediente>.GetQuery(_dbContext.Set<Ingrediente>().AsQueryable(), spec);
        }
    }
} 