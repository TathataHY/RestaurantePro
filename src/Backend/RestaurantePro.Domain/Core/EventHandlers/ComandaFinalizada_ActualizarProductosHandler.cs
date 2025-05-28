namespace RestaurantePro.Domain.Core.EventHandlers
{
    /// <summary>
    /// Manejador para el evento ComandaFinalizada que actualiza la información de productos en el contexto Core
    /// </summary>
    public class ComandaFinalizada_ActualizarProductosHandler 
        : IDomainEventHandler<Operaciones.Comandas.Events.Comanda.ComandaFinalizada>
    {
        private readonly Core.Services.ICoreOperacionesIntegrationService _integrationService;
        private readonly ILogger<ComandaFinalizada_ActualizarProductosHandler> _logger;
        
        /// <summary>
        /// Constructor con inyección de dependencias
        /// </summary>
        public ComandaFinalizada_ActualizarProductosHandler(
            Core.Services.ICoreOperacionesIntegrationService integrationService,
            ILogger<ComandaFinalizada_ActualizarProductosHandler> logger)
        {
            _integrationService = integrationService ?? throw new ArgumentNullException(nameof(integrationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Maneja el evento ComandaFinalizada
        /// </summary>
        /// <param name="evento">Evento con la información de la comanda finalizada</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        public async Task Handle(Operaciones.Comandas.Events.Comanda.ComandaFinalizada evento, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Procesando comanda finalizada {ComandaId} en contexto Core", evento.ComandaId);
                
                // Convertir los items de comanda a un diccionario de producto-cantidad
                var productosIdCantidad = new Dictionary<Guid, int>();
                
                foreach (var item in evento.Items)
                {
                    if (productosIdCantidad.ContainsKey(item.ProductoId))
                    {
                        productosIdCantidad[item.ProductoId] += item.Cantidad;
                    }
                    else
                    {
                        productosIdCantidad[item.ProductoId] = item.Cantidad;
                    }
                }
                
                // Procesar la comanda en el servicio de integración
                var result = await _integrationService.ProcesarComandaFinalizadaAsync(
                    evento.ComandaId, 
                    productosIdCantidad, 
                    cancellationToken);
                
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Error al procesar comanda finalizada {ComandaId} en contexto Core: {Errors}",
                        evento.ComandaId,
                        string.Join(", ", result.Errors));
                }
                else
                {
                    _logger.LogInformation("Comanda finalizada {ComandaId} procesada correctamente en contexto Core", 
                        evento.ComandaId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado al procesar comanda finalizada {ComandaId} en contexto Core",
                    evento.ComandaId);
            }
        }
    }
} 