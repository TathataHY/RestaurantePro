using RestaurantePro.Domain.Enums;

namespace RestaurantePro.Domain.Entities
{
    /// <summary>
    /// Representa un movimiento de inventario de un ingrediente
    /// </summary>
    public class InventarioMovimiento : BaseEntity
    {
        /// <summary>
        /// ID del ingrediente afectado por el movimiento
        /// </summary>
        public int IngredienteId { get; set; }

        /// <summary>
        /// Ingrediente afectado por el movimiento
        /// </summary>
        public virtual Ingrediente Ingrediente { get; set; }

        /// <summary>
        /// ID del usuario que realizó el movimiento
        /// </summary>
        public int UsuarioId { get; set; }

        /// <summary>
        /// Usuario que realizó el movimiento
        /// </summary>
        public virtual Usuario Usuario { get; set; }

        /// <summary>
        /// Tipo de movimiento de inventario
        /// </summary>
        public TipoMovimientoInventario TipoMovimiento { get; set; }

        /// <summary>
        /// Cantidad del movimiento (positiva para entradas, negativa para salidas)
        /// </summary>
        public decimal Cantidad { get; set; }

        /// <summary>
        /// Motivo o descripción del movimiento
        /// </summary>
        public string Motivo { get; set; }

        /// <summary>
        /// Referencia externa (número de factura, comanda, etc.)
        /// </summary>
        public string Referencia { get; set; }
    }
} 