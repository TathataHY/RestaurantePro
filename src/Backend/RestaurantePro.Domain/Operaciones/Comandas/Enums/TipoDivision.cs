namespace RestaurantePro.Domain.Operaciones.Comandas.Enums;

/// <summary>
/// Tipos de división de comanda disponibles en el sistema
/// Determina cómo se puede dividir una comanda entre múltiples clientes o mesas
/// </summary>
public enum TipoDivision
{
    /// <summary>
    /// División por items específicos
    /// Cada cliente se lleva items específicos de la comanda
    /// </summary>
    PorItems = 1,

    /// <summary>
    /// División por porcentaje del total
    /// Se divide el monto total basado en porcentajes
    /// </summary>
    PorPorcentaje = 2,

    /// <summary>
    /// División por monto fijo
    /// Cada parte paga un monto específico determinado
    /// </summary>
    PorMonto = 3,

    /// <summary>
    /// División equitativa entre todas las partes
    /// El total se divide igualmente
    /// </summary>
    Equitativa = 4,

    /// <summary>
    /// División por número de personas
    /// Se calcula basado en el número de comensales por grupo
    /// </summary>
    PorPersona = 5,

    /// <summary>
    /// División personalizada definida manualmente
    /// Permite configuración específica por parte del usuario
    /// </summary>
    Personalizada = 6
} 