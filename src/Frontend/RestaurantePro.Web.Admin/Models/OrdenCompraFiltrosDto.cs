using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// DTO para filtros de órdenes de compra
/// </summary>
public class OrdenCompraFiltrosDto
{
    /// <summary>
    /// Número de orden
    /// </summary>
    public string? NumeroOrden { get; set; }

    /// <summary>
    /// ID del proveedor
    /// </summary>
    public Guid? ProveedorId { get; set; }

    /// <summary>
    /// Estado de la orden
    /// </summary>
    public EstadoOrdenCompra? Estado { get; set; }

    /// <summary>
    /// Fecha desde
    /// </summary>
    public DateTime? FechaDesde { get; set; }

    /// <summary>
    /// Fecha hasta
    /// </summary>
    public DateTime? FechaHasta { get; set; }

    /// <summary>
    /// Usuario responsable
    /// </summary>
    public string? UsuarioResponsable { get; set; }

    /// <summary>
    /// Total desde
    /// </summary>
    public decimal? TotalDesde { get; set; }

    /// <summary>
    /// Total hasta
    /// </summary>
    public decimal? TotalHasta { get; set; }
}
