namespace RestaurantePro.Application.Proveedores.ContactosProveedor.Queries.ObtenerContactosPorProveedor;

/// <summary>
/// Query para obtener contactos por proveedor específico
/// </summary>
public class ObtenerContactosPorProveedorQuery : IRequest<Result<List<ContactoProveedorDto>>>
{
    /// <summary>
    /// ID del proveedor
    /// </summary>
    public Guid ProveedorId { get; set; }

    /// <summary>
    /// Filtrar solo contactos activos
    /// </summary>
    public bool SoloActivos { get; set; } = true;

    /// <summary>
    /// Filtrar contactos principales
    /// </summary>
    public bool? EsPrincipal { get; set; }

    /// <summary>
    /// Usuario que solicita la consulta
    /// </summary>
    public Guid UsuarioId { get; set; }

    public ObtenerContactosPorProveedorQuery() { }

    public ObtenerContactosPorProveedorQuery(Guid proveedorId, Guid usuarioId)
    {
        ProveedorId = proveedorId;
        UsuarioId = usuarioId;
    }
} 