namespace RestaurantePro.Domain.Inventario.EventHandlers
{
    /// <summary>
    /// Manejador de eventos que genera órdenes de compra automáticas cuando el stock 
    /// de un ingrediente cae por debajo del mínimo recomendado
    /// </summary>
    public class StockBajoMinimo_GenerarOrdenCompraAutomaticaHandler : IDomainEventHandler<Ingredientes.Events.StockBajoMinimo>
    {
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IOrdenCompraRepository _ordenCompraRepository;
        private readonly IDomainEventLog _eventLog;
        private readonly IDateTimeService _dateTimeService;
        
        public StockBajoMinimo_GenerarOrdenCompraAutomaticaHandler(
            IIngredienteRepository ingredienteRepository,
            IProveedorRepository proveedorRepository,
            IOrdenCompraRepository ordenCompraRepository,
            IDomainEventLog eventLog,
            IDateTimeService dateTimeService)
        {
            _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
            _proveedorRepository = proveedorRepository ?? throw new ArgumentNullException(nameof(proveedorRepository));
            _ordenCompraRepository = ordenCompraRepository ?? throw new ArgumentNullException(nameof(ordenCompraRepository));
            _eventLog = eventLog ?? throw new ArgumentNullException(nameof(eventLog));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }
        
        /// <summary>
        /// Maneja el evento StockBajoMinimo generando una orden de compra automática al proveedor principal
        /// Solo genera la orden si el ingrediente tiene un proveedor principal asociado y activo
        /// </summary>
        public async Task Handle(Ingredientes.Events.StockBajoMinimo evento, CancellationToken cancellationToken = default)
        {
            try
            {
                // Obtener el ingrediente completo
                var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(evento.IngredienteId, cancellationToken);
                if (ingrediente == null)
                {
                    await _eventLog.LogEvent(evento, 
                        $"No se pudo generar orden automática: Ingrediente con ID {evento.IngredienteId} no encontrado",
                        cancellationToken);
                    return;
                }
                
                // Verificar si tiene proveedor principal
                if (!ingrediente.ProveedorPrincipalId.HasValue)
                {
                    await _eventLog.LogEvent(evento, 
                        $"No se pudo generar orden automática: Ingrediente '{ingrediente.Nombre}' no tiene proveedor principal asignado",
                        cancellationToken);
                    return;
                }
                
                // Obtener el proveedor
                var proveedor = await _proveedorRepository.ObtenerPorIdAsync(ingrediente.ProveedorPrincipalId.Value, cancellationToken);
                if (proveedor == null || !proveedor.EstaActivo)
                {
                    await _eventLog.LogEvent(evento, 
                        $"No se pudo generar orden automática: Proveedor no encontrado o inactivo para ingrediente '{ingrediente.Nombre}'",
                        cancellationToken);
                    return;
                }
                
                // Verificar si ya existe una orden pendiente para este ingrediente
                var ordenesExistentes = await _ordenCompraRepository.ObtenerPendientesPorProveedorAsync(
                    proveedor.Id, cancellationToken);
                    
                foreach (var orden in ordenesExistentes)
                {
                    foreach (var item in orden.Items)
                    {
                        if (item.IngredienteId == ingrediente.Id)
                        {
                            await _eventLog.LogEvent(evento, 
                                $"No se generó orden automática: Ya existe una orden pendiente para '{ingrediente.Nombre}' (Orden ID: {orden.Id})",
                                cancellationToken);
                            return;
                        }
                    }
                }
                
                // Crear una nueva orden de compra
                var fechaActual = _dateTimeService.Now;
                var observaciones = $"Orden automática generada por stock bajo del ingrediente '{ingrediente.Nombre}'. " +
                                   $"Stock actual: {ingrediente.Stock}, Stock mínimo: {ingrediente.StockMinimo}";
                
                var ordenCompra = OrdenCompra.Crear(proveedor.Id, observaciones, fechaActual);
                
                // Calcular cantidad a pedir: al menos lo necesario para estar por encima del mínimo + margen adicional
                var deficitStock = ingrediente.StockMinimo - ingrediente.Stock;
                var cantidadPedir = deficitStock * 2; // Pedimos el doble del déficit para tener margen
                
                if (cantidadPedir <= 0) cantidadPedir = ingrediente.StockMinimo; // Garantizar un mínimo
                
                // Redondear hacia arriba según la unidad de medida
                cantidadPedir = Math.Ceiling(cantidadPedir);
                
                // Agregar el ítem a la orden
                ordenCompra.AgregarItem(
                    ingrediente.Id, 
                    ingrediente.Nombre, 
                    cantidadPedir, 
                    ingrediente.UnidadMedida);
                
                // Establecer fecha estimada de entrega (5 días hábiles)
                ordenCompra.EstablecerFechaEntrega(fechaActual.AddDays(5));
                
                // Guardar la orden de compra
                await _ordenCompraRepository.AgregarAsync(ordenCompra, cancellationToken);
                
                await _eventLog.LogEvent(evento, 
                    $"Se generó orden de compra automática (ID: {ordenCompra.Id}) para ingrediente '{ingrediente.Nombre}' " +
                    $"al proveedor '{proveedor.Nombre}'. Cantidad: {cantidadPedir} {ingrediente.UnidadMedida}",
                    cancellationToken);
            }
            catch (Exception ex)
            {
                await _eventLog.LogEvent(evento, 
                    $"Error al generar orden automática: {ex.Message}",
                    cancellationToken);
            }
        }
    }
} 