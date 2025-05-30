namespace RestaurantePro.Application.Comercial.Fidelizacion.DTOs;

/// <summary>
/// DTO para la respuesta del canje de puntos
/// Contiene información completa del resultado del canje
/// </summary>
public class CanjeoPuntosDto
{
    /// <summary>
    /// ID único del canje realizado
    /// </summary>
    public Guid CanjeId { get; set; }

    /// <summary>
    /// ID del cliente que realizó el canje
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// ID de la tarjeta de fidelización utilizada
    /// </summary>
    public Guid TarjetaId { get; set; }

    /// <summary>
    /// ID de la recompensa canjeada
    /// </summary>
    public Guid RecompensaId { get; set; }

    /// <summary>
    /// Información detallada de la recompensa canjeada
    /// </summary>
    public RecompensaCanjeadaDto Recompensa { get; set; } = new();

    /// <summary>
    /// Cantidad de recompensas canjeadas
    /// </summary>
    public int Cantidad { get; set; }

    /// <summary>
    /// Puntos utilizados en el canje
    /// </summary>
    public int PuntosUtilizados { get; set; }

    /// <summary>
    /// Saldo de puntos antes del canje
    /// </summary>
    public int SaldoAnterior { get; set; }

    /// <summary>
    /// Saldo de puntos después del canje
    /// </summary>
    public int SaldoActual { get; set; }

    /// <summary>
    /// Tipo de canje realizado
    /// </summary>
    public string TipoCanje { get; set; } = string.Empty;

    /// <summary>
    /// Método de entrega seleccionado
    /// </summary>
    public string MetodoEntrega { get; set; } = string.Empty;

    /// <summary>
    /// Estado actual del canje
    /// </summary>
    public EstadoCanje Estado { get; set; }

    /// <summary>
    /// Fecha y hora del canje
    /// </summary>
    public DateTime FechaCanje { get; set; }

    /// <summary>
    /// Fecha estimada de entrega
    /// </summary>
    public DateTime? FechaEntregaEstimada { get; set; }

    /// <summary>
    /// Código único del canje para seguimiento
    /// </summary>
    public string CodigoSeguimiento { get; set; } = string.Empty;

    /// <summary>
    /// Código del cupón/vale generado (para recompensas digitales)
    /// </summary>
    public string? CodigoCupon { get; set; }

    /// <summary>
    /// QR Code para validación del canje
    /// </summary>
    public string? QrCode { get; set; }

    /// <summary>
    /// Sucursal de recogida (si aplica)
    /// </summary>
    public string? SucursalRecogida { get; set; }

    /// <summary>
    /// Dirección de entrega (si aplica)
    /// </summary>
    public string? DireccionEntrega { get; set; }

    /// <summary>
    /// Descuento aplicado por promociones
    /// </summary>
    public decimal? DescuentoAplicado { get; set; }

    /// <summary>
    /// Promoción utilizada en el canje
    /// </summary>
    public string? PromocionAplicada { get; set; }

    /// <summary>
    /// Instrucciones para recoger o usar la recompensa
    /// </summary>
    public string? InstruccionesUso { get; set; }

    /// <summary>
    /// Fecha de vencimiento de la recompensa
    /// </summary>
    public DateTime? FechaVencimiento { get; set; }

    /// <summary>
    /// Términos y condiciones específicos del canje
    /// </summary>
    public List<string> TerminosCondiciones { get; set; } = new();

    /// <summary>
    /// Beneficios adicionales obtenidos
    /// </summary>
    public List<BeneficioAdicionalDto> BeneficiosAdicionales { get; set; } = new();

    /// <summary>
    /// Información de contacto para soporte
    /// </summary>
    public ContactoSoporteDto? ContactoSoporte { get; set; }

    /// <summary>
    /// Mensaje personalizado para el cliente
    /// </summary>
    public string MensajePersonalizado { get; set; } = string.Empty;

    /// <summary>
    /// Métricas del canje para gamificación
    /// </summary>
    public MetricasCanjeDto? Metricas { get; set; }

    /// <summary>
    /// Recompensas sugeridas para próximos canjes
    /// </summary>
    public List<RecompensaSugeridaDto> RecompensasSugeridas { get; set; } = new();

