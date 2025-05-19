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
        private readonly IDomainEventRegistry _eventRegistry;
        private readonly IDateTimeService _dateTimeService;
        
        public StockBajoMinimo_GenerarOrdenCompraAutomaticaHandler(
            IIngredienteRepository ingredienteRepository,
            IProveedorRepository proveedorRepository,
            IOrdenCompraRepository ordenCompraRepository,
            IDomainEventRegistry eventRegistry,
            IDateTimeService dateTimeService)
        {
            _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
            _proveedorRepository = proveedorRepository ?? throw new ArgumentNullException(nameof(proveedorRepository));
            _ordenCompraRepository = ordenCompraRepository ?? throw new ArgumentNullException(nameof(ordenCompraRepository));
            _eventRegistry = eventRegistry ?? throw new ArgumentNullException(nameof(eventRegistry));
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
                    await _eventRegistry.RegisterAsync(evento, cancellationToken);
                    return;
                }
                
                // Verificar si tiene proveedor principal
                if (!ingrediente.ProveedorPrincipalId.HasValue)
                {
                    await _eventRegistry.RegisterAsync(evento, cancellationToken);
                    return;
                }
                
                // Obtener el proveedor
                var proveedor = await _proveedorRepository.ObtenerPorIdAsync(ingrediente.ProveedorPrincipalId.Value, cancellationToken);
                if (proveedor == null || !proveedor.EstaActivo)
                {
                    await _eventRegistry.RegisterAsync(evento, cancellationToken);
                    return;
                }
                
                // Verificar si ya existe una orden pendiente para este ingrediente
                var ordenesExistentes = await _ordenCompraRepository.ObtenerPendientesPorProveedorAsync(
                    proveedor.Id, cancellationToken);
                    
                bool ingredienteYaEnOrden = false;
                foreach (var orden in ordenesExistentes)
                {
                    foreach (var item in orden.Items)
                    {
                        if (item.IngredienteId == ingrediente.Id)
                        {
                            ingredienteYaEnOrden = true;
                            await _eventRegistry.RegisterAsync(evento, cancellationToken);
                            break;
                        }
                    }
                    
                    if (ingredienteYaEnOrden)
                        break;
                }
                
                // Si ya existe una orden pendiente para este ingrediente, no crear una nueva
                if (ingredienteYaEnOrden)
                    return;
                
                // Crear una nueva orden de compra
                var fechaActual = _dateTimeService.Now;
                var observaciones = $"Orden automática generada por stock bajo del ingrediente '{ingrediente.Nombre}'. " +
                                   $"Stock actual: {ingrediente.Stock}, Stock mínimo: {ingrediente.StockMinimo}";
                
                var ordenCompra = OrdenCompra.Crear(proveedor.Id, observaciones, fechaActual);
                
                // IMPORTANTE: Establecer fecha estimada de entrega ANTES de agregar items
                // para evitar validaciones que fallen por inconsistencia de fechas
                var fechaEntrega = fechaActual.AddDays(5);
                ordenCompra.EstablecerFechaEntrega(fechaEntrega);
                
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
                
                // Guardar la orden de compra
                await _ordenCompraRepository.AgregarAsync(ordenCompra, cancellationToken);
                
                await _eventRegistry.RegisterAsync(evento, cancellationToken);
            }
            catch (Exception ex)
            {
                await _eventRegistry.RegisterAsync(evento, cancellationToken);
                // IMPORTANTE: Propagar la excepción para que las pruebas puedan detectarla
                throw;
            }
        }
    }
} 