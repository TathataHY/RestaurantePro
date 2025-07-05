using RestaurantePro.Mobile.Core.Models.DTOs;

namespace RestaurantePro.Mobile.Core.Services.Productos;

/// <summary>
/// Servicio para gestión de productos - Solo consultas para móvil
/// </summary>
public interface IProductosService
{
    /// <summary>
    /// Obtener todos los productos disponibles con paginación
    /// </summary>
    /// <param name="pageNumber">Número de página (por defecto 1)</param>
    /// <param name="pageSize">Tamaño de página (por defecto 20)</param>
    /// <param name="filtro">Filtro de búsqueda por nombre o descripción</param>
    /// <param name="soloActivos">Solo productos activos (por defecto true)</param>
    /// <returns>Lista paginada de productos</returns>
    Task<ApiResponse<List<ProductoDto>>> ObtenerProductosPaginadosAsync(
        int pageNumber = 1, 
        int pageSize = 20, 
        string? filtro = null, 
        bool soloActivos = true);

    /// <summary>
    /// Obtener producto por ID específico
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <returns>Producto solicitado</returns>
    Task<ApiResponse<ProductoDto>> ObtenerProductoPorIdAsync(Guid productoId);

    /// <summary>
    /// Obtener productos por categoría específica
    /// </summary>
    /// <param name="categoriaId">ID de la categoría</param>
    /// <param name="soloActivos">Solo productos activos (por defecto true)</param>
    /// <returns>Lista de productos de la categoría</returns>
    Task<ApiResponse<List<ProductoDto>>> ObtenerProductosPorCategoriaAsync(
        Guid categoriaId, 
        bool soloActivos = true);

    /// <summary>
    /// Verificar disponibilidad de un producto específico
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <returns>Información de disponibilidad del producto</returns>
    Task<ApiResponse<DisponibilidadProductoDto>> VerificarDisponibilidadProductoAsync(Guid productoId);

    /// <summary>
    /// Buscar productos por texto (nombre, descripción, categoría)
    /// </summary>
    /// <param name="textoBusqueda">Texto a buscar</param>
    /// <param name="soloActivos">Solo productos activos (por defecto true)</param>
    /// <returns>Lista de productos que coinciden con la búsqueda</returns>
    Task<ApiResponse<List<ProductoDto>>> BuscarProductosAsync(
        string textoBusqueda, 
        bool soloActivos = true);

    /// <summary>
    /// Obtener productos más populares (para recomendaciones)
    /// </summary>
    /// <param name="limite">Número máximo de productos a retornar (por defecto 10)</param>
    /// <returns>Lista de productos más populares</returns>
    Task<ApiResponse<List<ProductoDto>>> ObtenerProductosPopularesAsync(int limite = 10);

    /// <summary>
    /// Obtener productos disponibles para agregar a comandas (filtrados)
    /// </summary>
    /// <param name="categoriaId">ID de categoría opcional para filtrar</param>
    /// <returns>Lista de productos disponibles para comandas</returns>
    Task<ApiResponse<List<ProductoDto>>> ObtenerProductosDisponiblesParaComandasAsync(Guid? categoriaId = null);

    /// <summary>
    /// Obtener categorías de productos disponibles
    /// </summary>
    /// <returns>Lista de categorías con productos</returns>
    Task<ApiResponse<List<CategoriaProductoDto>>> ObtenerCategoriasAsync();
} 