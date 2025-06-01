namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.AjustarInventario;

/// <summary>
/// Comando para ajustar inventario de ingredientes
/// </summary>
public class AjustarInventarioCommand : IRequest<RestaurantePro.Domain.Core.SharedKernel.Results.Result<bool>>
{
    /// <summary>
    /// ID del ingrediente a ajustar
    /// </summary>
    public Guid IngredienteId { get; set; }

    /// <summary>
    /// Tipo de movimiento de inventario
    /// </summary>
    public TipoMovimientoInventario TipoMovimiento { get; set; }

    /// <summary>
    /// Cantidad a ajustar (siempre positiva)
    /// </summary>
    public decimal Cantidad { get; set; }

    /// <summary>
    /// Motivo del ajuste
    /// </summary>
    public string? Motivo { get; set; }

    /// <summary>
    /// Tipo de ajuste de inventario
    /// </summary>
    public string TipoAjuste { get; set; } = "Manual";

    /// <summary>
    /// Motivo específico del ajuste
    /// </summary>
    public string? MotivoAjuste { get; set; }

    /// <summary>
    /// ID del usuario que realiza el ajuste
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Constructor para crear comando de ajuste
    /// </summary>
    public AjustarInventarioCommand(
        Guid ingredienteId,
        TipoMovimientoInventario tipoMovimiento,
        decimal cantidad,
        Guid usuarioId,
        string? motivo = null)
    {
        IngredienteId = ingredienteId;
        TipoMovimiento = tipoMovimiento;
        Cantidad = cantidad;
        UsuarioId = usuarioId;
        Motivo = motivo;
    }

    /// <summary>
    /// Constructor sin parámetros para serialización
    /// </summary>
    public AjustarInventarioCommand() { }
} 