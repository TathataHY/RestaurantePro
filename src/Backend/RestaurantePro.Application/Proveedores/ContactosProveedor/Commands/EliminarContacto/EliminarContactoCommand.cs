namespace RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.EliminarContacto;

/// <summary>
/// Command para eliminar un contacto de un proveedor
/// Soporta eliminación lógica (desactivar) y física (borrar permanentemente)
/// </summary>
public class EliminarContactoCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// ID del contacto a eliminar
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del proveedor al que pertenece el contacto
    /// </summary>
    public Guid ProveedorId { get; set; }

    /// <summary>
    /// Tipo de eliminación: Logica (desactivar) o Fisica (borrar permanentemente)
    /// </summary>
    public TipoEliminacion TipoEliminacion { get; set; } = TipoEliminacion.Logica;

    /// <summary>
    /// Motivo de la eliminación (para auditoría)
    /// </summary>
    public string MotivoEliminacion { get; set; } = string.Empty;

    /// <summary>
    /// Indica si se debe reasignar automáticamente un nuevo contacto principal
    /// si el contacto eliminado era el principal
    /// </summary>
    public bool ReasignarContactoPrincipal { get; set; } = true;

    /// <summary>
    /// ID del contacto que se convertirá en principal si se elimina el actual principal
    /// </summary>
    public Guid? NuevoContactoPrincipalId { get; set; }

    /// <summary>
    /// Indica si se debe confirmar la eliminación de forma forzada
    /// (útil para casos donde el contacto tiene dependencias)
    /// </summary>
    public bool ForzarEliminacion { get; set; } = false;

    /// <summary>
    /// Datos adicionales de la eliminación
    /// </summary>
    public Dictionary<string, object>? DatosAdicionales { get; set; }

    public EliminarContactoCommand(Guid id, Guid proveedorId, string motivoEliminacion)
    {
        Id = id;
        ProveedorId = proveedorId;
        MotivoEliminacion = motivoEliminacion;
    }

    public EliminarContactoCommand() { }
}

/// <summary>
/// Tipos de eliminación disponibles para contactos
/// </summary>
public enum TipoEliminacion
{
    /// <summary>
    /// Eliminación lógica - El contacto se marca como inactivo pero se conserva en BD
    /// </summary>
    Logica = 1,

    /// <summary>
    /// Eliminación física - El contacto se borra permanentemente de la BD
    /// </summary>
    Fisica = 2
} 