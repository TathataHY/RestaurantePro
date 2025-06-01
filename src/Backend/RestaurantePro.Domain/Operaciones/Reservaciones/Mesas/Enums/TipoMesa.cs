namespace RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;

/// <summary>
/// Tipo de mesa según su ubicación y características
/// </summary>
public enum TipoMesa
{
    /// <summary>
    /// Mesa estándar en el interior del restaurante
    /// </summary>
    Interior = 1,

    /// <summary>
    /// Mesa estándar regular (alias para Interior)
    /// </summary>
    Estandar = 1,

    /// <summary>
    /// Mesa regular estándar (alias para Interior)
    /// </summary>
    Regular = 1,

    /// <summary>
    /// Mesa en la terraza o área exterior
    /// </summary>
    Terraza = 2,

    /// <summary>
    /// Mesa en área privada o reservada
    /// </summary>
    Privada = 3,

    /// <summary>
    /// Mesa en la barra del bar
    /// </summary>
    Barra = 4,

    /// <summary>
    /// Mesa junto a la ventana
    /// </summary>
    Ventana = 5,

    /// <summary>
    /// Mesa VIP con servicios especiales
    /// </summary>
    VIP = 6,

    /// <summary>
    /// Mesa comunitaria para grupos grandes
    /// </summary>
    Comunitaria = 7,

    /// <summary>
    /// Mesa alta tipo bistro
    /// </summary>
    Alta = 8,

    /// <summary>
    /// Mesa accesible para personas con discapacidad
    /// </summary>
    Accesible = 9
} 