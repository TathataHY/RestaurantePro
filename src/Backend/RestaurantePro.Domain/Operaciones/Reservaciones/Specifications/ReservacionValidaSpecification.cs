namespace RestaurantePro.Domain.Operaciones.Reservaciones.Specifications
{
    /// <summary>
    /// Especificación que verifica si una reservación es válida según las reglas de negocio.
    /// Una reservación es válida si:
    /// 1. No está cancelada
    /// 2. Está para una fecha futura o el mismo día
    /// 3. La cantidad de personas está dentro de los límites permitidos
    /// 4. La mesa asignada tiene capacidad suficiente (opcional, según configuración)
    /// </summary>
    public class ReservacionValidaSpecification : Specification<Reservacion>
    {
        private readonly bool _verificarCapacidadMesa;
        private readonly DateTime _fechaActual;
        private readonly int _minimoPersonas;
        private readonly int _maximoPersonas;
        
        /// <summary>
        /// Crea una nueva instancia de la especificación
        /// </summary>
        /// <param name="verificarCapacidadMesa">Indica si debe verificarse que la mesa tenga capacidad suficiente</param>
        /// <param name="fechaActual">Fecha actual para comparaciones (normalmente DateTime.Now)</param>
        /// <param name="minimoPersonas">Mínimo de personas para una reservación (por defecto 1)</param>
        /// <param name="maximoPersonas">Máximo de personas para una reservación (por defecto 12)</param>
        public ReservacionValidaSpecification(
            bool verificarCapacidadMesa = false,
            DateTime? fechaActual = null,
            int minimoPersonas = 1,
            int maximoPersonas = 12)
        {
            _verificarCapacidadMesa = verificarCapacidadMesa;
            _fechaActual = fechaActual ?? DateTime.Now;
            _minimoPersonas = minimoPersonas;
            _maximoPersonas = maximoPersonas;
        }
        
        /// <summary>
        /// Verifica si una reservación cumple con los criterios de validez
        /// </summary>
        public override Expression<Func<Reservacion, bool>> ToExpression()
        {
            return reservacion => 
                reservacion.Fecha.Date >= _fechaActual.Date &&
                reservacion.Estado != EstadoReservacion.Cancelada &&
                reservacion.Estado != EstadoReservacion.NoShow &&
                reservacion.CantidadPersonas >= _minimoPersonas &&
                reservacion.CantidadPersonas <= _maximoPersonas &&
                (!_verificarCapacidadMesa || TieneMesaCapacidadSuficiente(reservacion));
        }
        
        /// <summary>
        /// Verifica si la mesa asignada a la reservación tiene capacidad suficiente
        /// Nota: Este método es incompleto y serviría como punto de extensión en un sistema real
        /// </summary>
        private bool TieneMesaCapacidadSuficiente(Reservacion reservacion)
        {
            // En una implementación real, se consultaría la capacidad de la mesa
            // a través de un repositorio de mesas
            
            // Por ahora, asumimos que todas las mesas tienen capacidad suficiente
            return true;
        }
    }
} 