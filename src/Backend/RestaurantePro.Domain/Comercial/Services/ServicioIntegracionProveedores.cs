namespace RestaurantePro.Domain.Comercial.Services
{
    /// <summary>
    /// Servicio de integración entre los contextos Proveedores y Comercial.
    /// Implementa el patrón Anticorruption Layer para traducir conceptos entre los dos contextos.
    /// </summary>
    public class ServicioIntegracionProveedores : IServicioIntegracionProveedores
    {
        private readonly IOrdenCompraRepository _ordenCompraRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IServicioFacturacion _servicioFacturacion;
        private readonly IFacturaRepository _facturaRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly INotificationManager _notificationManager;
        
        /// <summary>
        /// Constructor con inyección de dependencias
        /// </summary>
        public ServicioIntegracionProveedores(
            IOrdenCompraRepository ordenCompraRepository,
            IProveedorRepository proveedorRepository,
            IServicioFacturacion servicioFacturacion,
            IFacturaRepository facturaRepository,
            IDateTimeService dateTimeService,
            INotificationManager notificationManager)
        {
            _ordenCompraRepository = ordenCompraRepository ?? throw new ArgumentNullException(nameof(ordenCompraRepository));
            _proveedorRepository = proveedorRepository ?? throw new ArgumentNullException(nameof(proveedorRepository));
            _servicioFacturacion = servicioFacturacion ?? throw new ArgumentNullException(nameof(servicioFacturacion));
            _facturaRepository = facturaRepository ?? throw new ArgumentNullException(nameof(facturaRepository));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
        }
        
        /// <summary>
        /// Procesa una orden de compra aprobada para generar la factura correspondiente
        /// y realizar las actualizaciones necesarias en ambos contextos.
        /// </summary>
        /// <param name="evento">Evento de orden de compra aprobada</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con información del procesamiento</returns>
        public async Task<Result<bool>> ProcesarOrdenCompraAprobadaAsync(
            OrdenCompraAprobada evento, 
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar el evento
            _notificationManager.Require(evento != null, "El evento de orden de compra aprobada no puede ser nulo", "Evento");
            _notificationManager.Require(evento.OrdenCompraId != Guid.Empty, "El ID de la orden de compra no puede estar vacío", "OrdenCompraId");
            _notificationManager.Require(evento.ProveedorId != Guid.Empty, "El ID del proveedor no puede estar vacío", "ProveedorId");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // 1. Obtener la orden de compra completa
                var ordenCompra = await _ordenCompraRepository.ObtenerPorIdAsync(
                    evento.OrdenCompraId, cancellationToken);
                    
                if (ordenCompra == null)
                {
                    _notificationManager.AddError($"No se encontró la orden de compra con ID {evento.OrdenCompraId}", "OrdenCompra");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // 2. Obtener el proveedor
                var proveedor = await _proveedorRepository.ObtenerPorIdAsync(
                    evento.ProveedorId, cancellationToken);
                    
                if (proveedor == null)
                {
                    _notificationManager.AddError($"No se encontró el proveedor con ID {evento.ProveedorId}", "Proveedor");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // 3. Crear la factura en el contexto Comercial
                var facturaResult = await CrearFacturaDesdeOrdenCompraAsync(ordenCompra, proveedor, cancellationToken);
                if (!facturaResult.Succeeded)
                {
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // 4. Actualizar estadísticas del proveedor
                var estadisticasResult = await ActualizarEstadisticasProveedorAsync(proveedor, ordenCompra.Total, cancellationToken);
                if (!estadisticasResult.Succeeded)
                {
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // 5. Enviar notificación al proveedor (si es necesario)
                // Esta lógica podría estar en un manejador de eventos separado
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al procesar la orden de compra: {ex.Message}", "ProcesarOrden");
                return _notificationManager.ToResult<bool>(false);
            }
        }
        
        /// <summary>
        /// Sincroniza información de un proveedor entre el contexto de Proveedores y Comercial.
        /// Asegura que ambos contextos tengan información consistente sobre el proveedor.
        /// </summary>
        /// <param name="proveedorId">ID del proveedor a sincronizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con información del procesamiento</returns>
        public async Task<Result<bool>> SincronizarInformacionProveedorAsync(
            Guid proveedorId,
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(proveedorId != Guid.Empty, "El ID del proveedor no puede estar vacío", "ProveedorId");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<bool>(false);
            }
            
            try
            {
                // 1. Obtener el proveedor del contexto de Proveedores
                var proveedor = await _proveedorRepository.ObtenerPorIdAsync(
                    proveedorId, cancellationToken);
                    
                if (proveedor == null)
                {
                    _notificationManager.AddError($"No se encontró el proveedor con ID {proveedorId}", "Proveedor");
                    return _notificationManager.ToResult<bool>(false);
                }
                
                // 2. Actualizar datos de contacto en el sistema de facturación si es necesario
                await ActualizarDatosContactoProveedorAsync(proveedor, cancellationToken);
                
                // 3. Verificar si hay facturas pendientes para este proveedor
                var facturasPendientes = await _facturaRepository.ObtenerFacturasPendientesPorProveedorAsync(
                    proveedorId, cancellationToken);
                
                // 4. Verificar si hay pagos pendientes que deban ser gestionados
                if (facturasPendientes.Any())
                {
                    // Aquí se implementaría la lógica para gestionar pagos pendientes
                    // Por ahora, solo registramos la actividad
                    _notificationManager.AddInformation(
                        $"Proveedor {proveedor.Nombre} tiene {facturasPendientes.Count()} facturas pendientes",
                        "SincronizarProveedor");
                }
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al sincronizar información del proveedor: {ex.Message}", "SincronizarProveedor");
                return _notificationManager.ToResult<bool>(false);
            }
        }
        
        /// <summary>
        /// Actualiza los datos de contacto del proveedor en el sistema de facturación
        /// </summary>
        private async Task ActualizarDatosContactoProveedorAsync(
            Proveedor proveedor,
            CancellationToken cancellationToken)
        {
            // En una implementación real, aquí se actualizarían los datos en el sistema de facturación
            // Por ahora, es solo un método de placeholder
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Crea una factura en el contexto Comercial a partir de una orden de compra del contexto Proveedores
        /// </summary>
        private async Task<Result<Factura>> CrearFacturaDesdeOrdenCompraAsync(
            OrdenCompra ordenCompra, 
            Proveedor proveedor,
            CancellationToken cancellationToken)
        {
            try
            {
                // Generar un número de factura simple basado en la orden de compra
                var numeroFactura = $"FC-OC-{ordenCompra.Id.ToString().Substring(0, 8)}";
                
                // Utilizar el servicio de facturación que ahora devuelve Result<Factura>
                var facturaResult = await _servicioFacturacion.GenerarFacturaParaComandaAsync(
                    ordenCompra.Id, // Usamos el ID de la orden como sustituto
                    TipoFactura.Fiscal,
                    proveedor.Nombre,
                    proveedor.Id,
                    proveedor.RFC,
                    proveedor.Direccion,
                    "Factura por orden de compra aprobada",
                    cancellationToken);
                
                if (!facturaResult.Succeeded)
                {
                    // Agregar los errores del servicio de facturación a nuestras notificaciones
                    foreach (var error in facturaResult.Errors)
                    {
                        _notificationManager.AddError(error, "CrearFactura");
                    }
                    
                    return _notificationManager.ToResult<Factura>(null);
                }
                
                return facturaResult;
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al crear factura desde orden de compra: {ex.Message}", "CrearFactura");
                return _notificationManager.ToResult<Factura>(null);
            }
        }
        
        /// <summary>
        /// Actualiza las estadísticas del proveedor con la nueva compra
        /// </summary>
        private async Task<Result<bool>> ActualizarEstadisticasProveedorAsync(
            Proveedor proveedor, 
            decimal montoCompra, 
            CancellationToken cancellationToken)
        {
            try
            {
                // Registrar la fecha de la orden
                proveedor.RegistrarOrden(_dateTimeService.Now);
                await _proveedorRepository.ActualizarAsync(proveedor, cancellationToken);
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al actualizar estadísticas del proveedor: {ex.Message}", "ActualizarEstadisticas");
                return _notificationManager.ToResult<bool>(false);
            }
        }
    }
    

} 