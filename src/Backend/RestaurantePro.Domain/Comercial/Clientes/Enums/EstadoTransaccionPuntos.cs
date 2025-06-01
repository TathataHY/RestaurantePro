namespace RestaurantePro.Domain.Comercial.Clientes.Enums;

/// <summary>
/// Estados de transacción de puntos
/// </summary>
public enum EstadoTransaccionPuntos
{
    /// <summary>
    /// Transacción pendiente
    /// </summary>
    Pendiente = 1,

    /// <summary>
    /// Transacción completada
    /// </summary>
    Completada = 2,

    /// <summary>
    /// Transacción cancelada
    /// </summary>
    Cancelada = 3,

    /// <summary>
    /// Transacción revertida
    /// </summary>
    Revertida = 4
} 