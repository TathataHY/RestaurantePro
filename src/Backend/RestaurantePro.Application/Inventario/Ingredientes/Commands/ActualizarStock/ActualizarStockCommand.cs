using RestaurantePro.Application.Inventario.Ingredientes.DTOs;

namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.ActualizarStock;

/// <summary>
/// Comando para actualizar el stock de un ingrediente
/// Soporta múltiples tipos de movimientos: ingreso, egreso, ajuste
/// </summary>
public class ActualizarStockCommand : IRequest<Result<IngredienteDto>>
{
    /// <summary>
    /// ID del ingrediente a actualizar
    /// </summary>
    public Guid IngredienteId { get; set; }

    /// <summary>
    /// Tipo de movimiento (Ingreso, Egreso, Ajuste)
    /// </summary>
    public string TipoMovimiento { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad del movimiento (siempre positiva, el tipo determina si suma o resta)
    /// </summary>
    public decimal Cantidad { get; set; }

    /// <summary>
    /// Motivo del movimiento
    /// </summary>
    public string Motivo { get; set; } = string.Empty;

    /// <summary>
    /// Nuevo costo unitario (opcional, para actualizar costo promedio)
    /// </summary>
    public decimal? NuevoCosto { get; set; }

    /// <summary>
    /// ID del usuario que realiza el movimiento
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Fecha del movimiento (opcional, por defecto usar fecha actual)
    /// </summary>
    public DateTime? FechaMovimiento { get; set; }

    /// <summary>
    /// Referencia externa (número de factura, orden de compra, etc.)
    /// </summary>
    public string? ReferenciaExterna { get; set; }

    /// <summary>
    /// ID del proveedor (para ingresos por compra)
    /// </summary>
    public Guid? ProveedorId { get; set; }

    /// <summary>
    /// Constructor sin parámetros para model binding
    /// </summary>
    public ActualizarStockCommand() { }

    /// <summary>
    /// Constructor con parámetros básicos
    /// </summary>
    public ActualizarStockCommand(Guid ingredienteId, string tipoMovimiento, decimal cantidad, string motivo, Guid usuarioId)
    {
        IngredienteId = ingredienteId;
        TipoMovimiento = tipoMovimiento;
        Cantidad = cantidad;
        Motivo = motivo;
        UsuarioId = usuarioId;
    }

    /// <summary>
    /// Factory method para entrada de stock (compra)
    /// </summary>
    public static ActualizarStockCommand CrearEntrada(
        Guid ingredienteId, 
        decimal cantidad, 
        string motivo,
        Guid usuarioId,
        decimal? costo = null,
        string? referenciaExterna = null,
        Guid? proveedorId = null)
    {
        return new ActualizarStockCommand
        {
            IngredienteId = ingredienteId,
            TipoMovimiento = "Ingreso",
            Cantidad = cantidad,
            Motivo = motivo,
            UsuarioId = usuarioId,
            NuevoCosto = costo,
            ReferenciaExterna = referenciaExterna,
            ProveedorId = proveedorId
        };
    }

    /// <summary>
    /// Factory method para salida de stock (consumo)
    /// </summary>
    public static ActualizarStockCommand CrearSalida(
        Guid ingredienteId, 
        decimal cantidad, 
        string motivo,
        Guid usuarioId,
        string? referenciaExterna = null)
    {
        return new ActualizarStockCommand
        {
            IngredienteId = ingredienteId,
            TipoMovimiento = "Egreso",
            Cantidad = cantidad,
            Motivo = motivo,
            UsuarioId = usuarioId,
            ReferenciaExterna = referenciaExterna
        };
    }

    /// <summary>
    /// Factory method para ajuste de inventario
    /// </summary>
    public static ActualizarStockCommand CrearAjuste(
        Guid ingredienteId, 
        decimal cantidadReal, 
        string motivo,
        Guid usuarioId)
    {
        return new ActualizarStockCommand
        {
            IngredienteId = ingredienteId,
            TipoMovimiento = "Ajuste",
            Cantidad = cantidadReal,
            Motivo = motivo,
            UsuarioId = usuarioId
        };
    }

    /// <summary>
    /// Factory method para merma/desperdicio
    /// </summary>
    public static ActualizarStockCommand CrearMerma(
        Guid ingredienteId, 
        decimal cantidad, 
        string motivoMerma,
        Guid usuarioId)
    {
        return new ActualizarStockCommand
        {
            IngredienteId = ingredienteId,
            TipoMovimiento = "Egreso",
            Cantidad = cantidad,
            Motivo = $"Merma: {motivoMerma}",
            UsuarioId = usuarioId
        };
    }
} 