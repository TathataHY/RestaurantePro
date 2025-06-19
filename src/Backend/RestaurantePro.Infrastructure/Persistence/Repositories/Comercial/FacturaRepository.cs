using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Comercial
{
    /// <summary>
    /// Implementación del repositorio de facturas
    /// </summary>
    public class FacturaRepository : Repository<Factura>, IFacturaRepository
    {
        private new readonly RestauranteProDbContext _dbContext;

        public FacturaRepository(RestauranteProDbContext dbContext, ILogger<FacturaRepository> logger)
            : base(dbContext, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Obtiene una factura por su número
        /// </summary>
        public async Task<Factura?> ObtenerPorNumeroAsync(string numeroFactura, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(f => f.Detalles)
                .FirstOrDefaultAsync(f => f.NumeroFactura == numeroFactura, cancellationToken);
        }

        /// <summary>
        /// Obtiene todas las facturas asociadas a una comanda
        /// </summary>
        public async Task<IEnumerable<Factura>> ObtenerPorComandaAsync(Guid comandaId, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(_dbSet
                .Include(f => f.Detalles)
                .AsEnumerable()
                .Where(f => f.ComandasIds.Contains(comandaId))
                .OrderByDescending(f => f.FechaEmision)
                .ToList());
        }

        /// <summary>
        /// Obtiene todas las facturas de un cliente
        /// </summary>
        public async Task<IEnumerable<Factura>> ObtenerPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(f => f.ClienteId == clienteId && f.Estado != EstadoFactura.Anulada)
                .Include(f => f.Detalles)
                .OrderByDescending(f => f.FechaEmision)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene las facturas por estado
        /// </summary>
        public async Task<IEnumerable<Factura>> ObtenerPorEstadoAsync(EstadoFactura estado, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(f => f.Estado == estado)
                .OrderByDescending(f => f.FechaEmision)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene las facturas en un rango de fechas
        /// </summary>
        public async Task<IEnumerable<Factura>> ObtenerPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(f => f.FechaEmision >= fechaInicio && f.FechaEmision <= fechaFin && f.Estado != EstadoFactura.Anulada)
                .OrderByDescending(f => f.FechaEmision)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene las facturas pendientes de pago
        /// </summary>
        public async Task<IEnumerable<Factura>> ObtenerPendientesPagoAsync(CancellationToken cancellationToken = default)
        {
            var estadosPendientes = new[] { EstadoFactura.Emitida, EstadoFactura.PagadaParcialmente };
            return await _dbSet
                .Where(f => estadosPendientes.Contains(f.Estado) && f.Total > f.TotalPagado)
                .OrderBy(f => f.FechaVencimiento)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene el siguiente número de factura disponible
        /// </summary>
        public async Task<string> ObtenerSiguienteNumeroFacturaAsync(string? prefijo = null, CancellationToken cancellationToken = default)
        {
            prefijo ??= "F";
            var query = _dbSet.Where(f => f.NumeroFactura.StartsWith(prefijo));

            var ultimasFacturas = await query
                .Select(f => f.NumeroFactura)
                .ToListAsync(cancellationToken);

            int maxNumero = 0;
            foreach (var numFactura in ultimasFacturas)
            {
                if (numFactura.Length > prefijo.Length && int.TryParse(numFactura.Substring(prefijo.Length), out int numero))
                {
                    if (numero > maxNumero)
                    {
                        maxNumero = numero;
                    }
                }
            }

            return $"{prefijo}{(maxNumero + 1):D5}";
        }

        /// <summary>
        /// Verifica si existe una factura con el número especificado
        /// </summary>
        public async Task<bool> ExisteNumeroFacturaAsync(string numeroFactura, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(f => f.NumeroFactura == numeroFactura, cancellationToken);
        }

        /// <summary>
        /// Obtiene las facturas pendientes de un proveedor específico
        /// </summary>
        public async Task<IEnumerable<Factura>> ObtenerFacturasPendientesPorProveedorAsync(Guid proveedorId, CancellationToken cancellationToken = default)
        {
            // En un caso real, esto requeriría un modelo de relación con proveedores y facturas de compra
            // Por ahora, implementamos una versión simplificada
            return await _dbSet
                .Where(f => f.IdentificacionFiscal == proveedorId.ToString() && 
                       f.Estado == EstadoFactura.Emitida && 
                       f.Total > f.TotalPagado)
                .OrderBy(f => f.FechaVencimiento)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene una factura por ID con sus detalles
        /// </summary>
        public override async Task<Factura?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(f => f.Detalles)
                .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        }

        public override async Task<IEnumerable<Factura>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(f => f.Estado != EstadoFactura.Anulada).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Factura>> ObtenerFacturasPendientesConVencimientoAsync(DateTime fechaVencimiento, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Factura>()
                .Include(f => f.Cliente)
                .Where(f => (f.Estado == EstadoFactura.Emitida || f.Estado == EstadoFactura.PagadaParcialmente) && f.FechaVencimiento <= fechaVencimiento && !f.EstaEliminado)
                .ToListAsync(cancellationToken);
        }
    }
} 