using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Repositories;
using RestaurantePro.Infrastructure.Persistence.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Operaciones
{
    /// <summary>
    /// Repositorio para la entidad Comanda
    /// </summary>
    public class ComandaRepository : Repository<Comanda>, IComandaRepository
    {
        public ComandaRepository(DbContext dbContext, ILogger<ComandaRepository> logger)
            : base(dbContext, logger)
        {
        }

        public override async Task<Comanda?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<Comanda?> ObtenerPorIdAsync(Guid id, bool incluirItems = true, CancellationToken cancellationToken = default)
        {
            if (incluirItems)
            {
                return await _dbSet
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            }
            
            return await _dbSet
                .FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<IEnumerable<Comanda>> ObtenerPorEstadoAsync(EstadoComanda estado, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.Estado == estado)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Comanda>> ObtenerPorMesaAsync(Guid mesaId, bool incluirItems = false, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Where(c => c.MesaId == mesaId);
                
            if (incluirItems)
            {
                query = query.Include(c => c.Items);
            }
            
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Comanda>> ObtenerPorMeseroAsync(Guid meseroId, bool incluirItems = false, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Where(c => c.MeseroId == meseroId);
                
            if (incluirItems)
            {
                query = query.Include(c => c.Items);
            }
            
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Comanda>> ObtenerPorClienteAsync(Guid clienteId, bool incluirItems = false, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Where(c => c.ClienteId == clienteId);
                
            if (incluirItems)
            {
                query = query.Include(c => c.Items);
            }
            
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Comanda>> ObtenerPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, bool incluirItems = false, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Where(c => c.FechaCreacion >= fechaInicio && c.FechaCreacion <= fechaFin);
                
            if (incluirItems)
            {
                query = query.Include(c => c.Items);
            }
            
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<ItemComanda?> ObtenerItemPorIdAsync(Guid itemId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { itemId }, cancellationToken);
        }

        public async Task<IEnumerable<ItemComanda>> ObtenerItemsPorProductoAsync(Guid productoId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(i => i.ProductoId == productoId).ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<Comanda> Comandas, int Total)> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, bool incluirItems = false, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            
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

        public override async Task<(IEnumerable<Comanda> Items, int Total)> ObtenerPaginadoAsync(
            int pagina, 
            int elementosPorPagina, 
            CancellationToken cancellationToken = default)
        {
            var total = await _dbSet.CountAsync(cancellationToken);
            var items = await _dbSet
                .Skip(pagina * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync(cancellationToken);

            return (Items: items, Total: total);
        }

        public async Task<IEnumerable<Comanda>> ObtenerPorIngredienteAsync(Guid ingredienteId, CancellationToken cancellationToken = default)
        {
            // Necesitamos unir varias tablas para encontrar comandas que incluyan un ingrediente específico
            // Esta es una implementación simplificada que asume que hay una relación entre ItemComanda y los ingredientes
            return await _dbSet
                .Include(c => c.Items)
                .Where(c => c.Items.Any(i => i.Ingredientes.Any(ing => ing.IngredienteId == ingredienteId)))
                .ToListAsync(cancellationToken);
        }

        public async Task<Dictionary<DateTime, int>> ObtenerEstadisticasPorPeriodoAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
        {
            var comandasPorDia = await _dbSet
                .Where(c => c.FechaCreacion >= fechaInicio && c.FechaCreacion <= fechaFin)
                .GroupBy(c => c.FechaCreacion.Date)
                .Select(g => new { Fecha = g.Key, Cantidad = g.Count() })
                .ToListAsync(cancellationToken);
                
            return comandasPorDia.ToDictionary(x => x.Fecha, x => x.Cantidad);
        }

        public override async Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        
        /// <summary>
        /// Obtiene una comanda por su número
        /// </summary>
        /// <param name="numero">Número de la comanda</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Comanda o null si no existe</returns>
        public async Task<Comanda?> ObtenerPorNumeroAsync(string numero, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Numero == numero && c.Activo, cancellationToken);
        }
        
        /// <summary>
        /// Obtiene las comandas activas
        /// </summary>
        /// <returns>Lista de comandas activas</returns>
        public async Task<IEnumerable<Comanda>> ObtenerComandasActivasAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.Items)
                .Where(c => c.Activo && c.Estado != "Completada" && c.Estado != "Cancelada")
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync(cancellationToken);
        }
        
        /// <summary>
        /// Obtiene las comandas por estado
        /// </summary>
        /// <param name="estado">Estado de las comandas</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de comandas en el estado indicado</returns>
        public async Task<IEnumerable<Comanda>> ObtenerComandasPorEstadoAsync(string estado, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.Items)
                .Where(c => c.Activo && c.Estado == estado)
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync(cancellationToken);
        }
        
        /// <summary>
        /// Obtiene las comandas de un cliente
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de comandas del cliente</returns>
        public async Task<IEnumerable<Comanda>> ObtenerComandasPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.Items)
                .Where(c => c.Activo && c.ClienteId == clienteId)
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync(cancellationToken);
        }
        
        /// <summary>
        /// Obtiene las comandas de una mesa
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de comandas de la mesa</returns>
        public async Task<IEnumerable<Comanda>> ObtenerComandasPorMesaAsync(Guid mesaId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.Items)
                .Where(c => c.Activo && c.MesaId == mesaId)
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync(cancellationToken);
        }
        
        /// <summary>
        /// Verifica si existe una comanda activa para una mesa
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si existe una comanda activa, false en caso contrario</returns>
        public async Task<bool> ExisteComandaActivaParaMesaAsync(Guid mesaId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(c => c.Activo && c.MesaId == mesaId && 
                         c.Estado != "Completada" && c.Estado != "Cancelada", 
                         cancellationToken);
        }

        // Implementación de métodos faltantes
        
        public async Task<IEnumerable<Comanda>> ObtenerComandasAbiertas(bool incluirItems = false, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Where(c => c.Activo && 
                      (c.Estado == "Creada" || c.Estado == "EnProceso" || c.Estado == "Lista"));
                      
            if (incluirItems)
            {
                query = query.Include(c => c.Items);
            }
            
            return await query.OrderByDescending(c => c.FechaCreacion)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExisteNumeroComandaAsync(string numeroComanda, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(c => c.Numero == numeroComanda, cancellationToken);
        }

        public async Task<int?> ObtenerUltimoSecuencialDelDiaAsync(DateTime fecha, CancellationToken cancellationToken = default)
        {
            var fechaInicio = fecha.Date;
            var fechaFin = fechaInicio.AddDays(1).AddTicks(-1);
            
            var ultimaComanda = await _dbSet
                .Where(c => c.FechaCreacion >= fechaInicio && c.FechaCreacion <= fechaFin)
                .OrderByDescending(c => c.Secuencial)
                .FirstOrDefaultAsync(cancellationToken);
                
            return ultimaComanda?.Secuencial;
        }

        public async Task<int> ObtenerUltimoSecuencialAsync(CancellationToken cancellationToken = default)
        {
            var ultimaComanda = await _dbSet
                .OrderByDescending(c => c.Secuencial)
                .FirstOrDefaultAsync(cancellationToken);
                
            return ultimaComanda?.Secuencial ?? 0;
        }

        public async Task<int> ObtenerUltimoSecuencialAsync(Guid sucursalId, DateTime fecha, CancellationToken cancellationToken = default)
        {
            var fechaInicio = fecha.Date;
            var fechaFin = fechaInicio.AddDays(1).AddTicks(-1);
            
            var ultimaComanda = await _dbSet
                .Where(c => c.SucursalId == sucursalId && c.FechaCreacion >= fechaInicio && c.FechaCreacion <= fechaFin)
                .OrderByDescending(c => c.Secuencial)
                .FirstOrDefaultAsync(cancellationToken);
                
            return ultimaComanda?.Secuencial ?? 0;
        }

        public async Task<(IEnumerable<Comanda> Comandas, int Total)> ObtenerComandasActivasAsync(Dictionary<string, object> criterios, int pagina, int elementosPorPagina, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(c => c.Activo).AsQueryable();
            
            // Aplicar filtros según los criterios recibidos
            foreach (var criterio in criterios)
            {
                switch (criterio.Key.ToLower())
                {
                    case "estado":
                        if (criterio.Value is string estado)
                            query = query.Where(c => c.Estado == estado);
                        break;
                    case "mesa":
                        if (criterio.Value is Guid mesaId)
                            query = query.Where(c => c.MesaId == mesaId);
                        break;
                    case "mesero":
                        if (criterio.Value is Guid meseroId)
                            query = query.Where(c => c.MeseroId == meseroId);
                        break;
                    case "cliente":
                        if (criterio.Value is Guid clienteId)
                            query = query.Where(c => c.ClienteId == clienteId);
                        break;
                    case "fechadesde":
                        if (criterio.Value is DateTime fechaDesde)
                            query = query.Where(c => c.FechaCreacion >= fechaDesde);
                        break;
                    case "fechahasta":
                        if (criterio.Value is DateTime fechaHasta)
                            query = query.Where(c => c.FechaCreacion <= fechaHasta);
                        break;
                    case "numero":
                        if (criterio.Value is string numero)
                            query = query.Where(c => c.Numero.Contains(numero));
                        break;
                }
            }
            
            var total = await query.CountAsync(cancellationToken);
            
            // Incluir Items y aplicar paginación
            query = query.Include(c => c.Items)
                    .OrderByDescending(c => c.FechaCreacion)
                    .Skip(pagina * elementosPorPagina)
                    .Take(elementosPorPagina);
                    
            var comandas = await query.ToListAsync(cancellationToken);
            
            return (comandas, total);
        }

        // Implementación de métodos IRepository<Comanda>
        public new async Task AgregarAsync(Comanda entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }

        public new async Task AgregarRangoAsync(IEnumerable<Comanda> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }

        public async Task<IEnumerable<Comanda>> BuscarAsync(Func<Comanda, bool> predicado, CancellationToken cancellationToken = default)
        {
            // Como Func<T, bool> no se puede traducir directamente a SQL, lo ejecutamos en memoria
            return _dbSet.AsEnumerable().Where(predicado).ToList();
        }

        public async Task<bool> ExisteAsync(Func<Comanda, bool> predicado, CancellationToken cancellationToken = default)
        {
            // Como Func<T, bool> no se puede traducir directamente a SQL, lo ejecutamos en memoria
            return _dbSet.AsEnumerable().Any(predicado);
        }

        public async Task<int> ContarAsync(Func<Comanda, bool> predicado, CancellationToken cancellationToken = default)
        {
            // Como Func<T, bool> no se puede traducir directamente a SQL, lo ejecutamos en memoria
            return _dbSet.AsEnumerable().Count(predicado);
        }

        public async Task<Comanda?> PrimeroODefaultAsync(Func<Comanda, bool> predicado, CancellationToken cancellationToken = default)
        {
            // Como Func<T, bool> no se puede traducir directamente a SQL, lo ejecutamos en memoria
            return _dbSet.AsEnumerable().FirstOrDefault(predicado);
        }

        public async Task<IEnumerable<Comanda>> ObtenerPorSpecAsync(ISpecification<Comanda> specification, CancellationToken cancellationToken = default)
        {
            // Implementación simplificada - debería traducir la especificación a consulta EF Core
            var query = _dbSet.AsQueryable();
            // Aplica la especificación (esto dependería de cómo se implementen las especificaciones)
            // Por ahora, simplemente devolvemos todos los elementos
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<int> ContarPorSpecAsync(ISpecification<Comanda> specification, CancellationToken cancellationToken = default)
        {
            // Implementación simplificada
            var query = _dbSet.AsQueryable();
            // Aplica la especificación
            return await query.CountAsync(cancellationToken);
        }

        public async Task<Comanda?> PrimeroODefaultPorSpecAsync(ISpecification<Comanda> specification, CancellationToken cancellationToken = default)
        {
            // Implementación simplificada
            var query = _dbSet.AsQueryable();
            // Aplica la especificación
            return await query.FirstOrDefaultAsync(cancellationToken);
        }
    }
} 