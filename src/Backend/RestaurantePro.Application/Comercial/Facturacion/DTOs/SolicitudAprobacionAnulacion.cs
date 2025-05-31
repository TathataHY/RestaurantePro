namespace RestaurantePro.Application.Comercial.Facturacion.DTOs;

/// <summary>
/// DTO para representar una solicitud de aprobación de anulación de factura
/// </summary>
public class SolicitudAprobacionAnulacion
{
    /// <summary>
    /// ID único de la solicitud
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID de la factura a anular
    /// </summary>
    public Guid FacturaId { get; set; }

    /// <summary>
    /// Número de la factura
    /// </summary>
    public string NumeroFactura { get; set; } = string.Empty;

    /// <summary>
    /// Monto total de la factura
    /// </summary>
    public decimal MontoFactura { get; set; }

    /// <summary>
    /// ID del usuario que solicita la anulación
    /// </summary>
    public Guid UsuarioSolicitaId { get; set; }

    /// <summary>
    /// Nombre del usuario que solicita
    /// </summary>
    public string NombreUsuarioSolicita { get; set; } = string.Empty;

    /// <summary>
    /// Motivo de la anulación
    /// </summary>
    public string MotivoAnulacion { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de anulación solicitada
    /// </summary>
    public TipoAnulacion TipoAnulacion { get; set; }

    /// <summary>
    /// Justificación detallada de la anulación
    /// </summary>
    public string JustificacionDetallada { get; set; } = string.Empty;

    /// <summary>
    /// Documentos de soporte adjuntos
    /// </summary>
    public List<DocumentoSoporteDto> DocumentosSoporte { get; set; } = new();

    /// <summary>
    /// Fecha de creación de la solicitud
    /// </summary>
    public DateTime FechaSolicitud { get; set; }

    /// <summary>
    /// Estado actual de la solicitud
    /// </summary>
    public EstadoSolicitudAnulacion Estado { get; set; }

    /// <summary>
    /// Prioridad de la solicitud
    /// </summary>
    public PrioridadAnulacion Prioridad { get; set; }

    /// <summary>
    /// ID del usuario aprobador asignado
    /// </summary>
    public Guid? UsuarioAprobadorId { get; set; }

    /// <summary>
    /// Nombre del usuario aprobador
    /// </summary>
    public string? NombreUsuarioAprobador { get; set; }

    /// <summary>
    /// Fecha de aprobación/rechazo
    /// </summary>
    public DateTime? FechaAprobacion { get; set; }

    /// <summary>
    /// Comentarios del aprobador
    /// </summary>
    public string? ComentariosAprobador { get; set; }

    /// <summary>
    /// Fecha límite para la aprobación
    /// </summary>
    public DateTime? FechaLimiteAprobacion { get; set; }

    /// <summary>
    /// Indica si requiere aprobación adicional
    /// </summary>
    public bool RequiereAprobacionAdicional { get; set; }

    /// <summary>
    /// Nivel de aprobación requerido (1-5)
    /// </summary>
    public int NivelAprobacionRequerido { get; set; }

    /// <summary>
    /// Aprobaciones obtenidas hasta el momento
    /// </summary>
    public List<AprobacionDto> AprobacionesObtenidas { get; set; } = new();

    /// <summary>
    /// Historial de estados de la solicitud
    /// </summary>
    public List<HistorialEstadoSolicitudDto> HistorialEstados { get; set; } = new();

    /// <summary>
    /// Observaciones adicionales
    /// </summary>
    public string? ObservacionesAdicionales { get; set; }

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public SolicitudAprobacionAnulacion()
    {
        Id = Guid.NewGuid();
        FechaSolicitud = DateTime.UtcNow;
        Estado = EstadoSolicitudAnulacion.Pendiente;
        Prioridad = PrioridadAnulacion.Normal;
        NivelAprobacionRequerido = 1;
    }

    /// <summary>
    /// Factory method para solicitud básica
    /// </summary>
    public static SolicitudAprobacionAnulacion CrearBasica(
        Guid facturaId,
        string numeroFactura,
        decimal montoFactura,
        Guid usuarioSolicitaId,
        string nombreUsuario,
        string motivo,
        TipoAnulacion tipo)
    {
        return new SolicitudAprobacionAnulacion
        {
            FacturaId = facturaId,
            NumeroFactura = numeroFactura,
            MontoFactura = montoFactura,
            UsuarioSolicitaId = usuarioSolicitaId,
            NombreUsuarioSolicita = nombreUsuario,
            MotivoAnulacion = motivo,
            TipoAnulacion = tipo,
            NivelAprobacionRequerido = DeterminarNivelAprobacion(montoFactura, tipo)
        };
    }

    /// <summary>
    /// Determina el nivel de aprobación requerido basado en el monto y tipo
    /// </summary>
    private static int DeterminarNivelAprobacion(decimal monto, TipoAnulacion tipo)
    {
        return tipo switch
        {
            TipoAnulacion.ErrorSistema => 1,
            TipoAnulacion.ErrorOperativo => monto > 10000 ? 2 : 1,
            TipoAnulacion.SolicitudCliente => monto > 5000 ? 2 : 1,
            TipoAnulacion.Devolucion => monto > 15000 ? 3 : 2,
            TipoAnulacion.Fraude => 3,
            TipoAnulacion.Otro => monto > 20000 ? 3 : 2,
            _ => 1
        };
    }

    /// <summary>
    /// Indica si la solicitud está vencida
    /// </summary>
    public bool EstaVencida => FechaLimiteAprobacion.HasValue && DateTime.UtcNow > FechaLimiteAprobacion.Value;

    /// <summary>
    /// Indica si la solicitud está aprobada
    /// </summary>
    public bool EstaAprobada => Estado == EstadoSolicitudAnulacion.Aprobada;

    /// <summary>
    /// Indica si la solicitud está rechazada
    /// </summary>
    public bool EstaRechazada => Estado == EstadoSolicitudAnulacion.Rechazada;

    /// <summary>
    /// Indica si la solicitud está pendiente
    /// </summary>
    public bool EstaPendiente => Estado == EstadoSolicitudAnulacion.Pendiente;
}

/// <summary>
/// DTO para documento de soporte
/// </summary>
public class DocumentoSoporteDto
{
    public Guid Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public string TipoArchivo { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public DateTime FechaCarga { get; set; }
    public string? Descripcion { get; set; }
}

/// <summary>
/// DTO para aprobaciones individuales
/// </summary>
public class AprobacionDto
{
    public Guid Id { get; set; }
    public Guid UsuarioAprobadorId { get; set; }
    public string NombreAprobador { get; set; } = string.Empty;
    public DateTime FechaAprobacion { get; set; }
    public bool Aprobado { get; set; }
    public string? Comentarios { get; set; }
    public int NivelAprobacion { get; set; }
}

/// <summary>
/// DTO para historial de estados
/// </summary>
public class HistorialEstadoSolicitudDto
{
    public Guid Id { get; set; }
    public EstadoSolicitudAnulacion EstadoAnterior { get; set; }
    public EstadoSolicitudAnulacion EstadoNuevo { get; set; }
    public DateTime FechaCambio { get; set; }
    public Guid UsuarioCambio { get; set; }
    public string NombreUsuarioCambio { get; set; } = string.Empty;
    public string? Motivo { get; set; }
}

/// <summary>
/// Enums relacionados
/// </summary>
public enum TipoAnulacion
{
    ErrorSistema,
    ErrorOperativo,
    SolicitudCliente,
    Devolucion,
    Fraude,
    Otro
}

public enum EstadoSolicitudAnulacion
{
    Pendiente,
    EnRevision,
    Aprobada,
    Rechazada,
    Vencida,
    Cancelada
}

public enum PrioridadAnulacion
{
    Baja,
    Normal,
    Alta,
    Critica,
    Urgente
} 