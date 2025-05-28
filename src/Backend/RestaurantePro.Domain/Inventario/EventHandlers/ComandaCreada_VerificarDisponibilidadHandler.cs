namespace RestaurantePro.Domain.Inventario.EventHandlers
{
    /// <summary>
    /// Manejador de eventos que verifica la disponibilidad de inventario cuando se crea una comanda
    /// No actualiza el inventario, solo verifica que haya stock suficiente
    /// </summary>
    public class ComandaCreada_VerificarDisponibilidadHandler : IDomainEventHandler<ComandaCreada>
    {
        private readonly Operaciones.Services.IOperacionesInventarioIntegrationService _integrationService;
        private readonly IDomainEventRegistry _eventRegistry;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ComandaCreada_VerificarDisponibilidadHandler(
            Operaciones.Services.IOperacionesInventarioIntegrationService integrationService,
            IDomainEventRegistry eventRegistry)
        {
            _integrationService = integrationService ?? throw new ArgumentNullException(nameof(integrationService));
            _eventRegistry = eventRegistry ?? throw new ArgumentNullException(nameof(eventRegistry));
        }
        
        /// <summary>
        /// Maneja el evento de creación de comanda verificando la disponibilidad en inventario
        /// </summary>
        public async Task Handle(ComandaCreada evento, CancellationToken cancellationToken = default)
        {
            try
            {
                string logMessage = $"===== Iniciando verificación de disponibilidad para comanda {evento.ComandaId} =====";
                Console.WriteLine(logMessage);
                
                // Usar el servicio de integración para verificar disponibilidad
                var resultado = await _integrationService.VerificarDisponibilidadIngredientesComandaAsync(
                    evento.ComandaId, 
                    cancellationToken);
                
                if (!resultado.Succeeded)
                {
                    var errores = ObtenerMensajesError(resultado);
                    logMessage = $"Error al verificar disponibilidad: {errores}";
                    Console.WriteLine(logMessage);
                    await _eventRegistry.RegisterAsync(evento, cancellationToken);
                    return;
                }
                
                logMessage = $"Resultado de verificación: TodosDisponibles={resultado.Value.TodosDisponibles}";
                Console.WriteLine(logMessage);
                
                // Si hay ingredientes con stock insuficiente, registrar una advertencia
                if (!resultado.Value.TodosDisponibles)
                {
                    var mensaje = $"Advertencia: stock insuficiente para comanda {evento.ComandaId}:\n";
                    
                    foreach (var item in resultado.Value.IngredientesFaltantes)
                    {
                        mensaje += $"- {item.Key}: Faltante {item.Value}\n";
                    }
                    
                    foreach (var item in resultado.Value.ProductosNoDisponibles)
                    {
                        mensaje += $"- Producto {item.Key}: {item.Value}\n";
                    }
                    
                    logMessage = $"Generando advertencia: {mensaje}";
                    Console.WriteLine(logMessage);
                    await _eventRegistry.RegisterAsync(evento, cancellationToken);
                    logMessage = "Advertencia generada correctamente";
                    Console.WriteLine(logMessage);
                }
                else
                {
                    logMessage = "No se encontraron ingredientes con stock insuficiente";
                    Console.WriteLine(logMessage);
                    // Asegurarnos de registrar siempre el evento, incluso cuando no hay problemas
                    await _eventRegistry.RegisterAsync(evento, cancellationToken);
                }
                
                logMessage = $"===== Finalizada verificación de disponibilidad para comanda {evento.ComandaId} =====";
                Console.WriteLine(logMessage);
            }
            catch (Exception ex)
            {
                string errorMessage = $"ERROR: {ex.Message}";
                Console.WriteLine(errorMessage);
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                await _eventRegistry.RegisterAsync(evento, cancellationToken);
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