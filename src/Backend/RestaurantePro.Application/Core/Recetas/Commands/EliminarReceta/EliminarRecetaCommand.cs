namespace RestaurantePro.Application.Core.Recetas.Commands.EliminarReceta;

/// <summary>
/// Command para eliminar una receta
/// </summary>
public class EliminarRecetaCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// ID de la receta a eliminar
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Constructor sin parámetros
    /// </summary>
    public EliminarRecetaCommand() { }

    /// <summary>
    /// Constructor con ID
    /// </summary>
    public EliminarRecetaCommand(Guid id)
    {
        Id = id;
    }
} 
