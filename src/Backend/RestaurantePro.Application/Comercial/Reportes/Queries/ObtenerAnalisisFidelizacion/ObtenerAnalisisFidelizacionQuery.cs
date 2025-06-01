namespace RestaurantePro.Application.Comercial.Reportes.Queries.ObtenerAnalisisFidelizacion;

/// <summary>
/// 🎯 Query avanzada para análisis completo de fidelización de clientes
/// Usa ComercialServiceFacade para análisis de negocio complejos
/// </summary>
public class ObtenerAnalisisFidelizacionQuery : IRequest<Result<AnalisisFidelizacionDto>>
{
    public DateTime FechaInicio { get; init; }
    public DateTime FechaFin { get; init; }
    public List<Guid>? ClientesEspecificos { get; init; }
    public NivelFidelizacion? NivelMinimo { get; init; }
    public bool IncluirClientesInactivos { get; init; } = false;
    public bool IncluirTendencias { get; init; } = true;
    public bool IncluirProyecciones { get; init; } = false;
    public TipoAnalisis TipoAnalisis { get; init; } = TipoAnalisis.Completo;

    /// <summary>
    /// Factory method para crear análisis estándar mensual
    /// </summary>
    public static ObtenerAnalisisFidelizacionQuery CrearAnalisisMensual(
        DateTime? fechaInicio = null, 
        NivelFidelizacion? nivelMinimo = null,
        bool incluirProyecciones = false)
    {
        var inicio = fechaInicio ?? DateTime.Now.AddMonths(-1);
        var fin = inicio.AddMonths(1).AddDays(-1);

        return new ObtenerAnalisisFidelizacionQuery
        {
            FechaInicio = inicio,
            FechaFin = fin,
            NivelMinimo = nivelMinimo,
            IncluirClientesInactivos = false,
            IncluirTendencias = true,
            IncluirProyecciones = incluirProyecciones,
            TipoAnalisis = TipoAnalisis.Completo
        };
    }

    /// <summary>
    /// Factory method para análisis específico de clientes
    /// </summary>
    public static ObtenerAnalisisFidelizacionQuery CrearAnalisisClientes(
        List<Guid> clienteIds,
        DateTime fechaInicio,
        DateTime fechaFin,
        bool incluirTendencias = true)
    {
        return new ObtenerAnalisisFidelizacionQuery
        {
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            ClientesEspecificos = clienteIds,
            IncluirClientesInactivos = true,
            IncluirTendencias = incluirTendencias,
            IncluirProyecciones = false,
            TipoAnalisis = TipoAnalisis.ClientesEspecificos
        };
    }
}

/// <summary>
/// 🎯 Tipos de análisis de fidelización
/// </summary>
public enum TipoAnalisis
{
    Basico = 1,              // Solo estadísticas básicas
    Completo = 2,            // Análisis completo con tendencias
    ClientesEspecificos = 3, // Análisis de clientes específicos
    Comparativo = 4,         // Análisis comparativo entre períodos
    Predictivo = 5           // Análisis predictivo con ML
}

/// <summary>
/// 🎯 Resultado completo del análisis de fidelización
/// </summary>
public class AnalisisFidelizacionResult
{
    // Estadísticas Generales
    public EstadisticasGenerales EstadisticasGenerales { get; set; } = new();
    
    // Análisis por Nivel
    public List<AnalisisNivel> AnalisisPorNivel { get; set; } = new();
    
    // Clientes Top
    public List<ClienteTop> ClientesTop { get; set; } = new();
    
    // Tendencias
    public TendenciasFidelizacion? Tendencias { get; set; }
    
    // Proyecciones (si se solicitaron)
    public ProyeccionesFidelizacion? Proyecciones { get; set; }
    
    // Alertas y Recomendaciones
    public List<AlertaFidelizacion> Alertas { get; set; } = new();
    public List<RecomendacionFidelizacion> Recomendaciones { get; set; } = new();
    
    // Metadatos del análisis
    public DateTime FechaAnalisis { get; set; } = DateTime.UtcNow;
    public TimeSpan TiempoGeneracion { get; set; }
    public TipoAnalisis TipoAnalisis { get; set; }
    public string VersionAnalisis { get; set; } = "1.0";
}

