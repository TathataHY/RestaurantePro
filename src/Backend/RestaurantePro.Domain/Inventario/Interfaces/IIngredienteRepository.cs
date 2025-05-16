using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Inventario.Entities;
using RestaurantePro.Domain.Inventario.Enums;

namespace RestaurantePro.Domain.Inventario.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de ingredientes
    /// </summary>
    public interface IIngredienteRepository : IRepository<Ingrediente>
    {
        /// <summary>
        /// Obtiene un ingrediente por su nombre
        /// </summary>
        /// <param name="nombre">Nombre del ingrediente a buscar</param>
        /// <returns>El ingrediente si existe, null en caso contrario</returns>
        Task<Ingrediente> ObtenerPorNombreAsync(string nombre);
        
        /// <summary>
        /// Obtiene todos los ingredientes activos
        /// </summary>
        /// <returns>Lista de ingredientes activos</returns>
        Task<List<Ingrediente>> ObtenerActivosAsync();
        
        /// <summary>
        /// Obtiene los ingredientes con stock por debajo del mu00ednimo
        /// </summary>
        /// <returns>Lista de ingredientes con stock bajo</returns>
        Task<List<Ingrediente>> ObtenerConStockBajoAsync();
        
        /// <summary>
        /// Obtiene ingredientes por unidad de medida
        /// </summary>
        /// <param name="unidadMedida">Unidad de medida a filtrar</param>
        /// <returns>Lista de ingredientes con la unidad de medida especificada</returns>
        Task<List<Ingrediente>> ObtenerPorUnidadMedidaAsync(UnidadMedida unidadMedida);
    }
}
