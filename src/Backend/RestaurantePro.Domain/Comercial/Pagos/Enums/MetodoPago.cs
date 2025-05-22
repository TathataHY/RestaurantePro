namespace RestaurantePro.Domain.Comercial.Pagos.Enums
{
    /// <summary>
    /// Enumera los diferentes métodos de pago aceptados en el sistema
    /// </summary>
    public enum MetodoPago
    {
        /// <summary>
        /// Pago en efectivo
        /// </summary>
        Efectivo = 1,

        /// <summary>
        /// Pago con tarjeta de crédito
        /// </summary>
        TarjetaCredito = 2,

        /// <summary>
        /// Pago con tarjeta de débito
        /// </summary>
        TarjetaDebito = 3,

        /// <summary>
        /// Pago mediante transferencia bancaria
        /// </summary>
        Transferencia = 4,

        /// <summary>
        /// Pago con monedero electrónico o billetera digital
        /// </summary>
        MonederoElectronico = 5,

        /// <summary>
        /// Pago con cheque
        /// </summary>
        Cheque = 6,

        /// <summary>
        /// Pago mediante aplicación de puntos de fidelización
        /// </summary>
        PuntosFidelizacion = 7,

        /// <summary>
        /// Pago mediante cupón promocional o descuento
        /// </summary>
        Cupon = 8
    }
} 