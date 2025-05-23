namespace RestaurantePro.Domain.Comercial.Facturacion.Services
{
    /// <summary>
    /// Implementación del servicio de facturación
    /// </summary>
    public class ServicioFacturacion : IServicioFacturacion
    {
        private readonly IFacturaRepository _facturaRepository;
        private readonly IComandaRepository _comandaRepository;
        private readonly IDateTimeService _dateTimeService;

        /// <summary>
        /// Constructor del servicio de facturación
        /// </summary>
        /// <param name="facturaRepository">Repositorio de facturas</param>
        /// <param name="comandaRepository">Repositorio de comandas</param>
        /// <param name="dateTimeService">Servicio de fecha/hora</param>
        public ServicioFacturacion(
            IFacturaRepository facturaRepository,
            IComandaRepository comandaRepository,
            IDateTimeService dateTimeService)
        {
            _facturaRepository = facturaRepository ?? throw new ArgumentNullException(nameof(facturaRepository));
            _comandaRepository = comandaRepository ?? throw new ArgumentNullException(nameof(comandaRepository));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }

        /// <inheritdoc />
        public async Task<Factura> GenerarFacturaParaComandaAsync(
            Guid comandaId,
            TipoFactura tipoFactura,
            string nombreCliente,
            Guid? clienteId = null,
            string? identificacionFiscal = null,
            string? direccionCliente = null,
            string? observaciones = null,
            CancellationToken cancellationToken = default)
        {
            // Verificar que la comanda exista
            var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
            if (comanda == null)
            {
                throw new InvalidOperationException($"No se encontró la comanda con ID {comandaId}");
            }

            // Verificar que la comanda no esté anulada
            if (comanda.Estado == EstadoComanda.Cancelada)
            {
                throw new InvalidOperationException($"No se puede generar factura para una comanda anulada");
            }

            // Generar número de factura
            var numeroFactura = await GenerarSiguienteNumeroFacturaAsync(null, cancellationToken);

            // Crear la factura
            var factura = Factura.Crear(
                numeroFactura,
                tipoFactura,
                nombreCliente,
                clienteId,
                identificacionFiscal,
                direccionCliente,
                new List<Guid> { comandaId },
                observaciones,
                _dateTimeService.Now,
                _dateTimeService);

            // Agregar detalles de la comanda a la factura
            foreach (var item in comanda.Items)
            {
                // Para cada ítem de la comanda, crear un detalle de factura
                factura.AgregarDetalle(
                    item.ProductoId,
                    item.Observaciones ?? $"Producto {item.ProductoId}",
                    item.Cantidad,
                    item.PrecioUnitario,
                    16.0m,  // IVA fijo del 16% - en una implementación real esto podría obtenerse del producto o de configuración
                    0m);    // No hay descuento por ítem
            }

            // Guardar la factura
            await _facturaRepository.AgregarAsync(factura, cancellationToken);
            await _facturaRepository.GuardarCambiosAsync(cancellationToken);

            return factura;
        }

        /// <inheritdoc />
        public async Task<Factura> GenerarFacturaParaComandasAsync(
            IEnumerable<Guid> comandasIds,
            TipoFactura tipoFactura,
            string nombreCliente,
            Guid? clienteId = null,
            string? identificacionFiscal = null,
            string? direccionCliente = null,
            string? observaciones = null,
            CancellationToken cancellationToken = default)
        {
            if (comandasIds == null || !comandasIds.Any())
            {
                throw new ArgumentException("Debe proporcionar al menos una comanda", nameof(comandasIds));
            }

            // Obtener todas las comandas
            var comandas = new List<Comanda>();
            foreach (var comandaId in comandasIds)
            {
                var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
                if (comanda == null)
                {
                    throw new InvalidOperationException($"No se encontró la comanda con ID {comandaId}");
                }

                // Verificar que la comanda no esté anulada
                if (comanda.Estado == EstadoComanda.Cancelada)
                {
                    throw new InvalidOperationException($"No se puede generar factura para una comanda anulada (ID: {comandaId})");
                }

                comandas.Add(comanda);
            }

            // Generar número de factura
            var numeroFactura = await GenerarSiguienteNumeroFacturaAsync(null, cancellationToken);

            // Crear la factura
            var factura = Factura.Crear(
                numeroFactura,
                tipoFactura,
                nombreCliente,
                clienteId,
                identificacionFiscal,
                direccionCliente,
                comandasIds,
                observaciones,
                _dateTimeService.Now,
                _dateTimeService);

            // Agregar detalles de todas las comandas a la factura
            foreach (var comanda in comandas)
            {
                foreach (var item in comanda.Items)
                {
                    // Para cada ítem de cada comanda, crear un detalle de factura
                    factura.AgregarDetalle(
                        item.ProductoId,
                        item.Observaciones ?? $"Producto {item.ProductoId}",
                        item.Cantidad,
                        item.PrecioUnitario,
                        16.0m,  // IVA fijo del 16%
                        0m);    // No hay descuento por ítem
                }
            }

            // Guardar la factura
            await _facturaRepository.AgregarAsync(factura, cancellationToken);
            await _facturaRepository.GuardarCambiosAsync(cancellationToken);

            return factura;
        }

        /// <inheritdoc />
        public async Task<Factura> EmitirFacturaAsync(
            Guid facturaId,
            int diasVencimiento = 0,
            CancellationToken cancellationToken = default)
        {
            // Obtener la factura
            var factura = await _facturaRepository.ObtenerPorIdAsync(facturaId, cancellationToken);
            if (factura == null)
            {
                throw new InvalidOperationException($"No se encontró la factura con ID {facturaId}");
            }

            // Emitir la factura
            factura.Emitir(_dateTimeService, diasVencimiento);

            // Guardar cambios
            await _facturaRepository.GuardarCambiosAsync(cancellationToken);

            return factura;
        }

        /// <inheritdoc />
        public async Task<Factura> AnularFacturaAsync(
            Guid facturaId,
            string motivo,
            CancellationToken cancellationToken = default)
        {
            // Obtener la factura
            var factura = await _facturaRepository.ObtenerPorIdAsync(facturaId, cancellationToken);
            if (factura == null)
            {
                throw new InvalidOperationException($"No se encontró la factura con ID {facturaId}");
            }

            // Anular la factura
            factura.Anular(motivo, _dateTimeService);

            // Guardar cambios
            await _facturaRepository.GuardarCambiosAsync(cancellationToken);

            return factura;
        }

        /// <inheritdoc />
        public async Task<Factura> RegistrarPagoFacturaAsync(
            Guid facturaId,
            Guid pagoId,
            decimal monto,
            CancellationToken cancellationToken = default)
        {
            // Obtener la factura
            var factura = await _facturaRepository.ObtenerPorIdAsync(facturaId, cancellationToken);
            if (factura == null)
            {
                throw new InvalidOperationException($"No se encontró la factura con ID {facturaId}");
            }

            // Registrar el pago
            factura.RegistrarPago(monto, pagoId, _dateTimeService);

            // Guardar cambios
            await _facturaRepository.GuardarCambiosAsync(cancellationToken);

            return factura;
        }

        /// <inheritdoc />
        public async Task<string> GenerarSiguienteNumeroFacturaAsync(
            string? prefijo = null,
            CancellationToken cancellationToken = default)
        {
            // Usar el repositorio para obtener el siguiente número
            return await _facturaRepository.ObtenerSiguienteNumeroFacturaAsync(prefijo, cancellationToken);
        }
    }
} 