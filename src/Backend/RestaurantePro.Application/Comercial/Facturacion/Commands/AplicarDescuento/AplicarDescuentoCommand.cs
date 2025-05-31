namespace RestaurantePro.Application.Comercial.Facturacion.Commands.AplicarDescuento;

/// <summary>
/// Command para aplicar descuentos a facturas existentes
/// Permite múltiples tipos de descuentos con autorización y auditoría
/// </summary>
public class AplicarDescuentoCommand : IRequest<Result<FacturaDto>>
{
    /// <summary>
    /// ID de la factura a la que se aplicará el descuento
    /// </summary>
    public Guid FacturaId { get; set; }

    /// <summary>
    /// Tipo de descuento a aplicar
    /// </summary>
    public string TipoDescuento { get; set; } = string.Empty;

    /// <summary>
    /// Porcentaje de descuento (0-100)
    /// </summary>
    public decimal Porcentaje { get; set; }

    /// <summary>
    /// Monto fijo de descuento (alternativo al porcentaje)
    /// </summary>
    public decimal MontoFijo { get; set; }

    /// <summary>
    /// Concepto o descripción del descuento
    /// </summary>
    public string Concepto { get; set; } = string.Empty;

    /// <summary>
    /// Motivo detallado del descuento
    /// </summary>
    public string Motivo { get; set; } = string.Empty;

    /// <summary>
    /// ID del usuario que autoriza el descuento
    /// </summary>
    public Guid UsuarioAutorizaId { get; set; }

    /// <summary>
    /// Código de autorización (si es requerido por políticas)
    /// </summary>
    public string? CodigoAutorizacion { get; set; }

    /// <summary>
    /// Indica si el descuento se aplica antes de impuestos
    /// </summary>
    public bool AplicarAntesDeImpuestos { get; set; } = true;

    /// <summary>
    /// Productos específicos a los que aplicar el descuento (opcional)
    /// </summary>
    public List<Guid> ProductosEspecificos { get; set; } = new();

    /// <summary>
    /// Categorías de productos a las que aplicar el descuento (opcional)
    /// </summary>
    public List<string> CategoriasAplicables { get; set; } = new();

    /// <summary>
    /// Monto mínimo de factura para aplicar el descuento
    /// </summary>
    public decimal? MontoMinimoFactura { get; set; }

    /// <summary>
    /// Monto máximo de descuento permitido
    /// </summary>
    public decimal? MontoMaximoDescuento { get; set; }

    /// <summary>
    /// Fecha de expiración del descuento (si aplica)
    /// </summary>
    public DateTime? FechaExpiracion { get; set; }

    /// <summary>
    /// Indica si el descuento es acumulable con otros descuentos
    /// </summary>
    public bool EsAcumulable { get; set; } = false;

    /// <summary>
    /// Prioridad del descuento (1-10, donde 10 es mayor prioridad)
    /// </summary>
    public int Prioridad { get; set; } = 5;

    /// <summary>
    /// Notas adicionales del descuento
    /// </summary>
    public string? NotasAdicionales { get; set; }

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public AplicarDescuentoCommand()
    {
    }

    /// <summary>
    /// Constructor básico para descuento por porcentaje
    /// </summary>
    public AplicarDescuentoCommand(Guid facturaId, decimal porcentaje, string concepto, string motivo, Guid usuarioAutorizaId)
    {
        FacturaId = facturaId;
        TipoDescuento = "Porcentaje";
        Porcentaje = porcentaje;
        Concepto = concepto;
        Motivo = motivo;
        UsuarioAutorizaId = usuarioAutorizaId;
    }

