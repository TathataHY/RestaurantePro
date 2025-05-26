namespace RestaurantePro.Domain.Inventario.Services
{
    /// <summary>
    /// Clase que representa el resultado de la verificación de stock
    /// </summary>
    public class ResultadoVerificacionStock
    {
        /// <summary>
        /// Órdenes de compra generadas durante la verificación
        /// </summary>
        public List<OrdenCompra> OrdenesGeneradas { get; } = new List<OrdenCompra>();
        
        /// <summary>
        /// Órdenes de compra existentes que fueron actualizadas
        /// </summary>
        public List<OrdenCompra> OrdenesActualizadas { get; } = new List<OrdenCompra>();
        
        /// <summary>
        /// Errores ocurridos durante la verificación
        /// </summary>
        public List<string> Errores { get; } = new List<string>();
    }
    
    /// <summary>
    /// Servicio de dominio que verifica el stock de ingredientes y genera órdenes de compra automáticas
    /// </summary>
    public class VerificadorStock : IVerificadorStock
    {
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly IOrdenCompraRepository _ordenCompraRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly INotificationManager _notificationManager;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public VerificadorStock(
            IIngredienteRepository ingredienteRepository,
            IOrdenCompraRepository ordenCompraRepository,
            IProveedorRepository proveedorRepository,
            IDateTimeService dateTimeService,
            INotificationManager notificationManager)
        {
            _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
            _ordenCompraRepository = ordenCompraRepository ?? throw new ArgumentNullException(nameof(ordenCompraRepository));
            _proveedorRepository = proveedorRepository ?? throw new ArgumentNullException(nameof(proveedorRepository));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
        }
        
        /// <summary>
        /// Verifica los ingredientes con stock bajo y genera órdenes de compra automáticas
        /// </summary>
        /// <returns>Resultado con la información de órdenes generadas y errores</returns>
        public async Task<Result<ResultadoVerificacionStock>> VerificarYGenerarOrdenesCompraAsync(CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            var resultado = new ResultadoVerificacionStock();
            
            // Obtener ingredientes con stock bajo
            var ingredientesBajoStock = await _ingredienteRepository.ObtenerConStockBajoAsync(cancellationToken);
            
            if (ingredientesBajoStock == null || !ingredientesBajoStock.Any())
            {
                return Result.Success(resultado); // No hay ingredientes con stock bajo
            }
            
            // Agrupar ingredientes por proveedor
            var ingredientesConProveedor = ingredientesBajoStock
                .Where(i => i != null && i.ProveedorPrincipalId.HasValue)
                .ToList();
                
            if (ingredientesConProveedor == null || !ingredientesConProveedor.Any())
            {
                _notificationManager.AddError("No hay ingredientes con proveedor principal asignado", "Ingredientes");
                return _notificationManager.ToResult(resultado);
            }
                
            var ingredientesPorProveedor = ingredientesConProveedor
                .GroupBy(i => i.ProveedorPrincipalId.Value)
                .ToDictionary(g => g.Key, g => g.ToList());
                
            // Procesar cada grupo de ingredientes por proveedor
            if (ingredientesPorProveedor != null && ingredientesPorProveedor.Count > 0)
            {
                foreach (var kvp in ingredientesPorProveedor)
                {
                    var proveedorId = kvp.Key;
                    var ingredientes = kvp.Value ?? new List<Ingrediente>();
                    
                    if (ingredientes.Count == 0)
                    {
                        continue;
                    }
                    
                    // Obtener el proveedor
                    var proveedor = await _proveedorRepository.ObtenerPorIdAsync(proveedorId, cancellationToken);
                    
                    if (proveedor == null)
                    {
                        resultado.Errores.Add($"No se encontró el proveedor con ID {proveedorId}");
                        _notificationManager.AddError($"No se encontró el proveedor con ID {proveedorId}", "Proveedor");
                        continue;
                    }
                    
                    // Verificar si el proveedor está activo
                    if (!proveedor.EstaActivo)
                    {
                        resultado.Errores.Add($"El proveedor {proveedor.Nombre} (ID: {proveedorId}) no está activo");
                        _notificationManager.AddError($"El proveedor {proveedor.Nombre} (ID: {proveedorId}) no está activo", "Proveedor");
                        continue;
                    }
                    
                    // Verificar si ya existe una orden pendiente para este proveedor
                    var ordenesPendientes = await _ordenCompraRepository.ObtenerPendientesPorProveedorAsync(proveedorId, cancellationToken);
                    
                    if (ordenesPendientes != null && ordenesPendientes.Any())
                    {
                        // Actualizar orden existente en lugar de crear una nueva
                        var ordenExistente = ordenesPendientes.First();
                        var resultadoActualizacion = ActualizarOrdenExistente(ordenExistente, ingredientes, resultado);
                        
                        if (resultadoActualizacion.Succeeded && !resultadoActualizacion.Value)
                        {
                            _notificationManager.AddError($"No se pudieron agregar ingredientes a la orden existente para el proveedor {proveedor.Nombre}", "ActualizacionOrden");
                        }
                    }
                    else
                    {
                        // Crear nueva orden de compra
                        var resultadoCreacion = CrearNuevaOrden(proveedor, ingredientes, resultado);
                        
                        if (!resultadoCreacion.Succeeded || !resultadoCreacion.Value)
                        {
                            _notificationManager.AddError($"No se pudo crear la orden para el proveedor {proveedor.Nombre}", "CreacionOrden");
                        }
                    }
                }
            }
            
            // Guardar las órdenes generadas y actualizadas
            try 
            {
                foreach (var orden in resultado.OrdenesGeneradas)
                {
                    await _ordenCompraRepository.AgregarAsync(orden, cancellationToken);
                }
                
                foreach (var orden in resultado.OrdenesActualizadas)
                {
                    await _ordenCompraRepository.ActualizarAsync(orden, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                resultado.Errores.Add($"Error al guardar órdenes: {ex.Message}");
                _notificationManager.AddError($"Error al guardar órdenes: {ex.Message}", "Persistencia");
            }
            
            // Si hay errores, los agregamos a las notificaciones
            foreach (var error in resultado.Errores)
            {
                _notificationManager.AddError(error, "VerificacionStock");
            }
            
            return _notificationManager.HasErrors
                ? _notificationManager.ToResult(resultado)
                : Result.Success(resultado);
        }
        
        /// <summary>
        /// Actualiza una orden de compra existente con nuevos ingredientes
        /// </summary>
        /// <returns>True si se agregaron ingredientes a la orden, False en caso contrario</returns>
        private Result<bool> ActualizarOrdenExistente(OrdenCompra orden, List<Ingrediente> ingredientes, ResultadoVerificacionStock resultado)
        {
            _notificationManager.RequireNotNull(orden, "Orden de compra no puede ser nula", "OrdenCompra");
            _notificationManager.RequireNotNull(ingredientes, "La lista de ingredientes no puede ser nula", "Ingredientes");
            _notificationManager.RequireNotNull(resultado, "El resultado de verificación no puede ser nulo", "Resultado");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
                
            var ingredientesAgregados = false;
            
            foreach (var ingrediente in ingredientes)
            {
                if (ingrediente == null)
                {
                    _notificationManager.AddError($"Ingrediente no puede ser nulo", "Ingrediente");
                    continue;
                }
                    
                // Verificar si el ingrediente ya está en la orden
                if (orden.Items.Any(i => i.IngredienteId == ingrediente.Id))
                {
                    continue; // Ya existe en la orden
                }
                
                // Calcular la cantidad a pedir (diferencia entre stock mínimo y stock actual)
                var cantidadAPedir = CalcularCantidadAPedir(ingrediente);
                
                try
                {
                    // Agregar el ingrediente a la orden
                    orden.AgregarItem(ingrediente.Id, ingrediente.Nombre, cantidadAPedir, ingrediente.UnidadMedida);
                    ingredientesAgregados = true;
                }
                catch (Exception ex)
                {
                    resultado.Errores.Add($"Error al agregar ingrediente '{ingrediente.Nombre}' a la orden: {ex.Message}");
                    _notificationManager.AddError($"Error al agregar ingrediente '{ingrediente.Nombre}' a la orden: {ex.Message}", "OrdenCompra");
                }
            }
            
            if (ingredientesAgregados)
            {
                resultado.OrdenesActualizadas.Add(orden);
            }
            
            return Result.Success(ingredientesAgregados);
        }
        
        /// <summary>
        /// Crea una nueva orden de compra para un proveedor con ingredientes
        /// </summary>
        /// <returns>True si se creó la orden correctamente, False en caso contrario</returns>
        private Result<bool> CrearNuevaOrden(Proveedor proveedor, List<Ingrediente> ingredientes, ResultadoVerificacionStock resultado)
        {
            _notificationManager.RequireNotNull(proveedor, "Proveedor no puede ser nulo", "Proveedor");
            _notificationManager.RequireNotNull(ingredientes, "La lista de ingredientes no puede ser nula", "Ingredientes");
            _notificationManager.RequireNotNull(resultado, "El resultado de verificación no puede ser nulo", "Resultado");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
                
            try
            {
                // Crear nueva orden
                var fechaActual = _dateTimeService.Now;
                var nuevaOrden = OrdenCompra.Crear(
                    proveedor.Id,
                    $"Orden automática - {fechaActual:dd/MM/yyyy}",
                    fechaActual);
                    
                // Establecer fecha de entrega estimada (7 días después de la fecha de emisión)
                nuevaOrden.EstablecerFechaEntrega(fechaActual.AddDays(7));
                
                var ingredientesAgregados = false;
                    
                // Agregar los ingredientes a la orden
                foreach (var ingrediente in ingredientes)
                {
                    if (ingrediente == null)
                    {
                        _notificationManager.AddError("Ingrediente no puede ser nulo", "Ingrediente");
                        continue;
                    }
                        
                    try
                    {
                        // Calcular la cantidad a pedir
                        var cantidadAPedir = CalcularCantidadAPedir(ingrediente);
                        
                        // Agregar el ingrediente a la orden
                        nuevaOrden.AgregarItem(ingrediente.Id, ingrediente.Nombre, cantidadAPedir, ingrediente.UnidadMedida);
                        ingredientesAgregados = true;
                    }
                    catch (Exception ex)
                    {
                        resultado.Errores.Add($"Error al agregar ingrediente '{ingrediente.Nombre}' a la orden: {ex.Message}");
                        _notificationManager.AddError($"Error al agregar ingrediente '{ingrediente.Nombre}' a la orden: {ex.Message}", "OrdenCompra");
                    }
                }
                
                if (ingredientesAgregados)
                {
                    resultado.OrdenesGeneradas.Add(nuevaOrden);
                    return Result.Success(true);
                }
                else
                {
                    _notificationManager.AddError("No se pudo agregar ningún ingrediente a la orden", "OrdenCompra");
                    return _notificationManager.ToResult<bool>(false);
                }
            }
            catch (Exception ex)
            {
                resultado.Errores.Add($"Error al crear orden para proveedor '{proveedor.Nombre}': {ex.Message}");
                _notificationManager.AddError($"Error al crear orden para proveedor '{proveedor.Nombre}': {ex.Message}", "OrdenCompra");
                return _notificationManager.ToResult<bool>(false);
            }
        }
        
        /// <summary>
        /// Calcula la cantidad a pedir de un ingrediente (diferencia entre stock mínimo y stock actual)
        /// </summary>
        private decimal CalcularCantidadAPedir(Ingrediente ingrediente)
        {
            // Calcular la cantidad necesaria para llegar al stock mínimo más un 20%
            var cantidadFaltante = ingrediente.StockMinimo - ingrediente.Stock;
            var cantidadSugerida = cantidadFaltante * 1.2m; // Agregar un 20% extra
            
            // Redondear hacia arriba a un número entero
            return Math.Ceiling(cantidadSugerida);
        }
    }
} 