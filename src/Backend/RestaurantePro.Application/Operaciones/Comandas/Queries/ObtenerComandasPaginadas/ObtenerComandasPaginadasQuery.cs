using RestaurantePro.Application.Common.DTOs;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;

namespace RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerComandasPaginadas;

/// <summary>
/// Query para obtener comandas con paginación y filtros opcionales
/// Permite filtrar por estado, mesa, mesero, cliente y fechas
/// </summary>
public class ObtenerComandasPaginadasQuery : FilterRequest, IRequest<Result<PaginatedList<ComandaDto>>>
{
    /// <summary>
    /// Filtrar por estado de comanda
    /// </summary>
    public string? Estado { get; set; }

    /// <summary>
    /// Filtrar por mesa específica
    /// </summary>
    public Guid? MesaId { get; set; }

    /// <summary>
    /// Filtrar por mesero específico
    /// </summary>
    public Guid? MeseroId { get; set; }

    /// <summary>
    /// Filtrar por cliente específico
    /// </summary>
    public Guid? ClienteId { get; set; }

    /// <summary>
    /// Mostrar solo comandas activas
    /// </summary>
    public bool SoloActivas { get; set; } = false;

    /// <summary>
    /// Fecha desde para filtrar comandas
    /// </summary>
    public DateTime? FechaDesde { get; set; }

    /// <summary>
    /// Fecha hasta para filtrar comandas
    /// </summary>
    public DateTime? FechaHasta { get; set; }

    /// <summary>
    /// Ordenar por campo específico
    /// </summary>
    public string? OrdenarPor { get; set; } = "FechaCreacion";

    /// <summary>
    /// Dirección del ordenamiento (asc/desc)
    /// </summary>
    public string? DireccionOrdenamiento { get; set; } = "desc";

    /// <summary>
    /// Constructor para facilitar la creación
    /// </summary>
    public ObtenerComandasPaginadasQuery()
    {
    }

    /// <summary>
    /// Constructor con parámetros básicos
    /// </summary>
    public ObtenerComandasPaginadasQuery(int pageNumber = 1, int pageSize = 10)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
} 