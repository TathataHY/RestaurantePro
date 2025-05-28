namespace RestaurantePro.Domain.Comercial.EventHandlers
{
    /// <summary>
    /// Manejador para el evento ProveedorActualizado que sincroniza la información del proveedor
    /// entre los contextos de Proveedores y Comercial.
    /// </summary>
    public class ProveedorActualizado_SincronizarInformacionHandler 
        : IDomainEventHandler<ProveedorActualizado>
    {
        private readonly IServicioIntegracionProveedores _servicioIntegracion;
        private readonly ILogger<ProveedorActualizado_SincronizarInformacionHandler> _logger;
        
        /// <summary>
        /// Constructor con inyección de dependencias
        /// </summary>
        public ProveedorActualizado_SincronizarInformacionHandler(
            IServicioIntegracionProveedores servicioIntegracion,
            ILogger<ProveedorActualizado_SincronizarInformacionHandler> logger)
        {
            _servicioIntegracion = servicioIntegracion ?? throw new ArgumentNullException(nameof(servicioIntegracion));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Maneja el evento de proveedor actualizado
        /// </summary>
        /// <param name="evento">Evento con la información del proveedor actualizado</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        public async Task Handle(ProveedorActualizado evento, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Sincronizando información del proveedor {ProveedorId}", evento.ProveedorId);
                
                // Llamar al servicio de integración para sincronizar la información
                var result = await _servicioIntegracion.SincronizarInformacionProveedorAsync(
                    evento.ProveedorId, 
                    cancellationToken);
                
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Error al sincronizar información del proveedor {ProveedorId}: {Errors}",
                        evento.ProveedorId,
                        string.Join(", ", result.Errors));
                }
                else
                {
                    _logger.LogInformation("Información del proveedor {ProveedorId} sincronizada correctamente", 
                        evento.ProveedorId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado al sincronizar información del proveedor {ProveedorId}",
                    evento.ProveedorId);
            }
        }
    }
} 