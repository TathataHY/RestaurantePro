namespace RestaurantePro.Application.Operaciones.Reportes.Commands.GenerarReporte;

/// <summary>
/// 🎯 Command para generar reportes personalizados del sistema
/// Permite generar diferentes tipos de reportes con configuraciones específicas
/// </summary>
public class GenerarReporteCommand : IRequest<Result<ReporteGeneradoResult>>
{
    public TipoReporte TipoReporte { get; init; }
    public DateTime FechaInicio { get; init; }
    public DateTime FechaFin { get; init; }
    public FormatoReporte Formato { get; init; } = FormatoReporte.PDF;
    public List<Guid>? FiltrosEspecificos { get; init; }
    public Dictionary<string, object>? ParametrosAdicionales { get; init; }
    public bool IncluirGraficos { get; init; } = true;
    public bool IncluirDetalles { get; init; } = true;
    public bool IncluirResumenEjecutivo { get; init; } = true;
    public string? NombrePersonalizado { get; init; }
    public Guid UsuarioSolicitanteId { get; init; }
    public NivelPrioridad Prioridad { get; init; } = NivelPrioridad.Media;
    public bool EnviarPorEmail { get; init; } = false;
    public string? EmailDestino { get; init; }

    /// <summary>
    /// Factory method para reporte de ventas diarias
    /// </summary>
    public static GenerarReporteCommand CrearReporteVentas(
        DateTime fecha,
        Guid usuarioId,
        FormatoReporte formato = FormatoReporte.PDF)
    {
        return new GenerarReporteCommand
        {
            TipoReporte = TipoReporte.VentasDiarias,
            FechaInicio = fecha.Date,
            FechaFin = fecha.Date.AddDays(1).AddSeconds(-1),
            Formato = formato,
            UsuarioSolicitanteId = usuarioId,
            IncluirGraficos = true,
            IncluirDetalles = true,
            IncluirResumenEjecutivo = true,
            Prioridad = NivelPrioridad.Media
        };
    }

    /// <summary>
    /// Factory method para reporte de inventario
    /// </summary>
    public static GenerarReporteCommand CrearReporteInventario(
        DateTime fechaInicio,
        DateTime fechaFin,
        Guid usuarioId,
        List<Guid>? ingredientesEspecificos = null)
    {
        return new GenerarReporteCommand
        {
            TipoReporte = TipoReporte.Inventario,
            FechaInicio = fechaInicio.Date,
            FechaFin = fechaFin.Date,
            FiltrosEspecificos = ingredientesEspecificos,
            UsuarioSolicitanteId = usuarioId,
            IncluirGraficos = true,
            IncluirDetalles = true,
            Prioridad = NivelPrioridad.Media
        };
    }

    /// <summary>
    /// Factory method para reporte financiero
    /// </summary>
    public static GenerarReporteCommand CrearReporteFinanciero(
        DateTime fechaInicio,
        DateTime fechaFin,
        Guid usuarioId,
        bool enviarEmail = false,
        string? emailDestino = null)
    {
        return new GenerarReporteCommand
        {
            TipoReporte = TipoReporte.Financiero,
            FechaInicio = fechaInicio.Date,
            FechaFin = fechaFin.Date,
            UsuarioSolicitanteId = usuarioId,
            IncluirGraficos = true,
            IncluirDetalles = true,
            IncluirResumenEjecutivo = true,
            Prioridad = NivelPrioridad.Alta,
            EnviarPorEmail = enviarEmail,
            EmailDestino = emailDestino
        };
    }
}

/// <summary>
/// 📊 Tipos de reportes disponibles
/// </summary>
public enum TipoReporte
{
    VentasDiarias = 1,
    VentasSemanales = 2,
    VentasMensuales = 3,
    Inventario = 4,
    Financiero = 5,
    ClientesFidelizacion = 6,
    RendimientoMeseros = 7,
    EficienciaMesas = 8,
    ProductosMasVendidos = 9,
    AnalisisCostos = 10,
    Personalizado = 99
}

/// <summary>
/// 📄 Formatos de reporte disponibles
/// </summary>
public enum FormatoReporte
{
    PDF = 1,
    Excel = 2,
    CSV = 3,
    JSON = 4,
    HTML = 5
}

/// <summary>
/// 🎯 Resultado de la generación del reporte
/// </summary>
public class ReporteGeneradoResult
{
    public Guid ReporteId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public FormatoReporte Formato { get; set; }
    public long TamanoBytes { get; set; }
    public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;
    public TimeSpan TiempoGeneracion { get; set; }
    public bool EnviadoPorEmail { get; set; }
    public string? UrlDescarga { get; set; }
    public DateTime FechaExpiracion { get; set; }
    public EstadoReporte Estado { get; set; } = EstadoReporte.Generado;
    public string? MensajeError { get; set; }
    public Dictionary<string, object> MetadatosAdicionales { get; set; } = new();
}

/// <summary>
/// 📊 Estado del reporte generado
/// </summary>
public enum EstadoReporte
{
    Pendiente = 1,
    Generando = 2,
    Generado = 3,
    Error = 4,
    Expirado = 5,
    Eliminado = 6
} 