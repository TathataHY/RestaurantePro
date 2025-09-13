using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Application.Common.Models.Dashboard;

/// <summary>
/// DTO para métricas del dashboard
/// </summary>
public class DashboardMetricasDto
{
    public decimal VentasHoy { get; set; }
    public decimal VentasAyer { get; set; }
    public decimal VentasSemana { get; set; }
    public decimal VentasMes { get; set; }
    public int MesasOcupadas { get; set; }
    public int MesasDisponibles { get; set; }
    public int TotalMesas { get; set; }
    public int ComandasActivas { get; set; }
    public int ComandasCompletadas { get; set; }
    public int ProductosVendidosHoy { get; set; }
    public int ClientesAtendidosHoy { get; set; }
    public decimal PromedioTicket { get; set; }
    public decimal CrecimientoVentas { get; set; }
    public DateTime UltimaActualizacion { get; set; }
}

/// <summary>
/// DTO para productos más vendidos del dashboard
/// </summary>
public class DashboardProductoMasVendidoDto
{
    public Guid ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public decimal Ingresos { get; set; }
    public decimal PrecioPromedio { get; set; }
    public decimal PorcentajeTotal { get; set; }
}

/// <summary>
/// DTO para ventas por período del dashboard
/// </summary>
public class DashboardVentaPorPeriodoDto
{
    public DateTime Fecha { get; set; }
    public decimal Ventas { get; set; }
    public int CantidadFacturas { get; set; }
}

/// <summary>
/// DTO para estado de mesas del dashboard
/// </summary>
public class DashboardEstadoMesasDto
{
    public int Ocupadas { get; set; }
    public int Disponibles { get; set; }
    public int Reservadas { get; set; }
    public int Mantenimiento { get; set; }
    public int Total { get; set; }
}

/// <summary>
/// DTO para comandas por estado del dashboard
/// </summary>
public class DashboardComandasPorEstadoDto
{
    public int Creadas { get; set; }
    public int EnProceso { get; set; }
    public int Lista { get; set; }
    public int Entregada { get; set; }
    public int Finalizada { get; set; }
    public int Cancelada { get; set; }
    public int Total { get; set; }
}

/// <summary>
/// DTO para ingresos por hora del dashboard
/// </summary>
public class DashboardIngresosPorHoraDto
{
    public int Hora { get; set; }
    public decimal Ingresos { get; set; }
    public int CantidadFacturas { get; set; }
}

/// <summary>
/// DTO para resumen completo del dashboard
/// </summary>
public class DashboardResumenDto
{
    public DashboardMetricasDto Metricas { get; set; } = new();
    public List<DashboardProductoMasVendidoDto> ProductosMasVendidos { get; set; } = new();
    public List<DashboardVentaPorPeriodoDto> VentasPorPeriodo { get; set; } = new();
    public DashboardEstadoMesasDto EstadoMesas { get; set; } = new();
    public DashboardComandasPorEstadoDto ComandasPorEstado { get; set; } = new();
    public List<DashboardIngresosPorHoraDto> IngresosPorHora { get; set; } = new();
}
