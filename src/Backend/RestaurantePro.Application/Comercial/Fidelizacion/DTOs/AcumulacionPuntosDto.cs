namespace RestaurantePro.Application.Comercial.Fidelizacion.DTOs
{
    /// <summary>
    /// DTO para representar el resultado de acumulación de puntos
    /// </summary>
    public class AcumulacionPuntosDto
    {
        /// <summary>
        /// ID de la tarjeta de fidelización
        /// </summary>
        public Guid TarjetaFidelizacionId { get; set; }

        /// <summary>
        /// ID del cliente
        /// </summary>
        public Guid ClienteId { get; set; }

        /// <summary>
        /// Puntos acumulados en esta transacción
        /// </summary>
        public int PuntosAcumulados { get; set; }

        /// <summary>
        /// Total de puntos después de la acumulación
        /// </summary>
        public int TotalPuntos { get; set; }

        /// <summary>
        /// Monto de la transacción que generó los puntos
        /// </summary>
        public decimal MontoTransaccion { get; set; }

        /// <summary>
        /// Factor de multiplicación aplicado
        /// </summary>
        public decimal FactorMultiplicacion { get; set; }

        /// <summary>
        /// Fecha de la acumulación
        /// </summary>
        public DateTime FechaAcumulacion { get; set; }

        /// <summary>
        /// Concepto de la acumulación
        /// </summary>
        public string Concepto { get; set; } = string.Empty;

        /// <summary>
        /// ID de la factura asociada (si aplica)
        /// </summary>
        public Guid? FacturaId { get; set; }

        /// <summary>
        /// ID de la promoción aplicada (si aplica)
        /// </summary>
        public Guid? PromocionId { get; set; }

        /// <summary>
        /// Indica si se aplicaron puntos bonus
        /// </summary>
        public bool TienePuntosBonus { get; set; }

        /// <summary>
        /// Cantidad de puntos bonus aplicados
        /// </summary>
        public int PuntosBonus { get; set; }

        /// <summary>
        /// Motivo de los puntos bonus
        /// </summary>
        public string? MotivoBonus { get; set; }
    }
} 