namespace RestaurantePro.Domain.Enums
{
    /// <summary>
    /// Métodos de pago disponibles
    /// </summary>
    public enum MetodoPago
    {
        /// <summary>
        /// Pago en efectivo
        /// </summary>
        Efectivo = 0,

        /// <summary>
        /// Pago con tarjeta de crédito
        /// </summary>
        TarjetaCredito = 1,

        /// <summary>
        /// Pago con tarjeta de débito
        /// </summary>
        TarjetaDebito = 2,

        /// <summary>
        /// Pago por transferencia bancaria
        /// </summary>
        Transferencia = 3,

        /// <summary>
        /// Pago con monedero electrónico
        /// </summary>
        MonederoElectronico = 4,

        /// <summary>
        /// Otro método de pago
        /// </summary>
        Otro = 5
    }
} 