using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Contexts;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Inventario;

/// <summary>
/// Implementación del repositorio de inventario de ingredientes
/// </summary>
public class InventarioIngredientesRepository : IInventarioIngredientesRepository
{
    private readonly RestauranteProDbContext _dbContext;
    private readonly ILogger<InventarioIngredientesRepository> _logger;

    public InventarioIngredientesRepository(RestauranteProDbContext dbContext, ILogger<InventarioIngredientesRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene el stock disponible de un ingrediente
    /// </summary>
    public async Task<decimal> ObtenerStockDisponibleAsync(Guid ingredienteId)
    {
        var ingrediente = await _dbContext.Set<Ingrediente>()
            .FirstOrDefaultAsync(i => i.Id == ingredienteId);

        return ingrediente?.Stock ?? 0;
    }

    /// <summary>
    /// Obtiene un ingrediente por su ID
    /// </summary>
    public async Task<Ingrediente?> ObtenerIngredientePorIdAsync(Guid ingredienteId)
    {
        return await _dbContext.Set<Ingrediente>()
            .FirstOrDefaultAsync(i => i.Id == ingredienteId);
    }

    /// <summary>
    /// Obtiene todos los ingredientes con stock bajo
    /// </summary>
    public async Task<IEnumerable<Ingrediente>> ObtenerIngredientesConStockBajoAsync(decimal stockMinimo)
    {
        return await _dbContext.Set<Ingrediente>()
            .Where(i => i.Stock <= stockMinimo)
            .ToListAsync();
    }

    /// <summary>
    /// Actualiza el stock de un ingrediente
    /// </summary>
    public async Task<bool> ActualizarStockAsync(Guid ingredienteId, decimal nuevoStock)
    {
        var ingrediente = await _dbContext.Set<Ingrediente>()
            .FirstOrDefaultAsync(i => i.Id == ingredienteId);

        if (ingrediente == null)
            return false;

        // Usar los métodos públicos IncrementarStock o DecrementarStock según corresponda
        var stockActual = ingrediente.Stock;
        if (nuevoStock > stockActual)
        {
            ingrediente.IncrementarStock(nuevoStock - stockActual, "Actualización de stock");
        }
        else if (nuevoStock < stockActual)
        {
            ingrediente.DecrementarStock(stockActual - nuevoStock, "Actualización de stock");
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Registra un movimiento de inventario
    /// </summary>
    public async Task<MovimientoInventario> RegistrarMovimientoAsync(MovimientoInventario movimiento)
    {
        _dbContext.Set<MovimientoInventario>().Add(movimiento);
        await _dbContext.SaveChangesAsync();
        return movimiento;
    }

    /// <summary>
    /// Obtiene el historial de movimientos de un ingrediente
    /// </summary>
    public async Task<IEnumerable<MovimientoInventario>> ObtenerHistorialMovimientosAsync(
        Guid ingredienteId, 
        DateTime? fechaDesde = null, 
        DateTime? fechaHasta = null)
    {
        var query = _dbContext.Set<MovimientoInventario>()
            .Where(m => m.IngredienteId == ingredienteId);

        if (fechaDesde.HasValue)
            query = query.Where(m => m.Fecha >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(m => m.Fecha <= fechaHasta.Value);

        return await query
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();
    }

    /// <summary>
    /// Actualiza un ingrediente
    /// </summary>
    public async Task<Ingrediente> ActualizarAsync(Ingrediente ingrediente)
    {
        _dbContext.Set<Ingrediente>().Update(ingrediente);
        await _dbContext.SaveChangesAsync();
        return ingrediente;
    }
} 