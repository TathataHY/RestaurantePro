namespace RestaurantePro.Domain.Comercial.EventHandlers
{
    /// <summary>
    /// Manejador de eventos que actualiza las estadísticas del proveedor cuando una orden de compra es aprobada
    /// </summary>
    public class OrdenCompraAprobada_ActualizarEstadisticasProveedorHandler : IDomainEventHandler<OrdenCompraAprobada>
    {
        private readonly IOrdenCompraRepository _ordenCompraRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IDateTimeService _dateTimeService;
        
        /// <summary>
        /// Constructor con inyección de dependencias
        /// </summary>
        public OrdenCompraAprobada_ActualizarEstadisticasProveedorHandler(
            IOrdenCompraRepository ordenCompraRepository,
            IProveedorRepository proveedorRepository,
            IDateTimeService dateTimeService)
        {
            _ordenCompraRepository = ordenCompraRepository ?? throw new ArgumentNullException(nameof(ordenCompraRepository));
            _proveedorRepository = proveedorRepository ?? throw new ArgumentNullException(nameof(proveedorRepository));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }
        
        /// <summary>
        /// Maneja el evento OrdenCompraAprobada
        /// </summary>
        /// <param name="evento">Evento a manejar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        public async Task Handle(OrdenCompraAprobada evento, CancellationToken cancellationToken)
        {
            // 1. Obtener el proveedor
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(
                evento.ProveedorId, cancellationToken);
                
            if (proveedor == null)
            {
                // Registrar error o lanzar excepción
                return;
            }
            
            // 2. Actualizar las estadísticas del proveedor
            // Como no tenemos el método RegistrarCompra, usamos RegistrarOrden en su lugar
            proveedor.RegistrarOrden(evento.FechaAprobacion);
            
            // Nota: En una implementación real, deberíamos actualizar también el monto total
            // por ahora solo registramos la fecha de la orden
            
            // 3. Calcular estadísticas adicionales si es necesario
            // Por ejemplo, actualizar la evaluación del proveedor, 
            // calcular promedio de compras, etc.
            
            // 4. Persistir los cambios
            await _proveedorRepository.ActualizarAsync(proveedor, cancellationToken);
        }
    }
} 