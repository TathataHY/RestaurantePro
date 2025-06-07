using System;

namespace RestaurantePro.Application.Operaciones.Mesas.DTOs;

/// <summary>
/// Resultado del proceso de liberación de mesa
/// </summary>
public class LiberarMesaResult
{
    /// <summary>
    /// ID de la mesa liberada
    /// </summary>
    public Guid MesaId { get; set; }

    /// <summary>
    /// Número de la mesa liberada
    /// </summary>
    public string Numero { get; set; } = string.Empty;

    /// <summary>
    /// Estado de la mesa después de la liberación
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Fecha y hora de la liberación
    /// </summary>
    public DateTime FechaLiberacion { get; set; } = DateTime.Now;

    /// <summary>
    /// Tiempo que estuvo ocupada la mesa
    /// </summary>
    public TimeSpan? TiempoOcupacion { get; set; }

    /// <summary>
    /// Indica si la liberación fue exitosa
    /// </summary>
    public bool Exitoso { get; set; }

    /// <summary>
    /// Mensaje descriptivo del resultado
    /// </summary>
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Estado anterior de la mesa
    /// </summary>
    public string EstadoAnterior { get; set; } = string.Empty;

    /// <summary>
    /// Indica si requiere limpieza
    /// </summary>
    public bool RequiereLimpieza { get; set; }

    /// <summary>
    /// Tiempo estimado de limpieza en minutos
    /// </summary>
    public int TiempoLimpiezaMinutos { get; set; }

    /// <summary>
    /// Indica si requiere mantenimiento
    /// </summary>
    public bool RequiereMantenimiento { get; set; }

    /// <summary>
    /// Tipo de mantenimiento requerido
    /// </summary>
    public string? TipoMantenimiento { get; set; }

    /// <summary>
    /// Indica si la liberación fue forzada
    /// </summary>
    public bool LiberacionForzada { get; set; }

    /// <summary>
    /// Motivo de la liberación forzada
    /// </summary>
    public string? MotivoLiberacionForzada { get; set; }

    /// <summary>
    /// Total facturado en esta mesa durante la ocupación
    /// </summary>
    public decimal TotalFacturado { get; set; }

    /// <summary>
    /// Número de comandas procesadas
    /// </summary>
    public int ComandasProcesadas { get; set; }

    /// <summary>
    /// Indica si hay facturación pendiente
    /// </summary>
    public bool FacturacionPendiente { get; set; }

    /// <summary>
    /// Lista de eventos generados durante la liberación
    /// </summary>
    public List<string> EventosGenerados { get; set; } = new();

    /// <summary>
    /// Observaciones adicionales
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Usuario que realizó la liberación
    /// </summary>
    public string? UsuarioLiberacion { get; set; }

    /// <summary>
    /// Próxima disponibilidad de la mesa
    /// </summary>
    public DateTime? ProximaDisponibilidad { get; set; }
} 