    /// <summary>
    /// URL para compartir el canje en redes sociales
    /// </summary>
    public string? UrlCompartir { get; set; }

    /// <summary>
    /// Datos adicionales del canje
    /// </summary>
    public Dictionary<string, object>? DatosAdicionales { get; set; }
}

/// <summary>
/// Estados posibles de un canje
/// </summary>
public enum EstadoCanje
{
    /// <summary>
    /// Canje procesado exitosamente
    /// </summary>
    Procesado = 1,

    /// <summary>
    /// Canje pendiente de confirmación
    /// </summary>
    Pendiente = 2,

    /// <summary>
    /// Recompensa en preparación
    /// </summary>
    Preparando = 3,

    /// <summary>
    /// Listo para entrega/recogida
    /// </summary>
    Listo = 4,

    /// <summary>
    /// Entregado al cliente
    /// </summary>
    Entregado = 5,

    /// <summary>
    /// Canje cancelado
    /// </summary>
    Cancelado = 6,

    /// <summary>
    /// Canje expirado
    /// </summary>
    Expirado = 7,

    /// <summary>
    /// En tránsito (para entregas a domicilio)
    /// </summary>
    EnTransito = 8
}

/// <summary>
/// DTO para información detallada de la recompensa canjeada
/// </summary>
public class RecompensaCanjeadaDto
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
    /// Categoría de la recompensa
    /// </summary>
    public string Categoria { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de recompensa
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Valor estimado de la recompensa
    /// </summary>
    public decimal? ValorEstimado { get; set; }

    /// <summary>
    /// URL de imagen de la recompensa
    /// </summary>
    public string? ImagenUrl { get; set; }

    /// <summary>
    /// Marca o proveedor de la recompensa
    /// </summary>
    public string? Marca { get; set; }

    /// <summary>
    /// Especificaciones técnicas o detalles
    /// </summary>
    public Dictionary<string, string>? Especificaciones { get; set; }
}

/// <summary>
/// DTO para beneficios adicionales del canje
/// </summary>
public class BeneficioAdicionalDto
{
    /// <summary>
    /// Tipo de beneficio
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del beneficio
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Valor del beneficio
    /// </summary>
    public string Valor { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de vencimiento del beneficio
    /// </summary>
    public DateTime? FechaVencimiento { get; set; }
}

/// <summary>
/// DTO para información de contacto de soporte
/// </summary>
public class ContactoSoporteDto
{
    /// <summary>
    /// Teléfono de soporte
    /// </summary>
    public string? Telefono { get; set; }

    /// <summary>
    /// Email de soporte
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Chat en línea disponible
    /// </summary>
    public bool ChatDisponible { get; set; }

    /// <summary>
    /// Horario de atención
    /// </summary>
    public string? HorarioAtencion { get; set; }

    /// <summary>
    /// WhatsApp de soporte
    /// </summary>
    public string? WhatsApp { get; set; }
}

/// <summary>
/// DTO para métricas del canje
/// </summary>
public class MetricasCanjeDto
{
    /// <summary>
    /// Número total de canjes del cliente
    /// </summary>
    public int TotalCanjes { get; set; }

    /// <summary>
    /// Puntos ahorrados con este canje vs precio regular
    /// </summary>
    public int PuntosAhorrados { get; set; }

    /// <summary>
    /// Porcentaje de descuento obtenido
    /// </summary>
    public decimal PorcentajeDescuento { get; set; }

    /// <summary>
    /// Posición en ranking de canjeadores
    /// </summary>
    public int? RankingPosition { get; set; }

    /// <summary>
    /// Logros desbloqueados con este canje
    /// </summary>
    public List<string> LogrosDesbloqueados { get; set; } = new();
}

/// <summary>
/// DTO para recompensas sugeridas
/// </summary>
public class RecompensaSugeridaDto
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
    /// Puntos necesarios
    /// </summary>
    public int PuntosNecesarios { get; set; }

    /// <summary>
    /// Razón por la cual se sugiere
    /// </summary>
    public string RazonSugerencia { get; set; } = string.Empty;

    /// <summary>
    /// Popularidad de la recompensa (1-5 estrellas)
    /// </summary>
    public int Popularidad { get; set; }

    /// <summary>
    /// URL de imagen pequeña
    /// </summary>
    public string? ImagenUrl { get; set; }
} 