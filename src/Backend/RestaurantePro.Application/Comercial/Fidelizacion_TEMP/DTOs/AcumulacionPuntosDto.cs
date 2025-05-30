namespace RestaurantePro.Application.Comercial.Fidelizacion.DTOs;

/// <summary>
/// DTO para la respuesta de acumulación de puntos
/// Contiene información completa del resultado de la transacción
/// </summary>
public class AcumulacionPuntosDto
{
    /// <summary>
    /// ID del cliente que acumuló puntos
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// ID de la tarjeta de fidelización utilizada
    /// </summary>
    public Guid TarjetaId { get; set; }

    /// <summary>
    /// ID único de la transacción de puntos generada
    /// </summary>
    public Guid TransaccionId { get; set; }

    /// <summary>
    /// Cantidad de puntos otorgados en esta transacción
    /// </summary>
    public int PuntosOtorgados { get; set; }

    /// <summary>
    /// Saldo de puntos antes de la transacción
    /// </summary>
    public int SaldoAnterior { get; set; }

    /// <summary>
    /// Saldo de puntos después de la transacción
    /// </summary>
    public int SaldoActual { get; set; }

    /// <summary>
    /// Tipo de transacción que generó los puntos
    /// </summary>
    public string TipoTransaccion { get; set; } = string.Empty;

    /// <summary>
    /// Monto de la compra que originó la acumulación
    /// </summary>
    public decimal MontoCompra { get; set; }

    /// <summary>
    /// Multiplicador total aplicado (incluye promociones, nivel, fecha especial, etc.)
    /// </summary>
    public decimal MultiplicadorAplicado { get; set; }

    /// <summary>
    /// Código de promoción aplicado (si aplica)
    /// </summary>
    public string? PromocionAplicada { get; set; }

    /// <summary>
    /// Fecha y hora de la transacción
    /// </summary>
    public DateTime FechaTransaccion { get; set; }

    /// <summary>
    /// Nuevo nivel alcanzado (si hubo ascenso)
    /// </summary>
    public string? NuevoNivel { get; set; }

    /// <summary>
    /// Indica si hubo ascenso de nivel en esta transacción
    /// </summary>
    public bool HuboAscensoNivel => !string.IsNullOrEmpty(NuevoNivel);

    /// <summary>
    /// Mensaje motivacional personalizado para el cliente
    /// </summary>
    public string MensajeMotivacional { get; set; } = string.Empty;

    /// <summary>
    /// Beneficios desbloqueados con el nuevo nivel (si aplica)
    /// </summary>
    public List<string> BeneficiosDesbloqueados { get; set; } = new();

    /// <summary>
    /// Información del próximo nivel y puntos necesarios
    /// </summary>
    public ProximoNivelInfo? ProximoNivel { get; set; }

    /// <summary>
    /// Logros conseguidos en esta transacción
    /// </summary>
    public List<LogroConseguidoDto> LogrosConseguidos { get; set; } = new();

    /// <summary>
    /// Recompensas disponibles para canjear
    /// </summary>
    public List<RecompensaDisponibleDto> RecompensasDisponibles { get; set; } = new();

    /// <summary>
    /// Estadísticas adicionales del cliente
    /// </summary>
    public EstadisticasFidelizacionDto? Estadisticas { get; set; }
}

/// <summary>
/// Información sobre el próximo nivel en el programa de fidelización
/// </summary>
public class ProximoNivelInfo
{
    /// <summary>
    /// Nombre del próximo nivel
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Puntos necesarios para alcanzar el próximo nivel
    /// </summary>
    public int PuntosNecesarios { get; set; }

    /// <summary>
    /// Puntos que faltan para alcanzar el próximo nivel
    /// </summary>
    public int PuntosFaltantes { get; set; }

    /// <summary>
    /// Porcentaje de progreso hacia el próximo nivel
    /// </summary>
    public decimal PorcentajeProgreso { get; set; }

    /// <summary>
    /// Beneficios que se desbloquearán con el próximo nivel
    /// </summary>
    public List<string> BeneficiosProximoNivel { get; set; } = new();
}

/// <summary>
/// DTO para logros conseguidos
/// </summary>
public class LogroConseguidoDto
{
    /// <summary>
    /// ID del logro
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre del logro
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del logro
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Puntos bonus otorgados por el logro
    /// </summary>
    public int PuntosBonus { get; set; }

    /// <summary>
    /// Icono o imagen del logro
    /// </summary>
    public string? Icono { get; set; }

    /// <summary>
    /// Fecha en que se consiguió el logro
    /// </summary>
    public DateTime FechaConseguido { get; set; }
}

/// <summary>
/// DTO para recompensas disponibles
/// </summary>
public class RecompensaDisponibleDto
{
    /// <summary>
    /// ID de la recompensa
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre de la recompensa
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de la recompensa
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Puntos necesarios para canjear
    /// </summary>
    public int PuntosNecesarios { get; set; }

    /// <summary>
    /// Categoría de la recompensa
    /// </summary>
    public string Categoria { get; set; } = string.Empty;

    /// <summary>
    /// Valor estimado de la recompensa
    /// </summary>
    public decimal? ValorEstimado { get; set; }

    /// <summary>
    /// Indica si el cliente tiene suficientes puntos para canjear
    /// </summary>
    public bool PuedeCanjear { get; set; }

    /// <summary>
    /// URL de imagen de la recompensa
    /// </summary>
    public string? ImagenUrl { get; set; }

    /// <summary>
    /// Fecha límite para canjear (si aplica)
    /// </summary>
    public DateTime? FechaVencimiento { get; set; }
}

/// <summary>
/// DTO para estadísticas de fidelización
/// </summary>
public class EstadisticasFidelizacionDto
{
    /// <summary>
    /// Total de puntos acumulados históricamente
    /// </summary>
    public int TotalPuntosAcumulados { get; set; }

    /// <summary>
    /// Total de puntos canjeados históricamente
    /// </summary>
    public int TotalPuntosCanjeados { get; set; }

    /// <summary>
    /// Número total de transacciones
    /// </summary>
    public int TotalTransacciones { get; set; }

    /// <summary>
    /// Compra promedio del cliente
    /// </summary>
    public decimal CompraPromedio { get; set; }

    /// <summary>
    /// Días desde la última transacción
    /// </summary>
    public int DiasSinTransacciones { get; set; }

    /// <summary>
    /// Mes con más actividad
    /// </summary>
    public string? MesMasActivo { get; set; }

    /// <summary>
    /// Nivel de actividad (Bajo, Medio, Alto)
    /// </summary>
    public string NivelActividad { get; set; } = string.Empty;

    /// <summary>
    /// Puntos que expiran próximamente
    /// </summary>
    public int PuntosProximosVencer { get; set; }

    /// <summary>
    /// Fecha de vencimiento de puntos más próxima
    /// </summary>
    public DateTime? FechaVencimientoProxima { get; set; }
} 