namespace RestaurantePro.Domain.Operaciones.Reservaciones.Entities
{
    /// <summary>
    /// Agregado que representa una reservación de mesa en el restaurante.
    /// 
    /// Invariantes:
    /// - Una reservación debe tener asociada una mesa válida y un cliente
    /// - La fecha de reservación debe ser futura al momento de crearla
    /// - La cantidad de personas debe ser mayor que cero
    /// - Una reservación cancelada no puede volver a activarse
    /// - Una reservación completada no puede modificarse
    /// 
    /// Ciclo de vida:
    /// - Creación/Pendiente → [Confirmada → Completada]
    ///                      ↘ [Cancelada | NoShow]
    /// 
    /// Reglas de negocio:
    /// - Solo se pueden confirmar reservaciones en estado Pendiente
    /// - Solo se pueden completar reservaciones Confirmadas o Pendientes
    /// - Solo se pueden cancelar reservaciones Pendientes o Confirmadas
    /// - Cada cambio de estado genera eventos de dominio
    /// - La cancelación requiere especificar un motivo
    /// </summary>
    public class Reservacion : EntityBase, IAggregateRoot
    {
        /// <summary>
        /// Identificador del cliente que realizó la reservación
        /// </summary>
        public Guid ClienteId { get; private set; }

        /// <summary>
        /// Identificador de la mesa reservada
        /// </summary>
        public Guid MesaId { get; private set; }

        /// <summary>
        /// Fecha de la reservación (sin la hora)
        /// </summary>
        public DateTime Fecha { get; private set; }

        /// <summary>
        /// Hora de la reservación
        /// </summary>
        public TimeSpan Hora { get; private set; }

        /// <summary>
        /// Número de personas para la reservación
        /// </summary>
        public int CantidadPersonas { get; private set; }

        /// <summary>
        /// Observaciones o requerimientos especiales
        /// </summary>
        public string Observaciones { get; private set; }

        /// <summary>
        /// Estado actual de la reservación
        /// </summary>
        public EstadoReservacion Estado { get; private set; }

        /// <summary>
        /// Motivo de cancelación (si aplica)
        /// </summary>
        public string MotivoCancelacion { get; private set; }

        /// <summary>
        /// Constructor privado para EF Core
        /// </summary>
        private Reservacion() { }

        /// <summary>
        /// Método de fábrica para crear una nueva reservación
        /// </summary>
        public static Reservacion Crear(Guid clienteId, Guid mesaId, DateTime fecha, TimeSpan hora, int cantidadPersonas, string observaciones = null)
        {
            // Validar que la fecha sea futura
            if (fecha.Date < DateTime.Now.Date)
            {
                throw new ArgumentException("La fecha de reservación debe ser futura", nameof(fecha));
            }

            // Validar la cantidad de personas
            if (cantidadPersonas <= 0)
            {
                throw new ArgumentException("La cantidad de personas debe ser mayor que cero", nameof(cantidadPersonas));
            }

            // Crear la reservación
            var reservacion = new Reservacion
            {
                Id = Guid.NewGuid(),
                ClienteId = clienteId,
                MesaId = mesaId,
                Fecha = fecha.Date, // Guardamos solo la fecha sin la hora
                Hora = hora,
                CantidadPersonas = cantidadPersonas,
                Observaciones = observaciones,
                Estado = EstadoReservacion.Pendiente,
                FechaCreacion = DateTime.Now
            };

            // Registrar el evento de dominio
            reservacion.AddDomainEvent(new ReservacionCreada(
                reservacion.Id,
                clienteId,
                mesaId,
                fecha.Date,
                hora,
                cantidadPersonas));

            reservacion.ValidarInvariantes();
            return reservacion;
        }

        /// <summary>
        /// Confirma la reservación
        /// </summary>
        public void Confirmar()
        {
            if (Estado != EstadoReservacion.Pendiente)
            {
                throw new InvalidOperationException($"No se puede confirmar una reservación con estado {Estado}");
            }

            Estado = EstadoReservacion.Confirmada;
            FechaActualizacion = DateTime.Now;

            ValidarInvariantes();
            AddDomainEvent(new ReservacionConfirmada(Id));
        }

        /// <summary>
        /// Cancela la reservación
        /// </summary>
        public void Cancelar(string motivo)
        {
            if (Estado == EstadoReservacion.Completada || Estado == EstadoReservacion.Cancelada)
            {
                throw new InvalidOperationException($"La reservación con estado {Estado} no puede cancelarse");
            }

            Estado = EstadoReservacion.Cancelada;
            MotivoCancelacion = motivo;
            FechaActualizacion = DateTime.Now;

            ValidarInvariantes();
            AddDomainEvent(new ReservacionCancelada(Id, motivo));
        }

        /// <summary>
        /// Completa la reservación (los clientes asistieron y fueron atendidos)
        /// </summary>
        public void Completar()
        {
            if (Estado != EstadoReservacion.Confirmada && Estado != EstadoReservacion.Pendiente)
            {
                throw new InvalidOperationException($"No se puede completar una reservación con estado {Estado}");
            }

            Estado = EstadoReservacion.Completada;
            FechaActualizacion = DateTime.Now;

            ValidarInvariantes();
            AddDomainEvent(new ReservacionCompletada(Id));
        }

        /// <summary>
        /// Marca la reservación como no-show (los clientes no se presentaron)
        /// </summary>
        public void MarcarComoNoShow()
        {
            if (Estado != EstadoReservacion.Confirmada && Estado != EstadoReservacion.Pendiente)
            {
                throw new InvalidOperationException($"No se puede marcar como no-show una reservación con estado {Estado}");
            }

            Estado = EstadoReservacion.NoShow;
            FechaActualizacion = DateTime.Now;

            ValidarInvariantes();
            // Aquí podríamos agregar un evento de dominio para el no-show
            // AddDomainEvent(new ReservacionNoShow(Id));
        }

        /// <summary>
        /// Valida las invariantes del agregado Reservacion
        /// </summary>
        private void ValidarInvariantes()
        {
            if (ClienteId == Guid.Empty)
            {
                throw new InvalidOperationException("La reservación debe tener un cliente asociado");
            }

            if (MesaId == Guid.Empty)
            {
                throw new InvalidOperationException("La reservación debe tener una mesa asociada");
            }

            if (CantidadPersonas <= 0)
            {
                throw new InvalidOperationException("La cantidad de personas debe ser mayor que cero");
            }

            if (!Enum.IsDefined(typeof(EstadoReservacion), Estado))
            {
                throw new InvalidOperationException($"El estado {Estado} no es válido para una reservación");
            }

            // Si está cancelada, debe tener un motivo
            if (Estado == EstadoReservacion.Cancelada && string.IsNullOrWhiteSpace(MotivoCancelacion))
            {
                throw new InvalidOperationException("Una reservación cancelada debe tener un motivo de cancelación");
            }
        }
    }
}
