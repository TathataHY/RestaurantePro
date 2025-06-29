namespace RestaurantePro.Application.Proveedores.ContactosProveedor.Queries.ObtenerTodosContactos;

/// <summary>
/// Query para obtener todos los contactos de proveedores
/// Permite filtros y paginación
/// </summary>
public class ObtenerTodosContactosQuery : IRequest<Result<List<ContactoProveedorDto>>>
{
    /// <summary>
    /// Término de búsqueda (nombre, cargo, email)
    /// </summary>
    public string? TerminoBusqueda { get; set; }

    /// <summary>
    /// Filtrar solo contactos activos
    /// </summary>
    public bool SoloActivos { get; set; } = true;

    /// <summary>
    /// Filtrar por proveedor específico
    /// </summary>
    public Guid? ProveedorId { get; set; }

    /// <summary>
    /// Filtrar contactos principales
    /// </summary>
    public bool? EsPrincipal { get; set; }

    /// <summary>
    /// Ordenar por campo específico
    /// </summary>
    public string CampoOrden { get; set; } = "Nombre";

    /// <summary>
    /// Dirección del ordenamiento
    /// </summary>
    public string DireccionOrden { get; set; } = "asc";

    /// <summary>
    /// Usuario que solicita la consulta
    /// </summary>
    public Guid UsuarioId { get; set; }

    public ObtenerTodosContactosQuery() { }

    public ObtenerTodosContactosQuery(Guid usuarioId)
    {
        UsuarioId = usuarioId;
    }
} 