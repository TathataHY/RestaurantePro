namespace RestaurantePro.Application.Proveedores.Proveedores.Queries.ObtenerProveedorPorId;

/// <summary>
/// Query para obtener un proveedor por su ID
/// Permite incluir o excluir información relacionada
/// </summary>
public class ObtenerProveedorPorIdQuery : IRequest<Result<ProveedorDto>>
{
    /// <summary>
    /// ID del proveedor a buscar
    /// </summary>
    public Guid ProveedorId { get; set; }

    /// <summary>
    /// Indica si se deben incluir los contactos del proveedor
    /// </summary>
    public bool IncluirContactos { get; set; } = true;

    /// <summary>
    /// Indica si se deben incluir las categorías del proveedor
    /// </summary>
    public bool IncluirCategorias { get; set; } = false;

    /// <summary>
    /// Usuario que solicita la consulta (para auditoría)
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Constructor sin parámetros
    /// </summary>
    public ObtenerProveedorPorIdQuery() { }

    /// <summary>
    /// Constructor con parámetros básicos
    /// </summary>
    public ObtenerProveedorPorIdQuery(Guid proveedorId, Guid usuarioId)
    {
        ProveedorId = proveedorId;
        UsuarioId = usuarioId;
    }

    /// <summary>
    /// Constructor completo
    /// </summary>
    public ObtenerProveedorPorIdQuery(
        Guid proveedorId, 
        bool incluirContactos, 
        bool incluirCategorias, 
        Guid usuarioId)
    {
        ProveedorId = proveedorId;
        IncluirContactos = incluirContactos;
        IncluirCategorias = incluirCategorias;
        UsuarioId = usuarioId;
    }

    /// <summary>
    /// Factory method para consulta básica (solo proveedor)
    /// </summary>
    public static ObtenerProveedorPorIdQuery ConsultaBasica(Guid proveedorId, Guid usuarioId)
    {
        return new ObtenerProveedorPorIdQuery
        {
            ProveedorId = proveedorId,
            IncluirContactos = false,
            IncluirCategorias = false,
            UsuarioId = usuarioId
        };
    }

    /// <summary>
    /// Factory method para consulta completa (con todas las relaciones)
    /// </summary>
    public static ObtenerProveedorPorIdQuery ConsultaCompleta(Guid proveedorId, Guid usuarioId)
    {
        return new ObtenerProveedorPorIdQuery
        {
            ProveedorId = proveedorId,
            IncluirContactos = true,
            IncluirCategorias = true,
            UsuarioId = usuarioId
        };
    }

    /// <summary>
    /// Factory method para consulta con contactos solamente
    /// </summary>
    public static ObtenerProveedorPorIdQuery ConsultaConContactos(Guid proveedorId, Guid usuarioId)
    {
        return new ObtenerProveedorPorIdQuery
        {
            ProveedorId = proveedorId,
            IncluirContactos = true,
            IncluirCategorias = false,
            UsuarioId = usuarioId
        };
    }
} 