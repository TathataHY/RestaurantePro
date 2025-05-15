namespace RestaurantePro.Domain.Enums
{
    /// <summary>
    /// Estados posibles de una mesa
    /// </summary>
    public enum EstadoMesa
    {
        /// <summary>
        /// Mesa disponible para asignar
        /// </summary>
        Libre = 0,

        /// <summary>
        /// Mesa ocupada por clientes
        /// </summary>
        Ocupada = 1,

        /// <summary>
        /// Mesa reservada para uso futuro
        /// </summary>
        Reservada = 2,
        
        /// <summary>
        /// Mesa en mantenimiento o no disponible
        /// </summary>
        Mantenimiento = 3
    }
} 