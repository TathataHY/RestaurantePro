namespace RestaurantePro.Application.Proveedores.ContactosProveedor.Queries.ObtenerContactoPorId;

/// <summary>
/// Query para obtener un contacto específico por ID
/// </summary>
public class ObtenerContactoPorIdQuery : IRequest<Result<ContactoProveedorDto>>
{
    /// <summary>
    /// ID del contacto a obtener
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Usuario que solicita la consulta
    /// </summary>
    public Guid UsuarioId { get; set; }

    public ObtenerContactoPorIdQuery() { }

    public ObtenerContactoPorIdQuery(Guid id, Guid usuarioId)
    {
        Id = id;
        UsuarioId = usuarioId;
    }
} 