namespace RestaurantePro.Domain.Inventario.Policies
{
    /// <summary>
    /// Implementación de la política de stock bajo
    /// </summary>
    public class StockBajoPolicy : IStockBajoPolicy
    {
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly IServicioNotificacionesInventario _servicioNotificaciones;
        private readonly IVerificadorStock _verificadorStock;
        private readonly IDateTimeService _dateTimeService;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public StockBajoPolicy(
            IIngredienteRepository ingredienteRepository,
            IServicioNotificacionesInventario servicioNotificaciones,
            IVerificadorStock verificadorStock,
            IDateTimeService dateTimeService)
        {
            _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
            _servicioNotificaciones = servicioNotificaciones ?? throw new ArgumentNullException(nameof(servicioNotificaciones));
            _verificadorStock = verificadorStock ?? throw new ArgumentNullException(nameof(verificadorStock));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }
        
        /// <inheritdoc />
        public async Task<ResultadoStockBajoPolicy> EjecutarPolicy(CancellationToken cancellationToken = default)
        {
            var resultado = new ResultadoStockBajoPolicy();
            
            // Obtener los ingredientes con stock bajo
            var ingredientes = await _ingredienteRepository.ObtenerConStockBajoAsync();
            
            if (!ingredientes.Any())
            {
                return resultado; // No hay ingredientes con stock bajo
            }
            
            // Enviar notificaciones para cada ingrediente con stock bajo
            foreach (var ingrediente in ingredientes)
            {
                var notificacionId = await _servicioNotificaciones.NotificarStockBajo(
                    ingrediente.Id,
                    ingrediente.Nombre,
                    ingrediente.Stock,
                    ingrediente.StockMinimo);
                    
                resultado.Notificaciones.Add(notificacionId);
            }
            
            // Generar órdenes de compra automáticas
            var resultadoVerificacion = await _verificadorStock.VerificarYGenerarOrdenesCompraAsync(cancellationToken);
            
            // Registrar las órdenes generadas en el resultado
            foreach (var orden in resultadoVerificacion.OrdenesGeneradas)
            {
                resultado.OrdenesCompraGeneradas.Add(orden.Id);
                
                // Notificar sobre la orden de compra generada
                await _servicioNotificaciones.NotificarOrdenCompraGenerada(
                    orden.Id,
                    orden.ProveedorId,
                    "Proveedor"); // Idealmente, obtendríamos el nombre real del proveedor
            }
            
            return resultado;
        }
        
        /// <inheritdoc />
        public async Task<ResultadoStockBajoPolicy> EjecutarPolicyParaIngrediente(Guid ingredienteId, CancellationToken cancellationToken = default)
        {
            var resultado = new ResultadoStockBajoPolicy();
            
            // Obtener el ingrediente
            var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId, cancellationToken);
            
            if (ingrediente == null)
            {
                throw new ArgumentException($"No existe un ingrediente con el ID {ingredienteId}", nameof(ingredienteId));
            }
            
            // Verificar si el stock está por debajo del mínimo
            if (ingrediente.Stock >= ingrediente.StockMinimo)
            {
                return resultado; // El stock no está bajo, no hay acciones a tomar
            }
            
            // Enviar notificación de stock bajo
            var notificacionId = await _servicioNotificaciones.NotificarStockBajo(
                ingrediente.Id,
                ingrediente.Nombre,
                ingrediente.Stock,
                ingrediente.StockMinimo);
                
            resultado.Notificaciones.Add(notificacionId);
            
            // Verificar si tiene proveedor principal
            if (!ingrediente.ProveedorPrincipalId.HasValue)
            {
                return resultado; // No se puede generar orden sin proveedor
            }
            
            // Intentar generar una orden de compra para este ingrediente
            // Podríamos expandir esto para usar GeneradorOrdenesCompra directamente
            // pero por simplicidad usamos el VerificadorStock que ya sabe cómo crearlas
            var resultadoVerificacion = await _verificadorStock.VerificarYGenerarOrdenesCompraAsync(cancellationToken);
            
            // Registrar las órdenes generadas en el resultado
            foreach (var orden in resultadoVerificacion.OrdenesGeneradas)
            {
                if (orden.Items.Any(i => i.IngredienteId == ingredienteId))
                {
                    resultado.OrdenesCompraGeneradas.Add(orden.Id);
                    
                    // Notificar sobre la orden de compra generada
                    await _servicioNotificaciones.NotificarOrdenCompraGenerada(
                        orden.Id,
                        orden.ProveedorId,
                        "Proveedor"); // Idealmente, obtendríamos el nombre real del proveedor
                }
            }
            
            return resultado;
        }
    }
} 