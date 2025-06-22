using RestaurantePro.Application.Operaciones.Comandas.DTOs;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.CambiarEstadoComanda;

/// <summary>
/// Comando para cambiar el estado de una comanda
/// Permite transiciones de estado como: Abierta -> EnPreparacion -> Lista -> Entregada -> Cerrada
/// </summary>
public record CambiarEstadoComandaCommand : IRequest<Result<ComandaDto>>
{
    /// <summary>
    /// ID de la comanda cuyo estado se va a cambiar
    /// </summary>
    public Guid ComandaId { get; init; }

    /// <summary>
    /// Nuevo estado de la comanda
    /// </summary>
    public string NuevoEstado { get; init; } = string.Empty;

    /// <summary>
    /// Observaciones adicionales sobre el cambio de estado
    /// </summary>
    public string? Observaciones { get; init; }

    /// <summary>
    /// ID del usuario que realiza el cambio
    /// </summary>
    public Guid? UsuarioId { get; set; }

    /// <summary>
    /// Constructor para facilitar la creación
    /// </summary>
    public CambiarEstadoComandaCommand()
    {
    }

    /// <summary>
    /// Constructor con parámetros básicos
    /// </summary>
    public CambiarEstadoComandaCommand(Guid comandaId, string nuevoEstado)
    {
        ComandaId = comandaId;
        NuevoEstado = nuevoEstado;
    }

    /// <summary>
    /// Método para crear una nueva instancia con ComandaId actualizado
    /// </summary>
    public CambiarEstadoComandaCommand WithComandaId(Guid comandaId)
    {
        return this with { ComandaId = comandaId };
    }
} 