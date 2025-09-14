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
    public decimal VentasTotalDia { get; set; } // Ventas totales del día completo (todos los turnos)
    public int MesasOcupadas { get; set; }
    public int MesasDisponibles { get; set; }
    public int TotalMesas { get; set; }
    public int ComandasActivas { get; set; }
    public int ComandasCompletadas { get; set; }
    public int ProductosVendidosHoy { get; set; }
        public int ClientesAtendidosHoy { get; set; }
        public decimal PromedioTicket { get; set; }
        public int TiempoPromedio { get; set; } // en minutos
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
    public decimal Monto { get; set; } // Cambiado para coincidir con el frontend
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
    public int EnLimpieza { get; set; }
    public int Total { get; set; }
}

/// <summary>
/// DTO para comandas por estado del dashboard
/// </summary>
public class DashboardComandasPorEstadoDto
{
    public int Pendientes { get; set; }
    public int EnProceso { get; set; }
    public int Listas { get; set; }
    public int Entregadas { get; set; }
    public int Canceladas { get; set; }
    public int Total { get; set; }
}

/// <summary>
/// DTO para ingresos por hora del dashboard
/// </summary>
public class DashboardIngresosPorHoraDto
{
    public int Hora { get; set; }
    public decimal Monto { get; set; } // Cambiado para coincidir con el frontend
    public int CantidadFacturas { get; set; }
}

/// <summary>
/// DTO para ingresos por categoría del dashboard
/// </summary>
public class DashboardIngresosPorCategoriaDto
{
    public string Categoria { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public decimal Porcentaje { get; set; }
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
    public List<DashboardIngresosPorCategoriaDto> IngresosPorCategoria { get; set; } = new();
    public DateTime UltimaActualizacion { get; set; }
}

/// <summary>
/// DTO para detalles de mesa en el mapa interactivo
/// </summary>
public class MesaDetalleDto
{
    public Guid Id { get; set; }
    public int Numero { get; set; }
    public int Capacidad { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}
