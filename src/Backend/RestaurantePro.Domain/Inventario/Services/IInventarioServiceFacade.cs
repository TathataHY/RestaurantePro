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
        /// <returns>Resultado con el ingrediente registrado</returns>
        Task<Result<Ingrediente>> RegistrarIngredienteAsync(
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
        /// Registra un nuevo ingrediente usando el IngredienteBuilder con opciones avanzadas
        /// </summary>
        /// <param name="nombre">Nombre del ingrediente</param>
        /// <param name="descripcion">Descripción del ingrediente</param>
        /// <param name="unidadMedida">Unidad de medida</param>
        /// <param name="stockMinimo">Stock mínimo</param>
        /// <param name="stockActual">Stock actual</param>
        /// <param name="codigo">Código personalizado (opcional - se genera automático si no se proporciona)</param>
        /// <param name="proveedorPrincipalId">ID del proveedor principal (opcional)</param>
        /// <param name="rotacion">Nivel de rotación del ingrediente</param>
        /// <param name="temporada">Temporada del ingrediente</param>
        /// <param name="costo">Costo promedio</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con el ingrediente registrado</returns>
        Task<Result<Ingrediente>> RegistrarIngredienteAvanzadoAsync(
            string nombre,
            string descripcion,
            UnidadMedida unidadMedida,
            decimal stockMinimo,
            decimal stockActual,
            string? codigo = null,
            Guid? proveedorPrincipalId = null,
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
        /// <returns>Resultado con el ingrediente actualizado o error si no se encontró</returns>
        Task<Result<Ingrediente>> ActualizarStockIngredienteAsync(
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
        /// <returns>Resultado con la lista de ingredientes con stock bajo</returns>
        Task<Result<IEnumerable<Ingrediente>>> VerificarIngredientesStockBajoAsync(CancellationToken cancellationToken = default);
        
        #endregion
        
        #region Órdenes de Compra
        
        /// <summary>
        /// Crea una nueva orden de compra para el proveedor especificado
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="fechaEntregaEstimada">Fecha estimada de entrega</param>
        /// <param name="observaciones">Observaciones adicionales (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la orden de compra creada</returns>
        Task<Result<OrdenCompra>> CrearOrdenCompraAsync(
            Guid proveedorId, 
            DateTime fechaEntregaEstimada, 
            string observaciones = "", 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Crea una orden de compra usando el OrdenCompraBuilder con múltiples ítems
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="fechaEntregaEstimada">Fecha estimada de entrega</param>
        /// <param name="items">Lista de ítems a incluir en la orden (ingredienteId, cantidad, precioUnitario)</param>
        /// <param name="observaciones">Observaciones (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la orden de compra creada</returns>
        Task<Result<OrdenCompra>> CrearOrdenCompraAvanzadaAsync(
            Guid proveedorId,
            DateTime fechaEntregaEstimada,
            List<(Guid ingredienteId, decimal cantidad, decimal precioUnitario)> items,
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
        /// <returns>Resultado con la orden de compra actualizada o error si no se encontró</returns>
        Task<Result<OrdenCompra>> AgregarItemOrdenCompraAsync(
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
        /// <returns>Resultado indicando éxito o error si no se encontró la orden</returns>
        Task<Result<bool>> ActualizarEstadoOrdenCompraAsync(
            Guid ordenCompraId, 
            EstadoOrdenCompra nuevoEstado, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Registra la recepción completa de una orden de compra
        /// </summary>
        /// <param name="ordenCompraId">ID de la orden de compra</param>
        /// <param name="observaciones">Observaciones de la recepción</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado indicando éxito o error si no se encontró la orden</returns>
        Task<Result<bool>> RecibirOrdenCompraCompletaAsync(
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
        /// <returns>Resultado indicando éxito o error si no se encontró la orden</returns>
        Task<Result<bool>> RecibirOrdenCompraParcialAsync(
            Guid ordenCompraId, 
            Dictionary<Guid, decimal> itemsRecibidos, 
            string observaciones = "", 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Genera órdenes de compra automáticas basadas en inventario bajo
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la lista de órdenes de compra generadas</returns>
        Task<Result<IEnumerable<OrdenCompra>>> GenerarOrdenesCompraAutomaticasAsync(CancellationToken cancellationToken = default);
        
        #endregion
    }
} 