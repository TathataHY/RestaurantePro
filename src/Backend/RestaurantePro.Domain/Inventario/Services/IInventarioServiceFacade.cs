namespace RestaurantePro.Domain.Inventario.Services
{
    /// <summary>
    /// Fachada de servicios para el contexto de Inventario
    /// Esta interfaz expone operaciones compuestas para ser utilizadas por la capa de Aplicación
    /// </summary>
    public interface IInventarioServiceFacade
    {
        #region Ingredientes
        
        /// <summary>
        /// Registra un nuevo ingrediente en el inventario
        /// </summary>
        /// <param name="nombre">Nombre del ingrediente</param>
        /// <param name="descripcion">Descripción del ingrediente</param>
        /// <param name="unidadMedida">Unidad de medida</param>
        /// <param name="stockMinimo">Stock mínimo requerido</param>
        /// <param name="stockActual">Stock actual</param>
        /// <param name="rotacion">Nivel de rotación del ingrediente</param>
        /// <param name="temporada">Indicador de temporada</param>
        /// <param name="costo">Costo promedio</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Ingrediente registrado</returns>
        Task<Ingrediente> RegistrarIngredienteAsync(
            string nombre, 
            string descripcion, 
            string unidadMedida, 
            decimal stockMinimo, 
            decimal stockActual,
            RotacionIngrediente rotacion = RotacionIngrediente.Media,
            TemporadaIngrediente temporada = TemporadaIngrediente.TodoElAño,
            decimal costo = 0,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza el stock de un ingrediente
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="cantidad">Cantidad a agregar (positivo) o restar (negativo)</param>
        /// <param name="tipoMovimiento">Tipo de movimiento</param>
        /// <param name="referencia">Referencia al documento origen (opcional)</param>
        /// <param name="observacion">Observación del movimiento (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Ingrediente actualizado o null si no se encontró</returns>
        Task<Ingrediente?> ActualizarStockIngredienteAsync(
            Guid ingredienteId, 
            decimal cantidad, 
            TipoMovimientoInventario tipoMovimiento, 
            string? referencia = null, 
            string? observacion = null, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Verifica los ingredientes con stock bajo
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de ingredientes con stock bajo</returns>
        Task<IEnumerable<Ingrediente>> VerificarIngredientesStockBajoAsync(CancellationToken cancellationToken = default);
        
        #endregion
        
        #region Órdenes de Compra
        
        /// <summary>
        /// Crea una nueva orden de compra
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="fechaEntregaEstimada">Fecha estimada de entrega</param>
        /// <param name="observaciones">Observaciones generales</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Orden de compra creada</returns>
        Task<OrdenCompra> CrearOrdenCompraAsync(
            Guid proveedorId, 
            DateTime fechaEntregaEstimada, 
            string observaciones = "", 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Agrega un ítem a una orden de compra
        /// </summary>
        /// <param name="ordenCompraId">ID de la orden de compra</param>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="cantidad">Cantidad</param>
        /// <param name="precioUnitario">Precio unitario</param>
        /// <param name="observacion">Observación del ítem</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Orden de compra actualizada o null si no se encontró</returns>
        Task<OrdenCompra?> AgregarItemOrdenCompraAsync(
            Guid ordenCompraId, 
            Guid ingredienteId, 
            decimal cantidad, 
            decimal precioUnitario, 
            string observacion = "", 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza el estado de una orden de compra
        /// </summary>
        /// <param name="ordenCompraId">ID de la orden de compra</param>
        /// <param name="nuevoEstado">Nuevo estado</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se actualizó correctamente, False si no se encontró la orden</returns>
        Task<bool> ActualizarEstadoOrdenCompraAsync(
            Guid ordenCompraId, 
            EstadoOrdenCompra nuevoEstado, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Registra la recepción completa de una orden de compra
        /// </summary>
        /// <param name="ordenCompraId">ID de la orden de compra</param>
        /// <param name="observaciones">Observaciones de la recepción</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se registró correctamente, False si no se encontró la orden</returns>
        Task<bool> RecibirOrdenCompraCompletaAsync(
            Guid ordenCompraId, 
            string observaciones = "", 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Registra la recepción parcial de una orden de compra
        /// </summary>
        /// <param name="ordenCompraId">ID de la orden de compra</param>
        /// <param name="itemsRecibidos">Diccionario con ID de ítem y cantidad recibida</param>
        /// <param name="observaciones">Observaciones de la recepción</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se registró correctamente, False si no se encontró la orden</returns>
        Task<bool> RecibirOrdenCompraParcialAsync(
            Guid ordenCompraId, 
            Dictionary<Guid, decimal> itemsRecibidos, 
            string observaciones = "", 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Genera órdenes de compra automáticas basadas en inventario bajo
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de órdenes de compra generadas</returns>
        Task<IEnumerable<OrdenCompra>> GenerarOrdenesCompraAutomaticasAsync(CancellationToken cancellationToken = default);
        
        #endregion
    }
} 