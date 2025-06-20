using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Comercial;

/// <summary>
/// Implementación del repositorio de transacciones de puntos
/// </summary>
public class TransaccionPuntosRepository : Repository<TransaccionPuntos>, ITransaccionPuntosRepository
{
    public TransaccionPuntosRepository(RestauranteProDbContext dbContext, ILogger<TransaccionPuntosRepository> logger)
        : base(dbContext, logger)
    {
    }

    /// <summary>
    /// Obtiene las transacciones de una tarjeta de fidelización
    /// </summary>
    public async Task<List<TransaccionPuntos>> ObtenerPorTarjetaAsync(Guid tarjetaId, DateTime? desde = null, DateTime? hasta = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(t => t.TarjetaFidelizacionId == tarjetaId);

        if (desde.HasValue)
            query = query.Where(t => t.Fecha >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(t => t.Fecha <= hasta.Value);

        return await query
            .OrderByDescending(t => t.Fecha)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtiene las transacciones de un cliente
    /// </summary>
    public async Task<List<TransaccionPuntos>> ObtenerPorClienteAsync(Guid clienteId, DateTime? desde = null, DateTime? hasta = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(t => t.ClienteId == clienteId);

        if (desde.HasValue)
            query = query.Where(t => t.Fecha >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(t => t.Fecha <= hasta.Value);

        return await query
            .OrderByDescending(t => t.Fecha)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtiene el saldo actual de puntos de una tarjeta
    /// </summary>
    public async Task<int> ObtenerSaldoPuntosAsync(Guid tarjetaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.TarjetaFidelizacionId == tarjetaId)
            .SumAsync(t => t.Puntos, cancellationToken);
    }

    /// <summary>
    /// Obtiene las transacciones por tipo
    /// </summary>
    public async Task<List<TransaccionPuntos>> ObtenerPorTipoAsync(Guid tarjetaId, TipoTransaccionPuntos tipo, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.TarjetaFidelizacionId == tarjetaId && t.Tipo == tipo)
            .OrderByDescending(t => t.Fecha)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtiene las transacciones asociadas a una factura
    /// </summary>
    public async Task<List<TransaccionPuntos>> ObtenerPorFacturaAsync(Guid facturaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.FacturaId == facturaId)
            .OrderByDescending(t => t.Fecha)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Marca transacciones como vencidas
    /// </summary>
    public async Task<int> MarcarVencidasAsync(DateTime fecha, CancellationToken cancellationToken = default)
    {
        var transaccionesVencidas = await _dbSet
            .Where(t => t.FechaVencimiento.HasValue && t.FechaVencimiento.Value <= fecha)
            .ToListAsync(cancellationToken);

        // En una implementación real, aquí se marcarían como vencidas
        // Por ahora solo retornamos el conteo
        return transaccionesVencidas.Count;
    }

    /// <summary>
    /// Obtiene estadísticas de transacciones para un período
    /// </summary>
    public async Task<EstadisticasTransacciones> ObtenerEstadisticasAsync(DateTime desde, DateTime hasta, CancellationToken cancellationToken = default)
    {
        var transacciones = await _dbSet
            .Where(t => t.Fecha >= desde && t.Fecha <= hasta)
            .ToListAsync(cancellationToken);

        var estadisticas = new EstadisticasTransacciones
        {
            TotalTransacciones = transacciones.Count,
            TotalPuntosAcumulados = transacciones.Where(t => t.Puntos > 0).Sum(t => t.Puntos),
            TotalPuntosCanjeados = transacciones.Where(t => t.Puntos < 0).Sum(t => Math.Abs(t.Puntos)),
            ValorTotalTransacciones = transacciones.Where(t => t.MontoAsociado.HasValue).Sum(t => t.MontoAsociado.Value)
        };

        if (estadisticas.TotalTransacciones > 0)
        {
            estadisticas.PromedioPuntosPorTransaccion = (decimal)estadisticas.TotalPuntosAcumulados / estadisticas.TotalTransacciones;
        }

        // Estadísticas por tipo
        foreach (TipoTransaccionPuntos tipo in Enum.GetValues<TipoTransaccionPuntos>())
        {
            var cantidadPorTipo = transacciones.Count(t => t.Tipo == tipo);
            if (cantidadPorTipo > 0)
            {
                estadisticas.TransaccionesPorTipo[tipo] = cantidadPorTipo;
            }
        }

        return estadisticas;
    }
} 