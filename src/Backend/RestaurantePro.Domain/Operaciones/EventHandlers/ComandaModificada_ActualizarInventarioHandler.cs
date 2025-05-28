namespace RestaurantePro.Domain.Operaciones.EventHandlers
{
    /// <summary>
    /// Manejador de eventos que actualiza el inventario cuando se modifica una comanda.
    /// Este handler maneja eventos de tipo ItemComandaCreado y ProductoEliminadoDeComanda.
    /// </summary>
    public class ComandaModificada_ActualizarInventarioHandler : 
        IDomainEventHandler<Comandas.Events.ItemComanda.ItemComandaCreado>,
        IDomainEventHandler<Comandas.Events.ItemComanda.ProductoEliminadoDeComanda>
    {
        private readonly Services.IOperacionesInventarioIntegrationService _integrationService;
        private readonly IDomainEventRegistry _eventRegistry;
        private readonly IDateTimeService _dateTimeService;

        public ComandaModificada_ActualizarInventarioHandler(
            Services.IOperacionesInventarioIntegrationService integrationService,
            IDomainEventRegistry eventRegistry,
            IDateTimeService dateTimeService)
        {
            _integrationService = integrationService ?? throw new ArgumentNullException(nameof(integrationService));
            _eventRegistry = eventRegistry ?? throw new ArgumentNullException(nameof(eventRegistry));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }

        /// <summary>
        /// Maneja el evento ItemComandaCreado y decrementa el stock de los ingredientes correspondientes.
        /// </summary>
        public async Task Handle(Comandas.Events.ItemComanda.ItemComandaCreado evento, CancellationToken cancellationToken)
        {
            // Registrar el evento para trazabilidad
            await _eventRegistry.RegisterAsync(evento, cancellationToken);

            // Usar el servicio de integración para reservar los ingredientes
            var resultado = await _integrationService.ReservarIngredientesComandaAsync(
                evento.ComandaId, cancellationToken);
                
            if (!resultado.Succeeded)
            {
                // Construir mensaje de error de manera segura
                var errores = ObtenerMensajesError(resultado);
                Console.WriteLine($"Error al reservar ingredientes: {errores}");
            }
            else
            {
                Console.WriteLine($"Ingredientes reservados correctamente para comanda {evento.ComandaId}");
            }
        }

        /// <summary>
        /// Maneja el evento ProductoEliminadoDeComanda e incrementa el stock de los ingredientes correspondientes.
        /// </summary>
        public async Task Handle(Comandas.Events.ItemComanda.ProductoEliminadoDeComanda evento, CancellationToken cancellationToken)
        {
            // Registrar el evento para trazabilidad
            await _eventRegistry.RegisterAsync(evento, cancellationToken);

            // Usar el servicio de integración para liberar los ingredientes
            var resultado = await _integrationService.LiberarReservaIngredientesAsync(
                evento.ComandaId,
                evento.Motivo,
                cancellationToken);
                
            if (!resultado.Succeeded)
            {
                // Construir mensaje de error de manera segura
                var errores = ObtenerMensajesError(resultado);
                Console.WriteLine($"Error al liberar ingredientes: {errores}");
            }
            else
            {
                Console.WriteLine($"Ingredientes liberados correctamente para comanda {evento.ComandaId}");
            }
        }

        /// <summary>
        /// Obtiene los mensajes de error de manera segura desde un Result
        /// </summary>
        private static string ObtenerMensajesError(Result resultado)
        {
            var mensajes = new List<string>();
            
            // Agregar error único si existe
            if (!string.IsNullOrEmpty(resultado.Error))
            {
                mensajes.Add(resultado.Error);
            }
            
            // Agregar errores múltiples si existen
            if (resultado.Errors?.Any() == true)
            {
                mensajes.AddRange(resultado.Errors);
            }
            
            // Si no hay errores específicos, usar mensaje genérico
            if (!mensajes.Any())
            {
                mensajes.Add("Error no especificado");
            }
            
            return string.Join(", ", mensajes);
        }
    }
} 