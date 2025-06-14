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
            return await _dbSet
                .Where(f => f.ComandasIds.Contains(comandaId))
                .Include(f => f.Detalles)
                .OrderByDescending(f => f.FechaEmision)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene todas las facturas de un cliente
        /// </summary>
        public async Task<IEnumerable<Factura>> ObtenerPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(f => f.ClienteId == clienteId)
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
                .Where(f => f.FechaEmision >= fechaInicio && f.FechaEmision <= fechaFin)
                .OrderByDescending(f => f.FechaEmision)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene las facturas pendientes de pago
        /// </summary>
        public async Task<IEnumerable<Factura>> ObtenerPendientesPagoAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(f => f.Estado == EstadoFactura.Emitida && f.Total > f.TotalPagado)
                .OrderBy(f => f.FechaVencimiento)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene el siguiente número de factura disponible
        /// </summary>
        public async Task<string> ObtenerSiguienteNumeroFacturaAsync(string? prefijo = null, CancellationToken cancellationToken = default)
        {
            prefijo ??= "F";
            
            // Obtener el último número de factura con el prefijo especificado
            var ultimaFactura = await _dbSet
                .Where(f => f.NumeroFactura.StartsWith(prefijo))
                .OrderByDescending(f => f.NumeroFactura)
                .FirstOrDefaultAsync(cancellationToken);

            if (ultimaFactura == null)
            {
                // Si no hay facturas, empezar desde 1
                return $"{prefijo}00001";
            }

            // Intentar extraer el número de la factura
            var numeroStr = ultimaFactura.NumeroFactura.Substring(prefijo.Length);
            if (int.TryParse(numeroStr, out int numero))
            {
                numero++;
                // Formato con ceros a la izquierda (5 dígitos)
                return $"{prefijo}{numero:D5}";
            }

            // Si no se puede extraer correctamente, usar un formato por defecto
            return $"{prefijo}{DateTime.Now:yyyyMMdd}001";
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
    }
} 