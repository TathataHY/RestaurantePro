using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Comercial;

/// <summary>
/// Implementación del repositorio de historial de puntos
/// </summary>
public class HistorialPuntosRepository : Repository<HistorialPuntos>, IHistorialPuntosRepository
{
    public HistorialPuntosRepository(RestauranteProDbContext dbContext, ILogger<HistorialPuntosRepository> logger)
        : base(dbContext, logger)
    {
    }

    /// <summary>
    /// Obtiene un registro de historial por su ID
    /// </summary>
    public new async Task<HistorialPuntos> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var historial = await _dbSet.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
        return historial ?? throw new KeyNotFoundException($"No se encontró el historial de puntos con ID {id}");
    }

    /// <summary>
    /// Obtiene todos los registros de una tarjeta de fidelización
    /// </summary>
    public async Task<IEnumerable<HistorialPuntos>> ObtenerPorTarjetaIdAsync(Guid tarjetaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(h => h.TarjetaFidelizacionId == tarjetaId)
            .OrderByDescending(h => h.FechaOperacion)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtiene los registros filtrados por tipo de operación
    /// </summary>
    public async Task<IEnumerable<HistorialPuntos>> ObtenerPorTipoOperacionAsync(Guid tarjetaId, TipoOperacionPuntos tipoOperacion, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(h => h.TarjetaFidelizacionId == tarjetaId && h.TipoOperacion == tipoOperacion)
            .OrderByDescending(h => h.FechaOperacion)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtiene los registros de puntos en un rango de fechas
    /// </summary>
    public async Task<IEnumerable<HistorialPuntos>> ObtenerPorRangoFechasAsync(Guid tarjetaId, DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(h => h.TarjetaFidelizacionId == tarjetaId && 
                       h.FechaOperacion >= fechaInicio && 
                       h.FechaOperacion <= fechaFin)
            .OrderByDescending(h => h.FechaOperacion)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtiene el total de puntos por tipo de operación en un periodo
    /// </summary>
    public async Task<int> ObtenerTotalPuntosPorTipoAsync(Guid tarjetaId, TipoOperacionPuntos tipoOperacion, DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(h => h.TarjetaFidelizacionId == tarjetaId && 
                       h.TipoOperacion == tipoOperacion &&
                       h.FechaOperacion >= fechaInicio && 
                       h.FechaOperacion <= fechaFin)
            .SumAsync(h => h.Puntos, cancellationToken);
    }

    /// <summary>
    /// Agrega un nuevo registro de historial
    /// </summary>
    public new async Task AgregarAsync(HistorialPuntos historial, CancellationToken cancellationToken = default)
    {
        await base.AgregarAsync(historial, cancellationToken);
    }

    /// <summary>
    /// Actualiza un registro de historial
    /// </summary>
    public new async Task ActualizarAsync(HistorialPuntos historial, CancellationToken cancellationToken = default)
    {
        await base.ActualizarAsync(historial, cancellationToken);
    }

    /// <summary>
    /// Elimina un registro de historial por ID
    /// </summary>
    public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await base.EliminarPorIdAsync(id, cancellationToken);
    }
} 