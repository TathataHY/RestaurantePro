using RestaurantePro.Domain.Enums;
using System;
using System.Collections.Generic;

namespace RestaurantePro.Domain.Entities
{
    /// <summary>
    /// Representa un detalle o ítem de una comanda
    /// </summary>
    public class ComandaDetalle : BaseEntity
    {
        /// <summary>
        /// ID de la comanda a la que pertenece este detalle
        /// </summary>
        public int ComandaId { get; set; }

        /// <summary>
        /// Comanda a la que pertenece este detalle
        /// </summary>
        public virtual Comanda Comanda { get; set; }

        /// <summary>
        /// ID del producto ordenado
        /// </summary>
        public int ProductoId { get; set; }

        /// <summary>
        /// Producto ordenado
        /// </summary>
        public virtual Producto Producto { get; set; }

        /// <summary>
        /// Cantidad del producto ordenado
        /// </summary>
        public int Cantidad { get; set; }

        /// <summary>
        /// Precio unitario del producto al momento de la orden
        /// </summary>
        public decimal PrecioUnitario { get; set; }

        /// <summary>
        /// Subtotal del detalle (cantidad * precio unitario)
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Estado actual del detalle de comanda
        /// </summary>
        public EstadoComandaDetalle Estado { get; set; }

        /// <summary>
        /// Notas especiales para este detalle
        /// </summary>
        public string NotasEspeciales { get; set; }

        /// <summary>
        /// Fecha en que se completó la preparación del detalle
        /// </summary>
        public DateTime? FechaCompletado { get; set; }

        /// <summary>
        /// Personalizaciones aplicadas a este detalle
        /// </summary>
        public virtual ICollection<ComandaDetallePersonalizacion> Personalizaciones { get; set; }
    }
} 