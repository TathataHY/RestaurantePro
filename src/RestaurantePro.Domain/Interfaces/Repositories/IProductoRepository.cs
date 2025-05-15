using RestaurantePro.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Repositorio para operaciones relacionadas con productos
    /// </summary>
    public interface IProductoRepository : IBaseRepository<Producto>
    {
        /// <summary>
        /// Obtiene productos por categoría
        /// </summary>
        /// <param name="categoriaId">ID de la categoría</param>
        /// <returns>Lista de productos de la categoría especificada</returns>
        Task<IReadOnlyList<Producto>> GetByCategoriaIdAsync(int categoriaId);

        /// <summary>
        /// Busca productos por nombre o descripción
        /// </summary>
        /// <param name="busqueda">Texto a buscar</param>
        /// <returns>Lista de productos que coinciden con la búsqueda</returns>
        Task<IReadOnlyList<Producto>> BuscarAsync(string busqueda);

        /// <summary>
        /// Obtiene productos más vendidos
        /// </summary>
        /// <param name="cantidad">Cantidad de productos a obtener</param>
        /// <returns>Lista de productos más vendidos</returns>
        Task<IReadOnlyList<Producto>> GetMasVendidosAsync(int cantidad);

        /// <summary>
        /// Actualiza la disponibilidad de un producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="disponible">Estado de disponibilidad</param>
        /// <returns>True si se actualizó correctamente, false si no se encontró el producto</returns>
        Task<bool> ActualizarDisponibilidadAsync(int productoId, bool disponible);

        /// <summary>
        /// Verifica la disponibilidad de todos los ingredientes de un producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <returns>True si todos los ingredientes están disponibles, false en caso contrario</returns>
        Task<bool> VerificarDisponibilidadIngredientesAsync(int productoId);
    }
} 