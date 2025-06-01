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

        /// <summary>
        /// Total de puntos del cliente después de la acumulación
        /// </summary>
        public int PuntosTotalesCliente { get; set; }

        /// <summary>
        /// Nivel actual del cliente
        /// </summary>
        public string NivelActualCliente { get; set; } = string.Empty;

        /// <summary>
        /// Nivel anterior del cliente (antes de la acumulación)
        /// </summary>
        public string? NivelAnterior { get; set; }

        /// <summary>
        /// Indica si hubo cambio de nivel
        /// </summary>
        public bool CambioDeNivel { get; set; }

        /// <summary>
        /// Mensaje sobre el cambio de nivel
        /// </summary>
        public string? MensajeCambioNivel { get; set; }

        /// <summary>
        /// Multiplicador aplicado en la acumulación
        /// </summary>
        public decimal MultiplicadorAplicado { get; set; } = 1.0m;

        /// <summary>
        /// Indica si se aplicó una promoción
        /// </summary>
        public bool PromocionAplicada { get; set; }

        /// <summary>
        /// Código de promoción usado
        /// </summary>
        public string? CodigoPromocionUsado { get; set; }

        /// <summary>
        /// Descripción de la promoción aplicada
        /// </summary>
        public string? DescripcionPromocion { get; set; }

        /// <summary>
        /// ID de la transacción
        /// </summary>
        public Guid TransaccionId { get; set; }

        /// <summary>
        /// Indica si es una acumulación manual
        /// </summary>
        public bool EsAcumulacionManual { get; set; }

        /// <summary>
        /// Usuario que realizó la acumulación manual
        /// </summary>
        public string? UsuarioQueAcumulo { get; set; }

        /// <summary>
        /// Motivo de la acumulación manual
        /// </summary>
        public string? MotivoManual { get; set; }

        /// <summary>
        /// Indica si se aplicó bonus de upgrade
        /// </summary>
        public bool BonusUpgradeAplicado { get; set; }

        /// <summary>
        /// Puntos bonus por upgrade de nivel
        /// </summary>
        public int PuntosBonusUpgrade { get; set; }

        /// <summary>
        /// Indica si se aplicaron beneficios VIP
        /// </summary>
        public bool BeneficiosVIPAplicados { get; set; }

        /// <summary>
        /// Descripción de los beneficios VIP aplicados
        /// </summary>
        public string? DescripcionBeneficiosVIP { get; set; }

        /// <summary>
        /// Número de eventos disparados
        /// </summary>
        public int EventosDisparados { get; set; }

        /// <summary>
        /// Tipos de eventos disparados
        /// </summary>
        public List<string> TiposEventosDisparados { get; set; } = new();
    }
} 