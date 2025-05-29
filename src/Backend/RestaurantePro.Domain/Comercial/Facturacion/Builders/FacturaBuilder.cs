namespace RestaurantePro.Domain.Comercial.Facturacion.Builders;

/// <summary>
/// Builder para construir instancias de Factura paso a paso con validaciones fluidas.
/// Permite crear facturas complejas de manera segura y legible, validando tipos,
/// detalles e información fiscal requerida.
/// </summary>
public class FacturaBuilder
{
    private string? _numeroFactura;
    private TipoFactura? _tipoFactura;
    private string? _nombreCliente;
    private Guid? _clienteId;
    private string? _identificacionFiscal;
    private string? _direccionCliente;
    private List<Guid>? _comandasIds;
    private string? _observaciones;
    private DateTime? _fechaEmision;
    private readonly List<DetalleItem> _detalles = new();
    private readonly INotificationManager _notificationManager;
    private readonly ILogger<FacturaBuilder> _logger;

    /// <summary>
    /// Elemento interno para almacenar temporalmente los detalles de factura
    /// </summary>
    private class DetalleItem
    {
        public Guid ProductoId { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PorcentajeImpuesto { get; set; }
        public decimal PorcentajeDescuento { get; set; }
    }

    /// <summary>
    /// Constructor del builder
    /// </summary>
    /// <param name="notificationManager">Manager de notificaciones</param>
    /// <param name="logger">Logger para eventos del builder</param>
    public FacturaBuilder(INotificationManager notificationManager, ILogger<FacturaBuilder> logger)
    {
        _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Establece el número de factura
    /// </summary>
    /// <param name="numeroFactura">Número único de la factura</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public FacturaBuilder ConNumero(string numeroFactura)
    {
        if (string.IsNullOrWhiteSpace(numeroFactura))
        {
            _notificationManager.AddError("El número de factura es obligatorio", "NumeroFactura");
            _logger.LogWarning("Intento de asignar número de factura vacío");
            return this;
        }

        if (numeroFactura.Length > 50)
        {
            _notificationManager.AddError("El número de factura no puede exceder 50 caracteres", "NumeroFactura");
            _logger.LogWarning("Intento de asignar número de factura muy largo: {Numero}", numeroFactura);
            return this;
        }

        _numeroFactura = numeroFactura;
        _logger.LogDebug("Número de factura {Numero} asignado", numeroFactura);
        return this;
    }

    /// <summary>
    /// Establece el tipo de factura
    /// </summary>
    /// <param name="tipoFactura">Tipo de factura</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public FacturaBuilder DeTipo(TipoFactura tipoFactura)
    {
        if (!Enum.IsDefined(typeof(TipoFactura), tipoFactura))
        {
            _notificationManager.AddError("El tipo de factura no es válido", "TipoFactura");
            _logger.LogWarning("Intento de asignar tipo de factura inválido: {Tipo}", tipoFactura);
            return this;
        }

        _tipoFactura = tipoFactura;
        _logger.LogDebug("Tipo de factura {Tipo} asignado", tipoFactura);
        return this;
    }

    /// <summary>
    /// Establece la información del cliente
    /// </summary>
    /// <param name="nombreCliente">Nombre del cliente</param>
    /// <param name="clienteId">ID del cliente (opcional)</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public FacturaBuilder ParaCliente(string nombreCliente, Guid? clienteId = null)
    {
        if (string.IsNullOrWhiteSpace(nombreCliente))
        {
            _notificationManager.AddError("El nombre del cliente es obligatorio", "NombreCliente");
            _logger.LogWarning("Intento de asignar nombre de cliente vacío");
            return this;
        }

        if (nombreCliente.Length > 200)
        {
            _notificationManager.AddError("El nombre del cliente no puede exceder 200 caracteres", "NombreCliente");
            _logger.LogWarning("Intento de asignar nombre de cliente muy largo");
            return this;
        }

        _nombreCliente = nombreCliente;
        _clienteId = clienteId;
        _logger.LogDebug("Cliente asignado: {Nombre}", nombreCliente);
        return this;
    }

    /// <summary>
    /// Establece la información fiscal del cliente
    /// </summary>
    /// <param name="identificacionFiscal">RUT, RFC o identificación fiscal</param>
    /// <param name="direccionCliente">Dirección del cliente</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public FacturaBuilder ConInformacionFiscal(string? identificacionFiscal, string? direccionCliente = null)
    {
        if (!string.IsNullOrWhiteSpace(identificacionFiscal))
        {
            if (identificacionFiscal.Length > 50)
            {
                _notificationManager.AddError("La identificación fiscal no puede exceder 50 caracteres", "IdentificacionFiscal");
                _logger.LogWarning("Intento de asignar identificación fiscal muy larga");
                return this;
            }
            _identificacionFiscal = identificacionFiscal;
        }

        if (!string.IsNullOrWhiteSpace(direccionCliente))
        {
            if (direccionCliente.Length > 300)
            {
                _notificationManager.AddError("La dirección no puede exceder 300 caracteres", "DireccionCliente");
                _logger.LogWarning("Intento de asignar dirección muy larga");
                return this;
            }
            _direccionCliente = direccionCliente;
        }

        _logger.LogDebug("Información fiscal asignada");
        return this;
    }

    /// <summary>
    /// Asocia comandas a la factura
    /// </summary>
    /// <param name="comandasIds">IDs de las comandas</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public FacturaBuilder PorComandas(params Guid[] comandasIds)
    {
        if (comandasIds != null && comandasIds.Length > 0)
        {
            // Validar que no hay IDs vacíos
            var idsVacios = comandasIds.Where(id => id == Guid.Empty).ToList();
            if (idsVacios.Any())
            {
                _notificationManager.AddError("Las comandas asociadas no pueden tener IDs vacíos", "ComandasIds");
                _logger.LogWarning("Intento de asociar comandas con IDs vacíos");
                return this;
            }

            _comandasIds = comandasIds.ToList();
            _logger.LogDebug("Asociadas {Cantidad} comandas a la factura", comandasIds.Length);
        }

        return this;
    }

    /// <summary>
    /// Establece la fecha de emisión
    /// </summary>
    /// <param name="fechaEmision">Fecha de emisión</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public FacturaBuilder ConFechaEmision(DateTime fechaEmision)
    {
        // La fecha de emisión no puede ser futura
        if (fechaEmision.Date > DateTime.Now.Date)
        {
            _notificationManager.AddError("La fecha de emisión no puede ser futura", "FechaEmision");
            _logger.LogWarning("Intento de asignar fecha de emisión futura: {Fecha}", fechaEmision);
            return this;
        }

        // La fecha de emisión no puede ser muy antigua (más de 1 año)
        if (fechaEmision.Date < DateTime.Now.Date.AddYears(-1))
        {
            _notificationManager.AddError("La fecha de emisión no puede ser mayor a 1 año", "FechaEmision");
            _logger.LogWarning("Intento de asignar fecha de emisión muy antigua: {Fecha}", fechaEmision);
            return this;
        }

        _fechaEmision = fechaEmision;
        _logger.LogDebug("Fecha de emisión {Fecha} asignada", fechaEmision.Date);
        return this;
    }

    /// <summary>
    /// Agrega un detalle a la factura
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <param name="descripcion">Descripción del producto</param>
    /// <param name="cantidad">Cantidad</param>
    /// <param name="precioUnitario">Precio unitario</param>
    /// <param name="porcentajeImpuesto">Porcentaje de impuesto</param>
    /// <param name="porcentajeDescuento">Porcentaje de descuento (opcional)</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public FacturaBuilder AgregarDetalle(
        Guid productoId,
        string descripcion,
        decimal cantidad,
        decimal precioUnitario,
        decimal porcentajeImpuesto,
        decimal porcentajeDescuento = 0)
    {
        if (productoId == Guid.Empty)
        {
            _notificationManager.AddError("El ID del producto no puede estar vacío", "ProductoId");
            _logger.LogWarning("Intento de agregar detalle con producto ID vacío");
            return this;
        }

        if (string.IsNullOrWhiteSpace(descripcion))
        {
            _notificationManager.AddError("La descripción del producto es obligatoria", "Descripcion");
            _logger.LogWarning("Intento de agregar detalle sin descripción");
            return this;
        }

        if (descripcion.Length > 200)
        {
            _notificationManager.AddError("La descripción no puede exceder 200 caracteres", "Descripcion");
            _logger.LogWarning("Intento de agregar detalle con descripción muy larga");
            return this;
        }

        if (cantidad <= 0)
        {
            _notificationManager.AddError("La cantidad debe ser mayor que cero", "Cantidad");
            _logger.LogWarning("Intento de agregar detalle con cantidad inválida: {Cantidad}", cantidad);
            return this;
        }

        if (precioUnitario < 0)
        {
            _notificationManager.AddError("El precio unitario no puede ser negativo", "PrecioUnitario");
            _logger.LogWarning("Intento de agregar detalle con precio negativo: {Precio}", precioUnitario);
            return this;
        }

        if (porcentajeImpuesto < 0 || porcentajeImpuesto > 100)
        {
            _notificationManager.AddError("El porcentaje de impuesto debe estar entre 0 y 100", "PorcentajeImpuesto");
            _logger.LogWarning("Intento de agregar detalle con impuesto inválido: {Impuesto}", porcentajeImpuesto);
            return this;
        }

        if (porcentajeDescuento < 0 || porcentajeDescuento > 100)
        {
            _notificationManager.AddError("El porcentaje de descuento debe estar entre 0 y 100", "PorcentajeDescuento");
            _logger.LogWarning("Intento de agregar detalle con descuento inválido: {Descuento}", porcentajeDescuento);
            return this;
        }

        // Verificar si ya existe un detalle para el mismo producto
        var detalleExistente = _detalles.FirstOrDefault(d => d.ProductoId == productoId);
        if (detalleExistente != null)
        {
            _notificationManager.AddError("Ya existe un detalle para este producto. Use ModificarDetalle si desea cambiar la cantidad", "ProductoDuplicado");
            _logger.LogWarning("Intento de agregar detalle duplicado para producto: {ProductoId}", productoId);
            return this;
        }

        _detalles.Add(new DetalleItem
        {
            ProductoId = productoId,
            Descripcion = descripcion,
            Cantidad = cantidad,
            PrecioUnitario = precioUnitario,
            PorcentajeImpuesto = porcentajeImpuesto,
            PorcentajeDescuento = porcentajeDescuento
        });

        _logger.LogDebug("Detalle agregado para producto {ProductoId}: {Cantidad} x {Precio}",
            productoId, cantidad, precioUnitario);
        return this;
    }

