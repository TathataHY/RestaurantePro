namespace RestaurantePro.Application.Inventario.MovimientosInventario.Commands.EliminarMovimiento;

/// <summary>
/// Command para eliminar un movimiento de inventario
/// </summary>
public class EliminarMovimientoCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// ID del movimiento a eliminar
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del usuario que realiza la eliminación
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Motivo de la eliminación
    /// </summary>
    public string? MotivoEliminacion { get; set; }

    /// <summary>
    /// Constructor sin parámetros para model binding
    /// </summary>
    public EliminarMovimientoCommand() { }

    /// <summary>
    /// Constructor con parámetros básicos
    /// </summary>
    public EliminarMovimientoCommand(Guid id, Guid usuarioId, string? motivoEliminacion = null)
    {
        Id = id;
        UsuarioId = usuarioId;
        MotivoEliminacion = motivoEliminacion;
    }

    /// <summary>
    /// Factory method para crear el command
    /// </summary>
    public static EliminarMovimientoCommand Create(Guid id, Guid usuarioId, string? motivoEliminacion = null)
    {
        return new EliminarMovimientoCommand(id, usuarioId, motivoEliminacion);
    }
} 