using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Inventario.MovimientosInventario.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;

namespace RestaurantePro.Application.Inventario.MovimientosInventario.Queries.ObtenerMovimientosInventario;

/// <summary>
/// Query para obtener movimientos de inventario con filtros avanzados
/// </summary>
public class ObtenerMovimientosInventarioQuery : IRequest<Result<PaginatedList<MovimientoInventarioDto>>>
{
    /// <summary>
    /// Número de página (base 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Tamaño de página (máximo 100)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Fecha de inicio para filtrar movimientos
    /// </summary>
    public DateTime? FechaInicio { get; set; }

    /// <summary>
    /// Fecha de fin para filtrar movimientos
    /// </summary>
    public DateTime? FechaFin { get; set; }

    /// <summary>
    /// Tipo de movimiento específico
    /// </summary>
    public TipoMovimientoInventario? TipoMovimiento { get; set; }

    /// <summary>
    /// ID del ingrediente específico
    /// </summary>
    public Guid? IngredienteId { get; set; }

    /// <summary>
    /// ID del proveedor específico
    /// </summary>
    public Guid? ProveedorId { get; set; }

    /// <summary>
    /// ID del usuario que realizó el movimiento
    /// </summary>
    public Guid? UsuarioId { get; set; }

    /// <summary>
    /// Término de búsqueda para motivo u observaciones
    /// </summary>
    public string? Buscar { get; set; }

    /// <summary>
    /// Ordenar por campo específico
    /// </summary>
    public string? OrdenarPor { get; set; }

    /// <summary>
    /// Dirección del ordenamiento (asc/desc)
    /// </summary>
    public string? DireccionOrdenamiento { get; set; }

    /// <summary>
    /// Constructor sin parámetros para model binding
    /// </summary>
    public ObtenerMovimientosInventarioQuery() { }

    /// <summary>
    /// Constructor con parámetros básicos
    /// </summary>
    public ObtenerMovimientosInventarioQuery(int pageNumber = 1, int pageSize = 20)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    /// <summary>
    /// Constructor completo con todos los filtros
    /// </summary>
    public ObtenerMovimientosInventarioQuery(
        int pageNumber,
        int pageSize,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null,
        TipoMovimientoInventario? tipoMovimiento = null,
        Guid? ingredienteId = null,
        Guid? proveedorId = null,
        Guid? usuarioId = null,
        string? buscar = null,
        string? ordenarPor = null,
        string? direccionOrdenamiento = null)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        TipoMovimiento = tipoMovimiento;
        IngredienteId = ingredienteId;
        ProveedorId = proveedorId;
        UsuarioId = usuarioId;
        Buscar = buscar;
        OrdenarPor = ordenarPor;
        DireccionOrdenamiento = direccionOrdenamiento;
    }
} 