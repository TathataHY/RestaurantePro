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
        private readonly Inventario.Ingredientes.Movimientos.Interfaces.IMovimientoInventarioRepository _movimientoRepository;
        private readonly Core.Productos.Services.IRecetaService _recetaService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ILogger<OperacionesInventarioIntegrationService> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public OperacionesInventarioIntegrationService(
            Comandas.Interfaces.IComandaRepository comandaRepository,
            Inventario.Ingredientes.Interfaces.IIngredienteRepository ingredienteRepository,
            Inventario.Ingredientes.Movimientos.Interfaces.IMovimientoInventarioRepository movimientoRepository,
            Core.Productos.Services.IRecetaService recetaService,
            IDateTimeService dateTimeService,
            ILogger<OperacionesInventarioIntegrationService> logger)
        {
            _comandaRepository = comandaRepository ?? throw new ArgumentNullException(nameof(comandaRepository));
            _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
            _movimientoRepository = movimientoRepository ?? throw new ArgumentNullException(nameof(movimientoRepository));
            _recetaService = recetaService ?? throw new ArgumentNullException(nameof(recetaService));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<Result<Results.DisponibilidadIngredientesResult>> VerificarDisponibilidadIngredientesComandaAsync(
            Guid comandaId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Verificando disponibilidad de ingredientes para comanda {ComandaId}", comandaId);
                
                // Obtener la comanda
                var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
                if (comanda == null)
                {
                    return Result.Failure<Results.DisponibilidadIngredientesResult>(
                        $"No se encontró la comanda con ID {comandaId}");
                }
                
                var resultado = new Results.DisponibilidadIngredientesResult
                {
                    TodosDisponibles = true
                };
                
                // Verificar disponibilidad para cada producto en la comanda
                foreach (var item in comanda.Items)
                {
                    // Obtener la receta del producto
                    var disponibilidadReceta = await _recetaService.VerificarDisponibilidadIngredientesAsync(
                        item.ProductoId, 
                        item.Cantidad, 
                        cancellationToken);
                        
                    if (!disponibilidadReceta.Succeeded)
                    {
                        _logger.LogWarning("Error al verificar disponibilidad para producto {ProductoId}: {Error}", 
                            item.ProductoId, disponibilidadReceta.Error);
                            
                        resultado.TodosDisponibles = false;
                        resultado.ProductosNoDisponibles.Add(item.ProductoId, 
                            disponibilidadReceta.Error ?? "Error desconocido");
                        continue;
                    }
                    
                    if (!disponibilidadReceta.Value)
                    {
                        resultado.TodosDisponibles = false;
                        
                        // Obtener ingredientes faltantes
                        var ingredientesFaltantes = await _recetaService.ObtenerIngredientesFaltantesAsync(
                            item.ProductoId, 
                            item.Cantidad, 
                            cancellationToken);
                            
                        if (ingredientesFaltantes.Succeeded)
                        {
                            foreach (var faltante in ingredientesFaltantes.Value)
                            {
                                var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(faltante.Key, cancellationToken);
                                
                                string nombreIngrediente = ingrediente?.Nombre ?? faltante.Key.ToString();
                                
                                if (resultado.IngredientesFaltantes.ContainsKey(nombreIngrediente))
                                {
                                    resultado.IngredientesFaltantes[nombreIngrediente] += faltante.Value;
                                }
                                else
                                {
                                    resultado.IngredientesFaltantes.Add(nombreIngrediente, faltante.Value);
                                }
                            }
                            
                            // Agregar razón de no disponibilidad
                            var razon = string.Join(", ", ingredientesFaltantes.Value.Select(kv => {
                                var ingrediente = _ingredienteRepository.ObtenerPorIdAsync(kv.Key, cancellationToken).Result;
                                return $"{ingrediente?.Nombre ?? kv.Key.ToString()}: {kv.Value}";
                            }));
                            
                            resultado.ProductosNoDisponibles.Add(item.ProductoId, $"Falta ingrediente: {razon}");
                        }
                        else
                        {
                            resultado.ProductosNoDisponibles.Add(item.ProductoId, "No se pudo determinar ingredientes faltantes");
                        }
                    }
                }
                
                _logger.LogInformation("Verificación completada para comanda {ComandaId}. Disponibilidad: {Disponibilidad}", 
                    comandaId, resultado.TodosDisponibles);
                    
                return Result.Success(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar disponibilidad para comanda {ComandaId}", comandaId);
                return Result.Failure<Results.DisponibilidadIngredientesResult>(
                    $"Error al verificar disponibilidad: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public async Task<Result<bool>> ReservarIngredientesComandaAsync(
            Guid comandaId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Reservando ingredientes para comanda {ComandaId}", comandaId);
                
                // Obtener la comanda
                var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
                if (comanda == null)
                {
                    return Result.Failure<bool>(
                        $"No se encontró la comanda con ID {comandaId}");
                }
                
                // Verificar disponibilidad primero
                var verificacion = await VerificarDisponibilidadIngredientesComandaAsync(comandaId, cancellationToken);
                if (!verificacion.Succeeded)
                {
                    return Result.Failure<bool>(verificacion.Error);
                }
                
                if (!verificacion.Value.TodosDisponibles)
                {
                    return Result.Failure<bool>(
                        "No hay suficiente stock para reservar los ingredientes");
                }
                
                // Crear movimientos de reserva para cada producto
                foreach (var item in comanda.Items)
                {
                    // Obtener ingredientes requeridos para el producto
                    var ingredientes = await _recetaService.ObtenerIngredientesParaProductoAsync(item.ProductoId, cancellationToken);
                    if (!ingredientes.Succeeded || ingredientes.Value == null)
                    {
                        _logger.LogWarning("No se encontraron ingredientes para producto {ProductoId}", item.ProductoId);
                        continue;
                    }
                    
                    foreach (var ingredienteReceta in ingredientes.Value)
                    {
                        // Obtener el ingrediente
                        var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteReceta.Key, cancellationToken);
                            
                        if (ingrediente == null)
                        {
                            _logger.LogWarning("No se encontró ingrediente con ID {IngredienteId}", ingredienteReceta.Key);
                            continue;
                        }
                        
                        // Calcular cantidad a reservar
                        var cantidadRequerida = ingredienteReceta.Value * item.Cantidad;
                        
                        // Crear movimiento de salida (egreso)
                        var motivo = $"Reserva para comanda {comandaId}, ítem {item.Id}";
                        var movimiento = ingrediente.DecrementarStock(cantidadRequerida, motivo);
                            
                        await _movimientoRepository.AgregarAsync(movimiento, cancellationToken);
                        
                        // El stock ya se ha actualizado dentro del método DecrementarStock
                        await _ingredienteRepository.ActualizarAsync(ingrediente, cancellationToken);
                    }
                }
                
                _logger.LogInformation("Ingredientes reservados correctamente para comanda {ComandaId}", comandaId);
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reservar ingredientes para comanda {ComandaId}", comandaId);
                return Result.Failure<bool>(
                    $"Error al reservar ingredientes: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public async Task<Result<bool>> ConfirmarConsumoIngredientesAsync(
            Guid comandaId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Confirmando consumo de ingredientes para comanda {ComandaId}", comandaId);
                
                // Obtener la comanda
                var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
                if (comanda == null)
                {
                    return Result.Failure<bool>(
                        $"No se encontró la comanda con ID {comandaId}");
                }
                
                // Crear movimientos de consumo para cada producto entregado
                // Nota: Para los productos entregados, los ingredientes ya se reservaron previamente,
                // así que no necesitamos reducir el stock nuevamente, solo registrar un movimiento informativo
                foreach (var item in comanda.Items.Where(i => i.Estado == Comandas.Enums.EstadoItemComanda.Entregado))
                {
                    // Obtener ingredientes requeridos para el producto
                    var ingredientes = await _recetaService.ObtenerIngredientesParaProductoAsync(item.ProductoId, cancellationToken);
                    if (!ingredientes.Succeeded || ingredientes.Value == null)
                    {
                        _logger.LogWarning("No se encontraron ingredientes para producto {ProductoId}", item.ProductoId);
                        continue;
                    }
                    
                    foreach (var ingredienteReceta in ingredientes.Value)
                    {
                        // Obtener el ingrediente
                        var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteReceta.Key, cancellationToken);
                            
                        if (ingrediente == null)
                        {
                            _logger.LogWarning("No se encontró ingrediente con ID {IngredienteId}", ingredienteReceta.Key);
                            continue;
                        }
                        
                        // Calcular cantidad consumida
                        var cantidadConsumida = ingredienteReceta.Value * item.Cantidad;
                        
                        // Crear un movimiento informativo de consumo sin afectar el stock
                        var motivo = $"Consumo por comanda {comandaId}, ítem {item.Id}";
                        var movimiento = Inventario.Ingredientes.Movimientos.Entities.MovimientoInventario.CrearEgreso(
                            ingrediente.Id, cantidadConsumida, motivo, _dateTimeService.Now);
                            
                        await _movimientoRepository.AgregarAsync(movimiento, cancellationToken);
                        
                        // No actualizamos el stock porque ya se hizo en la reserva
                    }
                }
                
                _logger.LogInformation("Consumo de ingredientes confirmado para comanda {ComandaId}", comandaId);
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar consumo de ingredientes para comanda {ComandaId}", comandaId);
                return Result.Failure<bool>(
                    $"Error al confirmar consumo: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public async Task<Result<bool>> LiberarReservaIngredientesAsync(
            Guid comandaId, 
            string motivo, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Liberando reserva de ingredientes para comanda {ComandaId}", comandaId);
                
                // Obtener la comanda
                var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
                if (comanda == null)
                {
                    return Result.Failure<bool>(
                        $"No se encontró la comanda con ID {comandaId}");
                }
                
                // Crear movimientos de liberación para cada producto
                foreach (var item in comanda.Items)
                {
                    // Obtener ingredientes requeridos para el producto
                    var ingredientes = await _recetaService.ObtenerIngredientesParaProductoAsync(item.ProductoId, cancellationToken);
                    if (!ingredientes.Succeeded || ingredientes.Value == null)
                    {
                        _logger.LogWarning("No se encontraron ingredientes para producto {ProductoId}", item.ProductoId);
                        continue;
                    }
                    
                    foreach (var ingredienteReceta in ingredientes.Value)
                    {
                        // Obtener el ingrediente
                        var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteReceta.Key, cancellationToken);
                            
                        if (ingrediente == null)
                        {
                            _logger.LogWarning("No se encontró ingrediente con ID {IngredienteId}", ingredienteReceta.Key);
                            continue;
                        }
                        
                        // Calcular cantidad a liberar
                        var cantidadLiberada = ingredienteReceta.Value * item.Cantidad;
                        
                        // Crear movimiento de entrada (ingreso) y actualizar el stock
                        var descripcionMotivo = $"Liberación para comanda {comandaId}, ítem {item.Id}. Motivo: {motivo}";
                        var movimiento = ingrediente.IncrementarStock(cantidadLiberada, descripcionMotivo);
                            
                        await _movimientoRepository.AgregarAsync(movimiento, cancellationToken);
                        
                        // El stock ya se ha actualizado dentro del método IncrementarStock
                        await _ingredienteRepository.ActualizarAsync(ingrediente, cancellationToken);
                    }
                }
                
                _logger.LogInformation("Reserva de ingredientes liberada para comanda {ComandaId}", comandaId);
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al liberar reserva de ingredientes para comanda {ComandaId}", comandaId);
                return Result.Failure<bool>(
                    $"Error al liberar reserva: {ex.Message}");
            }
        }
    }
} 