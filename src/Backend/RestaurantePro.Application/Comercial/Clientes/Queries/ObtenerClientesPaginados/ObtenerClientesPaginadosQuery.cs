using RestaurantePro.Application.Comercial.Clientes.DTOs;

namespace RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientesPaginados;

/// <summary>
/// Query para obtener clientes paginados con filtros avanzados
/// Demuestra el uso de FilterRequest y QueryableExtensions
/// </summary>
public class ObtenerClientesPaginadosQuery : IRequest<Result<PaginatedList<ClienteSummaryDto>>>
{
    /// <summary>
    /// Filtros base para paginación y búsqueda
    /// </summary>
    public FilterRequest Filtros { get; set; } = new();

    /// <summary>
    /// Filtrar solo clientes activos (override del FilterRequest base)
    /// </summary>
    public bool? SoloActivos { get; set; }

    /// <summary>
    /// Filtrar por segmento específico
    /// </summary>
    public string? Segmento { get; set; }

    /// <summary>
    /// Puntos mínimos acumulados
    /// </summary>
    public int? PuntosMinimos { get; set; }

    /// <summary>
    /// Cantidad mínima de visitas
    /// </summary>
    public int? VisitasMinimas { get; set; }

    /// <summary>
    /// Solo clientes con tarjeta de fidelización
    /// </summary>
    public bool? ConTarjetaFidelizacion { get; set; }

    /// <summary>
    /// Edad mínima del cliente
    /// </summary>
    public int? EdadMinima { get; set; }

    /// <summary>
    /// Edad máxima del cliente
    /// </summary>
    public int? EdadMaxima { get; set; }
} 