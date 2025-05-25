namespace RestaurantePro.Domain.Comercial.EventHandlers
{
    /// <summary>
    /// Manejador de eventos que procesa el evento OrdenCompraAprobada para crear facturas
    /// </summary>
    public class OrdenCompraAprobada_FacturacionHandler : IDomainEventHandler<OrdenCompraAprobada>
    {
        private readonly IOrdenCompraRepository _ordenCompraRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IFacturaRepository _facturaRepository;
        private readonly IDateTimeService _dateTimeService;
        
        /// <summary>
        /// Constructor con inyección de dependencias
        /// </summary>
        public OrdenCompraAprobada_FacturacionHandler(
            IOrdenCompraRepository ordenCompraRepository,
            IProveedorRepository proveedorRepository,
            IFacturaRepository facturaRepository,
            IDateTimeService dateTimeService)
        {
            _ordenCompraRepository = ordenCompraRepository ?? throw new ArgumentNullException(nameof(ordenCompraRepository));
            _proveedorRepository = proveedorRepository ?? throw new ArgumentNullException(nameof(proveedorRepository));
            _facturaRepository = facturaRepository ?? throw new ArgumentNullException(nameof(facturaRepository));
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
            
            // 3. Crear los detalles de la factura
            var detallesFactura = new List<DetalleFactura>();
            
            foreach (var item in ordenCompra.Items)
            {
                // Nota: DetalleFactura.Crear requiere parámetros adicionales según su implementación actual
                var detalle = DetalleFactura.Crear(
                    Guid.Empty, // FacturaId temporal
                    item.IngredienteId, // ProductoId
                    item.NombreIngrediente, // Descripción
                    item.Cantidad, // Cantidad
                    item.PrecioUnitario, // PrecioUnitario
                    16, // PorcentajeImpuesto (fijo para este ejemplo)
                    0); // PorcentajeDescuento
                    
                detallesFactura.Add(detalle);
            }
            
            // 4. Calcular fecha de vencimiento (30 días después de la fecha de emisión)
            var fechaVencimiento = _dateTimeService.Now.AddDays(30);
            
            // 5. Crear la factura
            var numeroFactura = $"FC-OC-{ordenCompra.Id.ToString().Substring(0, 8)}";
            var factura = Factura.Crear(
                numeroFactura,
                TipoFactura.Fiscal,
                proveedor.Nombre,
                observaciones: "Factura por orden de compra aprobada",
                fechaEmision: _dateTimeService.Now,
                dateTimeService: _dateTimeService);
                
            // 6. Establecer fecha de vencimiento
            factura.Emitir(_dateTimeService, 30);
            
            // 7. Después de emitir la factura, ya tiene un ID, así que podemos actualizar los detalles con ese ID
            foreach (var detalle in detallesFactura)
            {
                // Como DetalleFactura es una entidad interna al agregado Factura, 
                // lo correcto es usar el método AgregarDetalle de la entidad Factura
                factura.AgregarDetalle(
                    detalle.ProductoId,
                    detalle.Descripcion,
                    detalle.Cantidad,
                    detalle.PrecioUnitario,
                    16.0m, // Porcentaje de impuesto fijo para órdenes de compra
                    0.0m); // Sin descuento para órdenes de compra
            }
            
            // 7. Persistir la factura
            await _facturaRepository.AgregarAsync(factura, cancellationToken);
        }
    }
} 