/// <summary>
/// 📊 Estadísticas generales de fidelización
/// </summary>
public class EstadisticasGenerales
{
    public int TotalClientes { get; set; }
    public int ClientesActivos { get; set; }
    public int ClientesInactivos { get; set; }
    public decimal TasaRetencion { get; set; }
    public decimal ValorVidaPromedio { get; set; }
    public int PuntosAcumuladosTotal { get; set; }
    public int PuntosCanjeadosTotal { get; set; }
    public decimal TasaCanjePromedio { get; set; }
    public decimal TicketPromedioClienteFiel { get; set; }
    public decimal IncrementoVentasVsFrecuente { get; set; }
}

/// <summary>
/// 📊 Análisis por nivel de fidelización
/// </summary>
public class AnalisisNivel
{
    public NivelFidelizacion Nivel { get; set; }
    public int CantidadClientes { get; set; }
    public decimal PorcentajeTotalClientes { get; set; }
    public decimal VentasTotal { get; set; }
    public decimal VentasPromedioPorCliente { get; set; }
    public int PuntosPromedio { get; set; }
    public decimal TasaCanjeNivel { get; set; }
    public int CambiosHaciaNivel { get; set; }
    public int CambiosDesdeNivel { get; set; }
    public decimal TasaRetencionNivel { get; set; }
}

/// <summary>
/// 🏆 Cliente top en fidelización
/// </summary>
public class ClienteTop
{
    public Guid ClienteId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public NivelFidelizacion NivelActual { get; set; }
    public int PuntosAcumulados { get; set; }
    public decimal VentasTotales { get; set; }
    public int VisitasTotales { get; set; }
    public DateTime UltimaVisita { get; set; }
    public decimal TicketPromedio { get; set; }
    public string TipoCliente { get; set; } = string.Empty; // "Frecuente", "Alto Valor", "Fiel", etc.
}

/// <summary>
/// 📈 Tendencias de fidelización
/// </summary>
public class TendenciasFidelizacion
{
    public List<PuntoTendencia> EvolucionClientes { get; set; } = new();
    public List<PuntoTendencia> EvolucionPuntos { get; set; } = new();
    public List<PuntoTendencia> EvolucionCanjes { get; set; } = new();
    public List<CambioNivel> CambiosNiveles { get; set; } = new();
    public decimal CrecimientoMensual { get; set; }
    public string TendenciaGeneral { get; set; } = string.Empty; // "Positiva", "Estable", "Negativa"
}

/// <summary>
/// 📊 Punto de tendencia temporal
/// </summary>
public class PuntoTendencia
{
    public DateTime Fecha { get; set; }
    public decimal Valor { get; set; }
    public string Etiqueta { get; set; } = string.Empty;
}

/// <summary>
/// 🔄 Cambio de nivel de fidelización
/// </summary>
public class CambioNivel
{
    public DateTime Fecha { get; set; }
    public NivelFidelizacion NivelAnterior { get; set; }
    public NivelFidelizacion NivelNuevo { get; set; }
    public int CantidadClientes { get; set; }
}

/// <summary>
/// 🔮 Proyecciones de fidelización
/// </summary>
public class ProyeccionesFidelizacion
{
    public List<ProyeccionMes> ProyeccionMensual { get; set; } = new();
    public decimal ClientesProyectados3Meses { get; set; }
    public decimal VentasProyectadas3Meses { get; set; }
    public string ConfiabilidadProyeccion { get; set; } = string.Empty; // "Alta", "Media", "Baja"
}

/// <summary>
/// 📅 Proyección mensual
/// </summary>
public class ProyeccionMes
{
    public DateTime Mes { get; set; }
    public int ClientesProyectados { get; set; }
    public decimal VentasProyectadas { get; set; }
    public int PuntosProyectados { get; set; }
}

/// <summary>
/// ⚠️ Alerta de fidelización
/// </summary>
public class AlertaFidelizacion
{
    public TipoAlerta Tipo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public NivelPrioridad Prioridad { get; set; }
    public DateTime FechaDeteccion { get; set; }
    public Dictionary<string, object> Datos { get; set; } = new();
}

/// <summary>
/// 💡 Recomendación para mejorar fidelización
/// </summary>
public class RecomendacionFidelizacion
{
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public decimal ImpactoEsperado { get; set; }
    public NivelPrioridad Prioridad { get; set; }
    public string Categoria { get; set; } = string.Empty; // "Retención", "Acquisition", "Monetización"
}

/// <summary>
/// 🚨 Tipos de alerta
/// </summary>
public enum TipoAlerta
{
    ClientesInactivos = 1,
    BajaTasaCanje = 2,
    DisminucionNivel = 3,
    PuntosProximosVencer = 4,
    TendenciaNegativa = 5
} 