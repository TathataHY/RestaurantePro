using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

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
/// DTO para productos más vendidos
/// </summary>
public class ProductoMasVendidoDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public decimal Ingresos { get; set; }
    public decimal PorcentajeTotal { get; set; }
}

/// <summary>
/// DTO para ventas por período
/// </summary>
public class VentaPorPeriodoDto
{
    public DateTime Fecha { get; set; }
    public decimal Monto { get; set; }
    public int CantidadComandas { get; set; }
    public int CantidadProductos { get; set; }
}

/// <summary>
/// DTO para estado de mesas
/// </summary>
public class EstadoMesasDto
{
    public int Disponibles { get; set; }
    public int Ocupadas { get; set; }
    public int Reservadas { get; set; }
    public int EnLimpieza { get; set; }
    public int Total { get; set; }
}

/// <summary>
/// DTO para comandas por estado
/// </summary>
public class ComandasPorEstadoDto
{
    public int Pendientes { get; set; }
    public int EnPreparacion { get; set; }
    public int Listas { get; set; }
    public int Completadas { get; set; }
    public int Canceladas { get; set; }
    public int Total { get; set; }
}

/// <summary>
/// DTO para ingresos por hora
/// </summary>
public class IngresosPorHoraDto
{
    public int Hora { get; set; }
    public decimal Monto { get; set; }
    public int CantidadComandas { get; set; }
}

/// <summary>
/// DTO para resumen de dashboard
/// </summary>
public class DashboardResumenDto
{
    public DashboardMetricasDto Metricas { get; set; } = new();
    public List<ProductoMasVendidoDto> ProductosMasVendidos { get; set; } = new();
    public List<VentaPorPeriodoDto> VentasUltimos7Dias { get; set; } = new();
    public EstadoMesasDto EstadoMesas { get; set; } = new();
    public ComandasPorEstadoDto ComandasPorEstado { get; set; } = new();
    public List<IngresosPorHoraDto> IngresosPorHora { get; set; } = new();
}
