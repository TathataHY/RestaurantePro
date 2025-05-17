namespace RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Events
{
    /// <summary>
    /// Evento de dominio que se lanza cuando se aplica un movimiento de inventario al stock
    /// </summary>
    public class MovimientoAplicado : DomainEvent
    {
                
        /// <summary>
        /// Id del movimiento de inventario aplicado
        /// </summary>
        public Guid MovimientoId { get; }
        
        /// <summary>
        /// Id del ingrediente afectado
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Tipo de movimiento (ingreso o egreso)
        /// </summary>
        public Enums.TipoMovimientoInventario TipoMovimiento { get; }
        
        /// <summary>
        /// Cantidad movida
        /// </summary>
        public decimal Cantidad { get; }
        
        /// <summary>
        /// Stock antes del movimiento
        /// </summary>
        public decimal StockAnterior { get; }
        
        /// <summary>
        /// Stock después del movimiento
        /// </summary>
        public decimal StockNuevo { get; }
        
        public MovimientoAplicado(Guid movimientoId, Guid ingredienteId, 
            Enums.TipoMovimientoInventario tipoMovimiento, decimal cantidad, 
            decimal stockAnterior, decimal stockNuevo)
        {
                        MovimientoId = movimientoId;
            IngredienteId = ingredienteId;
            TipoMovimiento = tipoMovimiento;
            Cantidad = cantidad;
            StockAnterior = stockAnterior;
            StockNuevo = stockNuevo;
        }
    }
} 

