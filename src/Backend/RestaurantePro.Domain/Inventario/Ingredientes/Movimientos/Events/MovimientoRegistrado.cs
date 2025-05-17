namespace RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Events
{
    /// <summary>
    /// Evento de dominio que se lanza cuando se registra un nuevo movimiento de inventario
    /// </summary>
    public class MovimientoRegistrado : IDomainEvent
    {
        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; }
        
        /// <summary>
        /// Id del movimiento de inventario registrado
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
        /// Cantidad del movimiento
        /// </summary>
        public decimal Cantidad { get; }
        
        /// <summary>
        /// Motivo del movimiento
        /// </summary>
        public string Motivo { get; }
        
        public MovimientoRegistrado(Guid movimientoId, Guid ingredienteId, 
            Enums.TipoMovimientoInventario tipoMovimiento, decimal cantidad, string motivo)
        {
            OccurredOn = DateTime.UtcNow;
            MovimientoId = movimientoId;
            IngredienteId = ingredienteId;
            TipoMovimiento = tipoMovimiento;
            Cantidad = cantidad;
            Motivo = motivo;
        }
    }
} 
