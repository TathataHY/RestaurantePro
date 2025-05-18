namespace RestaurantePro.Domain.Inventario.EventHandlers
{
    /// <summary>
    /// Manejador de eventos que verifica la disponibilidad de inventario cuando se crea una comanda
    /// No actualiza el inventario, solo verifica que haya stock suficiente
    /// </summary>
    public class ComandaCreada_VerificarDisponibilidadHandler : IDomainEventHandler<ComandaCreada>
    {
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly IComandaRepository _comandaRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IProductoIngredienteRepository _productoIngredienteRepository;
        private readonly IDomainEventLog _eventLog;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ComandaCreada_VerificarDisponibilidadHandler(
            IIngredienteRepository ingredienteRepository,
            IComandaRepository comandaRepository,
            IProductoRepository productoRepository,
            IProductoIngredienteRepository productoIngredienteRepository,
            IDomainEventLog eventLog)
        {
            _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
            _comandaRepository = comandaRepository ?? throw new ArgumentNullException(nameof(comandaRepository));
            _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
            _productoIngredienteRepository = productoIngredienteRepository ?? throw new ArgumentNullException(nameof(productoIngredienteRepository));
            _eventLog = eventLog ?? throw new ArgumentNullException(nameof(eventLog));
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
                
                // Obtener la comanda completa
                var comanda = await _comandaRepository.ObtenerPorIdAsync(evento.ComandaId, cancellationToken);
                if (comanda == null)
                {
                    string errorMsg = $"No se encontró la comanda con ID {evento.ComandaId}";
                    await _eventLog.LogEvent(evento, errorMsg, cancellationToken);
                    return;
                }
                
                logMessage = $"Comanda encontrada: ID={comanda.Id}, Items: {comanda.Items?.Count ?? 0}";
                Console.WriteLine(logMessage);
                
                // Verificar que la comanda tenga items
                if (comanda.Items == null || !comanda.Items.Any())
                {
                    logMessage = "La comanda no tiene items, nada que verificar";
                    Console.WriteLine(logMessage);
                    return;
                }
                
                // Lista para almacenar cualquier ingrediente con stock insuficiente
                var ingredientesInsuficientes = new List<(string Nombre, decimal StockActual, decimal StockNecesario)>();
                
                // Procesar todos los items de la comanda
                foreach (var item in comanda.Items)
                {
                    logMessage = $"Verificando item: ProductoId={item.ProductoId}, Cantidad={item.Cantidad}";
                    Console.WriteLine(logMessage);
                    
                    // Obtener el producto
                    var producto = await _productoRepository.ObtenerPorIdAsync(item.ProductoId, cancellationToken);
                    if (producto == null)
                    {
                        string errorMsg = $"No se encontró el producto con ID {item.ProductoId}";
                        await _eventLog.LogEvent(evento, errorMsg, cancellationToken);
                        continue;
                    }
                    
                    logMessage = $"Producto encontrado: {producto.Nombre} (ID={producto.Id})";
                    Console.WriteLine(logMessage);
                    
                    // Obtener los ingredientes necesarios para el producto
                    Console.WriteLine($"Buscando ingredientes para el producto {producto.Id}...");
                    var ingredientes = await _ingredienteRepository.ObtenerIngredientesPorProductoAsync(producto.Id, cancellationToken);
                    
                    Console.WriteLine($"Resultado de ObtenerIngredientesPorProductoAsync: {(ingredientes == null ? "NULL" : ingredientes.Count().ToString())} ingredientes");
                    
                    if (ingredientes == null || !ingredientes.Any())
                    {
                        logMessage = $"No se encontraron ingredientes para el producto {producto.Nombre}";
                        Console.WriteLine(logMessage);
                        await _eventLog.LogEvent(evento, logMessage, cancellationToken);
                        continue;
                    }
                    
                    logMessage = $"Se encontraron {ingredientes.Count()} ingredientes para el producto {producto.Nombre}";
                    Console.WriteLine(logMessage);
                    
                    // Verificar el stock disponible para cada ingrediente
                    foreach (var ingrediente in ingredientes)
                    {
                        logMessage = $"Verificando ingrediente: {ingrediente.Nombre} (ID={ingrediente.Id}), Stock={ingrediente.Stock}";
                        Console.WriteLine(logMessage);
                        
                        // Obtener la cantidad necesaria del ingrediente por unidad de producto
                        var productoIngrediente = await _productoIngredienteRepository.ObtenerPorProductoEIngredienteAsync(
                            producto.Id, ingrediente.Id, cancellationToken);
                            
                        if (productoIngrediente == null)
                        {
                            logMessage = $"No se encontró la relación producto-ingrediente para {producto.Id} e {ingrediente.Id}";
                            Console.WriteLine(logMessage);
                            await _eventLog.LogEvent(evento, logMessage, cancellationToken);
                            continue;
                        }
                        
                        // Calcular la cantidad total necesaria
                        decimal cantidadNecesaria = productoIngrediente.Cantidad * item.Cantidad;
                        logMessage = $"Cantidad necesaria: {cantidadNecesaria}, Stock disponible: {ingrediente.Stock}";
                        Console.WriteLine(logMessage);
                        
                        // Verificar si hay suficiente stock
                        if (ingrediente.Stock < cantidadNecesaria)
                        {
                            logMessage = $"¡STOCK INSUFICIENTE! Añadiendo a la lista: {ingrediente.Nombre} - Necesario: {cantidadNecesaria}, Disponible: {ingrediente.Stock}";
                            Console.WriteLine(logMessage);
                            ingredientesInsuficientes.Add((ingrediente.Nombre, ingrediente.Stock, cantidadNecesaria));
                        }
                        else
                        {
                            logMessage = $"Stock suficiente para {ingrediente.Nombre}";
                            Console.WriteLine(logMessage);
                        }
                    }
                }
                
                logMessage = $"Se encontraron {ingredientesInsuficientes.Count} ingredientes con stock insuficiente";
                Console.WriteLine(logMessage);
                
                // Si hay ingredientes con stock insuficiente, registrar una advertencia
                if (ingredientesInsuficientes.Any())
                {
                    var mensaje = $"Advertencia: stock insuficiente para comanda {evento.ComandaId}:\n";
                    foreach (var item in ingredientesInsuficientes)
                    {
                        mensaje += $"- {item.Nombre}: Stock actual {item.StockActual}, Necesario {item.StockNecesario}\n";
                    }
                    
                    logMessage = $"Generando advertencia: {mensaje}";
                    Console.WriteLine(logMessage);
                    await _eventLog.LogEvent(evento, mensaje, cancellationToken);
                    logMessage = "Advertencia generada correctamente";
                    Console.WriteLine(logMessage);
                }
                else
                {
                    logMessage = "No se encontraron ingredientes con stock insuficiente";
                    Console.WriteLine(logMessage);
                }
                
                logMessage = $"===== Finalizada verificación de disponibilidad para comanda {evento.ComandaId} =====";
                Console.WriteLine(logMessage);
            }
            catch (Exception ex)
            {
                string errorMessage = $"ERROR: {ex.Message}";
                Console.WriteLine(errorMessage);
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                await _eventLog.LogEvent(evento, $"Error al verificar inventario: {ex.Message}", cancellationToken);
            }
        }
    }
} 