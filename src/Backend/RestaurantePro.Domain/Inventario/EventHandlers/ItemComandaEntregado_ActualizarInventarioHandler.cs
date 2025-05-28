namespace RestaurantePro.Domain.Inventario.EventHandlers
{
    /// <summary>
    /// Manejador que actualiza el inventario cuando un ítem de comanda se marca como entregado
    /// Este enfoque permite actualizar el inventario en tiempo real, cuando el ítem se entrega al cliente
    /// en lugar de esperar a que toda la comanda sea finalizada
    /// </summary>
    public class ItemComandaEntregado_ActualizarInventarioHandler : IDomainEventHandler<ItemComandaEntregado>
    {
        private readonly Operaciones.Services.IOperacionesInventarioIntegrationService _integrationService;
        private readonly IDomainEventRegistry _eventRegistry;
        
        public ItemComandaEntregado_ActualizarInventarioHandler(
            Operaciones.Services.IOperacionesInventarioIntegrationService integrationService,
            IDomainEventRegistry eventRegistry)
        {
            _integrationService = integrationService ?? throw new ArgumentNullException(nameof(integrationService));
            _eventRegistry = eventRegistry ?? throw new ArgumentNullException(nameof(eventRegistry));
        }
        
        /// <summary>
        /// Maneja el evento actualizando el inventario cuando se entrega un ítem
        /// </summary>
        public async Task Handle(ItemComandaEntregado evento, CancellationToken cancellationToken = default)
        {
            try
            {
                // Usar el servicio de integración para confirmar el consumo de ingredientes
                var resultado = await _integrationService.ConfirmarConsumoIngredientesAsync(
                    evento.ComandaId, cancellationToken);
                    
                if (!resultado.Succeeded)
                {
                    Console.WriteLine($"Error al confirmar consumo de ingredientes: {string.Join(", ", resultado.Errors)}");
                }
                else
                {
                    Console.WriteLine($"Consumo de ingredientes confirmado correctamente para comanda {evento.ComandaId}");
                }
                
                // Registrar el evento para trazabilidad
                await _eventRegistry.RegisterAsync(evento, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar inventario: {ex.Message}");
                await _eventRegistry.RegisterAsync(evento, cancellationToken);
            }
        }
    }
}