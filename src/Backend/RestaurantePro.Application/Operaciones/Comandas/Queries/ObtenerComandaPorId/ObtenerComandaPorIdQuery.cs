namespace RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerComandaPorId;

/// <summary>
/// Query para obtener una comanda específica por su ID
/// Retorna el DTO completo con todos los detalles de la comanda
/// </summary>
public class ObtenerComandaPorIdQuery : IRequest<Result<ComandaDto>>
{
    /// <summary>
    /// ID de la comanda a buscar
    /// </summary>
    public Guid ComandaId { get; set; }

    /// <summary>
    /// Indica si se deben incluir los items de la comanda
    /// </summary>
    public bool IncluirItems { get; set; } = true;

    /// <summary>
    /// Constructor para facilitar la creación
    /// </summary>
    public ObtenerComandaPorIdQuery(Guid comandaId, bool incluirItems = true)
    {
        ComandaId = comandaId;
        IncluirItems = incluirItems;
    }

    /// <summary>
    /// Constructor sin parámetros para model binding
    /// </summary>
    public ObtenerComandaPorIdQuery()
    {
    }

    /// <summary>
    /// Factory method para crear el query
    /// </summary>
    public static ObtenerComandaPorIdQuery Create(Guid comandaId, bool incluirItems = true)
    {
        return new ObtenerComandaPorIdQuery(comandaId, incluirItems);
    }
} 