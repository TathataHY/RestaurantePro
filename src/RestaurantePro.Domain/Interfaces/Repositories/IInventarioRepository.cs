using RestaurantePro.Domain.Entities;
using RestaurantePro.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Repositorio para operaciones relacionadas con inventario
    /// </summary>
    public interface IInventarioRepository : IBaseRepository<InventarioMovimiento>
    {
        /// <summary>
        /// Obtiene el stock actual de un ingrediente
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <returns>Cantidad actual en stock</returns>
        Task<decimal> GetStockActualAsync(int ingredienteId);

        /// <summary>
        /// Obtiene movimientos de inventario por ingrediente
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <returns>Lista de movimientos del ingrediente</returns>
        Task<IReadOnlyList<InventarioMovimiento>> GetMovimientosByIngredienteIdAsync(int ingredienteId);

        /// <summary>
        /// Obtiene movimientos de inventario por tipo
        /// </summary>
        /// <param name="tipo">Tipo de movimiento</param>
        /// <returns>Lista de movimientos del tipo especificado</returns>
        Task<IReadOnlyList<InventarioMovimiento>> GetMovimientosByTipoAsync(TipoMovimientoInventario tipo);

        /// <summary>
        /// Registra un nuevo movimiento de inventario
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="cantidad">Cantidad del movimiento</param>
        /// <param name="tipo">Tipo de movimiento</param>
        /// <param name="usuarioId">ID del usuario que realiza el movimiento</param>
        /// <param name="motivo">Motivo del movimiento</param>
        /// <param name="referencia">Referencia (opcional)</param>
        /// <returns>Movimiento registrado</returns>
        Task<InventarioMovimiento> RegistrarMovimientoAsync(
            int ingredienteId, 
            decimal cantidad, 
            TipoMovimientoInventario tipo, 
            int usuarioId, 
            string motivo, 
            string referencia = null);

        /// <summary>
        /// Obtiene una lista de ingredientes con stock bajo
        /// </summary>
        /// <returns>Lista de ingredientes con stock por debajo del mínimo</returns>
        Task<IReadOnlyList<Ingrediente>> GetIngredientesStockBajoAsync();
    }
} 