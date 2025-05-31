namespace RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientePorId;

/// <summary>
/// Query para obtener un ingrediente específico por su ID
/// Incluye movimientos recientes y propiedades calculadas
/// </summary>
public class ObtenerIngredientePorIdQuery : IRequest<Result<IngredienteDto>>
{
    /// <summary>
    /// ID del ingrediente a buscar
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Indica si incluir movimientos recientes (últimos 10)
    /// </summary>
    public bool IncluirMovimientos { get; set; } = true;

    /// <summary>
    /// Constructor sin parámetros para model binding
    /// </summary>
    public ObtenerIngredientePorIdQuery() { }

    /// <summary>
    /// Constructor con parámetros
    /// </summary>
    public ObtenerIngredientePorIdQuery(Guid id, bool incluirMovimientos = true)
    {
        Id = id;
        IncluirMovimientos = incluirMovimientos;
    }

    /// <summary>
    /// Factory method para crear query básica
    /// </summary>
    public static ObtenerIngredientePorIdQuery Crear(Guid id)
        => new(id);

    /// <summary>
    /// Factory method para crear query sin movimientos
    /// </summary>
    public static ObtenerIngredientePorIdQuery CrearSinMovimientos(Guid id)
        => new(id, false);
} 