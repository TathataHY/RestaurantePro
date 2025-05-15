using RestaurantePro.Domain.Enums;
using System;

namespace RestaurantePro.Application.DTOs
{
    /// <summary>
    /// DTO para transferencia de datos de pago
    /// </summary>
    public class PagoDto
    {
        /// <summary>
        /// ID del pago
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID de la comanda
        /// </summary>
        public int ComandaId { get; set; }

        /// <summary>
        /// Número de comanda
        /// </summary>
        public string NumeroComanda { get; set; }

        /// <summary>
        /// ID del usuario que registró el pago
        /// </summary>
        public int UsuarioId { get; set; }

        /// <summary>
        /// Nombre del usuario que registró el pago
        /// </summary>
        public string NombreUsuario { get; set; }

        /// <summary>
        /// Fecha del pago
        /// </summary>
        public DateTime FechaPago { get; set; }

        /// <summary>
        /// Monto del pago
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Método de pago
        /// </summary>
        public MetodoPago MetodoPago { get; set; }

        /// <summary>
        /// Nombre del método de pago para mostrar
        /// </summary>
        public string MetodoPagoNombre => MetodoPago.ToString();

        /// <summary>
        /// Estado del pago
        /// </summary>
        public EstadoPago Estado { get; set; }

        /// <summary>
        /// Nombre del estado para mostrar
        /// </summary>
        public string EstadoNombre => Estado.ToString();

        /// <summary>
        /// Referencia del pago (número de transacción, etc.)
        /// </summary>
        public string Referencia { get; set; }

        /// <summary>
        /// Monto de propina
        /// </summary>
        public decimal Propina { get; set; }

        /// <summary>
        /// Notas adicionales
        /// </summary>
        public string Notas { get; set; }
    }
} 