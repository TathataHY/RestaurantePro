namespace RestaurantePro.Domain.Comercial.EventHandlers
{
    /// <summary>
    /// Manejador de eventos que envía notificaciones a proveedores cuando sus órdenes de compra son aprobadas
    /// </summary>
    public class OrdenCompraAprobada_NotificacionProveedorHandler : IDomainEventHandler<OrdenCompraAprobada>
    {
        private readonly IOrdenCompraRepository _ordenCompraRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IServicioNotificaciones _servicioNotificaciones;
        private readonly IDateTimeService _dateTimeService;
        
        /// <summary>
        /// Constructor con inyección de dependencias
        /// </summary>
        public OrdenCompraAprobada_NotificacionProveedorHandler(
            IOrdenCompraRepository ordenCompraRepository,
            IProveedorRepository proveedorRepository,
            IServicioNotificaciones servicioNotificaciones,
            IDateTimeService dateTimeService)
        {
            _ordenCompraRepository = ordenCompraRepository ?? throw new ArgumentNullException(nameof(ordenCompraRepository));
            _proveedorRepository = proveedorRepository ?? throw new ArgumentNullException(nameof(proveedorRepository));
            _servicioNotificaciones = servicioNotificaciones ?? throw new ArgumentNullException(nameof(servicioNotificaciones));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }
        
        /// <summary>
        /// Maneja el evento OrdenCompraAprobada
        /// </summary>
        /// <param name="evento">Evento a manejar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        public async Task Handle(OrdenCompraAprobada evento, CancellationToken cancellationToken)
        {
            // 1. Obtener la orden de compra
            var ordenCompra = await _ordenCompraRepository.ObtenerPorIdAsync(
                evento.OrdenCompraId, cancellationToken);
                
            if (ordenCompra == null)
            {
                // Registrar error o lanzar excepción
                return;
            }
            
            // 2. Obtener el proveedor
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(
                evento.ProveedorId, cancellationToken);
                
            if (proveedor == null)
            {
                // Registrar error o lanzar excepción
                return;
            }
            
            // 3. Crear la notificación
            var titulo = $"Orden de Compra Aprobada: {ordenCompra.Id}";
            var mensaje = $"Estimado proveedor {proveedor.Nombre},\n\n" +
                          $"Le informamos que su Orden de Compra {ordenCompra.Id} ha sido aprobada " +
                          $"el {evento.FechaAprobacion:dd/MM/yyyy} por un total de {evento.Total:C}.\n\n" +
                          $"La entrega está programada para el {ordenCompra.FechaEntregaEstimada:dd/MM/yyyy}.\n\n" +
                          $"Saludos cordiales,\nEquipo de Compras";
            
            // 4. Enviar notificación al proveedor (si tiene un usuario asociado)
            // Suponiendo que el proveedor tiene un usuario asociado para recibir notificaciones
            // Si no, se podría enviar a un usuario administrador del sistema
            var destinatarioId = Guid.Empty; // Usuario del sistema por defecto
            
            // En este caso, como Proveedor no tiene UsuarioAsociadoId implementado,
            // enviamos la notificación al usuario por defecto (administrador)
            destinatarioId = Guid.Empty; // Usar un ID fijo o consultar desde configuración en un entorno real
            
            // 5. Enviar la notificación
            await _servicioNotificaciones.EnviarNotificacionAsync(
                titulo,
                mensaje,
                TipoNotificacion.Informativa,
                destinatarioId,
                evento.OrdenCompraId,
                cancellationToken);
        }
    }
} 