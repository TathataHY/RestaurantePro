namespace RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;

/// <summary>
/// Repositorio para operaciones de inventario de ingredientes (Domain)
/// </summary>
public interface IInventarioIngredientesRepository
{
    /// <summary>
    /// Obtiene el stock disponible de un ingrediente
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <returns>Cantidad disponible en stock</returns>
    Task<decimal> ObtenerStockDisponibleAsync(Guid ingredienteId);

    /// <summary>
    /// Obtiene un ingrediente por su ID
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <returns>Ingrediente encontrado o null</returns>
    Task<Ingrediente?> ObtenerIngredientePorIdAsync(Guid ingredienteId);

    /// <summary>
    /// Obtiene todos los ingredientes con stock bajo
    /// </summary>
    /// <param name="stockMinimo">Límite de stock mínimo</param>
    /// <returns>Lista de ingredientes con stock bajo</returns>
    Task<IEnumerable<Ingrediente>> ObtenerIngredientesConStockBajoAsync(decimal stockMinimo);

    /// <summary>
    /// Actualiza el stock de un ingrediente
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <param name="nuevoStock">Nueva cantidad de stock</param>
    /// <returns>True si se actualizó correctamente</returns>
    Task<bool> ActualizarStockAsync(Guid ingredienteId, decimal nuevoStock);

    /// <summary>
    /// Registra un movimiento de inventario
    /// </summary>
    /// <param name="movimiento">Datos del movimiento</param>
    /// <returns>Movimiento registrado</returns>
    Task<MovimientoInventario> RegistrarMovimientoAsync(MovimientoInventario movimiento);

    /// <summary>
    /// Obtiene el historial de movimientos de un ingrediente
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <param name="fechaDesde">Fecha inicial del rango</param>
    /// <param name="fechaHasta">Fecha final del rango</param>
    /// <returns>Lista de movimientos</returns>
    Task<IEnumerable<MovimientoInventario>> ObtenerHistorialMovimientosAsync(
        Guid ingredienteId, 
        DateTime? fechaDesde = null, 
        DateTime? fechaHasta = null);

    /// <summary>
    /// Actualiza un ingrediente
    /// </summary>
    /// <param name="ingrediente">Ingrediente a actualizar</param>
    /// <returns>Ingrediente actualizado</returns>
    Task<Ingrediente> ActualizarAsync(Ingrediente ingrediente);
} 