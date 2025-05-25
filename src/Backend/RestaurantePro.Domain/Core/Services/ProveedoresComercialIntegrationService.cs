namespace RestaurantePro.Domain.Core.Services
{
    /// <summary>
    /// Servicio de integración entre los contextos Proveedores y Comercial.
    /// Implementa el patrón Anticorruption Layer para traducir conceptos entre los dos contextos.
    /// </summary>
    public class ProveedoresComercialIntegrationService : IProveedoresComercialIntegrationService
    {
        private readonly IOrdenCompraRepository _ordenCompraRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IServicioFacturacion _servicioFacturacion;
        private readonly IFacturaRepository _facturaRepository;
        private readonly IDateTimeService _dateTimeService;
        
        /// <summary>
        /// Constructor con inyección de dependencias
        /// </summary>
        public ProveedoresComercialIntegrationService(
            IOrdenCompraRepository ordenCompraRepository,
            IProveedorRepository proveedorRepository,
            IServicioFacturacion servicioFacturacion,
            IFacturaRepository facturaRepository,
            IDateTimeService dateTimeService)
        {
            _ordenCompraRepository = ordenCompraRepository ?? throw new ArgumentNullException(nameof(ordenCompraRepository));
            _proveedorRepository = proveedorRepository ?? throw new ArgumentNullException(nameof(proveedorRepository));
            _servicioFacturacion = servicioFacturacion ?? throw new ArgumentNullException(nameof(servicioFacturacion));
            _facturaRepository = facturaRepository ?? throw new ArgumentNullException(nameof(facturaRepository));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }
        
        /// <summary>
        /// Procesa una orden de compra aprobada para generar la factura correspondiente
        /// y realizar las actualizaciones necesarias en ambos contextos.
        /// </summary>
        /// <param name="evento">Evento de orden de compra aprobada</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea asíncrona</returns>
        public async Task ProcesarOrdenCompraAprobadaAsync(
            OrdenCompraAprobada evento, 
            CancellationToken cancellationToken = default)
        {
            // 1. Obtener la orden de compra completa
            var ordenCompra = await _ordenCompraRepository.ObtenerPorIdAsync(
                evento.OrdenCompraId, cancellationToken);
                
            if (ordenCompra == null)
            {
                throw new InvalidOperationException($"No se encontró la orden de compra con ID {evento.OrdenCompraId}");
            }
            
            // 2. Obtener el proveedor
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(
                evento.ProveedorId, cancellationToken);
                
            if (proveedor == null)
            {
                throw new InvalidOperationException($"No se encontró el proveedor con ID {evento.ProveedorId}");
            }
            
            // 3. Crear la factura en el contexto Comercial
            var factura = await CrearFacturaDesdeOrdenCompraAsync(ordenCompra, proveedor, cancellationToken);
            
            // 4. Actualizar estadísticas del proveedor
            await ActualizarEstadisticasProveedorAsync(proveedor, ordenCompra.Total, cancellationToken);
            
            // 5. Enviar notificación al proveedor (si es necesario)
            // Esta lógica podría estar en un manejador de eventos separado
        }
        
        /// <summary>
        /// Crea una factura en el contexto Comercial a partir de una orden de compra del contexto Proveedores
        /// </summary>
        private async Task<Factura> CrearFacturaDesdeOrdenCompraAsync(
            OrdenCompra ordenCompra, 
            Proveedor proveedor,
            CancellationToken cancellationToken)
        {
            // Crear los detalles de la factura
            var detallesFactura = new List<DetalleFactura>();
            
            foreach (var item in ordenCompra.Items)
            {
                // Utilizamos la firma correcta de DetalleFactura.Crear
                var detalle = DetalleFactura.Crear(
                    Guid.Empty, // FacturaId temporal (se actualizará cuando tengamos la factura)
                    item.IngredienteId, // ProductoId
                    item.NombreIngrediente, // Descripción
                    item.Cantidad, // Cantidad
                    item.PrecioUnitario, // PrecioUnitario
                    16, // PorcentajeImpuesto estándar
                    0); // Sin descuento
                    
                detallesFactura.Add(detalle);
            }
            
            // Calcular fecha de vencimiento (30 días después de la fecha de emisión)
            var fechaVencimiento = _dateTimeService.Now.AddDays(30);
            
            // Este servicio de integración necesita usar un servicio de facturación adecuado
            // ya que no tenemos acceso directo al repositorio de facturas
            // Generamos un número de factura simple basado en la orden de compra
            var numeroFactura = $"FC-OC-{ordenCompra.Id.ToString().Substring(0, 8)}";
            
            // En una implementación completa, invocaríamos los métodos adecuados
            // Por ahora, simulamos la creación de una factura usando un servicio de facturación
            var factura = await _servicioFacturacion.GenerarFacturaParaComandaAsync(
                ordenCompra.Id, // Usamos el ID de la orden como sustituto
                TipoFactura.Fiscal,
                proveedor.Nombre,
                proveedor.Id,
                proveedor.RFC,
                proveedor.Direccion,
                "Factura por orden de compra aprobada",
                cancellationToken);
                
            return factura;
        }
        
        /// <summary>
        /// Actualiza las estadísticas del proveedor con la nueva compra
        /// </summary>
        private async Task ActualizarEstadisticasProveedorAsync(
            Proveedor proveedor, 
            decimal montoCompra, 
            CancellationToken cancellationToken)
        {
            // Registrar la fecha de la orden
            proveedor.RegistrarOrden(_dateTimeService.Now);
            await _proveedorRepository.ActualizarAsync(proveedor, cancellationToken);
        }
    }
    
    /// <summary>
    /// Interfaz para el servicio de integración entre Proveedores y Comercial
    /// </summary>
    public interface IProveedoresComercialIntegrationService
    {
        /// <summary>
        /// Procesa una orden de compra aprobada
        /// </summary>
        /// <param name="evento">Evento de orden aprobada</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task ProcesarOrdenCompraAprobadaAsync(
            OrdenCompraAprobada evento,
            CancellationToken cancellationToken = default);
    }
} 