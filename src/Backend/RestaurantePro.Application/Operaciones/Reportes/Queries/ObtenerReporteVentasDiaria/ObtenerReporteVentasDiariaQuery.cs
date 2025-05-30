namespace RestaurantePro.Application.Operaciones.Reportes.Queries.ObtenerReporteVentasDiaria;

/// <summary>
/// 🎯 Query avanzada para reporte de ventas diarias con análisis operacional
/// Usa OperacionesServiceFacade para métricas complejas
/// </summary>
public class ObtenerReporteVentasDiariaQuery : IRequest<Result<ReporteVentasDiariaResult>>
{
    public DateTime FechaReporte { get; init; }
    public bool IncluirComparativoPeriodoAnterior { get; init; } = true;
    public bool IncluirAnalisisPorMesa { get; init; } = true;
    public bool IncluirAnalisisPorMesero { get; init; } = true;
    public bool IncluirAnalisisProductos { get; init; } = true;
    public bool IncluirTendenciasSemana { get; init; } = false;
    public List<Guid>? MesesEspecificos { get; init; }
    public List<Guid>? MeserosEspecificos { get; init; }
    public NivelDetalle NivelDetalle { get; init; } = NivelDetalle.Completo;

    /// <summary>
    /// Factory method para reporte estándar del día actual
    /// </summary>
    public static ObtenerReporteVentasDiariaQuery CrearReporteHoy(
        bool incluirComparativo = true,
        bool incluirTendencias = false,
        NivelDetalle nivel = NivelDetalle.Completo)
    {
        return new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            IncluirComparativoPeriodoAnterior = incluirComparativo,
            IncluirAnalisisPorMesa = true,
            IncluirAnalisisPorMesero = true,
            IncluirAnalisisProductos = true,
            IncluirTendenciasSemana = incluirTendencias,
            NivelDetalle = nivel
        };
    }

    /// <summary>
    /// Factory method para reporte de fecha específica
    /// </summary>
    public static ObtenerReporteVentasDiariaQuery CrearReporteFecha(
        DateTime fecha,
        bool incluirComparativo = true,
        NivelDetalle nivel = NivelDetalle.Completo)
    {
        return new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = fecha.Date,
            IncluirComparativoPeriodoAnterior = incluirComparativo,
            IncluirAnalisisPorMesa = true,
            IncluirAnalisisPorMesero = true,
            IncluirAnalisisProductos = true,
            IncluirTendenciasSemana = false,
            NivelDetalle = nivel
        };
    }

    /// <summary>
    /// Factory method para reporte específico de meseros
    /// </summary>
    public static ObtenerReporteVentasDiariaQuery CrearReporteMeseros(
        DateTime fecha,
        List<Guid> meseroIds,
        bool incluirComparativo = false)
    {
        return new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = fecha.Date,
            MeserosEspecificos = meseroIds,
            IncluirComparativoPeriodoAnterior = incluirComparativo,
            IncluirAnalisisPorMesa = false,
            IncluirAnalisisPorMesero = true,
            IncluirAnalisisProductos = false,
            NivelDetalle = NivelDetalle.Meseros
        };
    }
}

/// <summary>
/// 🎯 Niveles de detalle del reporte
/// </summary>
public enum NivelDetalle
{
    Basico = 1,      // Solo números principales
    Intermedio = 2,  // Incluye análisis por mesa y mesero
    Completo = 3,    // Análisis completo con productos
    Meseros = 4,     // Enfocado en análisis de meseros
    Mesas = 5        // Enfocado en análisis de mesas
}

/// <summary>
/// 🎯 Resultado completo del reporte de ventas diarias
/// </summary>
public class ReporteVentasDiariaResult
{
    // Información del Reporte
    public DateTime FechaReporte { get; set; }
    public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;
    public TimeSpan TiempoGeneracion { get; set; }
    public NivelDetalle NivelDetalle { get; set; }

    // Resumen Ejecutivo
    public ResumenEjecutivo ResumenEjecutivo { get; set; } = new();

    // Análisis Detallados
    public List<VentaPorMesa>? VentasPorMesa { get; set; }
    public List<VentaPorMesero>? VentasPorMesero { get; set; }
    public List<VentaPorProducto>? VentasPorProducto { get; set; }

    // Comparativo (si se solicitó)
    public ComparativoPeriodos? Comparativo { get; set; }

    // Tendencias Semanales (si se solicitó)
    public TendenciasSemana? TendenciasSemana { get; set; }

    // Alertas y Observaciones
    public List<AlertaOperacional> Alertas { get; set; } = new();
    public List<ObservacionReporte> Observaciones { get; set; } = new();

    // Métricas de Rendimiento
    public MetricasRendimiento MetricasRendimiento { get; set; } = new();
}

/// <summary>
/// 📊 Resumen ejecutivo del día
/// </summary>
public class ResumenEjecutivo
{
    // Ventas
    public decimal VentasTotalDia { get; set; }
    public decimal TicketPromedio { get; set; }
    public int TotalComandas { get; set; }
    public int TotalClientes { get; set; }

    // Operaciones
    public int MesasAtendidas { get; set; }
    public int MeserosActivos { get; set; }
    public TimeSpan TiempoPromedioServicio { get; set; }
    public decimal EficienciaOperacional { get; set; }

    // Productos
    public int ProductosVendidos { get; set; }
    public string ProductoMasVendido { get; set; } = string.Empty;
    public decimal VentasProductoTop { get; set; }

    // Comparativo Simple
    public decimal CambioPorcentualVentas { get; set; }
    public string TendenciaGeneral { get; set; } = string.Empty; // "Positiva", "Estable", "Negativa"
}

