namespace RestaurantePro.Application.Operaciones.Comandas.Commands.FinalizarComanda;

/// <summary>
/// 🍽️ Command para finalizar una comanda específica
/// Marca la comanda como completada y lista para facturación
/// </summary>
public class FinalizarComandaCommand : IRequest<Result<ComandaDto>>
{
    public Guid ComandaId { get; init; }
    public Guid UsuarioId { get; init; }
    public string? ObservacionesFinalizacion { get; init; }
    public bool ValidarTodosItemsListos { get; init; } = true;
    public bool NotificarMesero { get; init; } = true;
    public DateTime? FechaFinalizacion { get; init; }

    /// <summary>
    /// Factory method básico
    /// </summary>
    public static FinalizarComandaCommand Crear(Guid comandaId, Guid usuarioId, string? observaciones = null)
    {
        if (comandaId == Guid.Empty)
            throw new ArgumentException("ComandaId no puede estar vacío", nameof(comandaId));
            
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("UsuarioId no puede estar vacío", nameof(usuarioId));

        return new FinalizarComandaCommand
        {
            ComandaId = comandaId,
            UsuarioId = usuarioId,
            ObservacionesFinalizacion = observaciones,
            ValidarTodosItemsListos = true,
            NotificarMesero = true,
            FechaFinalizacion = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method con validación opcional
    /// </summary>
    public static FinalizarComandaCommand FinalizarSinValidacion(
        Guid comandaId, 
        Guid usuarioId, 
        string motivo,
        string? observaciones = null)
    {
        return new FinalizarComandaCommand
        {
            ComandaId = comandaId,
            UsuarioId = usuarioId,
            ObservacionesFinalizacion = $"Finalizada sin validación: {motivo}. {observaciones}",
            ValidarTodosItemsListos = false,
            NotificarMesero = true,
            FechaFinalizacion = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method silencioso (sin notificaciones)
    /// </summary>
    public static FinalizarComandaCommand FinalizarSilencioso(
        Guid comandaId, 
        Guid usuarioId, 
        string? observaciones = null)
    {
        return new FinalizarComandaCommand
        {
            ComandaId = comandaId,
            UsuarioId = usuarioId,
            ObservacionesFinalizacion = observaciones,
            ValidarTodosItemsListos = true,
            NotificarMesero = false,
            FechaFinalizacion = DateTime.UtcNow
        };
    }
} 