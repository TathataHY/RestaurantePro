using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// DTO para estadísticas de órdenes de compra
/// </summary>
public class OrdenCompraEstadisticasDto
{
    /// <summary>
    /// Total de órdenes
    /// </summary>
    public int TotalOrdenes { get; set; }

    /// <summary>
    /// Órdenes pendientes
    /// </summary>
    public int OrdenesPendientes { get; set; }

    /// <summary>
    /// Órdenes en tránsito
    /// </summary>
    public int OrdenesEnTransito { get; set; }

    /// <summary>
    /// Órdenes entregadas
    /// </summary>
    public int OrdenesEntregadas { get; set; }

    /// <summary>
    /// Órdenes completadas
    /// </summary>
    public int OrdenesCompletadas { get; set; }

    /// <summary>
    /// Órdenes canceladas
    /// </summary>
    public int OrdenesCanceladas { get; set; }

    /// <summary>
    /// Valor total de órdenes
    /// </summary>
    public decimal ValorTotalOrdenes { get; set; }

    /// <summary>
    /// Valor promedio por orden
    /// </summary>
    public decimal ValorPromedioOrden { get; set; }

    /// <summary>
    /// Proveedor con más órdenes
    /// </summary>
    public string? ProveedorMasOrdenes { get; set; }

    /// <summary>
    /// Cantidad de órdenes del proveedor más activo
    /// </summary>
    public int CantidadOrdenesProveedorMasActivo { get; set; }

    /// <summary>
    /// Fecha de la última orden
    /// </summary>
    public DateTime? FechaUltimaOrden { get; set; }

    /// <summary>
    /// Fecha de la última entrega
    /// </summary>
    public DateTime? FechaUltimaEntrega { get; set; }

    /// <summary>
    /// Tiempo promedio de entrega (días)
    /// </summary>
    public decimal? TiempoPromedioEntrega { get; set; }

    /// <summary>
    /// Fecha de la última actualización
    /// </summary>
    public DateTime FechaUltimaActualizacion { get; set; }
}
