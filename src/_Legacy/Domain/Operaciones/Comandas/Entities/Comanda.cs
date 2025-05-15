using RestaurantePro.Domain.Enums;
using System;
using System.Collections.Generic;

namespace RestaurantePro.Domain.Entities
{
    /// <summary>
    /// Representa una orden o pedido de un cliente
    /// </summary>
    public class Comanda : BaseEntity
    {
        /// <summary>
        /// ID de la mesa asociada a la comanda
        /// </summary>
        public int MesaId { get; set; }

        /// <summary>
        /// Mesa asociada a la comanda
        /// </summary>
        public virtual Mesa Mesa { get; set; }

        /// <summary>
        /// ID del usuario (mesero) que creó la comanda
        /// </summary>
        public int UsuarioId { get; set; }

        /// <summary>
        /// Usuario (mesero) que creó la comanda
        /// </summary>
        public virtual Usuario Usuario { get; set; }

        /// <summary>
        /// Número de comanda (único)
        /// </summary>
        public string NumeroComanda { get; set; }

        /// <summary>
        /// Estado actual de la comanda
        /// </summary>
        public EstadoComanda Estado { get; set; }

        /// <summary>
        /// Notas adicionales para la comanda
        /// </summary>
        public string Notas { get; set; }

        /// <summary>
        /// Subtotal de la comanda (sin impuestos)
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Impuestos aplicados a la comanda
        /// </summary>
        public decimal Impuestos { get; set; }

        /// <summary>
        /// Total de la comanda (subtotal + impuestos)
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Fecha en que se completó la comanda
        /// </summary>
        public DateTime? FechaCompletado { get; set; }

        /// <summary>
        /// Indica si el inventario ya ha sido procesado para esta comanda
        /// </summary>
        public bool InventarioProcesado { get; set; }

        /// <summary>
        /// Detalles de los productos incluidos en la comanda
        /// </summary>
        public virtual ICollection<ComandaDetalle> ComandaDetalles { get; set; }

        /// <summary>
        /// Pagos asociados a esta comanda
        /// </summary>
        public virtual ICollection<Pago> Pagos { get; set; }
    }
} 