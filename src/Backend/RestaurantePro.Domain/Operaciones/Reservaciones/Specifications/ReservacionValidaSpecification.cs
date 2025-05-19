using RestaurantePro.Domain.Core.SharedKernel.Specifications;
using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;

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
    public class ReservacionValidaSpecification : SpecificationBase<Reservacion>
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
        /// <param name="reservacion">Reservación a evaluar</param>
        /// <returns>True si la reservación es válida, False en caso contrario</returns>
        public override bool IsSatisfiedBy(Reservacion reservacion)
        {
            // Verificación básica
            if (reservacion == null)
                return false;
                
            // No debe estar cancelada
            if (reservacion.Estado == EstadoReservacion.Cancelada)
                return false;
                
            // Debe ser para hoy o una fecha futura
            if (reservacion.Fecha.Date < _fechaActual.Date)
                return false;
                
            // Si es para hoy, la hora debe ser futura
            if (reservacion.Fecha.Date == _fechaActual.Date && 
                reservacion.Hora <= _fechaActual.TimeOfDay)
                return false;
                
            // La cantidad de personas debe estar dentro de los límites
            if (reservacion.CantidadPersonas < _minimoPersonas || 
                reservacion.CantidadPersonas > _maximoPersonas)
                return false;
                
            // Verificación de capacidad de mesa (si se solicitó)
            if (_verificarCapacidadMesa)
            {
                // En un sistema real, deberíamos consultar la capacidad de la mesa
                // Para este ejemplo, asumimos que no tenemos acceso directo a esa información
                // y sería necesario obtenerla a través de un repositorio
                
                // Esta es una implementación de ejemplo incompleta:
                // if (!TieneMesaCapacidadSuficiente(reservacion))
                //    return false;
            }
            
            return true;
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