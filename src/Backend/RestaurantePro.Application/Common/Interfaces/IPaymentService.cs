using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Common.Interfaces
{
    /// <summary>
    /// Interfaz para servicios de pago
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Procesa un pago
        /// </summary>
        /// <param name="monto">Monto a cobrar</param>
        /// <param name="moneda">Moneda (USD, EUR, MXN, etc.)</param>
        /// <param name="descripcion">Descripción del pago</param>
        /// <param name="metadatos">Metadatos adicionales</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado del pago</returns>
        Task<Result<PaymentResult>> ProcesarPagoAsync(
            decimal monto, 
            string moneda, 
            string descripcion, 
            Dictionary<string, string>? metadatos = null,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Reembolsa un pago
        /// </summary>
        /// <param name="transaccionId">ID de la transacción a reembolsar</param>
        /// <param name="montoReembolso">Monto a reembolsar (opcional, si es null se reembolsa todo)</param>
        /// <param name="motivo">Motivo del reembolso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado del reembolso</returns>
        Task<Result<RefundResult>> ReembolsarPagoAsync(
            string transaccionId, 
            decimal? montoReembolso = null, 
            string? motivo = null,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Verifica el estado de un pago
        /// </summary>
        /// <param name="transaccionId">ID de la transacción</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Estado del pago</returns>
        Task<Result<PaymentStatus>> VerificarEstadoPagoAsync(
            string transaccionId,
            CancellationToken cancellationToken = default);
    }
    
    /// <summary>
    /// Resultado de un pago
    /// </summary>
    public class PaymentResult
    {
        /// <summary>
        /// ID de la transacción
        /// </summary>
        public string TransaccionId { get; set; } = string.Empty;
        
        /// <summary>
        /// Estado del pago
        /// </summary>
        public PaymentStatus Estado { get; set; }
        
        /// <summary>
        /// Monto cobrado
        /// </summary>
        public decimal Monto { get; set; }
        
        /// <summary>
        /// Moneda
        /// </summary>
        public string Moneda { get; set; } = string.Empty;
        
        /// <summary>
        /// Referencia del comerciante
        /// </summary>
        public string ReferenciaComercio { get; set; } = string.Empty;
        
        /// <summary>
        /// Mensaje adicional
        /// </summary>
        public string Mensaje { get; set; } = string.Empty;
        
        /// <summary>
        /// Fecha de la transacción
        /// </summary>
        public DateTime FechaTransaccion { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Resultado de un reembolso
    /// </summary>
    public class RefundResult
    {
        /// <summary>
        /// ID del reembolso
        /// </summary>
        public string ReembolsoId { get; set; } = string.Empty;
        
        /// <summary>
        /// ID de la transacción original
        /// </summary>
        public string TransaccionOriginalId { get; set; } = string.Empty;
        
        /// <summary>
        /// Monto reembolsado
        /// </summary>
        public decimal MontoReembolsado { get; set; }
        
        /// <summary>
        /// Estado del reembolso
        /// </summary>
        public RefundStatus Estado { get; set; }
        
        /// <summary>
        /// Fecha del reembolso
        /// </summary>
        public DateTime FechaReembolso { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Estado de un pago
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>
        /// Pendiente
        /// </summary>
        Pendiente,
        
        /// <summary>
        /// Completado
        /// </summary>
        Completado,
        
        /// <summary>
        /// Fallido
        /// </summary>
        Fallido,
        
        /// <summary>
        /// Reembolsado
        /// </summary>
        Reembolsado,
        
        /// <summary>
        /// Parcialmente reembolsado
        /// </summary>
        ReembolsadoParcial,
        
        /// <summary>
        /// Cancelado
        /// </summary>
        Cancelado,
        
        /// <summary>
        /// En disputa
        /// </summary>
        Disputa
    }
    
    /// <summary>
    /// Estado de un reembolso
    /// </summary>
    public enum RefundStatus
    {
        /// <summary>
        /// Pendiente
        /// </summary>
        Pendiente,
        
        /// <summary>
        /// Completado
        /// </summary>
        Completado,
        
        /// <summary>
        /// Fallido
        /// </summary>
        Fallido
    }
} 