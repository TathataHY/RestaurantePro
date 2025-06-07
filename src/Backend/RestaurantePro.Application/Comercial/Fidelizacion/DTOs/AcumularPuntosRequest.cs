using System;

namespace RestaurantePro.Application.Comercial.Fidelizacion.DTOs
{
    /// <summary>
    /// Solicitud para acumular puntos de fidelización por una compra
    /// </summary>
    public class AcumularPuntosRequest
    {
        /// <summary>
        /// ID del cliente que acumula puntos
        /// </summary>
        public Guid ClienteId { get; set; }
        
        /// <summary>
        /// ID de la factura asociada a la compra
        /// </summary>
        public Guid FacturaId { get; set; }
        
        /// <summary>
        /// Monto total de la factura para calcular los puntos
        /// </summary>
        public decimal MontoFactura { get; set; }
        
        /// <summary>
        /// Fecha de la operación
        /// </summary>
        public DateTime FechaOperacion { get; set; } = DateTime.Now;
        
        /// <summary>
        /// ID del usuario que registra la operación
        /// </summary>
        public Guid UsuarioId { get; set; }
        
        /// <summary>
        /// Comentarios adicionales
        /// </summary>
        public string Comentarios { get; set; } = string.Empty;
    }
} 