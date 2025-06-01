namespace RestaurantePro.Domain.Comercial.Clientes.Enums
{
    /// <summary>
    /// Tipos de tarjetas de fidelización disponibles en el sistema
    /// </summary>
    public enum TipoTarjetaFidelizacion
    {
        /// <summary>
        /// Tarjeta estándar básica
        /// </summary>
        Estandar = 0,
        
        /// <summary>
        /// Tarjeta premium con beneficios adicionales
        /// </summary>
        Premium = 1,
        
        /// <summary>
        /// Tarjeta VIP con máximos beneficios
        /// </summary>
        Vip = 2,
        
        /// <summary>
        /// Tarjeta para clientes corporativos
        /// </summary>
        Corporativa = 3,
        
        /// <summary>
        /// Tarjeta para empleados del restaurante
        /// </summary>
        Empleado = 4,
        
        /// <summary>
        /// Tarjeta promocional temporal
        /// </summary>
        Promocional = 5
    }
} 