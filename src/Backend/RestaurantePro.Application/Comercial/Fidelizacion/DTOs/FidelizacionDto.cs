namespace RestaurantePro.Application.Comercial.Fidelizacion.DTOs;

/// <summary>
/// DTO para información completa de fidelización de un cliente
/// </summary>
public class FidelizacionDto
{
    /// <summary>
    /// ID de la tarjeta de fidelización
    /// </summary>
    public Guid TarjetaId { get; set; }

    /// <summary>
    /// Número de la tarjeta de fidelización
    /// </summary>
    public string Numero { get; set; } = string.Empty;

    /// <summary>
    /// Puntos actuales en la tarjeta
    /// </summary>
    public int PuntosActuales { get; set; }

    /// <summary>
    /// Total de puntos canjeados históricamente
    /// </summary>
    public int PuntosCanjeados { get; set; }

    /// <summary>
    /// Puntos que están próximos a vencer
    /// </summary>
    public int PuntosPorVencer { get; set; }

    /// <summary>
    /// Fecha del próximo vencimiento de puntos
    /// </summary>
    public DateTime? ProximoVencimiento { get; set; }

    /// <summary>
    /// Nivel actual de fidelización
    /// </summary>
    public string NivelFidelizacion { get; set; } = string.Empty;

    /// <summary>
    /// Fecha estimada para alcanzar el próximo nivel
    /// </summary>
    public DateTime? FechaProximoNivel { get; set; }

    /// <summary>
    /// Puntos necesarios para alcanzar el próximo nivel
    /// </summary>
    public int PuntosParaProximoNivel { get; set; }

    /// <summary>
    /// Historial reciente de movimientos de puntos
    /// </summary>
    public List<MovimientoPuntosDto>? HistorialReciente { get; set; }

    /// <summary>
    /// Porcentaje de progreso hacia el próximo nivel
    /// </summary>
    public decimal PorcentajeProgresoNivel
    {
        get
        {
            if (PuntosParaProximoNivel <= 0) return 100;
            
            var puntosBase = ObtenerPuntosBaseNivel();
            var puntosProgreso = PuntosActuales - puntosBase;
            var puntosNecesarios = PuntosParaProximoNivel;
            
            return puntosNecesarios > 0 ? (decimal)puntosProgreso / puntosNecesarios * 100 : 100;
        }
    }

    /// <summary>
    /// Indica si hay puntos próximos a vencer
    /// </summary>
    public bool TienePuntosPorVencer => PuntosPorVencer > 0 && ProximoVencimiento.HasValue;

    private int ObtenerPuntosBaseNivel()
    {
        return NivelFidelizacion?.ToLower() switch
        {
            "bronce" => 0,
            "plata" => 1000,
            "oro" => 2500,
            "vip" => 5000,
            _ => 0
        };
    }
}

/// <summary>
/// DTO para movimientos de puntos
/// </summary>
public class MovimientoPuntosDto
{
    /// <summary>
    /// Fecha del movimiento
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Tipo de movimiento (Acumulacion, Canje, Vencimiento, etc.)
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad de puntos (positivos para acumulación, negativos para canje)
    /// </summary>
    public int Puntos { get; set; }

    /// <summary>
    /// Motivo del movimiento
    /// </summary>
    public string Motivo { get; set; } = string.Empty;

    /// <summary>
    /// Referencia del movimiento (factura, canje, etc.)
    /// </summary>
    public string? Referencia { get; set; }

    /// <summary>
    /// Descripción del movimiento para mostrar al usuario
    /// </summary>
    public string DescripcionMovimiento => Puntos > 0 
        ? $"+{Puntos} puntos por {Motivo}" 
        : $"{Puntos} puntos canjeados en {Motivo}";
} 