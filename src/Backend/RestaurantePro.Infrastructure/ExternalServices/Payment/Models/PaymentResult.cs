using System;

namespace RestaurantePro.Infrastructure.ExternalServices.Payment.Models
{
    /// <summary>
    /// Resultado de una operación de pago
    /// </summary>
    public class PaymentResult
    {
        /// <summary>
        /// Identificador de la transacción
        /// </summary>
        public string TransactionId { get; set; }
        
        /// <summary>
        /// Estado del pago
        /// </summary>
        public PaymentStatus Status { get; set; }
        
        /// <summary>
        /// Fecha y hora del pago
        /// </summary>
        public DateTime TransactionDate { get; set; }
        
        /// <summary>
        /// Referencia del comerciante
        /// </summary>
        public string MerchantReference { get; set; }
        
        /// <summary>
        /// Mensaje descriptivo
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Indica si la transacción fue exitosa
        /// </summary>
        public bool IsSuccessful => Status == PaymentStatus.Completed || Status == PaymentStatus.Authorized;
    }
    
    /// <summary>
    /// Estados posibles de un pago
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>
        /// Pendiente
        /// </summary>
        Pending,
        
        /// <summary>
        /// Autorizado, pero no capturado
        /// </summary>
        Authorized,
        
        /// <summary>
        /// Completado
        /// </summary>
        Completed,
        
        /// <summary>
        /// Fallido
        /// </summary>
        Failed,
        
        /// <summary>
        /// Cancelado
        /// </summary>
        Cancelled,
        
        /// <summary>
        /// Reembolsado
        /// </summary>
        Refunded
    }
} 