    /// <summary>
    /// Establece observaciones adicionales
    /// </summary>
    /// <param name="observaciones">Observaciones o notas</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public FacturaBuilder ConObservaciones(string observaciones)
    {
        if (!string.IsNullOrWhiteSpace(observaciones))
        {
            if (observaciones.Length > 1000)
            {
                _notificationManager.AddError("Las observaciones no pueden exceder 1000 caracteres", "Observaciones");
                _logger.LogWarning("Intento de asignar observaciones muy largas");
                return this;
            }

            _observaciones = observaciones;
            _logger.LogDebug("Observaciones agregadas: {Observaciones}", observaciones[..Math.Min(50, observaciones.Length)]);
        }
        return this;
    }

    /// <summary>
    /// Construye la factura validando todos los datos
    /// </summary>
    /// <returns>Resultado con la factura creada o errores de validación</returns>
    public Result<Factura> Construir()
    {
        // NO limpiar notificaciones si ya hay errores acumulados
        if (!_notificationManager.HasErrors)
        {
            _notificationManager.CreateNewNotification();
        }

        try
        {
            // Validaciones obligatorias
            ValidarDatosObligatorios();

            // Si hay errores, retornar resultado fallido
            if (_notificationManager.HasErrors)
            {
                _logger.LogWarning("No se pudo construir la factura debido a errores de validación");
                return Result.Failure<Factura>("Errores de validación en la construcción de la factura");
            }

            // Crear la factura usando el método de fábrica
            var factura = Factura.Crear(
                _numeroFactura!,
                _tipoFactura!.Value,
                _nombreCliente!,
                _clienteId,
                _identificacionFiscal,
                _direccionCliente,
                _comandasIds,
                _observaciones,
                _fechaEmision);

            // Agregar todos los detalles
            foreach (var detalle in _detalles)
            {
                factura.AgregarDetalle(
                    detalle.ProductoId,
                    detalle.Descripcion,
                    detalle.Cantidad,
                    detalle.PrecioUnitario,
                    detalle.PorcentajeImpuesto,
                    detalle.PorcentajeDescuento);
            }

            _logger.LogInformation("Factura {Numero} construida exitosamente con {CantidadDetalles} detalles",
                _numeroFactura, _detalles.Count);

            return Result.Success(factura);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al construir la factura");
            _notificationManager.AddError($"Error interno: {ex.Message}", "ErrorInterno");
            return Result.Failure<Factura>("Error interno al construir la factura");
        }
    }

