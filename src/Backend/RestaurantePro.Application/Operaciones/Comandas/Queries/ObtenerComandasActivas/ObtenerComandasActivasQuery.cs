using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Application.Common.DTOs;

namespace RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerComandasActivas;

/// <summary>
/// Query para obtener comandas activas (en proceso, listas, etc.)
/// Incluye filtros y paginación para dashboards operativos
/// </summary>
public class ObtenerComandasActivasQuery : IRequest<Result<PaginatedList<ComandaSummaryDto>>>
{
    /// <summary>
    /// Número de página (base 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Tamaño de página
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Filtro por estado específico de comanda
    /// </summary>
    public string? EstadoFiltro { get; set; }

    /// <summary>
    /// Filtro por mesa específica
    /// </summary>
    public Guid? MesaId { get; set; }

    /// <summary>
    /// Filtro por mesero específico
    /// </summary>
    public Guid? MeseroId { get; set; }

    /// <summary>
    /// Filtro por cliente específico
    /// </summary>
    public Guid? ClienteId { get; set; }

    /// <summary>
    /// Filtrar solo comandas atrasadas (tiempo excedido)
    /// </summary>
    public bool SoloAtrasadas { get; set; } = false;

    /// <summary>
    /// Filtrar solo comandas con descuentos aplicados
    /// </summary>
    public bool SoloConDescuentos { get; set; } = false;

    /// <summary>
    /// Ordenamiento de resultados
    /// Valores: FechaCreacion, TiempoTranscurrido, Total, Estado
    /// </summary>
    public string OrdenarPor { get; set; } = "FechaCreacion";

    /// <summary>
    /// Dirección del ordenamiento (Asc, Desc)
    /// </summary>
    public string DireccionOrden { get; set; } = "Desc";

    /// <summary>
    /// Incluir solo comandas de hoy
    /// </summary>
    public bool SoloHoy { get; set; } = true;

    /// <summary>
    /// Fecha específica para filtrar (opcional)
    /// Si no se especifica y SoloHoy es false, trae de todos los días
    /// </summary>
    public DateTime? FechaEspecifica { get; set; }

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public ObtenerComandasActivasQuery() { }

    /// <summary>
    /// Constructor para casos básicos con paginación
    /// </summary>
    public ObtenerComandasActivasQuery(int pageNumber = 1, int pageSize = 20)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    /// <summary>
    /// Constructor para filtrar por mesa específica
    /// </summary>
    public ObtenerComandasActivasQuery(Guid mesaId, int pageNumber = 1, int pageSize = 20)
    {
        MesaId = mesaId;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    /// <summary>
    /// Constructor para filtrar por mesero específico
    /// </summary>
    public static ObtenerComandasActivasQuery PorMesero(Guid meseroId, int pageNumber = 1, int pageSize = 20)
    {
        return new ObtenerComandasActivasQuery
        {
            MeseroId = meseroId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Constructor para obtener solo comandas atrasadas
    /// </summary>
    public static ObtenerComandasActivasQuery CrearParaAtrasadas(int pageNumber = 1, int pageSize = 20)
    {
        return new ObtenerComandasActivasQuery
        {
            SoloAtrasadas = true,
            OrdenarPor = "TiempoTranscurrido",
            DireccionOrden = "Desc",
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
} 