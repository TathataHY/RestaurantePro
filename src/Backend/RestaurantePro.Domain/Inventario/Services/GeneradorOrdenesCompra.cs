namespace RestaurantePro.Domain.Inventario.Services
{
    /// <summary>
    /// Generador que analiza el inventario y crea órdenes de compra automáticas
    /// cuando los ingredientes están por debajo del stock mínimo
    /// </summary>
    public class GeneradorOrdenesCompra : IGeneradorOrdenesCompra
    {
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly IOrdenCompraRepository _ordenCompraRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly INotificationManager _notificationManager;
        
        /// <summary>
        /// Constructor del generador de órdenes de compra
        /// </summary>
        public GeneradorOrdenesCompra(
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
        
        /// <inheritdoc />
        public async Task<Result<IEnumerable<Guid>>> GenerarOrdenesCompraAutomaticas(CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Obtener todos los ingredientes con stock bajo
            var ingredientesConStockBajo = await _ingredienteRepository.ObtenerConStockBajoAsync(cancellationToken);
            
            if (!ingredientesConStockBajo.Any())
            {
                return Result.Success<IEnumerable<Guid>>(Enumerable.Empty<Guid>());
            }
            
            // Agrupar ingredientes por proveedor para generar una orden por proveedor
            var ingredientesPorProveedor = ingredientesConStockBajo
                .Where(i => i.ProveedorPrincipalId.HasValue)
                .GroupBy(i => i.ProveedorPrincipalId.Value);
                
            var ordenesGeneradas = new List<Guid>();
            
            foreach (var grupo in ingredientesPorProveedor)
            {
                var proveedorId = grupo.Key;
                var ingredientes = grupo.ToList();
                
                // Verificar si hay ingredientes para este proveedor
                if (!ingredientes.Any())
                    continue;
                    
                // Obtener información del proveedor
                var proveedor = await _proveedorRepository.ObtenerPorIdAsync(proveedorId, cancellationToken);
                if (proveedor == null)
                {
                    _notificationManager.AddError($"No se encontró el proveedor con ID {proveedorId}", "Proveedor");
                    continue;
                }
                
                // Crear la orden de compra para este proveedor
                var resultadoCreacion = await CrearOrdenCompraParaProveedorAsync(proveedor, ingredientes, cancellationToken);
                if (resultadoCreacion.Succeeded && resultadoCreacion.Value != Guid.Empty)
                {
                    ordenesGeneradas.Add(resultadoCreacion.Value);
                }
            }
            
            return _notificationManager.HasErrors
                ? _notificationManager.ToResult<IEnumerable<Guid>>(ordenesGeneradas)
                : Result.Success<IEnumerable<Guid>>(ordenesGeneradas);
        }
        
        /// <inheritdoc />
        public async Task<Result<Guid?>> GenerarOrdenCompraParaIngrediente(Guid ingredienteId, CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar el ID del ingrediente
            _notificationManager.Require(
                ingredienteId != Guid.Empty, 
                "El ID del ingrediente no puede estar vacío", 
                "IngredienteId");
                
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Guid?>(null);
            }
            
            // Obtener el ingrediente
            var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId, false, cancellationToken);
            if (ingrediente == null)
            {
                return Result.Failure<Guid?>($"No existe un ingrediente con el ID {ingredienteId}");
            }
            
            // Verificar si el stock está por debajo del mínimo
            if (ingrediente.Stock >= ingrediente.StockMinimo)
            {
                return Result.Success<Guid?>(null); // No es necesario generar una orden
            }
            
            // Verificar que tenga proveedor principal asignado
            if (!ingrediente.ProveedorPrincipalId.HasValue)
            {
                return Result.Failure<Guid?>($"El ingrediente {ingrediente.Nombre} no tiene un proveedor principal asignado");
            }
            
            // Obtener el proveedor
            var proveedorId = ingrediente.ProveedorPrincipalId.Value;
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(proveedorId, cancellationToken);
            if (proveedor == null)
            {
                return Result.Failure<Guid?>($"No se encontró el proveedor con ID {proveedorId} asignado al ingrediente");
            }
            
            // Crear la orden de compra para este ingrediente
            var resultadoCreacion = await CrearOrdenCompraParaIngredienteAsync(proveedor, ingrediente, cancellationToken);
            return resultadoCreacion;
        }
        
        /// <summary>
        /// Crea una orden de compra para un proveedor con una lista de ingredientes
        /// </summary>
        /// <param name="proveedor">Proveedor para el que se creará la orden</param>
        /// <param name="ingredientes">Lista de ingredientes a incluir en la orden</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>ID de la orden creada o Guid.Empty si hubo errores</returns>
        private async Task<Result<Guid>> CrearOrdenCompraParaProveedorAsync(
            Proveedor proveedor, 
            List<Ingrediente> ingredientes, 
            CancellationToken cancellationToken)
        {
            _notificationManager.RequireNotNull(proveedor, "El proveedor no puede ser nulo", "Proveedor");
            _notificationManager.RequireNotNull(ingredientes, "La lista de ingredientes no puede ser nula", "Ingredientes");
            _notificationManager.Require(ingredientes.Any(), "La lista de ingredientes no puede estar vacía", "Ingredientes");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Guid>(Guid.Empty);
            }
            
            try
            {
                // Crear la orden de compra
                var ordenCompra = OrdenCompra.Crear(
                    proveedor.Id,
                    $"Orden automática por stock bajo - {_dateTimeService.Now:dd/MM/yyyy}",
                    _dateTimeService.Now
                );
                
                // Establecer fecha de entrega estimada (7 días después de la fecha de emisión)
                ordenCompra.EstablecerFechaEntrega(_dateTimeService.Now.AddDays(7));
                
                // Agregar los items a la orden
                var hayItemsAgregados = false;
                foreach (var ingrediente in ingredientes)
                {
                    var resultadoAgregar = AgregarIngredienteAOrden(ordenCompra, ingrediente);
                    if (resultadoAgregar.Succeeded && resultadoAgregar.Value)
                    {
                        hayItemsAgregados = true;
                    }
                }
                
                if (!hayItemsAgregados)
                {
                    _notificationManager.AddError("No se pudo agregar ningún ingrediente a la orden", "OrdenCompra");
                    return _notificationManager.ToResult<Guid>(Guid.Empty);
                }
                
                // Guardar la orden de compra
                await _ordenCompraRepository.AddAsync(ordenCompra, cancellationToken);
                return Result.Success(ordenCompra.Id);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError(
                    $"Error al crear orden para proveedor '{proveedor.Nombre}': {ex.Message}", 
                    "OrdenCompra");
                return _notificationManager.ToResult<Guid>(Guid.Empty);
            }
        }
        
        /// <summary>
        /// Crea una orden de compra para un solo ingrediente
        /// </summary>
        /// <param name="proveedor">Proveedor para el que se creará la orden</param>
        /// <param name="ingrediente">Ingrediente a incluir en la orden</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>ID de la orden creada o null si hubo errores</returns>
        private async Task<Result<Guid?>> CrearOrdenCompraParaIngredienteAsync(
            Proveedor proveedor, 
            Ingrediente ingrediente, 
            CancellationToken cancellationToken)
        {
            _notificationManager.RequireNotNull(proveedor, "El proveedor no puede ser nulo", "Proveedor");
            _notificationManager.RequireNotNull(ingrediente, "El ingrediente no puede ser nulo", "Ingrediente");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Guid?>(null);
            }
            
            try
            {
                // Crear la orden de compra
                var ordenCompra = OrdenCompra.Crear(
                    proveedor.Id,
                    $"Orden automática para {ingrediente.Nombre} - {_dateTimeService.Now:dd/MM/yyyy}",
                    _dateTimeService.Now
                );
                
                // Establecer fecha de entrega estimada (7 días después de la fecha de emisión)
                ordenCompra.EstablecerFechaEntrega(_dateTimeService.Now.AddDays(7));
                
                // Agregar el ingrediente a la orden
                var resultadoAgregar = AgregarIngredienteAOrden(ordenCompra, ingrediente);
                if (!resultadoAgregar.Succeeded || !resultadoAgregar.Value)
                {
                    return _notificationManager.ToResult<Guid?>(null);
                }
                
                // Guardar la orden de compra
                await _ordenCompraRepository.AddAsync(ordenCompra, cancellationToken);
                return Result.Success<Guid?>(ordenCompra.Id);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError(
                    $"Error al crear orden para ingrediente '{ingrediente.Nombre}': {ex.Message}", 
                    "OrdenCompra");
                return _notificationManager.ToResult<Guid?>(null);
            }
        }
        
        /// <summary>
        /// Agrega un ingrediente a una orden de compra
        /// </summary>
        /// <param name="orden">Orden a la que se agregará el ingrediente</param>
        /// <param name="ingrediente">Ingrediente a agregar</param>
        /// <returns>True si se agregó correctamente, False en caso contrario</returns>
        private Result<bool> AgregarIngredienteAOrden(OrdenCompra orden, Ingrediente ingrediente)
        {
            _notificationManager.RequireNotNull(orden, "La orden no puede ser nula", "Orden");
            _notificationManager.RequireNotNull(ingrediente, "El ingrediente no puede ser nulo", "Ingrediente");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // Calcular la cantidad a pedir
                var cantidadAPedir = CalcularCantidadAPedir(ingrediente);
                
                // Agregar el ingrediente a la orden
                orden.AgregarItem(
                    ingrediente.Id,
                    ingrediente.Nombre,
                    cantidadAPedir,
                    ingrediente.UnidadMedida
                );
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError(
                    $"Error al agregar ingrediente '{ingrediente.Nombre}' a la orden: {ex.Message}", 
                    "OrdenCompra");
                return _notificationManager.ToResult<bool>(false);
            }
        }
        
        /// <summary>
        /// Calcula la cantidad a pedir de un ingrediente basado en su stock actual y mínimo
        /// </summary>
        /// <param name="ingrediente">Ingrediente a evaluar</param>
        /// <returns>Cantidad a pedir</returns>
        private decimal CalcularCantidadAPedir(Ingrediente ingrediente)
        {
            if (ingrediente == null)
                return 0;
                
            // Calcular la cantidad faltante (diferencia entre stock mínimo y actual)
            var cantidadFaltante = ingrediente.StockMinimo - ingrediente.Stock;
            
            // Si no falta nada, pedir al menos 1 unidad
            if (cantidadFaltante <= 0)
                return 1;
                
            // Agregar un 20% extra y redondear hacia arriba
            return Math.Ceiling(cantidadFaltante * 1.2m);
        }
    }
} 