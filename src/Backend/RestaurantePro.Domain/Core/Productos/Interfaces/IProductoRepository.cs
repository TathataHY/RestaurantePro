using RestaurantePro.Domain.Core.Productos.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace RestaurantePro.Domain.Core.Productos.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de productos
    /// </summary>
    public interface IProductoRepository : IRepository<Producto>
    {
        /// <summary>
        /// Obtiene un producto por su ID
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Producto si existe, null en caso contrario</returns>
        Task<Producto?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene todos los productos disponibles
        /// </summary>
        /// <param name="soloActivos">True para obtener solo productos activos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de productos</returns>
        Task<IEnumerable<Producto>> ObtenerTodosAsync(bool soloActivos = true, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene productos por categoría
        /// </summary>
        /// <param name="categoriaId">ID de la categoría</param>
        /// <param name="soloActivos">True para obtener solo productos activos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de productos de la categoría</returns>
        Task<List<Producto>> ObtenerPorCategoriaAsync(Guid categoriaId, bool soloActivos = true, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene productos que utilizan un ingrediente específico
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de productos que utilizan el ingrediente</returns>
        Task<List<Producto>> ObtenerProductosPorIngredienteAsync(Guid ingredienteId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Agrega un nuevo producto
        /// </summary>
        /// <param name="producto">Producto a agregar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task AgregarAsync(Producto producto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza un producto existente
        /// </summary>
        /// <param name="producto">Producto con los cambios</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task ActualizarAsync(Producto producto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Elimina un producto por su ID
        /// </summary>
        /// <param name="id">ID del producto a eliminar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task EliminarAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si un producto existe por su nombre
        /// </summary>
        /// <param name="nombre">Nombre del producto</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el producto existe, false en caso contrario</returns>
        Task<bool> ExisteProductoPorNombre(string nombre, CancellationToken cancellationToken = default);

        Task<List<Producto>> ObtenerProductosPorCategoriaAsync(Guid categoriaId, CancellationToken cancellationToken);
        Task<bool> ExisteProductoConNombreAsync(string nombre, CancellationToken cancellationToken);
        Task<List<Producto>> BuscarAsync(Expression<Func<Producto, bool>> predicate, CancellationToken cancellationToken);
    }
} 