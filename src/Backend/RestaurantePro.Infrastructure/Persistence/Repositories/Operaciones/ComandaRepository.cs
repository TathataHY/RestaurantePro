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
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Specifications;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Operaciones
{
    /// <summary>
    /// Repositorio para la entidad Comanda
    /// </summary>
    public class ComandaRepository : Repository<Comanda>, IComandaRepository
    {
        public ComandaRepository(RestauranteProDbContext dbContext, ILogger<ComandaRepository> logger)
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

        public async Task<Comanda?> ObtenerPorIdConItemsAsync(Guid comandaId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == comandaId, cancellationToken);
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
            
            // Aplicar ordenamiento al final para evitar error con SplitQuery
            return await query.OrderByDescending(c => c.FechaCreacion).ToListAsync(cancellationToken);
        }

        public async Task<ItemComanda?> ObtenerItemPorIdAsync(Guid itemId, CancellationToken cancellationToken = default)
        {
            // Buscar el ItemComanda en su propio DbSet o recuperándolo desde Comandas
            return await _dbContext.Set<ItemComanda>().FindAsync(new object[] { itemId }, cancellationToken);
        }

        public async Task<IEnumerable<ItemComanda>> ObtenerItemsPorProductoAsync(Guid productoId, CancellationToken cancellationToken = default)
        {
            // Obtener directamente desde el DbSet de ItemComanda
            return await _dbContext.Set<ItemComanda>()
                .Where(i => i.ProductoId == productoId)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<Comanda> Comandas, int Total)> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, bool incluirItems = false, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            
            if (incluirItems)
            {
                query = query.Include(c => c.Items);
            }
            
            var total = await query.CountAsync(cancellationToken);
            
            // Aplicar ordenamiento y paginación al final para evitar error con SplitQuery
            var comandas = await query
                .OrderByDescending(c => c.FechaCreacion)
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
                .OrderByDescending(c => c.FechaCreacion) // Agregar ordenamiento para evitar error con SplitQuery
                .Skip(pagina * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync(cancellationToken);

            return (Items: items, Total: total);
        }

        public async Task<IEnumerable<Comanda>> ObtenerPorIngredienteAsync(Guid ingredienteId, CancellationToken cancellationToken = default)
        {
            // Buscar comandas que tienen ítems con personalizaciones que usan este ingrediente
            return await _dbSet
                .Include(c => c.Items)
                    .ThenInclude(i => i.Personalizaciones)
                .Where(c => c.Items.Any(i => i.Personalizaciones.Any(p => p.IngredienteId == ingredienteId)))
                .ToListAsync(cancellationToken);
        }

        public async Task<Dictionary<DateTime, int>> ObtenerEstadisticasPorPeriodoAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
        {
            var comandasPorDia = await _dbSet
                .Where(c => c.FechaCreacion >= fechaInicio && c.FechaCreacion <= fechaFin)
                .GroupBy(c => c.FechaCreacion.Date)
                .Select(g => new { Fecha = g.Key, Cantidad = g.Count() })
                .ToListAsync(cancellationToken);

            return comandasPorDia.ToDictionary(x => DateTime.SpecifyKind(x.Fecha, DateTimeKind.Utc), x => x.Cantidad);
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
                .FirstOrDefaultAsync(c => c.NumeroComanda == numero && !c.EstaEliminado, cancellationToken);
        }
        
        /// <summary>
        /// Obtiene las comandas activas
        /// </summary>
        /// <returns>Lista de comandas activas</returns>
        public async Task<IEnumerable<Comanda>> ObtenerComandasActivasAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.Items)
                .Where(c => !c.EstaEliminado && 
                        c.Estado != EstadoComanda.Finalizada && 
                        c.Estado != EstadoComanda.Cancelada)
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync(cancellationToken);
        }
        
        /// <summary>
        /// Obtiene las comandas por estado
        /// </summary>
        /// <param name="estado">Estado de las comandas</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de comandas en el estado indicado</returns>
        public async Task<IEnumerable<Comanda>> ObtenerComandasPorEstadoAsync(EstadoComanda estado, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.Items)
                .Where(c => !c.EstaEliminado && c.Estado == estado)
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
                .Where(c => !c.EstaEliminado && c.ClienteId == clienteId)
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
                .Where(c => !c.EstaEliminado && c.MesaId == mesaId)
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
                .AnyAsync(c => !c.EstaEliminado && c.MesaId == mesaId && 
                         c.Estado != EstadoComanda.Finalizada && c.Estado != EstadoComanda.Cancelada, 
                         cancellationToken);
        }

        // Implementación de métodos faltantes
        
        public async Task<IEnumerable<Comanda>> ObtenerComandasAbiertas(bool incluirItems = false, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Where(c => !c.EstaEliminado && 
                    (c.Estado == EstadoComanda.Creada || 
                     c.Estado == EstadoComanda.EnProceso || 
                     c.Estado == EstadoComanda.Lista));
                      
            if (incluirItems)
            {
                query = query.Include(c => c.Items);
            }
            
            return await query.OrderByDescending(c => c.FechaCreacion)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExisteNumeroComandaAsync(string numeroComanda, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(c => c.NumeroComanda == numeroComanda, cancellationToken);
        }

        public async Task<int?> ObtenerUltimoSecuencialDelDiaAsync(DateTime fecha, CancellationToken cancellationToken = default)
        {
            var fechaInicio = fecha.Date;
            var fechaFin = fechaInicio.AddDays(1).AddTicks(-1);
            
            // Como no existe la propiedad Secuencial, usamos el NumeroComanda para extraer un número secuencial
            var ultimaComanda = await _dbSet
                .Where(c => c.FechaCreacion >= fechaInicio && c.FechaCreacion <= fechaFin)
                .OrderByDescending(c => c.FechaCreacion)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (ultimaComanda == null)
                return null;
                
            // Intentamos extraer un número secuencial del NumeroComanda
            if (int.TryParse(ultimaComanda.NumeroComanda.Split('-').LastOrDefault(), out int secuencial))
                return secuencial;
                
            return null;
        }

        public async Task<int> ObtenerUltimoSecuencialAsync(CancellationToken cancellationToken = default)
        {
            var ultimaComanda = await _dbSet
                .OrderByDescending(c => c.FechaCreacion)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (ultimaComanda == null)
                return 0;
                
            // Intentamos extraer un número secuencial del NumeroComanda
            if (int.TryParse(ultimaComanda.NumeroComanda.Split('-').LastOrDefault(), out int secuencial))
                return secuencial;
                
            return 0;
        }

        public async Task<int> ObtenerUltimoSecuencialAsync(Guid sucursalId, DateTime fecha, CancellationToken cancellationToken = default)
        {
            var fechaInicio = fecha.Date;
            var fechaFin = fechaInicio.AddDays(1).AddTicks(-1);
            
            // Como no existe la propiedad SucursalId, ignoramos ese filtro
            var ultimaComanda = await _dbSet
                .Where(c => c.FechaCreacion >= fechaInicio && c.FechaCreacion <= fechaFin)
                .OrderByDescending(c => c.FechaCreacion)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (ultimaComanda == null)
                return 0;
                
            // Intentamos extraer un número secuencial del NumeroComanda
            if (int.TryParse(ultimaComanda.NumeroComanda.Split('-').LastOrDefault(), out int secuencial))
                return secuencial;
                
            return 0;
        }

        public async Task<(IEnumerable<Comanda> Comandas, int Total)> ObtenerComandasActivasAsync(Dictionary<string, object> criterios, int pagina, int elementosPorPagina, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(c => !c.EstaEliminado).AsQueryable();
            
            // Aplicar filtros según los criterios recibidos
            foreach (var criterio in criterios)
            {
                switch (criterio.Key.ToLower())
                {
                    case "estado":
                        if (criterio.Value is string estadoStr && Enum.TryParse<EstadoComanda>(estadoStr, out var estado))
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
                            query = query.Where(c => c.NumeroComanda.Contains(numero));
                        break;
                }
            }
            
            var total = await query.CountAsync(cancellationToken);
            
            // Incluir Items y aplicar paginación
            query = query.Include(c => c.Items)
                    .OrderByDescending(c => c.FechaCreacion) // Ya tiene ordenamiento
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

        public new async Task<IEnumerable<Comanda>> BuscarAsync(Func<Comanda, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbSet.Where(predicado).ToList();
            return result;
        }

        public new async Task<bool> ExisteAsync(Func<Comanda, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbSet.Any(predicado);
            return result;
        }

        public new async Task<int> ContarAsync(Func<Comanda, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbSet.Count(predicado);
            return result;
        }

        public new async Task<Comanda?> PrimeroODefaultAsync(Func<Comanda, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbSet.FirstOrDefault(predicado);
            return result;
        }

        public new async Task<IEnumerable<Comanda>> ObtenerPorSpecAsync(
            ISpecification<Comanda> spec, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            var resultados = SpecificationEvaluator.GetQuery(query, spec);
            return await resultados.ToListAsync(cancellationToken);
        }

        public new async Task<int> ContarPorSpecAsync(
            ISpecification<Comanda> spec, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            var resultados = SpecificationEvaluator.GetQuery(query, spec);
            return await resultados.CountAsync(cancellationToken);
        }

        public new async Task<Comanda?> PrimeroODefaultPorSpecAsync(
            ISpecification<Comanda> spec, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            var resultados = SpecificationEvaluator.GetQuery(query, spec);
            return await resultados.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<Comanda>> ObtenerComandasCompletadasAsync(
            DateTime fechaInicio, 
            DateTime fechaFin, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.Estado == EstadoComanda.Finalizada && 
                            c.FechaCreacion >= fechaInicio && 
                            c.FechaCreacion <= fechaFin)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> ContarComandasCompletadasAsync(
            DateTime fechaInicio, 
            DateTime fechaFin, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .CountAsync(c => c.Estado == EstadoComanda.Finalizada && 
                                c.FechaCreacion >= fechaInicio && 
                                c.FechaCreacion <= fechaFin, 
                            cancellationToken);
        }

        public async Task<int> ContarComandasCompletadasPorMesasAsync(
            IEnumerable<Guid> mesaIds, 
            DateTime fechaInicio, 
            DateTime fechaFin, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .CountAsync(c => c.Estado == EstadoComanda.Finalizada && 
                                c.MesaId.HasValue && 
                                mesaIds.Contains(c.MesaId.Value) &&
                                c.FechaCreacion >= fechaInicio && 
                                c.FechaCreacion <= fechaFin, 
                            cancellationToken);
        }

        public async Task<int> ContarComandasPorEstadoAsync(
            EstadoComanda estado, 
            DateTime fechaInicio, 
            DateTime fechaFin, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .CountAsync(c => c.Estado == estado && 
                                c.FechaCreacion >= fechaInicio && 
                                c.FechaCreacion <= fechaFin, 
                            cancellationToken);
        }

        public async Task<IEnumerable<Comanda>> ObtenerComandasFinalizadasPorMesaAsync(
            Guid mesaId, 
            DateTime fechaInicio, 
            DateTime fechaFin, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.Items)
                .Where(c => c.MesaId == mesaId && 
                           c.Estado == EstadoComanda.Finalizada &&
                           c.FechaCreacion >= fechaInicio && 
                           c.FechaCreacion <= fechaFin)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Elimina un item de comanda de la base de datos
        /// </summary>
        /// <param name="itemId">ID del item a eliminar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        public async Task EliminarItemAsync(Guid itemId, CancellationToken cancellationToken = default)
        {
            var item = await _dbContext.Set<ItemComanda>().FindAsync(new object[] { itemId }, cancellationToken);
            if (item != null)
            {
                _dbContext.Set<ItemComanda>().Remove(item);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("🗑️ Item de comanda {ItemId} eliminado de la base de datos", itemId);
            }
            else
            {
                _logger.LogWarning("⚠️ No se encontró el item de comanda {ItemId} para eliminar", itemId);
            }
        }

        /// <summary>
        /// Obtiene el total de ventas de comandas finalizadas en un rango de fechas usando consulta SQL directa
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio del rango</param>
        /// <param name="fechaFin">Fecha de fin del rango</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Total de ventas en el período</returns>
        public async Task<decimal> ObtenerVentasTotalesAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("💰 Calculando ventas totales desde {FechaInicio} hasta {FechaFin} - CONSULTA DIRECTA EF", fechaInicio, fechaFin);
            
            // Usar Entity Framework para sumar directamente el campo Total de la BD
            // Esto evita el problema del Value Object y accede al campo directo
            var comandasFinalizadas = await _dbSet
                .Where(c => c.Estado == EstadoComanda.Finalizada && 
                           c.FechaCreacion >= fechaInicio && 
                           c.FechaCreacion <= fechaFin)
                .ToListAsync(cancellationToken);
            
            // Calcular total usando reflection para acceder al campo directo de la BD
            decimal total = 0;
            foreach (var comanda in comandasFinalizadas)
            {
                // Usar reflection para acceder al campo Total privado de la BD
                var totalField = typeof(Comanda).GetField("_total", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (totalField?.GetValue(comanda) is decimal totalValue)
                {
                    total += totalValue;
                }
                else
                {
                    // Si no hay field privado, usar el getter público
                    total += comanda.Total?.Total ?? 0;
                }
            }
                
            _logger.LogInformation("💰 Total de ventas calculado: S/. {Total} de {Comandas} comandas", total, comandasFinalizadas.Count);
            return total;
        }
    }
} 