    /// <summary>
    /// Factory method para descuento general por porcentaje
    /// </summary>
    public static AplicarDescuentoCommand CrearDescuentoPorcentaje(
        Guid facturaId,
        decimal porcentaje,
        string concepto,
        string motivo,
        Guid usuarioAutorizaId)
    {
        return new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "General",
            Porcentaje = porcentaje,
            Concepto = concepto,
            Motivo = motivo,
            UsuarioAutorizaId = usuarioAutorizaId,
            AplicarAntesDeImpuestos = true,
            EsAcumulable = false,
            Prioridad = 5
        };
    }

    /// <summary>
    /// Factory method para descuento por monto fijo
    /// </summary>
    public static AplicarDescuentoCommand CrearDescuentoMontoFijo(
        Guid facturaId,
        decimal montoFijo,
        string concepto,
        string motivo,
        Guid usuarioAutorizaId)
    {
        return new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "MontoFijo",
            MontoFijo = montoFijo,
            Concepto = concepto,
            Motivo = motivo,
            UsuarioAutorizaId = usuarioAutorizaId,
            AplicarAntesDeImpuestos = true,
            EsAcumulable = false,
            Prioridad = 5
        };
    }

    /// <summary>
    /// Factory method para descuento de empleado/cliente frecuente
    /// </summary>
    public static AplicarDescuentoCommand CrearDescuentoEmpleado(
        Guid facturaId,
        decimal porcentaje,
        Guid usuarioAutorizaId)
    {
        return new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "Empleado",
            Porcentaje = porcentaje,
            Concepto = "Descuento por empleado",
            Motivo = "Política de descuentos para empleados",
            UsuarioAutorizaId = usuarioAutorizaId,
            AplicarAntesDeImpuestos = true,
            EsAcumulable = false,
            Prioridad = 8
        };
    }

    /// <summary>
    /// Factory method para descuento promocional
    /// </summary>
    public static AplicarDescuentoCommand CrearDescuentoPromocional(
        Guid facturaId,
        decimal porcentaje,
        string codigoPromocional,
        DateTime fechaExpiracion,
        Guid usuarioAutorizaId)
    {
        return new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "Promocional",
            Porcentaje = porcentaje,
            Concepto = $"Descuento promocional - {codigoPromocional}",
            Motivo = "Aplicación de código promocional",
            CodigoAutorizacion = codigoPromocional,
            FechaExpiracion = fechaExpiracion,
            UsuarioAutorizaId = usuarioAutorizaId,
            AplicarAntesDeImpuestos = true,
            EsAcumulable = true,
            Prioridad = 6
        };
    }

    /// <summary>
    /// Factory method para descuento por volumen
    /// </summary>
    public static AplicarDescuentoCommand CrearDescuentoVolumen(
        Guid facturaId,
        decimal porcentaje,
        decimal montoMinimo,
        Guid usuarioAutorizaId)
    {
        return new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "Volumen",
            Porcentaje = porcentaje,
            Concepto = "Descuento por volumen de compra",
            Motivo = $"Compra mínima de {montoMinimo:C} alcanzada",
            MontoMinimoFactura = montoMinimo,
            UsuarioAutorizaId = usuarioAutorizaId,
            AplicarAntesDeImpuestos = true,
            EsAcumulable = true,
            Prioridad = 7
        };
    }

    /// <summary>
    /// Factory method para descuento específico por productos
    /// </summary>
    public static AplicarDescuentoCommand CrearDescuentoProductos(
        Guid facturaId,
        List<Guid> productosIds,
        decimal porcentaje,
        string motivo,
        Guid usuarioAutorizaId)
    {
        return new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "ProductosEspecificos",
            Porcentaje = porcentaje,
            Concepto = "Descuento en productos específicos",
            Motivo = motivo,
            ProductosEspecificos = productosIds,
            UsuarioAutorizaId = usuarioAutorizaId,
            AplicarAntesDeImpuestos = true,
            EsAcumulable = true,
            Prioridad = 4
        };
    }

    /// <summary>
    /// Factory method para descuento de categoría
    /// </summary>
    public static AplicarDescuentoCommand CrearDescuentoCategoria(
        Guid facturaId,
        List<string> categorias,
        decimal porcentaje,
        string motivo,
        Guid usuarioAutorizaId)
    {
        return new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "Categoria",
            Porcentaje = porcentaje,
            Concepto = $"Descuento en categorías: {string.Join(", ", categorias)}",
            Motivo = motivo,
            CategoriasAplicables = categorias,
            UsuarioAutorizaId = usuarioAutorizaId,
            AplicarAntesDeImpuestos = true,
            EsAcumulable = true,
            Prioridad = 4
        };
    }

    /// <summary>
    /// Factory method para descuento de cortesía
    /// </summary>
    public static AplicarDescuentoCommand CrearDescuentoCortesia(
        Guid facturaId,
        decimal montoFijo,
        string motivo,
        Guid usuarioAutorizaId,
        string codigoAutorizacion)
    {
        return new AplicarDescuentoCommand
        {
            FacturaId = facturaId,
            TipoDescuento = "Cortesia",
            MontoFijo = montoFijo,
            Concepto = "Descuento de cortesía",
            Motivo = motivo,
            CodigoAutorizacion = codigoAutorizacion,
            UsuarioAutorizaId = usuarioAutorizaId,
            AplicarAntesDeImpuestos = false,
            EsAcumulable = false,
            Prioridad = 9
        };
    }
} 