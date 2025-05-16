namespace RestaurantePro.Domain.Inventario.Events.MovimientoInventario
{
    /// <summary>
    /// Evento de dominio que se lanza cuando se aplica un movimiento de inventario al stock
    /// </summary>
    public class MovimientoAplicadoEvent : IDomainEvent
    {
        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; }
        
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
        
        public MovimientoAplicadoEvent(Guid movimientoId, Guid ingredienteId, 
            Enums.TipoMovimientoInventario tipoMovimiento, decimal cantidad, 
            decimal stockAnterior, decimal stockNuevo)
        {
            OccurredOn = DateTime.UtcNow;
            MovimientoId = movimientoId;
            IngredienteId = ingredienteId;
            TipoMovimiento = tipoMovimiento;
            Cantidad = cantidad;
            StockAnterior = stockAnterior;
            StockNuevo = stockNuevo;
        }
    }
} 