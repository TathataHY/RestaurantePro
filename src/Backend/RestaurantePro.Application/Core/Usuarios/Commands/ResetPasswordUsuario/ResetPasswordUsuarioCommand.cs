namespace RestaurantePro.Application.Core.Usuarios.Commands.ResetPasswordUsuario;

/// <summary>
/// Comando para resetear la contraseña de un usuario
/// </summary>
public class ResetPasswordUsuarioCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// ID del usuario cuya contraseña se va a resetear
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del usuario que realiza el reset
    /// </summary>
    public Guid UsuarioReseteadorId { get; set; }

    /// <summary>
    /// Nueva contraseña temporal (opcional, si no se proporciona se genera automáticamente)
    /// </summary>
    public string? NuevaPasswordTemporal { get; set; }

    /// <summary>
    /// Motivo del reset de contraseña
    /// </summary>
    public string? Motivo { get; set; }

    /// <summary>
    /// Constructor sin parámetros
    /// </summary>
    public ResetPasswordUsuarioCommand() { }

    /// <summary>
    /// Constructor con parámetros básicos
    /// </summary>
    public ResetPasswordUsuarioCommand(Guid id, Guid usuarioReseteadorId)
    {
        Id = id;
        UsuarioReseteadorId = usuarioReseteadorId;
    }

    /// <summary>
    /// Factory method para reset básico
    /// </summary>
    public static ResetPasswordUsuarioCommand Create(Guid id, Guid usuarioReseteadorId)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El ID del usuario es requerido", nameof(id));
            
        return new ResetPasswordUsuarioCommand(id, usuarioReseteadorId);
    }
} 