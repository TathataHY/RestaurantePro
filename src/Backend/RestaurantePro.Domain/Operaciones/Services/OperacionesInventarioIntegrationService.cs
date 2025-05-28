namespace RestaurantePro.Domain.Operaciones.Services
{
    /// <summary>
    /// Implementación del servicio de integración entre los contextos de Operaciones e Inventario.
    /// Actúa como una capa anticorrupción (ACL) entre ambos contextos.
    /// </summary>
    public class OperacionesInventarioIntegrationService : IOperacionesInventarioIntegrationService
    {
        private readonly Comandas.Interfaces.IComandaRepository _comandaRepository;
        private readonly Inventario.Ingredientes.Interfaces.IIngredienteRepository _ingredienteRepository;
        private readonly Core.Productos.Interfaces.IProductoIngredienteRepository _productoIngredienteRepository;
        private readonly Core.Productos.Interfaces.IProductoRepository _productoRepository;
        private readonly Core.SharedKernel.Validation.INotificationManager _notificationManager;
        private readonly Core.Base.Services.IDateTimeService _dateTimeService;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public OperacionesInventarioIntegrationService(
            Comandas.Interfaces.IComandaRepository comandaRepository,
            Inventario.Ingredientes.Interfaces.IIngredienteRepository ingredienteRepository,
            Core.Productos.Interfaces.IProductoIngredienteRepository productoIngredienteRepository,
            Core.Productos.Interfaces.IProductoRepository productoRepository,
            Core.SharedKernel.Validation.INotificationManager notificationManager,
            Core.Base.Services.IDateTimeService dateTimeService)
        {
            _comandaRepository = comandaRepository ?? throw new ArgumentNullException(nameof(comandaRepository));
            _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
            _productoIngredienteRepository = productoIngredienteRepository ?? throw new ArgumentNullException(nameof(productoIngredienteRepository));
            _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }
        
        /// <inheritdoc/>
        public async Task<Result<DisponibilidadIngredientesResult>> VerificarDisponibilidadIngredientesComandaAsync(
            Guid comandaId,
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            try
            {
                var resultado = new DisponibilidadIngredientesResult();
                
                // Validar el ID de la comanda
                if (comandaId == Guid.Empty)
                {
                    _notificationManager.AddError("El ID de la comanda no puede estar vacío", "ComandaId");
                    return _notificationManager.ToResult<DisponibilidadIngredientesResult>(resultado);
                }
                
                // Obtener la comanda
                var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
                if (comanda == null)
                {
                    _notificationManager.AddError($"No se encontró la comanda con ID {comandaId}", "ComandaId");
                    return _notificationManager.ToResult<DisponibilidadIngredientesResult>(resultado);
                }
                
                // Verificar que la comanda tenga items
                if (comanda.Items == null || !comanda.Items.Any())
                {
                    // Si no hay items, consideramos que todo está disponible
                    return Result.Success(resultado);
                }
                
                // Procesar todos los items de la comanda
                foreach (var item in comanda.Items)
                {
                    // Obtener el producto
                    var producto = await _productoRepository.ObtenerPorIdAsync(item.ProductoId, cancellationToken);
                    if (producto == null)
                    {
                        resultado.TodosDisponibles = false;
                        resultado.ProductosNoDisponibles[item.ProductoId] = "Producto no encontrado";
                        continue;
                    }
                    
                    // Obtener los ingredientes necesarios para el producto
                    var ingredientes = await _ingredienteRepository.ObtenerIngredientesPorProductoAsync(
                        producto.Id, cancellationToken);
                    
                    if (ingredientes == null || !ingredientes.Any())
                    {
                        // Si no hay ingredientes definidos, asumimos que está disponible
                        continue;
                    }
                    
                    // Verificar disponibilidad de cada ingrediente
                    foreach (var ingrediente in ingredientes)
                    {
                        // Obtener la cantidad necesaria
                        var productoIngrediente = await _productoIngredienteRepository.ObtenerPorProductoEIngredienteAsync(
                            producto.Id, ingrediente.Id, cancellationToken);
                            
                        if (productoIngrediente == null)
                        {
                            continue; // Si no está definida la relación, continuamos
                        }
                        
                        // Calcular la cantidad total necesaria
                        decimal cantidadNecesaria = productoIngrediente.Cantidad * item.Cantidad;
                        
                        // Verificar si hay suficiente stock
                        if (ingrediente.Stock < cantidadNecesaria)
                        {
                            resultado.TodosDisponibles = false;
                            resultado.IngredientesFaltantes[ingrediente.Nombre] = cantidadNecesaria - ingrediente.Stock;
                            
                            // Si no hay suficiente stock para este ingrediente, marcar el producto como no disponible
                            if (!resultado.ProductosNoDisponibles.ContainsKey(producto.Id))
                            {
                                resultado.ProductosNoDisponibles[producto.Id] = 
                                    $"Falta ingrediente: {ingrediente.Nombre}";
                            }
                        }
                    }
                }
                
                return Result.Success(resultado);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al verificar disponibilidad: {ex.Message}", "VerificarDisponibilidad");
                return _notificationManager.ToResult<DisponibilidadIngredientesResult>(new DisponibilidadIngredientesResult
                {
                    TodosDisponibles = false
                });
            }
        }
        
        /// <inheritdoc/>
        public async Task<Result<bool>> ReservarIngredientesComandaAsync(
            Guid comandaId,
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            try
            {
                // Validar el ID de la comanda
                if (comandaId == Guid.Empty)
                {
                    _notificationManager.AddError("El ID de la comanda no puede estar vacío", "ComandaId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Obtener la comanda
                var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
                if (comanda == null)
                {
                    _notificationManager.AddError($"No se encontró la comanda con ID {comandaId}", "ComandaId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Verificar que la comanda tenga items
                if (comanda.Items == null || !comanda.Items.Any())
                {
                    return Result.Success(true); // No hay nada que reservar
                }
                
                // Primero verificamos que haya suficiente stock disponible
                var verificacionResult = await VerificarDisponibilidadIngredientesComandaAsync(comandaId, cancellationToken);
                if (!verificacionResult.Succeeded || !verificacionResult.Value.TodosDisponibles)
                {
                    if (verificacionResult.Value != null)
                    {
                        foreach (var item in verificacionResult.Value.ProductosNoDisponibles)
                        {
                            _notificationManager.AddError($"Producto {item.Key}: {item.Value}", "Producto");
                        }
                        
                        foreach (var item in verificacionResult.Value.IngredientesFaltantes)
                        {
                            _notificationManager.AddError($"Ingrediente {item.Key}: Falta {item.Value}", "Ingrediente");
                        }
                    }
                    
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Procesamos cada item de la comanda
                foreach (var item in comanda.Items)
                {
                    var producto = await _productoRepository.ObtenerPorIdAsync(item.ProductoId, cancellationToken);
                    if (producto == null) continue;
                    
                    var ingredientes = await _ingredienteRepository.ObtenerIngredientesPorProductoAsync(
                        producto.Id, cancellationToken);
                    
                    if (ingredientes == null || !ingredientes.Any()) continue;
                    
                    // Reservar cada ingrediente
                    foreach (var ingrediente in ingredientes)
                    {
                        var productoIngrediente = await _productoIngredienteRepository.ObtenerPorProductoEIngredienteAsync(
                            producto.Id, ingrediente.Id, cancellationToken);
                            
                        if (productoIngrediente == null) continue;
                        
                        decimal cantidadNecesaria = productoIngrediente.Cantidad * item.Cantidad;
                        
                        // Decrementar el stock (reservar)
                        ingrediente.DecrementarStock(cantidadNecesaria, $"Reserva para comanda #{comandaId}");
                        
                        // Guardar cambios
                        await _ingredienteRepository.ActualizarAsync(ingrediente, cancellationToken);
                    }
                }
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al reservar ingredientes: {ex.Message}", "ReservarIngredientes");
                return _notificationManager.ToResult<bool>(false);
            }
        }
        
        /// <inheritdoc/>
        public async Task<Result<bool>> ConfirmarConsumoIngredientesAsync(
            Guid comandaId,
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            try
            {
                // En la implementación actual, cuando reservamos los ingredientes ya decrementamos el stock,
                // por lo que confirmar el consumo no requiere acciones adicionales en el inventario.
                // Sin embargo, podríamos registrar el consumo para fines de auditoría o estadísticas.
                
                // Validar el ID de la comanda
                if (comandaId == Guid.Empty)
                {
                    _notificationManager.AddError("El ID de la comanda no puede estar vacío", "ComandaId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Obtener la comanda
                var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
                if (comanda == null)
                {
                    _notificationManager.AddError($"No se encontró la comanda con ID {comandaId}", "ComandaId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // En una implementación más avanzada, podríamos registrar el consumo definitivo
                // y liberar cualquier cantidad adicional que se haya reservado pero no utilizado.
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al confirmar consumo: {ex.Message}", "ConfirmarConsumo");
                return _notificationManager.ToResult<bool>(false);
            }
        }
        
        /// <inheritdoc/>
        public async Task<Result<bool>> LiberarReservaIngredientesAsync(
            Guid comandaId,
            string motivo,
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            try
            {
                // Validar el ID de la comanda
                if (comandaId == Guid.Empty)
                {
                    _notificationManager.AddError("El ID de la comanda no puede estar vacío", "ComandaId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Obtener la comanda
                var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
                if (comanda == null)
                {
                    _notificationManager.AddError($"No se encontró la comanda con ID {comandaId}", "ComandaId");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // Verificar que la comanda tenga items
                if (comanda.Items == null || !comanda.Items.Any())
                {
                    return Result.Success(true); // No hay nada que liberar
                }
                
                // Procesamos cada item de la comanda
                foreach (var item in comanda.Items)
                {
                    var producto = await _productoRepository.ObtenerPorIdAsync(item.ProductoId, cancellationToken);
                    if (producto == null) continue;
                    
                    var ingredientes = await _ingredienteRepository.ObtenerIngredientesPorProductoAsync(
                        producto.Id, cancellationToken);
                    
                    if (ingredientes == null || !ingredientes.Any()) continue;
                    
                    // Liberar cada ingrediente
                    foreach (var ingrediente in ingredientes)
                    {
                        var productoIngrediente = await _productoIngredienteRepository.ObtenerPorProductoEIngredienteAsync(
                            producto.Id, ingrediente.Id, cancellationToken);
                            
                        if (productoIngrediente == null) continue;
                        
                        decimal cantidadNecesaria = productoIngrediente.Cantidad * item.Cantidad;
                        
                        // Incrementar el stock (liberar)
                        ingrediente.IncrementarStock(cantidadNecesaria, $"Liberación de reserva comanda #{comandaId}. Motivo: {motivo}");
                        
                        // Guardar cambios
                        await _ingredienteRepository.ActualizarAsync(ingrediente, cancellationToken);
                    }
                }
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al liberar reserva: {ex.Message}", "LiberarReserva");
                return _notificationManager.ToResult<bool>(false);
            }
        }
    }
} 