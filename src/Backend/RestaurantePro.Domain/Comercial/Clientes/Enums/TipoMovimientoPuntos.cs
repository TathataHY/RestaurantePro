namespace RestaurantePro.Domain.Comercial.Clientes.Enums
{
    /// <summary>
    /// Tipos de movimientos de puntos en tarjetas de fidelización
    /// </summary>
    public enum TipoMovimientoPuntos
    {
        /// <summary>
        /// Acumulación por compra regular
        /// </summary>
        AcumulacionCompra = 1,

        /// <summary>
        /// Acumulación por promoción especial
        /// </summary>
        AcumulacionPromocion = 2,

        /// <summary>
        /// Acumulación manual por administrador
        /// </summary>
        AcumulacionManual = 3,

        /// <summary>
        /// Canje de puntos por beneficios
        /// </summary>
        Canje = 4,

        /// <summary>
        /// Ajuste positivo de puntos
        /// </summary>
        AjustePositivo = 5,

        /// <summary>
        /// Ajuste negativo de puntos
        /// </summary>
        AjusteNegativo = 6,

        /// <summary>
        /// Vencimiento automático de puntos
        /// </summary>
        Vencimiento = 7,

        /// <summary>
        /// Transferencia entre tarjetas
        /// </summary>
        Transferencia = 8,

        /// <summary>
        /// Bono de bienvenida
        /// </summary>
        BonoInicial = 9,

        /// <summary>
        /// Bono por cumpleaños
        /// </summary>
        BonoCumpleanos = 10,

        /// <summary>
        /// Bono por registro online
        /// </summary>
        BonoRegistro = 11
    }
} 