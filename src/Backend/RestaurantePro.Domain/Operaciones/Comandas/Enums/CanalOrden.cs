namespace RestaurantePro.Domain.Operaciones.Comandas.Enums
{
    /// <summary>
    /// Canales disponibles para realizar órdenes
    /// </summary>
    public enum CanalOrden
    {
        /// <summary>
        /// Orden realizada a través de la página web
        /// </summary>
        Web = 1,

        /// <summary>
        /// Orden realizada a través de aplicación móvil
        /// </summary>
        Movil = 2,

        /// <summary>
        /// Orden realizada por teléfono
        /// </summary>
        Telefono = 3,

        /// <summary>
        /// Orden realizada presencialmente en el restaurante
        /// </summary>
        Presencial = 4
    }
} 