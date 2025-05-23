namespace RestaurantePro.Domain.Comercial.Facturacion.Services
{
    /// <summary>
    /// Servicio para gestionar facturas vencidas
    /// </summary>
    public class ServicioGestionFacturasVencidas : IServicioGestionFacturasVencidas
    {
        private readonly IFacturaRepository _facturaRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly IServicioNotificaciones _servicioNotificaciones;

        /// <summary>
        /// Constructor del servicio
        /// </summary>
        /// <param name="facturaRepository">Repositorio de facturas</param>
        /// <param name="dateTimeService">Servicio de fecha/hora</param>
        /// <param name="servicioNotificaciones">Servicio de notificaciones</param>
        public ServicioGestionFacturasVencidas(
            IFacturaRepository facturaRepository,
            IDateTimeService dateTimeService,
            IServicioNotificaciones servicioNotificaciones)
        {
            _facturaRepository = facturaRepository ?? throw new ArgumentNullException(nameof(facturaRepository));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _servicioNotificaciones = servicioNotificaciones ?? throw new ArgumentNullException(nameof(servicioNotificaciones));
        }

        /// <summary>
        /// Procesa las facturas vencidas
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de facturas procesadas</returns>
        public async Task<int> ProcesarFacturasVencidasAsync(CancellationToken cancellationToken = default)
        {
            // Obtener fecha actual
            var fechaActual = _dateTimeService.Now;

            // Crear especificación para facturas vencidas
            var facturaVencidaSpec = new FacturaVencidaSpecification(fechaActual);

            // Obtener facturas vencidas
            var facturasVencidas = await _facturaRepository.ObtenerPorSpecAsync(facturaVencidaSpec, cancellationToken);
            var listaFacturas = facturasVencidas.ToList();

            if (!listaFacturas.Any())
            {
                return 0;
            }

            // Actualizar estado de facturas vencidas
            foreach (var factura in listaFacturas)
            {
                // Cambiar estado a vencida si no lo está ya
                if (factura.Estado != EstadoFactura.Vencida)
                {
                    await CambiarEstadoAVencidaAsync(factura, cancellationToken);
                }
            }

            return listaFacturas.Count();
        }

        /// <summary>
        /// Cambia el estado de una factura a vencida
        /// </summary>
        /// <param name="factura">Factura a actualizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        private async Task CambiarEstadoAVencidaAsync(Factura factura, CancellationToken cancellationToken)
        {
            try
            {
                // Utilizar el método de la entidad para marcar como vencida
                factura.MarcarComoVencida(_dateTimeService);
                
                // Crear notificación
                var notificacion = Notificacion.Crear(
                    titulo: $"Factura {factura.NumeroFactura} vencida",
                    mensaje: $"La factura {factura.NumeroFactura} ha vencido el {factura.FechaVencimiento:dd/MM/yyyy}. Monto pendiente: {factura.Total - factura.TotalPagado:C}",
                    tipo: TipoNotificacion.Alerta,
                    destinatarioId: Guid.Empty, // Destinatario genérico o sistema
                    entidadRelacionadaId: factura.Id);

                // Enviar notificación
                await _servicioNotificaciones.EnviarNotificacionAsync(notificacion, cancellationToken);
                
                // Guardar cambios
                await _facturaRepository.GuardarCambiosAsync(cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                // Loguear el error - en un sistema real esto podría ir a un sistema de logging
                Console.WriteLine($"Error al marcar factura {factura.NumeroFactura} como vencida: {ex.Message}");
            }
        }

        /// <summary>
        /// Genera un informe de facturas vencidas
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Informe de facturas vencidas</returns>
        public async Task<InformeFacturasVencidas> GenerarInformeFacturasVencidasAsync(CancellationToken cancellationToken = default)
        {
            // Obtener fecha actual
            var fechaActual = _dateTimeService.Now;

            // Crear especificación para facturas vencidas
            var facturaVencidaSpec = new FacturaVencidaSpecification(fechaActual);

            // Obtener facturas vencidas
            var facturasVencidas = await _facturaRepository.ObtenerPorSpecAsync(facturaVencidaSpec, cancellationToken);
            var listaFacturas = facturasVencidas.ToList();

            // Crear informe
            var informe = new InformeFacturasVencidas
            {
                FechaGeneracion = fechaActual,
                CantidadFacturasVencidas = listaFacturas.Count(),
                MontoTotalVencido = listaFacturas.Sum(f => f.Total - f.TotalPagado),
                FacturasMasAntiguas = listaFacturas
                    .OrderBy(f => f.FechaVencimiento)
                    .Take(5)
                    .Select(f => new FacturaVencidaResumen
                    {
                        Id = f.Id,
                        NumeroFactura = f.NumeroFactura,
                        Cliente = f.NombreCliente,
                        FechaVencimiento = f.FechaVencimiento.Value,
                        DiasVencimiento = (int)(fechaActual - f.FechaVencimiento.Value).TotalDays,
                        MontoPendiente = f.Total - f.TotalPagado
                    })
                    .ToList()
            };

            return informe;
        }
    }

    /// <summary>
    /// Clase que representa un informe de facturas vencidas
    /// </summary>
    public class InformeFacturasVencidas
    {
        /// <summary>
        /// Fecha de generación del informe
        /// </summary>
        public DateTime FechaGeneracion { get; set; }

        /// <summary>
        /// Cantidad total de facturas vencidas
        /// </summary>
        public int CantidadFacturasVencidas { get; set; }

        /// <summary>
        /// Monto total vencido
        /// </summary>
        public decimal MontoTotalVencido { get; set; }

        /// <summary>
        /// Lista de las facturas más antiguas
        /// </summary>
        public List<FacturaVencidaResumen> FacturasMasAntiguas { get; set; } = new List<FacturaVencidaResumen>();
    }

    /// <summary>
    /// Resumen de una factura vencida
    /// </summary>
    public class FacturaVencidaResumen
    {
        /// <summary>
        /// Identificador de la factura
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Número de la factura
        /// </summary>
        public string NumeroFactura { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del cliente
        /// </summary>
        public string Cliente { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de vencimiento
        /// </summary>
        public DateTime FechaVencimiento { get; set; }

        /// <summary>
        /// Días transcurridos desde el vencimiento
        /// </summary>
        public int DiasVencimiento { get; set; }

        /// <summary>
        /// Monto pendiente de pago
        /// </summary>
        public decimal MontoPendiente { get; set; }
    }
} 