using RestaurantePro.Domain.Enums;

namespace RestaurantePro.Domain.Entities
{
    /// <summary>
    /// Representa un pago realizado para una comanda
    /// </summary>
    public class Pago : BaseEntity
    {
        /// <summary>
        /// ID de la comanda asociada al pago
        /// </summary>
        public int ComandaId { get; set; }

        /// <summary>
        /// Comanda asociada al pago
        /// </summary>
        public virtual Comanda Comanda { get; set; }

        /// <summary>
        /// ID del usuario que procesó el pago
        /// </summary>
        public int UsuarioId { get; set; }

        /// <summary>
        /// Usuario que procesó el pago
        /// </summary>
        public virtual Usuario Usuario { get; set; }

        /// <summary>
        /// Método de pago utilizado
        /// </summary>
        public MetodoPago MetodoPago { get; set; }

        /// <summary>
        /// Referencia del pago (número de transacción, últimos dígitos de tarjeta, etc.)
        /// </summary>
        public string Referencia { get; set; }

        /// <summary>
        /// Monto del pago
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Estado actual del pago
        /// </summary>
        public EstadoPago Estado { get; set; }

        /// <summary>
        /// Notas adicionales sobre el pago
        /// </summary>
        public string Notas { get; set; }
    }
} 