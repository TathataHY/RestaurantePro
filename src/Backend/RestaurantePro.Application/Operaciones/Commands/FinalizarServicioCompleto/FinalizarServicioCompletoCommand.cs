namespace RestaurantePro.Application.Operaciones.Commands.FinalizarServicioCompleto;

/// <summary>
/// 🚀 COMMAND MÁS AVANZADO: Finalizar servicio completo de restaurante
/// Orquesta: Comanda + Facturación + Fidelización + Mesa + Notificaciones + Analytics
/// PATRÓN SAGA para operaciones distribuidas
/// </summary>
public class FinalizarServicioCompletoCommand : IRequest<Result<ServicioCompletoResult>>
{
    public Guid ComandaId { get; init; }
    public Guid? ClienteId { get; init; }
    public TipoFinalizacion TipoFinalizacion { get; init; } = TipoFinalizacion.Normal;
    public decimal? PropinaSugerida { get; init; }
    public bool GenerarFacturaInmediata { get; init; } = true;
    public bool AplicarDescuentoFidelizacion { get; init; } = true;
    public bool LiberarMesaAutomaticamente { get; init; } = true;
    public bool EnviarNotificacionCliente { get; init; } = true;
    public bool RegistrarEstadisticas { get; init; } = true;
    public string? ObservacionesFinalizacion { get; init; }

    /// <summary>
    /// Factory method para crear el command con configuración completa
    /// </summary>
    public static FinalizarServicioCompletoCommand Crear(
        Guid comandaId,
        Guid? clienteId = null,
        TipoFinalizacion tipo = TipoFinalizacion.Normal,
        decimal? propina = null,
        bool generarFactura = true,
        bool aplicarDescuento = true,
        bool liberarMesa = true,
        bool enviarNotificacion = true,
        string? observaciones = null)
    {
        if (comandaId == Guid.Empty)
            throw new ArgumentException("ComandaId no puede estar vacío", nameof(comandaId));

        return new FinalizarServicioCompletoCommand
        {
            ComandaId = comandaId,
            ClienteId = clienteId,
            TipoFinalizacion = tipo,
            PropinaSugerida = propina,
            GenerarFacturaInmediata = generarFactura,
            AplicarDescuentoFidelizacion = aplicarDescuento,
            LiberarMesaAutomaticamente = liberarMesa,
            EnviarNotificacionCliente = enviarNotificacion,
            RegistrarEstadisticas = true,
            ObservacionesFinalizacion = observaciones
        };
    }
}

/// <summary>
/// 🎯 Tipos de finalización de servicio
/// </summary>
public enum TipoFinalizacion
{
    Normal = 1,           // Finalización estándar
    Express = 2,          // Finalización rápida sin extras
    Premium = 3,          // Finalización con servicios adicionales
    Cortesia = 4,         // Sin costo (cortesía de la casa)
    Cancelacion = 5       // Cancelación con procesos especiales
}

/// <summary>
/// 🎯 Resultado completo del procesamiento de servicio
/// </summary>
public class ServicioCompletoResult
{
    public Guid ComandaId { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid? MesaId { get; set; }
    public Guid? FacturaId { get; set; }
    public EstadoProcesamiento Estado { get; set; } = EstadoProcesamiento.Exitoso;
    
    // Información Financiera
    public decimal TotalOriginal { get; set; }
    public decimal TotalDescuentos { get; set; }
    public decimal TotalPropinas { get; set; }
    public decimal TotalFinal { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    
    // Información de Fidelización
    public int PuntosOtorgados { get; set; }
    public int PuntosUtilizados { get; set; }
    public string NivelFidelizacionAnterior { get; set; } = string.Empty;
    public string NivelFidelizacionActual { get; set; } = string.Empty;
    public bool HuboCambioNivel { get; set; }
    
    // Información Operacional
    public DateTime FechaInicioServicio { get; set; }
    public DateTime FechaFinServicio { get; set; }
    public TimeSpan DuracionServicio { get; set; }
    public bool MesaLiberada { get; set; }
    public bool NotificacionEnviada { get; set; }
    
    // Seguimiento del Proceso
    public List<PasoServicio> PasosEjecutados { get; set; } = new();
    public List<string> Advertencias { get; set; } = new();
    public List<string> Errores { get; set; } = new();
    public List<string> Mensajes { get; set; } = new();
    
    // Estadísticas y Analytics
    public ServicioEstadisticas Estadisticas { get; set; } = new();
    public DateTime FechaProcesamiento { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// ⚙️ Estado del procesamiento completo
/// </summary>
public enum EstadoProcesamiento
{
    Exitoso = 1,              // Todo procesado correctamente
    ExitosoConAdvertencias = 2, // Procesado con advertencias menores
    FallosParciales = 3,       // Algunos pasos fallaron pero se completó
    Fallido = 4               // Falló completamente
}

/// <summary>
/// 📋 Paso individual del procesamiento de servicio
/// </summary>
public class PasoServicio
{
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaEjecucion { get; set; }
    public bool Exitoso { get; set; }
    public string? Detalle { get; set; }
    public double DuracionMs { get; set; }
    public int Orden { get; set; }
}

/// <summary>
/// 📊 Estadísticas detalladas del servicio
/// </summary>
public class ServicioEstadisticas
{
    public int TotalItems { get; set; }
    public decimal TicketPromedio { get; set; }
    public decimal EficienciaServicio { get; set; }
    public decimal SatisfaccionCliente { get; set; }
    public int TiempoPromedioPreparacion { get; set; }
    public bool ServicioRapido { get; set; }
    public Dictionary<string, object> MetricasAdicionales { get; set; } = new();
} 