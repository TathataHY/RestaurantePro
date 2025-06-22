namespace RestaurantePro.Application.Operaciones.Comandas.Commands.EliminarComanda;

/// <summary>
/// Comando para eliminar una comanda
/// Realiza soft delete marcando la comanda como eliminada
/// </summary>
public class EliminarComandaCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// ID de la comanda a eliminar
    /// </summary>
    public Guid ComandaId { get; set; }

    /// <summary>
    /// Constructor para facilitar la creación
    /// </summary>
    public EliminarComandaCommand()
    {
    }

    /// <summary>
    /// Constructor con ID de comanda
    /// </summary>
    public EliminarComandaCommand(Guid comandaId)
    {
        ComandaId = comandaId;
    }

    /// <summary>
    /// Factory method para crear el comando
    /// </summary>
    public static EliminarComandaCommand Create(Guid comandaId)
    {
        return new EliminarComandaCommand(comandaId);
    }
} 