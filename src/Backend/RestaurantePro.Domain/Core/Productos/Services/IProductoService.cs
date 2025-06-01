namespace RestaurantePro.Domain.Core.Productos.Services;

/// <summary>
/// Servicio de dominio para operaciones con productos
/// </summary>
public interface IProductoService
{
    /// <summary>
    /// Obtiene un producto por su ID
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <returns>Producto encontrado o null</returns>
    Task<Producto?> ObtenerPorIdAsync(Guid productoId);

    /// <summary>
    /// Obtiene la receta de un producto
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <returns>Receta del producto o null</returns>
    Task<Receta?> ObtenerRecetaPorProductoIdAsync(Guid productoId);

    /// <summary>
    /// Verifica si un producto está disponible
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <returns>True si está disponible</returns>
    Task<bool> EstaDisponibleAsync(Guid productoId);

    /// <summary>
    /// Obtiene productos por categoría
    /// </summary>
    /// <param name="categoriaId">ID de la categoría</param>
    /// <returns>Lista de productos</returns>
    Task<IEnumerable<Producto>> ObtenerPorCategoriaAsync(Guid categoriaId);

    /// <summary>
    /// Verifica la disponibilidad de ingredientes para un producto
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <param name="cantidad">Cantidad solicitada</param>
    /// <returns>True si hay ingredientes suficientes</returns>
    Task<bool> TieneIngredientesSuficientesAsync(Guid productoId, int cantidad = 1);
} 