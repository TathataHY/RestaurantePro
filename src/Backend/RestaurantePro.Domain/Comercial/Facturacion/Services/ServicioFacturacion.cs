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
        private readonly INotificationManager _notificationManager;
        private readonly ILogger<FacturaBuilder> _facturaBuilderLogger;

        /// <summary>
        /// Constructor del servicio de facturación
        /// </summary>
        /// <param name="facturaRepository">Repositorio de facturas</param>
        /// <param name="comandaRepository">Repositorio de comandas</param>
        /// <param name="dateTimeService">Servicio de fecha/hora</param>
        /// <param name="notificationManager">Gestor de notificaciones para validaciones</param>
        /// <param name="facturaBuilderLogger">Logger para el FacturaBuilder</param>
        public ServicioFacturacion(
            IFacturaRepository facturaRepository,
            IComandaRepository comandaRepository,
            IDateTimeService dateTimeService,
            INotificationManager notificationManager,
            ILogger<FacturaBuilder> facturaBuilderLogger)
        {
            _facturaRepository = facturaRepository ?? throw new ArgumentNullException(nameof(facturaRepository));
            _comandaRepository = comandaRepository ?? throw new ArgumentNullException(nameof(comandaRepository));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
            _facturaBuilderLogger = facturaBuilderLogger ?? throw new ArgumentNullException(nameof(facturaBuilderLogger));
        }

        /// <inheritdoc />
        public async Task<Result<Factura>> GenerarFacturaParaComandaAsync(
            Guid comandaId,
            TipoFactura tipoFactura,
            string nombreCliente,
            Guid? clienteId = null,
            string? identificacionFiscal = null,
            string? direccionCliente = null,
            string? observaciones = null,
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(comandaId != Guid.Empty, "El ID de la comanda no puede estar vacío", "ComandaId");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(nombreCliente), "El nombre del cliente no puede estar vacío", "NombreCliente");
            _notificationManager.Require(Enum.IsDefined(typeof(TipoFactura), tipoFactura), $"El tipo de factura '{tipoFactura}' no es válido", "TipoFactura");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Factura>(null);
            }
            
            try
            {
                // Verificar que la comanda exista
                var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
                if (comanda == null)
                {
                    _notificationManager.AddError($"No se encontró la comanda con ID {comandaId}", "ComandaId");
                    return _notificationManager.ToResult<Factura>(null);
                }

                // Verificar que la comanda no esté anulada
                if (comanda.Estado == EstadoComanda.Cancelada)
                {
                    _notificationManager.AddError("No se puede generar factura para una comanda anulada", "ComandaEstado");
                    return _notificationManager.ToResult<Factura>(null);
                }

                // Generar número de factura
                var numeroFacturaResult = await GenerarSiguienteNumeroFacturaAsync(null, cancellationToken);
                if (!numeroFacturaResult.Succeeded)
                {
                    return _notificationManager.ToResult<Factura>(null);
                }
                
                var numeroFactura = numeroFacturaResult.Value;

                // Usar FacturaBuilder para crear la factura con validaciones robustas
                var builder = new FacturaBuilder(_notificationManager, _facturaBuilderLogger);
                
                builder
                    .ConNumero(numeroFactura)
                    .DeTipo(tipoFactura)
                    .ParaCliente(nombreCliente, clienteId)
                    .ConInformacionFiscal(identificacionFiscal, direccionCliente)
                    .ConFechaEmision(_dateTimeService.Now)
                    .ConObservaciones(observaciones)
                    .PorComandas(comandaId);

                // Agregar detalles de la comanda al builder antes de construir
                foreach (var item in comanda.Items)
                {
                    builder.AgregarDetalle(
                        item.ProductoId,
                        item.Observaciones ?? $"Producto {item.ProductoId}",
                        item.Cantidad,
                        item.PrecioUnitario,
                        16.0m,  // IVA fijo del 16% - en una implementación real esto podría obtenerse del producto o de configuración
                        0m);    // No hay descuento por ítem
                }

                // Construir la factura con todos los detalles
                var resultadoFactura = builder.Construir();
                
                if (!resultadoFactura.Succeeded)
                {
                    return resultadoFactura; // Ya tiene los errores del builder
                }
                
                var factura = resultadoFactura.Value!;

                // Guardar la factura
                await _facturaRepository.AgregarAsync(factura, cancellationToken);
                await _facturaRepository.GuardarCambiosAsync(cancellationToken);

                return Result.Success(factura);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al generar factura: {ex.Message}", "GenerarFactura");
                return _notificationManager.ToResult<Factura>(null);
            }
        }

        /// <inheritdoc />
        public async Task<Result<Factura>> GenerarFacturaParaComandasAsync(
            IEnumerable<Guid> comandasIds,
            TipoFactura tipoFactura,
            string nombreCliente,
            Guid? clienteId = null,
            string? identificacionFiscal = null,
            string? direccionCliente = null,
            string? observaciones = null,
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(comandasIds != null && comandasIds.Any(), "Debe proporcionar al menos una comanda", "ComandasIds");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(nombreCliente), "El nombre del cliente no puede estar vacío", "NombreCliente");
            _notificationManager.Require(Enum.IsDefined(typeof(TipoFactura), tipoFactura), $"El tipo de factura '{tipoFactura}' no es válido", "TipoFactura");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Factura>(null);
            }
            
            try
            {
                // Obtener todas las comandas
                var comandas = new List<Comanda>();
                foreach (var comandaId in comandasIds)
                {
                    if (comandaId == Guid.Empty)
                    {
                        _notificationManager.AddError("El ID de la comanda no puede estar vacío", "ComandaId");
                        continue;
                    }
                    
                    var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
                    if (comanda == null)
                    {
                        _notificationManager.AddError($"No se encontró la comanda con ID {comandaId}", "ComandaId");
                        continue;
                    }

                    // Verificar que la comanda no esté anulada
                    if (comanda.Estado == EstadoComanda.Cancelada)
                    {
                        _notificationManager.AddError($"No se puede generar factura para una comanda anulada (ID: {comandaId})", "ComandaEstado");
                        continue;
                    }

                    comandas.Add(comanda);
                }
                
                if (_notificationManager.HasErrors || !comandas.Any())
                {
                    if (!comandas.Any() && !_notificationManager.HasErrors)
                    {
                        _notificationManager.AddError("No se encontraron comandas válidas para facturar", "Comandas");
                    }
                    return _notificationManager.ToResult<Factura>(null);
                }

                // Generar número de factura
                var numeroFacturaResult = await GenerarSiguienteNumeroFacturaAsync(null, cancellationToken);
                if (!numeroFacturaResult.Succeeded)
                {
                    return _notificationManager.ToResult<Factura>(null);
                }
                
                var numeroFactura = numeroFacturaResult.Value;

                // Usar FacturaBuilder para crear la factura con validaciones robustas
                var builder = new FacturaBuilder(_notificationManager, _facturaBuilderLogger);
                
                builder
                    .ConNumero(numeroFactura)
                    .DeTipo(tipoFactura)
                    .ParaCliente(nombreCliente, clienteId)
                    .ConInformacionFiscal(identificacionFiscal, direccionCliente)
                    .ConFechaEmision(_dateTimeService.Now)
                    .ConObservaciones(observaciones)
                    .PorComandas(comandas.Select(c => c.Id).ToArray());

                // Agregar detalles de todas las comandas al builder antes de construir
                foreach (var comanda in comandas)
                {
                    foreach (var item in comanda.Items)
                    {
                        builder.AgregarDetalle(
                            item.ProductoId,
                            item.Observaciones ?? $"Producto {item.ProductoId}",
                            item.Cantidad,
                            item.PrecioUnitario,
                            16.0m,  // IVA fijo del 16%
                            0m);    // No hay descuento por ítem
                    }
                }

                // Construir la factura con todos los detalles
                var resultadoFactura = builder.Construir();
                
                if (!resultadoFactura.Succeeded)
                {
                    return resultadoFactura; // Ya tiene los errores del builder
                }
                
                var factura = resultadoFactura.Value!;

                // Guardar la factura
                await _facturaRepository.AgregarAsync(factura, cancellationToken);
                await _facturaRepository.GuardarCambiosAsync(cancellationToken);

                return Result.Success(factura);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al generar factura: {ex.Message}", "GenerarFactura");
                return _notificationManager.ToResult<Factura>(null);
            }
        }

        /// <inheritdoc />
        public async Task<Result<Factura>> EmitirFacturaAsync(
            Guid facturaId,
            int diasVencimiento = 0,
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(facturaId != Guid.Empty, "El ID de la factura no puede estar vacío", "FacturaId");
            _notificationManager.Require(diasVencimiento >= 0, "Los días de vencimiento no pueden ser negativos", "DiasVencimiento");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Factura>(null);
            }
            
            try
            {
                // Obtener la factura
                var factura = await _facturaRepository.ObtenerPorIdAsync(facturaId, cancellationToken);
                if (factura == null)
                {
                    _notificationManager.AddError($"No se encontró la factura con ID {facturaId}", "FacturaId");
                    return _notificationManager.ToResult<Factura>(null);
                }

                try
                {
                    // Emitir la factura
                    factura.Emitir(_dateTimeService, diasVencimiento);
                }
                catch (InvalidOperationException ex)
                {
                    _notificationManager.AddError(ex.Message, "EmitirFactura");
                    return _notificationManager.ToResult<Factura>(null);
                }

                // Guardar cambios
                await _facturaRepository.GuardarCambiosAsync(cancellationToken);

                return Result.Success(factura);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al emitir factura: {ex.Message}", "EmitirFactura");
                return _notificationManager.ToResult<Factura>(null);
            }
        }

        /// <inheritdoc />
        public async Task<Result<Factura>> AnularFacturaAsync(
            Guid facturaId,
            string motivo,
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(facturaId != Guid.Empty, "El ID de la factura no puede estar vacío", "FacturaId");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(motivo), "El motivo de anulación no puede estar vacío", "Motivo");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Factura>(null);
            }
            
            try
            {
                // Obtener la factura
                var factura = await _facturaRepository.ObtenerPorIdAsync(facturaId, cancellationToken);
                if (factura == null)
                {
                    _notificationManager.AddError($"No se encontró la factura con ID {facturaId}", "FacturaId");
                    return _notificationManager.ToResult<Factura>(null);
                }

                try
                {
                    // Anular la factura
                    factura.Anular(motivo, _dateTimeService);
                }
                catch (InvalidOperationException ex)
                {
                    _notificationManager.AddError(ex.Message, "AnularFactura");
                    return _notificationManager.ToResult<Factura>(null);
                }

                // Guardar cambios
                await _facturaRepository.GuardarCambiosAsync(cancellationToken);

                return Result.Success(factura);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al anular factura: {ex.Message}", "AnularFactura");
                return _notificationManager.ToResult<Factura>(null);
            }
        }

        /// <inheritdoc />
        public async Task<Result<Factura>> RegistrarPagoFacturaAsync(
            Guid facturaId,
            Guid pagoId,
            decimal monto,
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(facturaId != Guid.Empty, "El ID de la factura no puede estar vacío", "FacturaId");
            _notificationManager.Require(pagoId != Guid.Empty, "El ID del pago no puede estar vacío", "PagoId");
            _notificationManager.Require(monto > 0, "El monto debe ser mayor a cero", "Monto");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Factura>(null);
            }
            
            try
            {
                // Obtener la factura
                var factura = await _facturaRepository.ObtenerPorIdAsync(facturaId, cancellationToken);
                if (factura == null)
                {
                    _notificationManager.AddError($"No se encontró la factura con ID {facturaId}", "FacturaId");
                    return _notificationManager.ToResult<Factura>(null);
                }

                try
                {
                    // Registrar el pago
                    factura.RegistrarPago(monto, pagoId, _dateTimeService);
                }
                catch (InvalidOperationException ex)
                {
                    _notificationManager.AddError(ex.Message, "RegistrarPago");
                    return _notificationManager.ToResult<Factura>(null);
                }

                // Guardar cambios
                await _facturaRepository.GuardarCambiosAsync(cancellationToken);

                return Result.Success(factura);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al registrar pago: {ex.Message}", "RegistrarPago");
                return _notificationManager.ToResult<Factura>(null);
            }
        }

        /// <inheritdoc />
        public async Task<Result<string>> GenerarSiguienteNumeroFacturaAsync(
            string? prefijo = null,
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            try
            {
                // Usar el repositorio para obtener el siguiente número
                var numeroFactura = await _facturaRepository.ObtenerSiguienteNumeroFacturaAsync(prefijo, cancellationToken);
                
                if (string.IsNullOrEmpty(numeroFactura))
                {
                    _notificationManager.AddError("No se pudo generar un número de factura válido", "NumeroFactura");
                    return _notificationManager.ToResult<string>(string.Empty);
                }
                
                return Result.Success(numeroFactura);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al generar número de factura: {ex.Message}", "NumeroFactura");
                return _notificationManager.ToResult<string>(string.Empty);
            }
        }

        /// <inheritdoc />
        public async Task<Result<Factura>> AplicarDescuentoAsync(
            Guid facturaId,
            string tipoDescuento,
            decimal montoDescuento,
            string concepto,
            string motivo,
            Guid usuarioAutorizaId,
            bool aplicarAntesDeImpuestos = false,
            string? codigoAutorizacion = null,
            CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(facturaId != Guid.Empty, "El ID de la factura no puede estar vacío", "FacturaId");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(tipoDescuento), "El tipo de descuento no puede estar vacío", "TipoDescuento");
            _notificationManager.Require(montoDescuento > 0, "El monto del descuento debe ser mayor a cero", "MontoDescuento");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(concepto), "El concepto del descuento no puede estar vacío", "Concepto");
            _notificationManager.Require(!string.IsNullOrWhiteSpace(motivo), "El motivo del descuento no puede estar vacío", "Motivo");
            _notificationManager.Require(usuarioAutorizaId != Guid.Empty, "El ID del usuario que autoriza no puede estar vacío", "UsuarioAutorizaId");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<Factura>(null);
            }
            
            try
            {
                // Obtener la factura
                var factura = await _facturaRepository.ObtenerPorIdAsync(facturaId, cancellationToken);
                if (factura == null)
                {
                    _notificationManager.AddError($"No se encontró la factura con ID {facturaId}", "FacturaId");
                    return _notificationManager.ToResult<Factura>(null);
                }

                // Verificar que la factura esté en estado borrador
                if (factura.Estado != EstadoFactura.Borrador)
                {
                    _notificationManager.AddError("Solo se pueden aplicar descuentos a facturas en estado borrador", "FacturaEstado");
                    return _notificationManager.ToResult<Factura>(null);
                }

                // Validar que el monto del descuento no supere el subtotal de la factura
                var subtotal = factura.Detalles.Sum(d => d.Subtotal);
                if (montoDescuento > subtotal)
                {
                    _notificationManager.AddError($"El monto del descuento ({montoDescuento:C}) no puede ser mayor al subtotal de la factura ({subtotal:C})", "MontoDescuento");
                    return _notificationManager.ToResult<Factura>(null);
                }

                // Validar tipos de descuento permitidos
                var tiposPermitidos = new[] { "Promocional", "Empleado", "Volumen", "Cortesia" };
                if (!tiposPermitidos.Contains(tipoDescuento, StringComparer.OrdinalIgnoreCase))
                {
                    _notificationManager.AddError($"El tipo de descuento '{tipoDescuento}' no es válido. Tipos permitidos: {string.Join(", ", tiposPermitidos)}", "TipoDescuento");
                    return _notificationManager.ToResult<Factura>(null);
                }

                // Aplicar el descuento usando el método de la entidad
                var resultadoDescuento = factura.AplicarDescuento(
                    tipoDescuento,
                    montoDescuento,
                    concepto,
                    motivo,
                    usuarioAutorizaId,
                    aplicarAntesDeImpuestos,
                    codigoAutorizacion);

                if (!resultadoDescuento.Succeeded)
                {
                    _notificationManager.AddError(resultadoDescuento.Error ?? "Error al aplicar descuento", "AplicarDescuento");
                    return _notificationManager.ToResult<Factura>(null);
                }

                // Actualizar la factura en el repositorio
                await _facturaRepository.ActualizarAsync(factura, cancellationToken);
                await _facturaRepository.GuardarCambiosAsync(cancellationToken);

                return Result.Success(factura);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al aplicar descuento: {ex.Message}", "AplicarDescuento");
                return _notificationManager.ToResult<Factura>(null);
            }
        }
    }
} 