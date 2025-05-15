namespace RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums
{
    /// <summary>
    /// Representa los estados posibles de una mesa en el restaurante
    /// </summary>
    public enum EstadoMesa
    {
        /// <summary>
        /// La mesa está disponible para ser ocupada
        /// </summary>
        Disponible = 1,
        
        /// <summary>
        /// La mesa está ocupada por clientes
        /// </summary>
        Ocupada = 2,
        
        /// <summary>
        /// La mesa está reservada para una futura ocupación
        /// </summary>
        Reservada = 3,
        
        /// <summary>
        /// La mesa está temporalmente fuera de servicio
        /// </summary>
        FueraDeServicio = 4
    }
} 