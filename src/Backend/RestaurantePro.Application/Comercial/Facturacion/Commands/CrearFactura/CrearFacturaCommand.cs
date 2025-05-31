namespace RestaurantePro.Application.Comercial.Facturacion.Commands.CrearFactura;

/// <summary>
/// Command para crear una nueva factura
/// Permite crear facturas para una o múltiples comandas con información fiscal completa
/// </summary>
public class CrearFacturaCommand : IRequest<Result<FacturaDto>>
{
    /// <summary>
    /// IDs de las comandas a facturar
    /// </summary>
    public List<Guid> ComandasIds { get; set; } = new();

    /// <summary>
    /// Tipo de factura a generar
    /// </summary>
    public string TipoFactura { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del cliente o razón social
    /// </summary>
    public string NombreCliente { get; set; } = string.Empty;

    /// <summary>
    /// ID del cliente (opcional, para facturas con cliente registrado)
    /// </summary>
    public Guid? ClienteId { get; set; }

    /// <summary>
    /// RFC o identificador fiscal del cliente (requerido para facturas fiscales)
    /// </summary>
    public string? IdentificacionFiscal { get; set; }

    /// <summary>
    /// Dirección fiscal del cliente
    /// </summary>
    public string? DireccionCliente { get; set; }

    /// <summary>
    /// Email del cliente para envío de factura
    /// </summary>
    public string? EmailCliente { get; set; }

    /// <summary>
    /// Teléfono del cliente
    /// </summary>
    public string? TelefonoCliente { get; set; }

    /// <summary>
    /// Observaciones adicionales de la factura
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Fecha de emisión personalizada (opcional, por defecto fecha actual)
    /// </summary>
    public DateTime? FechaEmision { get; set; }

    /// <summary>
    /// Días de crédito para el vencimiento (0 = pago inmediato)
    /// </summary>
    public int DiasCredito { get; set; } = 0;

    /// <summary>
    /// Indica si la factura debe emitirse automáticamente tras la creación
    /// </summary>
    public bool EmitirInmediatamente { get; set; } = true;

    /// <summary>
    /// Indica si se debe enviar la factura por email al cliente
    /// </summary>
    public bool EnviarPorEmail { get; set; } = false;

    /// <summary>
    /// Descuentos adicionales a aplicar a nivel de factura
    /// </summary>
    public List<DescuentoFacturaDto> DescuentosAdicionales { get; set; } = new();

    /// <summary>
    /// Método de pago preferido del cliente
    /// </summary>
    public string? MetodoPagoPreferido { get; set; }

    /// <summary>
    /// Moneda de la factura (por defecto MXN)
    /// </summary>
    public string Moneda { get; set; } = "MXN";

    /// <summary>
    /// Tipo de cambio si es diferente a la moneda base
    /// </summary>
    public decimal? TipoCambio { get; set; }

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public CrearFacturaCommand()
    {
    }

    /// <summary>
    /// Constructor para factura simple de una comanda
    /// </summary>
    public CrearFacturaCommand(Guid comandaId, string tipoFactura, string nombreCliente)
    {
        ComandasIds = new List<Guid> { comandaId };
        TipoFactura = tipoFactura;
        NombreCliente = nombreCliente;
    }

    /// <summary>
    /// Factory method para factura de consumidor final
    /// </summary>
    public static CrearFacturaCommand CrearConsumidorFinal(Guid comandaId, string nombreCliente)
    {
        return new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = nombreCliente,
            EmitirInmediatamente = true,
            DiasCredito = 0
        };
    }

    /// <summary>
    /// Factory method para factura fiscal empresarial
    /// </summary>
    public static CrearFacturaCommand CrearFiscal(
        List<Guid> comandasIds, 
        string nombreCliente, 
        string rfc, 
        string direccion,
        string email)
    {
        return new CrearFacturaCommand
        {
            ComandasIds = comandasIds,
            TipoFactura = "Fiscal",
            NombreCliente = nombreCliente,
            IdentificacionFiscal = rfc,
            DireccionCliente = direccion,
            EmailCliente = email,
            EmitirInmediatamente = true,
            EnviarPorEmail = true,
            DiasCredito = 30
        };
    }

    /// <summary>
    /// Factory method para factura con cliente registrado
    /// </summary>
    public static CrearFacturaCommand CrearParaCliente(
        List<Guid> comandasIds, 
        Guid clienteId, 
        string tipoFactura = "Normal")
    {
        return new CrearFacturaCommand
        {
            ComandasIds = comandasIds,
            ClienteId = clienteId,
            TipoFactura = tipoFactura,
            EmitirInmediatamente = true,
            EnviarPorEmail = true
        };
    }

    /// <summary>
    /// Factory method para factura con descuentos especiales
    /// </summary>
    public static CrearFacturaCommand CrearConDescuentos(
        Guid comandaId, 
        string tipoFactura, 
        string nombreCliente,
        List<DescuentoFacturaDto> descuentos)
    {
        return new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = tipoFactura,
            NombreCliente = nombreCliente,
            DescuentosAdicionales = descuentos,
            EmitirInmediatamente = false // Revisar descuentos antes de emitir
        };
    }
}

/// <summary>
/// DTO para descuentos adicionales en factura
/// </summary>
public class DescuentoFacturaDto
{
    public string Concepto { get; set; } = string.Empty;
    public decimal Porcentaje { get; set; }
    public decimal MontoFijo { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public bool AplicarAntesDeImpuestos { get; set; } = true;
} 