    /// <summary>
    /// Valida que todos los datos obligatorios estén presentes
    /// </summary>
    private void ValidarDatosObligatorios()
    {
        if (string.IsNullOrWhiteSpace(_numeroFactura))
        {
            _notificationManager.AddError("El número de factura es obligatorio", "NumeroFactura");
        }

        if (!_tipoFactura.HasValue)
        {
            _notificationManager.AddError("El tipo de factura es obligatorio", "TipoFactura");
        }

        if (string.IsNullOrWhiteSpace(_nombreCliente))
        {
            _notificationManager.AddError("El nombre del cliente es obligatorio", "NombreCliente");
        }

        if (_detalles.Count == 0)
        {
            _notificationManager.AddError("La factura debe tener al menos un detalle", "Detalles");
        }

        // Validaciones específicas según el tipo de factura
        if (_tipoFactura.HasValue)
        {
            switch (_tipoFactura.Value)
            {
                case TipoFactura.Fiscal:
                case TipoFactura.Electronica:
                    if (string.IsNullOrWhiteSpace(_identificacionFiscal))
                    {
                        _notificationManager.AddError("Las facturas fiscales y electrónicas requieren identificación fiscal", "IdentificacionFiscal");
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Reinicia el builder para permitir reutilización
    /// </summary>
    /// <returns>Builder reiniciado</returns>
    public FacturaBuilder Reset()
    {
        _numeroFactura = null;
        _tipoFactura = null;
        _nombreCliente = null;
        _clienteId = null;
        _identificacionFiscal = null;
        _direccionCliente = null;
        _comandasIds = null;
        _observaciones = null;
        _fechaEmision = null;
        _detalles.Clear();

        // Limpiar notificaciones acumuladas
        _notificationManager.CreateNewNotification();

        _logger.LogDebug("FacturaBuilder reiniciado");
        return this;
    }

    /// <summary>
    /// Método estático para crear una nueva instancia del builder
    /// </summary>
    /// <param name="notificationManager">Manager de notificaciones</param>
    /// <param name="logger">Logger</param>
    /// <returns>Nueva instancia del builder</returns>
    public static FacturaBuilder Nuevo(INotificationManager notificationManager, ILogger<FacturaBuilder> logger)
    {
        return new FacturaBuilder(notificationManager, logger);
    }
} 