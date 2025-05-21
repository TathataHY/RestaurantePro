using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;

namespace RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de movimientos de inventario
    /// </summary>
    public interface IMovimientoInventarioRepository : IRepository<MovimientoInventario>
    {
        /// <summary>
        /// Obtiene un movimiento por su ID
        /// </summary>
        /// <param name="id">ID del movimiento</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Movimiento encontrado o null si no existe</returns>
        new Task<MovimientoInventario> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene todos los movimientos de un ingrediente
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de movimientos del ingrediente</returns>
        Task<IEnumerable<MovimientoInventario>> ObtenerPorIngredienteAsync(Guid ingredienteId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene los movimientos de inventario en un rango de fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de movimientos en el rango de fechas</returns>
        Task<IEnumerable<MovimientoInventario>> ObtenerPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene los movimientos por tipo
        /// </summary>
        /// <param name="tipoMovimiento">Tipo de movimiento</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de movimientos del tipo especificado</returns>
        Task<IEnumerable<MovimientoInventario>> ObtenerPorTipoAsync(TipoMovimientoInventario tipoMovimiento, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Registra un nuevo movimiento de inventario
        /// </summary>
        /// <param name="movimiento">Movimiento a registrar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Movimiento registrado</returns>
        Task<MovimientoInventario> RegistrarAsync(MovimientoInventario movimiento, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza un movimiento existente
        /// </summary>
        /// <param name="movimiento">Movimiento a actualizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        new Task ActualizarAsync(MovimientoInventario movimiento, CancellationToken cancellationToken = default);

        /// <summary>
        /// Calcula el stock actual de un ingrediente basado en sus movimientos
        /// </summary>
        Task<decimal> CalcularStockActualAsync(Guid ingredienteId);

        /// <summary>
        /// Agrega un nuevo movimiento
        /// </summary>
        Task AgregarAsync(MovimientoInventario movimiento);
        
        /// <summary>
        /// Guarda los cambios en la base de datos
        /// </summary>
        Task GuardarCambiosAsync();
    }
} 
