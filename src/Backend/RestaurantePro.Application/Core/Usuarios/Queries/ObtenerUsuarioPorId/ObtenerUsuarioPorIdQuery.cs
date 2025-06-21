namespace RestaurantePro.Application.Core.Usuarios.Queries.ObtenerUsuarioPorId;

/// <summary>
/// Query para obtener un usuario específico por su ID
/// </summary>
public class ObtenerUsuarioPorIdQuery : IRequest<Result<UsuarioDto>>
{
    /// <summary>
    /// ID único del usuario a consultar
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Constructor sin parámetros
    /// </summary>
    public ObtenerUsuarioPorIdQuery() { }

    /// <summary>
    /// Constructor con ID del usuario
    /// </summary>
    public ObtenerUsuarioPorIdQuery(Guid id)
    {
        Id = id;
    }

    /// <summary>
    /// Factory method para consulta básica por ID
    /// </summary>
    public static ObtenerUsuarioPorIdQuery Create(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El ID del usuario es requerido", nameof(id));
            
        return new ObtenerUsuarioPorIdQuery(id);
    }
} 