/// <summary>
/// 🪑 Análisis de ventas por mesa
/// </summary>
public class VentaPorMesa
{
    public Guid MesaId { get; set; }
    public string NumeroMesa { get; set; } = string.Empty;
    public int CapacidadMesa { get; set; }
    public decimal VentasTotal { get; set; }
    public int ComandasAtendidas { get; set; }
    public TimeSpan TiempoOcupacionTotal { get; set; }
    public decimal EficienciaMesa { get; set; }
    public decimal VentasPorHora { get; set; }
    public int RotacionMesa { get; set; }
    public string EstadoFinal { get; set; } = string.Empty;
}

/// <summary>
/// 👨‍🍳 Análisis de ventas por mesero
/// </summary>
public class VentaPorMesero
{
    public Guid MeseroId { get; set; }
    public string NombreMesero { get; set; } = string.Empty;
    public decimal VentasTotal { get; set; }
    public int ComandasAtendidas { get; set; }
    public int MesasAtendidas { get; set; }
    public decimal TicketPromedio { get; set; }
    public TimeSpan TiempoServicioPromedio { get; set; }
    public decimal EficienciaMesero { get; set; }
    public decimal PropinasTotal { get; set; }
    public int ClientesAtendidos { get; set; }
    public decimal SatisfaccionPromedio { get; set; }
    public string RankingRendimiento { get; set; } = string.Empty;
}

/// <summary>
/// 🍽️ Análisis de ventas por producto
/// </summary>
public class VentaPorProducto
{
    public Guid ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string CategoriaProducto { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public decimal VentasTotal { get; set; }
    public decimal PrecioPromedio { get; set; }
    public decimal MargenGanancia { get; set; }
    public int VecesOrdenado { get; set; }
    public decimal TasaConversion { get; set; }
    public List<HoraPico> HorasPico { get; set; } = new();
    public string TendenciaVenta { get; set; } = string.Empty;
}

/// <summary>
/// ⏰ Hora pico de venta
/// </summary>
public class HoraPico
{
    public TimeSpan Hora { get; set; }
    public int CantidadVendida { get; set; }
    public string Periodo { get; set; } = string.Empty; // "Desayuno", "Almuerzo", "Cena"
}

/// <summary>
/// 📊 Comparativo entre períodos
/// </summary>
public class ComparativoPeriodos
{
    public DateTime FechaPeriodoAnterior { get; set; }
    public decimal VentasPeriodoAnterior { get; set; }
    public decimal CambioAbsoluto { get; set; }
    public decimal CambioPorcentual { get; set; }
    public int CambioComandas { get; set; }
    public decimal CambioTicketPromedio { get; set; }
    public List<CambioMetrica> CambiosDetallados { get; set; } = new();
    public string AnalisisComparativo { get; set; } = string.Empty;
}

/// <summary>
/// 📈 Cambio en métrica específica
/// </summary>
public class CambioMetrica
{
    public string NombreMetrica { get; set; } = string.Empty;
    public decimal ValorActual { get; set; }
    public decimal ValorAnterior { get; set; }
    public decimal CambioPorcentual { get; set; }
    public string TipoCambio { get; set; } = string.Empty; // "Mejora", "Empeoramiento", "Estable"
}

/// <summary>
/// 📅 Tendencias de la semana
/// </summary>
public class TendenciasSemana
{
    public List<PuntoTendenciaDia> VentasPorDia { get; set; } = new();
    public List<PuntoTendenciaDia> ComandasPorDia { get; set; } = new();
    public string DiaMasVentas { get; set; } = string.Empty;
    public string DiaMenosVentas { get; set; } = string.Empty;
    public decimal PromedioSemana { get; set; }
    public decimal DesviacionEstandar { get; set; }
    public string PatronSemana { get; set; } = string.Empty;
}

/// <summary>
/// 📊 Punto de tendencia diaria
/// </summary>
public class PuntoTendenciaDia
{
    public DateTime Fecha { get; set; }
    public decimal Valor { get; set; }
    public string DiaSemana { get; set; } = string.Empty;
    public bool EsDiaEspecial { get; set; }
}

/// <summary>
/// ⚠️ Alerta operacional
/// </summary>
public class AlertaOperacional
{
    public TipoAlertaOperacional Tipo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public NivelPrioridad Prioridad { get; set; }
    public Dictionary<string, object> Datos { get; set; } = new();
}

/// <summary>
/// 📝 Observación del reporte
/// </summary>
public class ObservacionReporte
{
    public string Categoria { get; set; } = string.Empty;
    public string Observacion { get; set; } = string.Empty;
    public string Recomendacion { get; set; } = string.Empty;
    public decimal ImpactoEstimado { get; set; }
}

/// <summary>
/// ⚡ Métricas de rendimiento del día
/// </summary>
public class MetricasRendimiento
{
    public decimal UtilizacionMesas { get; set; }
    public decimal EficienciaMeseros { get; set; }
    public decimal VelocidadServicio { get; set; }
    public decimal SatisfaccionGeneral { get; set; }
    public decimal RotacionInventario { get; set; }
    public string ClasificacionDia { get; set; } = string.Empty; // "Excelente", "Bueno", "Regular", "Malo"
}

/// <summary>
/// 🚨 Tipos de alerta operacional
/// </summary>
public enum TipoAlertaOperacional
{
    VentasBajas = 1,
    TiempoServicioLento = 2,
    BajaEficienciaMesa = 3,
    MeseroSobrecargado = 4,
    ProductoBajoStock = 5
} 