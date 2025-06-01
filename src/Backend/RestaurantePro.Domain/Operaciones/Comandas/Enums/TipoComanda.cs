namespace RestaurantePro.Domain.Operaciones.Comandas.Enums
{
    /// <summary>
    /// Tipos de comanda disponibles en el sistema
    /// </summary>
    public enum TipoComanda
    {
        /// <summary>
        /// Comanda para mesa del restaurante
        /// </summary>
        Mesa = 1,

        /// <summary>
        /// Comanda para entrega a domicilio
        /// </summary>
        Delivery = 2,

        /// <summary>
        /// Comanda para llevar (take away)
        /// </summary>
        TakeAway = 3
    }
} 