using RestaurantePro.Application.Operaciones.Comandas.DTOs;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.ActualizarComanda;

/// <summary>
/// Comando para actualizar una comanda existente
/// Permite modificar observaciones y otros datos básicos de la comanda
/// </summary>
public record ActualizarComandaCommand : IRequest<Result<ComandaDto>>
{
    /// <summary>
    /// ID de la comanda a actualizar
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// ID del mesero responsable de la comanda
    /// </summary>
    public Guid? MeseroId { get; init; }

    /// <summary>
    /// ID de la mesa donde se toma la comanda
    /// </summary>
    public Guid? MesaId { get; init; }

    /// <summary>
    /// ID del cliente asociado
    /// </summary>
    public Guid? ClienteId { get; init; }

    /// <summary>
    /// Observaciones actualizadas de la comanda
    /// </summary>
    public string? Observaciones { get; init; }

    /// <summary>
    /// Estado de la comanda
    /// </summary>
    public string? Estado { get; init; }

    /// <summary>
    /// Constructor para facilitar la creación
    /// </summary>
    public ActualizarComandaCommand()
    {
    }

    /// <summary>
    /// Constructor con ID de comanda
    /// </summary>
    public ActualizarComandaCommand(Guid id)
    {
        Id = id;
    }

    /// <summary>
    /// Método para crear una nueva instancia con ID actualizado
    /// </summary>
    public ActualizarComandaCommand WithId(Guid id)
    {
        return this with { Id = id };
    }
} 