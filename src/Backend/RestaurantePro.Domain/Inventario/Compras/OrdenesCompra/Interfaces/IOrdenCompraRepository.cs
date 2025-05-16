namespace RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces
{
    /// <summary>
    /// Repositorio para gestionar las órdenes de compra
    /// </summary>
    public interface IOrdenCompraRepository
    {
        /// <summary>
        /// Obtiene una orden de compra por su identificador
        /// </summary>
        /// <param name="id">Identificador de la orden</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>La orden de compra o null si no existe</returns>
        Task<OrdenCompra> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene todas las órdenes de compra
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de órdenes de compra</returns>
        Task<IEnumerable<OrdenCompra>> ObtenerTodasAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene órdenes de compra por su estado
        /// </summary>
        /// <param name="estado">Estado de las órdenes a buscar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de órdenes de compra en el estado especificado</returns>
        Task<IEnumerable<OrdenCompra>> ObtenerPorEstadoAsync(EstadoOrdenCompra estado, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene las órdenes de compra de un proveedor específico
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de órdenes de compra del proveedor</returns>
        Task<IEnumerable<OrdenCompra>> ObtenerPorProveedorAsync(Guid proveedorId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene las órdenes de compra que contienen un ingrediente específico
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de órdenes de compra que contienen el ingrediente</returns>
        Task<IEnumerable<OrdenCompra>> ObtenerPorIngredienteAsync(Guid ingredienteId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene las órdenes de compra en un rango de fechas
        /// </summary>
        /// <param name="desde">Fecha de inicio</param>
        /// <param name="hasta">Fecha de fin</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de órdenes de compra en el rango de fechas</returns>
        Task<IEnumerable<OrdenCompra>> ObtenerPorRangoFechasAsync(DateTime desde, DateTime hasta, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Agrega una nueva orden de compra
        /// </summary>
        /// <param name="ordenCompra">Orden de compra a agregar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task AgregarAsync(OrdenCompra ordenCompra, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza una orden de compra existente
        /// </summary>
        /// <param name="ordenCompra">Orden de compra a actualizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task ActualizarAsync(OrdenCompra ordenCompra, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Guarda los cambios en el repositorio
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de entidades afectadas</returns>
        Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);
    }
} 
