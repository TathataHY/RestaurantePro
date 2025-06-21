namespace RestaurantePro.Application.Core.Usuarios.Commands.EliminarUsuario;

/// <summary>
/// Comando para eliminar un usuario del sistema
/// </summary>
public class EliminarUsuarioCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// ID del usuario a eliminar
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Motivo de la eliminación
    /// </summary>
    public string? Motivo { get; set; }

    /// <summary>
    /// ID del usuario que realiza la eliminación
    /// </summary>
    public Guid UsuarioEliminadorId { get; set; }

    /// <summary>
    /// Constructor sin parámetros
    /// </summary>
    public EliminarUsuarioCommand() { }

    /// <summary>
    /// Constructor con ID del usuario
    /// </summary>
    public EliminarUsuarioCommand(Guid id, Guid usuarioEliminadorId)
    {
        Id = id;
        UsuarioEliminadorId = usuarioEliminadorId;
    }

    /// <summary>
    /// Factory method para eliminación básica
    /// </summary>
    public static EliminarUsuarioCommand Create(Guid id, Guid usuarioEliminadorId)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El ID del usuario es requerido", nameof(id));
            
        return new EliminarUsuarioCommand(id, usuarioEliminadorId);
    }
} 