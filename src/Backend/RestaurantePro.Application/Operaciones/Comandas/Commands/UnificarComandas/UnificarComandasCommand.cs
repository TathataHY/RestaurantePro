namespace RestaurantePro.Application.Operaciones.Comandas.Commands.UnificarComandas;

/// <summary>
/// Command para unificar múltiples comandas en una sola comanda
/// Permite combinar comandas independientes en una comanda consolidada
/// </summary>
public class UnificarComandasCommand : IRequest<Result<UnificarComandasDto>>
{
    /// <summary>
    /// IDs de las comandas a unificar
    /// </summary>
    public List<Guid> ComandasIds { get; set; } = new();

    /// <summary>
    /// ID de la comanda principal (opcional, si no se especifica se crea una nueva)
    /// </summary>
    public Guid? ComandaPrincipalId { get; set; }

    /// <summary>
    /// Mesa de destino para la comanda unificada
    /// </summary>
    public Guid MesaDestinoId { get; set; }

    /// <summary>
    /// Mesero asignado a la comanda unificada
    /// </summary>
    public Guid MeseroId { get; set; }

    /// <summary>
    /// Motivo de la unificación
    /// </summary>
    public string MotivoUnificacion { get; set; } = string.Empty;

    /// <summary>
    /// Usuario que autoriza la unificación
    /// </summary>
    public Guid? AutorizadoPor { get; set; }

    /// <summary>
    /// Notas adicionales sobre la unificación
    /// </summary>
    public string? NotasUnificacion { get; set; }

    /// <summary>
    /// Estrategia para manejar descuentos conflictivos
    /// </summary>
    public EstrategiaDescuentos EstrategiaDescuentos { get; set; } = EstrategiaDescuentos.Sumar;

    /// <summary>
    /// Indica si se debe mantener el histórico de comandas originales
    /// </summary>
    public bool MantenerHistorico { get; set; } = true;

    /// <summary>
    /// Observaciones para la comanda unificada
    /// </summary>
    public string? ObservacionesUnificada { get; set; }

    /// <summary>
    /// Datos adicionales de la unificación
    /// </summary>
    public Dictionary<string, object>? DatosAdicionales { get; set; }

    public UnificarComandasCommand(List<Guid> comandasIds, Guid mesaDestinoId, Guid meseroId, string motivoUnificacion)
    {
        ComandasIds = comandasIds;
        MesaDestinoId = mesaDestinoId;
        MeseroId = meseroId;
        MotivoUnificacion = motivoUnificacion;
    }

    public UnificarComandasCommand() { }
}

/// <summary>
/// Estrategias para manejar descuentos al unificar comandas
/// </summary>
public enum EstrategiaDescuentos
{
    /// <summary>
    /// Sumar todos los descuentos
    /// </summary>
    Sumar = 1,

    /// <summary>
    /// Tomar el descuento mayor
    /// </summary>
    TomarMayor = 2,

    /// <summary>
    /// Tomar el descuento menor
    /// </summary>
    TomarMenor = 3,

    /// <summary>
    /// Promedio de descuentos
    /// </summary>
    Promedio = 4,

    /// <summary>
    /// Sin descuentos (resetear)
    /// </summary>
    SinDescuentos = 5
}

/// <summary>
/// DTO de respuesta para la unificación de comandas
/// </summary>
public class UnificarComandasDto
{
    public List<Guid> ComandasOriginalesIds { get; set; } = new();
    public Guid ComandaUnificadaId { get; set; }
    public Guid MesaDestinoId { get; set; }
    public Guid MeseroId { get; set; }
    public string MotivoUnificacion { get; set; } = string.Empty;
    public DateTime FechaUnificacion { get; set; }
    public Guid? AutorizadoPor { get; set; }
    public bool UnificacionExitosa { get; set; }
    public int TotalItemsUnificados { get; set; }
    public decimal MontoTotalUnificado { get; set; }
    public EstrategiaDescuentos EstrategiaDescuentos { get; set; }
} 