namespace RestaurantePro.Domain.Comercial.Facturacion.EventHandlers
{
    /// <summary>
    /// Manejador del evento FacturaPagada
    /// </summary>
    public class FacturaPagadaHandler : IDomainEventHandler<FacturaPagada>
    {
        private readonly IServicioNotificaciones _servicioNotificaciones;

        /// <summary>
        /// Constructor del manejador
        /// </summary>
        /// <param name="servicioNotificaciones">Servicio de notificaciones</param>
        public FacturaPagadaHandler(IServicioNotificaciones servicioNotificaciones)
        {
            _servicioNotificaciones = servicioNotificaciones ?? throw new ArgumentNullException(nameof(servicioNotificaciones));
        }

        /// <summary>
        /// Maneja el evento FacturaPagada
        /// </summary>
        /// <param name="evento">Evento a manejar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        public async Task Handle(FacturaPagada evento, CancellationToken cancellationToken)
        {
            // Crear una notificación informando que la factura ha sido pagada completamente
            var notificacion = Notificacion.Crear(
                titulo: $"Factura {evento.NumeroFactura} pagada completamente",
                mensaje: $"La factura {evento.NumeroFactura} ha sido pagada completamente en fecha {evento.FechaPago:dd/MM/yyyy HH:mm}",
                tipo: TipoNotificacion.Informativa,
                destinatarioId: Guid.Empty, // Destinatario genérico o sistema
                entidadRelacionadaId: evento.FacturaId);

            // Enviar la notificación
            await _servicioNotificaciones.EnviarNotificacionAsync(
                notificacion.Titulo,
                notificacion.Mensaje,
                notificacion.Tipo,
                notificacion.DestinatarioId,
                notificacion.EntidadRelacionadaId,
                cancellationToken);

            // Aquí podrían ir otras acciones como:
            // - Actualizar estadísticas de ventas
            // - Generar asientos contables
            // - Actualizar inventario
            // - Etc.
        }
    }
} 