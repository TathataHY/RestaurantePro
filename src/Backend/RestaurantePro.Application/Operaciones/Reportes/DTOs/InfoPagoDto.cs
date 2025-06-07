using System;

namespace RestaurantePro.Application.Operaciones.Reportes.DTOs
{
    /// <summary>
    /// DTO con información de pago para procesar pedido
    /// </summary>
    public class InfoPagoDto
    {
        /// <summary>
        /// Monto total del pago
        /// </summary>
        public decimal MontoTotal { get; set; }
        
        /// <summary>
        /// Monto recibido del cliente (para pagos en efectivo)
        /// </summary>
        public decimal MontoRecibido { get; set; }
        
        /// <summary>
        /// Tipo de pago (Efectivo, Tarjeta, Digital, Transferencia)
        /// </summary>
        public string TipoPago { get; set; }
        
        /// <summary>
        /// Número de tarjeta (para pagos con tarjeta)
        /// </summary>
        public string NumeroTarjeta { get; set; }
        
        /// <summary>
        /// Código de transacción para pagos digitales
        /// </summary>
        public string CodigoTransaccion { get; set; }
        
        /// <summary>
        /// Código de transferencia (para pagos por transferencia)
        /// </summary>
        public string CodigoTransferencia { get; set; }
        
        /// <summary>
        /// Fecha de vencimiento de la tarjeta
        /// </summary>
        public string FechaVencimiento { get; set; }
        
        /// <summary>
        /// Código de seguridad de la tarjeta
        /// </summary>
        public string CodigoSeguridad { get; set; }
        
        /// <summary>
        /// Cambio a devolver
        /// </summary>
        public decimal? Cambio { get; set; }
        
        /// <summary>
        /// Indica si se requiere factura fiscal
        /// </summary>
        public bool RequiereFactura { get; set; }
        
        /// <summary>
        /// Indica si la transacción fue aprobada
        /// </summary>
        public bool Aprobado { get; set; }
        
        /// <summary>
        /// Fecha y hora de la transacción
        /// </summary>
        public DateTime FechaTransaccion { get; set; }
    }
} 