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
        private readonly INotificationManager _notificationManager;

        /// <summary>
        /// Constructor del servicio
        /// </summary>
        /// <param name="facturaRepository">Repositorio de facturas</param>
        /// <param name="dateTimeService">Servicio de fecha/hora</param>
        /// <param name="servicioNotificaciones">Servicio de notificaciones</param>
        /// <param name="notificationManager">Gestor de notificaciones para validaciones</param>
        public ServicioGestionFacturasVencidas(
            IFacturaRepository facturaRepository,
            IDateTimeService dateTimeService,
            IServicioNotificaciones servicioNotificaciones,
            INotificationManager notificationManager)
        {
            _facturaRepository = facturaRepository ?? throw new ArgumentNullException(nameof(facturaRepository));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _servicioNotificaciones = servicioNotificaciones ?? throw new ArgumentNullException(nameof(servicioNotificaciones));
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
        }

        /// <summary>
        /// Procesa las facturas vencidas
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con el número de facturas procesadas</returns>
        public async Task<Result<int>> ProcesarFacturasVencidasAsync(CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            try
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
                    return Result.Success(0);
                }

                // Actualizar estado de facturas vencidas
                var facturasActualizadas = 0;
                foreach (var factura in listaFacturas)
                {
                    // Cambiar estado a vencida si no lo está ya
                    if (factura.Estado != EstadoFactura.Vencida)
                    {
                        var resultado = await CambiarEstadoAVencidaAsync(factura, cancellationToken);
                        if (resultado)
                        {
                            facturasActualizadas++;
                        }
                    }
                }

                return Result.Success(facturasActualizadas);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al procesar facturas vencidas: {ex.Message}", "ProcesarFacturas");
                return _notificationManager.ToResult<int>(0);
            }
        }

        /// <summary>
        /// Cambia el estado de una factura a vencida
        /// </summary>
        /// <param name="factura">Factura a actualizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si la operación fue exitosa, false en caso contrario</returns>
        private async Task<bool> CambiarEstadoAVencidaAsync(Factura factura, CancellationToken cancellationToken)
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
                await _servicioNotificaciones.EnviarNotificacionAsync(
                    notificacion.Titulo,
                    notificacion.Mensaje,
                    notificacion.Tipo,
                    notificacion.DestinatarioId,
                    notificacion.EntidadRelacionadaId,
                    cancellationToken);
                
                // Guardar cambios
                await _facturaRepository.GuardarCambiosAsync(cancellationToken);
                
                return true;
            }
            catch (InvalidOperationException ex)
            {
                // Registrar el error
                _notificationManager.AddError($"Error al marcar factura {factura.NumeroFactura} como vencida: {ex.Message}", "MarcarComoVencida");
                return false;
            }
            catch (Exception ex)
            {
                // Registrar el error
                _notificationManager.AddError($"Error inesperado al marcar factura {factura.NumeroFactura} como vencida: {ex.Message}", "MarcarComoVencida");
                return false;
            }
        }

        /// <summary>
        /// Genera un informe de facturas vencidas
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con el informe de facturas vencidas</returns>
        public async Task<Result<InformeFacturasVencidas>> GenerarInformeFacturasVencidasAsync(CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            try
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

                return Result.Success(informe);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al generar informe de facturas vencidas: {ex.Message}", "GenerarInforme");
                return _notificationManager.ToResult<InformeFacturasVencidas>(null);
            }
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