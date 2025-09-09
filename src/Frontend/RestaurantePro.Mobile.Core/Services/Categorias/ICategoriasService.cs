using RestaurantePro.Mobile.Core.Models.DTOs;

namespace RestaurantePro.Mobile.Core.Services.Categorias;

/// <summary>
/// Interfaz para el servicio de categorías de productos
/// </summary>
public interface ICategoriasService
{
    /// <summary>
    /// Obtener todas las categorías de productos
    /// </summary>
    /// <param name="soloActivas">Solo categorías activas (por defecto true)</param>
    /// <returns>Lista de categorías de productos</returns>
    Task<ApiResponse<List<CategoriaProductoDto>>> ObtenerCategoriasAsync(bool soloActivas = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtener una categoría específica por ID
    /// </summary>
    /// <param name="id">ID de la categoría</param>
    /// <returns>Categoría de producto</returns>
    Task<ApiResponse<CategoriaProductoDto>> ObtenerCategoriaAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtener productos por categoría
    /// </summary>
    /// <param name="categoriaId">ID de la categoría</param>
    /// <param name="soloActivos">Solo productos activos (por defecto true)</param>
    /// <returns>Lista de productos de la categoría</returns>
    Task<ApiResponse<List<ProductoDto>>> ObtenerProductosPorCategoriaAsync(Guid categoriaId, bool soloActivos = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtener categorías activas con productos
    /// </summary>
    /// <returns>Lista de categorías activas con sus productos</returns>
    Task<ApiResponse<List<CategoriaProductoDto>>> ObtenerCategoriasActivasAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Buscar categorías por nombre
    /// </summary>
    /// <param name="nombre">Nombre a buscar</param>
    /// <returns>Lista de categorías que coinciden</returns>
    Task<ApiResponse<List<CategoriaProductoDto>>> BuscarCategoriasAsync(string nombre, CancellationToken cancellationToken = default);
} 