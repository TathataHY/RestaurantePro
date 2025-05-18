namespace RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities
{
    /// <summary>
    /// Entidad que representa un movimiento en el inventario de ingredientes
    /// </summary>
    public class MovimientoInventario : EntityBase
    {
        /// <summary>
        /// Identificador del ingrediente afectado por el movimiento
        /// </summary>
        public Guid IngredienteId { get; private set; }
        
        /// <summary>
        /// Tipo de movimiento (ingreso o egreso)
        /// </summary>
        public TipoMovimientoInventario TipoMovimiento { get; private set; }
        
        /// <summary>
        /// Cantidad afectada por el movimiento
        /// </summary>
        public decimal Cantidad { get; private set; }
        
        /// <summary>
        /// Fecha en que se realizó el movimiento
        /// </summary>
        public DateTime Fecha { get; private set; }
        
        /// <summary>
        /// Motivo del movimiento
        /// </summary>
        public string Motivo { get; private set; }
        
        /// <summary>
        /// Cantidad final del stock después de aplicar el movimiento
        /// </summary>
        public decimal? CantidadFinal { get; private set; }
        
        /// <summary>
        /// Indica si el movimiento ya fue aplicado al stock
        /// </summary>
        public bool EstaAplicado { get; private set; }
        
        // Constructor privado para EF Core
        private MovimientoInventario() { }
        
        /// <summary>
        /// Crea un nuevo movimiento de ingreso de inventario
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="cantidad">Cantidad a ingresar</param>
        /// <param name="motivo">Motivo del ingreso</param>
        /// <param name="fecha">Fecha del movimiento (opcional)</param>
        /// <returns>Nuevo movimiento de ingreso</returns>
        /// <exception cref="ArgumentException">Si los datos no son válidos</exception>
        public static MovimientoInventario CrearIngreso(Guid ingredienteId, decimal cantidad, string motivo, DateTime? fecha = null)
        {
            return CrearMovimiento(ingredienteId, cantidad, motivo, TipoMovimientoInventario.Ingreso, fecha);
        }
        
        /// <summary>
        /// Crea un nuevo movimiento de egreso de inventario
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="cantidad">Cantidad a egresar</param>
        /// <param name="motivo">Motivo del egreso</param>
        /// <param name="fecha">Fecha del movimiento (opcional)</param>
        /// <returns>Nuevo movimiento de egreso</returns>
        /// <exception cref="ArgumentException">Si los datos no son válidos</exception>
        public static MovimientoInventario CrearEgreso(Guid ingredienteId, decimal cantidad, string motivo, DateTime? fecha = null)
        {
            return CrearMovimiento(ingredienteId, cantidad, motivo, TipoMovimientoInventario.Egreso, fecha);
        }
        
        private static MovimientoInventario CrearMovimiento(Guid ingredienteId, decimal cantidad, string motivo, 
            TipoMovimientoInventario tipoMovimiento, DateTime? fecha = null)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(cantidad));
                
            if (string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException("El motivo no puede estar vacío", nameof(motivo));
                
            var movimiento = new MovimientoInventario
            {
                IngredienteId = ingredienteId,
                Cantidad = cantidad,
                Motivo = motivo,
                TipoMovimiento = tipoMovimiento,
                Fecha = fecha ?? DateTime.Now,
                EstaAplicado = false
            };
            
            movimiento.AddDomainEvent(new MovimientoRegistrado(
                movimiento.Id, 
                ingredienteId, 
                tipoMovimiento, 
                cantidad, 
                motivo
            ));
            
            return movimiento;
        }
        
        /// <summary>
        /// Aplica el movimiento al stock actual del ingrediente
        /// </summary>
        /// <param name="stockActual">Stock actual del ingrediente</param>
        /// <returns>Nuevo valor del stock después de aplicar el movimiento</returns>
        /// <exception cref="InvalidOperationException">Si el movimiento ya fue aplicado o no hay stock suficiente</exception>
        public decimal Aplicar(decimal stockActual)
        {
            if (EstaAplicado)
                throw new InvalidOperationException("El movimiento ya fue aplicado");
                
            decimal nuevoStock;
            
            if (TipoMovimiento == TipoMovimientoInventario.Ingreso)
            {
                nuevoStock = stockActual + Cantidad;
            }
            else
            {
                if (stockActual < Cantidad)
                    throw new InvalidOperationException("No hay stock suficiente para completar el movimiento");
                    
                nuevoStock = stockActual - Cantidad;
            }
            
            CantidadFinal = nuevoStock;
            EstaAplicado = true;
            MarkAsModified();
            
            AddDomainEvent(new MovimientoAplicado(
                Id,
                IngredienteId,
                TipoMovimiento,
                Cantidad,
                stockActual,
                nuevoStock
            ));
            
            return nuevoStock;
        }
        
        /// <summary>
        /// Calcula el nuevo stock después de aplicar este movimiento, sin modificar el estado
        /// </summary>
        /// <param name="stockActual">Stock actual antes de aplicar el movimiento</param>
        /// <returns>Nuevo stock calculado después de aplicar el movimiento</returns>
        public decimal CalcularNuevoStock(decimal stockActual)
        {
            if (TipoMovimiento == TipoMovimientoInventario.Ingreso)
            {
                return stockActual + Cantidad;
            }
            else
            {
                return stockActual - Cantidad;
            }
        }
    }
} 
