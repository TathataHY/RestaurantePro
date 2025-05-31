namespace RestaurantePro.Domain.Core.Usuarios.Events.Usuario;

/// <summary>
/// Evento que se lanza cuando se actualiza la información organizacional de un usuario
/// </summary>
public class UsuarioInformacionOrganizacionalActualizada : DomainEvent
{
    /// <summary>
    /// ID del usuario cuya información se actualizó
    /// </summary>
    public Guid UsuarioId { get; }
    
    /// <summary>
    /// ID del supervisor asignado
    /// </summary>
    public Guid? SupervisorId { get; }
    
    /// <summary>
    /// Departamento asignado
    /// </summary>
    public string? Departamento { get; }
    
    /// <summary>
    /// Posición asignada
    /// </summary>
    public string? Posicion { get; }
    
    /// <summary>
    /// Constructor del evento
    /// </summary>
    public UsuarioInformacionOrganizacionalActualizada(Guid usuarioId, Guid? supervisorId, string? departamento, string? posicion)
    {
        UsuarioId = usuarioId;
        SupervisorId = supervisorId;
        Departamento = departamento;
        Posicion = posicion;
    }
} 