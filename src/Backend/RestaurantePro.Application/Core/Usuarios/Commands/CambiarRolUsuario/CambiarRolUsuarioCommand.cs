namespace RestaurantePro.Application.Core.Usuarios.Commands.CambiarRolUsuario;

/// <summary>
/// Comando para cambiar el rol de un usuario
/// </summary>
public class CambiarRolUsuarioCommand : IRequest<Result<UsuarioDto>>
{
    /// <summary>
    /// ID del usuario cuyo rol se va a cambiar
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nuevo rol a asignar
    /// </summary>
    public string NuevoRol { get; set; } = string.Empty;

    /// <summary>
    /// ID del usuario que realiza el cambio
    /// </summary>
    public Guid UsuarioCambiadorId { get; set; }

    /// <summary>
    /// Motivo del cambio de rol
    /// </summary>
    public string? Motivo { get; set; }

    /// <summary>
    /// Constructor sin parámetros
    /// </summary>
    public CambiarRolUsuarioCommand() { }

    /// <summary>
    /// Constructor con parámetros básicos
    /// </summary>
    public CambiarRolUsuarioCommand(Guid id, string nuevoRol, Guid usuarioCambiadorId)
    {
        Id = id;
        NuevoRol = nuevoRol;
        UsuarioCambiadorId = usuarioCambiadorId;
    }

    /// <summary>
    /// Factory method para cambio de rol básico
    /// </summary>
    public static CambiarRolUsuarioCommand Create(Guid id, string nuevoRol, Guid usuarioCambiadorId)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El ID del usuario es requerido", nameof(id));
            
        if (string.IsNullOrWhiteSpace(nuevoRol))
            throw new ArgumentException("El nuevo rol es requerido", nameof(nuevoRol));
            
        return new CambiarRolUsuarioCommand(id, nuevoRol, usuarioCambiadorId);
    }
} 