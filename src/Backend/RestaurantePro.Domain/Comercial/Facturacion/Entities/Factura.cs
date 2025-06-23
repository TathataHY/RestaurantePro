namespace RestaurantePro.Domain.Comercial.Facturacion.Entities
{
    /// <summary>
    /// Representa una factura emitida para una comanda o conjunto de comandas.
    /// 
    /// Reglas de negocio:
    /// - Una factura debe estar asociada a al menos una comanda
    /// - Una factura tiene un tipo que determina su formato y requisitos fiscales
    /// - Una factura tiene un estado que refleja su ciclo de vida
    /// - Una factura puede ser anulada pero no eliminada
    /// - Una factura puede tener múltiples pagos asociados
    /// - El total de la factura debe coincidir con la suma de los detalles
    /// </summary>
    public class Factura : EntityBase, IAggregateRoot
    {
        private readonly List<DetalleFactura> _detalles = new();

        /// <summary>
        /// Número único de la factura (puede incluir prefijo según normativa fiscal)
        /// </summary>
        public string NumeroFactura { get; private set; }

        /// <summary>
        /// Tipo de factura
        /// </summary>
        public TipoFactura TipoFactura { get; private set; }

        /// <summary>
        /// Estado actual de la factura
        /// </summary>
        public EstadoFactura Estado { get; private set; }

        /// <summary>
        /// Fecha de emisión de la factura
        /// </summary>
        public DateTime FechaEmision { get; private set; }

        /// <summary>
        /// Fecha de vencimiento para el pago (si aplica)
        /// </summary>
        public DateTime? FechaVencimiento { get; private set; }

        /// <summary>
        /// Fecha en que se pagó completamente la factura (si aplica)
        /// </summary>
        public DateTime? FechaPago { get; private set; }

        /// <summary>
        /// Identificador del cliente al que se emite la factura
        /// </summary>
        public Guid? ClienteId { get; private set; }

        /// <summary>
        /// Nombre del cliente o razón social
        /// </summary>
        public string NombreCliente { get; private set; }

        /// <summary>
        /// RFC o identificador fiscal del cliente
        /// </summary>
        public string? IdentificacionFiscal { get; private set; }

        /// <summary>
        /// Dirección del cliente
        /// </summary>
        public string? DireccionCliente { get; private set; }

        /// <summary>
        /// Subtotal de la factura (antes de impuestos)
        /// </summary>
        public decimal Subtotal { get; private set; }

        /// <summary>
        /// Total de impuestos aplicados
        /// </summary>
        public decimal TotalImpuestos { get; private set; }

        /// <summary>
        /// Total de descuentos aplicados
        /// </summary>
        public decimal TotalDescuentos { get; private set; }

        /// <summary>
        /// Total final de la factura
        /// </summary>
        public decimal Total { get; private set; }

        /// <summary>
        /// Total pagado hasta el momento
        /// </summary>
        public decimal TotalPagado { get; private set; }

        /// <summary>
        /// Motivo de anulación (si aplica)
        /// </summary>
        public string? MotivoAnulacion { get; private set; }

        /// <summary>
        /// Observaciones adicionales
        /// </summary>
        public string? Observaciones { get; private set; }

        /// <summary>
        /// Lista de IDs de comandas asociadas a esta factura
        /// </summary>
        public IReadOnlyList<Guid> ComandasIds { get; private set; } = new List<Guid>();

        /// <summary>
        /// Lista de detalles de la factura
        /// </summary>
        public IReadOnlyCollection<DetalleFactura> Detalles => _detalles.AsReadOnly();

        // 🔥 NAVEGACIONES AGREGADAS para queries más eficientes
        /// <summary>
        /// Navegación hacia la entidad Cliente (si existe)
        /// Se carga usando lazy loading para obtener información completa del cliente
        /// </summary>
        public virtual Cliente? Cliente { get; set; }

        /// <summary>
        /// Navegación hacia las Comandas asociadas a esta factura
        /// Se carga bajo demanda para acceder a los detalles de las comandas
        /// </summary>
        public virtual ICollection<Comanda> Comandas { get; set; } = new List<Comanda>();

        /// <summary>
        /// Navegación hacia los pagos asociados a esta factura
        /// Útil para obtener el historial completo de pagos
        /// </summary>
        public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

        // Constructor privado para EF Core
        private Factura() { }

        /// <summary>
        /// Crea una nueva factura
        /// </summary>
        /// <param name="numeroFactura">Número único de la factura</param>
        /// <param name="tipoFactura">Tipo de factura</param>
        /// <param name="clienteId">ID del cliente (opcional)</param>
        /// <param name="nombreCliente">Nombre del cliente o razón social</param>
        /// <param name="identificacionFiscal">RFC o identificador fiscal (opcional)</param>
        /// <param name="direccionCliente">Dirección del cliente (opcional)</param>
        /// <param name="comandasIds">Lista de IDs de comandas asociadas</param>
        /// <param name="observaciones">Observaciones adicionales (opcional)</param>
        /// <param name="fechaEmision">Fecha de emisión (si no se proporciona, se usa la fecha actual)</param>
        /// <param name="dateTimeService">Servicio de fecha/hora (opcional)</param>
        /// <returns>Una nueva instancia de Factura</returns>
        public static Factura Crear(
            string numeroFactura,
            TipoFactura tipoFactura,
            string nombreCliente,
            Guid? clienteId = null,
            string? identificacionFiscal = null,
            string? direccionCliente = null,
            IEnumerable<Guid>? comandasIds = null,
            string? observaciones = null,
            DateTime? fechaEmision = null,
            IDateTimeService? dateTimeService = null)
        {
            if (string.IsNullOrWhiteSpace(numeroFactura))
            {
                throw new ArgumentException("El número de factura no puede estar vacío", nameof(numeroFactura));
            }

            if (string.IsNullOrWhiteSpace(nombreCliente))
            {
                throw new ArgumentException("El nombre del cliente no puede estar vacío", nameof(nombreCliente));
            }

            // Validar requisitos adicionales según el tipo de factura
            if (tipoFactura == TipoFactura.Fiscal && string.IsNullOrWhiteSpace(identificacionFiscal))
            {
                throw new ArgumentException("El identificador fiscal es obligatorio para facturas fiscales", nameof(identificacionFiscal));
            }

            var fechaActual = dateTimeService?.Now ?? DateTime.Now;
            var fechaEmisionReal = fechaEmision ?? fechaActual;

            var factura = new Factura
            {
                Id = Guid.NewGuid(),
                NumeroFactura = numeroFactura,
                TipoFactura = tipoFactura,
                Estado = EstadoFactura.Borrador,
                FechaEmision = fechaEmisionReal,
                ClienteId = clienteId,
                NombreCliente = nombreCliente,
                IdentificacionFiscal = identificacionFiscal,
                DireccionCliente = direccionCliente,
                Observaciones = observaciones,
                Subtotal = 0,
                TotalImpuestos = 0,
                TotalDescuentos = 0,
                Total = 0,
                TotalPagado = 0
            };

            // Añadir comandas si se proporcionan
            if (comandasIds != null)
            {
                factura.ComandasIds = comandasIds.ToList();
            }

            // Emitir evento de dominio
            factura.AddDomainEvent(new FacturaCreada(factura.Id, factura.NumeroFactura, factura.TipoFactura, fechaEmisionReal));

            return factura;
        }

        /// <summary>
        /// Añade un detalle a la factura
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="descripcion">Descripción del producto</param>
        /// <param name="cantidad">Cantidad</param>
        /// <param name="precioUnitario">Precio unitario</param>
        /// <param name="porcentajeImpuesto">Porcentaje de impuesto aplicable</param>
        /// <param name="porcentajeDescuento">Porcentaje de descuento (opcional)</param>
        /// <returns>El detalle añadido</returns>
        public DetalleFactura AgregarDetalle(
            Guid productoId,
            string descripcion,
            decimal cantidad,
            decimal precioUnitario,
            decimal porcentajeImpuesto,
            decimal porcentajeDescuento = 0)
        {
            if (Estado != EstadoFactura.Borrador)
            {
                throw new InvalidOperationException("Solo se pueden agregar detalles a facturas en estado Borrador");
            }

            // Crear el detalle
            var detalle = DetalleFactura.Crear(
                Id,
                productoId,
                descripcion,
                cantidad,
                precioUnitario,
                porcentajeImpuesto,
                porcentajeDescuento);

            // Agregar a la colección
            _detalles.Add(detalle);

            // Recalcular totales
            RecalcularTotales();

            return detalle;
        }

        /// <summary>
        /// Emite la factura
        /// </summary>
        /// <param name="dateTimeService">Servicio de fecha/hora (opcional)</param>
        /// <param name="diasVencimiento">Días para vencimiento (0 para pago inmediato)</param>
        public void Emitir(IDateTimeService? dateTimeService = null, int diasVencimiento = 0)
        {
            if (Estado != EstadoFactura.Borrador)
            {
                throw new InvalidOperationException($"No se puede emitir una factura en estado {Estado}");
            }

            if (!_detalles.Any())
            {
                throw new InvalidOperationException("No se puede emitir una factura sin detalles");
            }

            var fechaActual = dateTimeService?.Now ?? DateTime.Now;

            // Establecer fecha de vencimiento si aplica
            if (diasVencimiento > 0)
            {
                FechaVencimiento = fechaActual.AddDays(diasVencimiento);
            }

            // Cambiar estado
            Estado = EstadoFactura.Emitida;

            // Emitir evento de dominio
            AddDomainEvent(new FacturaEmitida(Id, NumeroFactura, fechaActual, FechaVencimiento));
        }

        /// <summary>
        /// Anula la factura
        /// </summary>
        /// <param name="motivo">Motivo de la anulación</param>
        /// <param name="dateTimeService">Servicio de fecha/hora (opcional)</param>
        public void Anular(string motivo, IDateTimeService? dateTimeService = null)
        {
            if (Estado == EstadoFactura.Anulada)
            {
                throw new InvalidOperationException("La factura ya está anulada");
            }

            if (string.IsNullOrWhiteSpace(motivo))
            {
                throw new ArgumentException("El motivo de anulación es obligatorio", nameof(motivo));
            }

            var fechaActual = dateTimeService?.Now ?? DateTime.Now;

            // Guardar motivo de anulación
            MotivoAnulacion = motivo;

            // Cambiar estado
            Estado = EstadoFactura.Anulada;

            // Emitir evento de dominio
            AddDomainEvent(new FacturaAnulada(Id, NumeroFactura, motivo, fechaActual));
        }

        /// <summary>
        /// Registra un pago parcial o total de la factura
        /// </summary>
        /// <param name="monto">Monto del pago</param>
        /// <param name="pagoId">ID del pago asociado</param>
        /// <param name="dateTimeService">Servicio de fecha/hora (opcional)</param>
        public void RegistrarPago(decimal monto, Guid pagoId, IDateTimeService? dateTimeService = null)
        {
            if (Estado == EstadoFactura.Anulada)
            {
                throw new InvalidOperationException("No se puede registrar pagos en una factura anulada");
            }

            if (Estado == EstadoFactura.Pagada)
            {
                throw new InvalidOperationException("La factura ya está completamente pagada");
            }

            if (monto <= 0)
            {
                throw new ArgumentException("El monto del pago debe ser mayor que cero", nameof(monto));
            }

            var fechaActual = dateTimeService?.Now ?? DateTime.Now;

            // Calcular nuevo total pagado
            decimal nuevoTotalPagado = TotalPagado + monto;

            // Verificar que no exceda el total
            if (nuevoTotalPagado > Total)
            {
                throw new InvalidOperationException($"El pago excede el total pendiente. Total factura: {Total}, Ya pagado: {TotalPagado}, Nuevo pago: {monto}");
            }

            // Actualizar total pagado
            TotalPagado = nuevoTotalPagado;

            // Actualizar estado según el monto pagado
            if (Math.Abs(TotalPagado - Total) < 0.01m) // Comparación con tolerancia para evitar problemas de redondeo
            {
                Estado = EstadoFactura.Pagada;
                FechaPago = fechaActual;
                AddDomainEvent(new FacturaPagada(Id, NumeroFactura, fechaActual));
            }
            else if (TotalPagado > 0)
            {
                Estado = EstadoFactura.PagadaParcialmente;
            }

            // Emitir evento de pago
            AddDomainEvent(new PagoFacturaRegistrado(Id, NumeroFactura, pagoId, monto, TotalPagado, fechaActual));
        }

        /// <summary>
        /// Recalcula los totales de la factura basados en los detalles
        /// </summary>
        private void RecalcularTotales()
        {
            Subtotal = _detalles.Sum(d => d.Subtotal);
            TotalImpuestos = _detalles.Sum(d => d.ImporteImpuesto);
            TotalDescuentos = _detalles.Sum(d => d.ImporteDescuento);
            Total = Subtotal + TotalImpuestos - TotalDescuentos;
        }

        /// <summary>
        /// Modifica la información fiscal de la factura si aún está en borrador
        /// </summary>
        /// <param name="nombreCliente">Nuevo nombre del cliente</param>
        /// <param name="identificacionFiscal">Nuevo identificador fiscal</param>
        /// <param name="direccionCliente">Nueva dirección</param>
        public void ModificarInformacionFiscal(string nombreCliente, string? identificacionFiscal, string? direccionCliente)
        {
            if (Estado != EstadoFactura.Borrador)
            {
                throw new InvalidOperationException("Solo se puede modificar la información fiscal en facturas en estado Borrador");
            }

            if (string.IsNullOrWhiteSpace(nombreCliente))
            {
                throw new ArgumentException("El nombre del cliente no puede estar vacío", nameof(nombreCliente));
            }

            NombreCliente = nombreCliente;
            IdentificacionFiscal = identificacionFiscal;
            DireccionCliente = direccionCliente;
        }

        /// <summary>
        /// Aplica un descuento a la factura
        /// </summary>
        /// <param name="tipoDescuento">Tipo de descuento (Promocional, Empleado, Volumen, Cortesia)</param>
        /// <param name="montoDescuento">Monto del descuento a aplicar</param>
        /// <param name="concepto">Concepto o descripción del descuento</param>
        /// <param name="motivo">Motivo del descuento</param>
        /// <param name="usuarioAutorizaId">Usuario que autoriza el descuento</param>
        /// <param name="aplicarAntesDeImpuestos">Si el descuento se aplica antes de calcular impuestos</param>
        /// <param name="codigoAutorizacion">Código de autorización (opcional)</param>
        /// <returns>Resultado de la operación</returns>
        public Result AplicarDescuento(
            string tipoDescuento,
            decimal montoDescuento,
            string concepto,
            string motivo,
            Guid usuarioAutorizaId,
            bool aplicarAntesDeImpuestos = false,
            string? codigoAutorizacion = null)
        {
            // Validar que la factura esté en estado borrador
            if (Estado != EstadoFactura.Borrador)
            {
                return Result.Failure("Solo se pueden aplicar descuentos a facturas en estado borrador");
            }

            // Validar parámetros básicos
            if (string.IsNullOrWhiteSpace(tipoDescuento))
            {
                return Result.Failure("El tipo de descuento no puede estar vacío");
            }

            if (montoDescuento <= 0)
            {
                return Result.Failure("El monto del descuento debe ser mayor que cero");
            }

            if (string.IsNullOrWhiteSpace(concepto))
            {
                return Result.Failure("El concepto del descuento no puede estar vacío");
            }

            if (string.IsNullOrWhiteSpace(motivo))
            {
                return Result.Failure("El motivo del descuento no puede estar vacío");
            }

            if (usuarioAutorizaId == Guid.Empty)
            {
                return Result.Failure("Debe especificar el usuario que autoriza el descuento");
            }

            // Validar que el descuento no supere el subtotal actual
            var subtotalActual = _detalles.Sum(d => d.Subtotal);
            if (montoDescuento > subtotalActual)
            {
                return Result.Failure($"El descuento ({montoDescuento:C}) no puede ser mayor al subtotal de la factura ({subtotalActual:C})");
            }

            // Aplicar el descuento agregando un detalle con valor negativo
            var detalleDescuento = DetalleFactura.Crear(
                Id, // facturaId
                Guid.Empty, // Sin producto asociado para descuentos
                $"DESCUENTO - {concepto}",
                1, // cantidad 1
                -montoDescuento, // precio unitario negativo
                aplicarAntesDeImpuestos ? 0 : 16.0m, // impuesto solo si no se aplica antes de impuestos
                0); // sin descuento adicional

            _detalles.Add(detalleDescuento);

            // Recalcular totales
            RecalcularTotales();

            // Registrar evento de dominio
            AddDomainEvent(new DescuentoAplicado(Id, NumeroFactura, tipoDescuento, montoDescuento, concepto, motivo, usuarioAutorizaId, codigoAutorizacion, DateTime.Now));

            return Result.Success();
        }

        /// <summary>
        /// Marca la factura como vencida
        /// </summary>
        /// <param name="dateTimeService">Servicio de fecha/hora (opcional)</param>
        public void MarcarComoVencida(IDateTimeService? dateTimeService = null)
        {
            if (Estado == EstadoFactura.Anulada)
            {
                throw new InvalidOperationException("No se puede marcar como vencida una factura anulada");
            }

            if (Estado == EstadoFactura.Pagada)
            {
                throw new InvalidOperationException("No se puede marcar como vencida una factura pagada");
            }

            if (Estado == EstadoFactura.Vencida)
            {
                throw new InvalidOperationException("La factura ya está marcada como vencida");
            }

            if (!FechaVencimiento.HasValue)
            {
                throw new InvalidOperationException("No se puede marcar como vencida una factura sin fecha de vencimiento");
            }

            var fechaActual = dateTimeService?.Now ?? DateTime.Now;

            if (FechaVencimiento.Value > fechaActual)
            {
                throw new InvalidOperationException("No se puede marcar como vencida una factura cuya fecha de vencimiento es futura");
            }

            // Cambiar estado
            Estado = EstadoFactura.Vencida;

            // Emitir evento de dominio
            AddDomainEvent(new FacturaVencida(Id, NumeroFactura, FechaVencimiento.Value, fechaActual));
        }

        /// <summary>
        /// Modifica las observaciones de la factura si aún está en borrador
        /// </summary>
        /// <param name="observaciones">Nuevas observaciones</param>
        public void ModificarObservaciones(string observaciones)
        {
            if (Estado != EstadoFactura.Borrador)
            {
                throw new InvalidOperationException("Solo se pueden modificar las observaciones en facturas en estado Borrador");
            }
            if (string.IsNullOrWhiteSpace(observaciones))
            {
                throw new ArgumentException("Las observaciones no pueden estar vacías", nameof(observaciones));
            }
            Observaciones = observaciones;
        }
    }
} 