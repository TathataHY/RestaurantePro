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
        /// Fecha y hora completa de la reservación
        /// </summary>
        public DateTime FechaReservacion => Fecha.Add(Hora);

        /// <summary>
        /// Duración estimada de la reservación
        /// </summary>
        public TimeSpan DuracionEstimada { get; private set; }

        /// <summary>
        /// Número de personas para la reservación
        /// </summary>
        public int CantidadPersonas { get; private set; }

        /// <summary>
        /// Observaciones o requerimientos especiales
        /// </summary>
        public string Observaciones { get; private set; }

        /// <summary>
        /// Teléfono de contacto para la reservación
        /// </summary>
        public string Telefono { get; private set; }

        /// <summary>
        /// Email de contacto para la reservación
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Estado actual de la reservación
        /// </summary>
        public EstadoReservacion Estado { get; private set; }

        /// <summary>
        /// Motivo de cancelación (si aplica)
        /// </summary>
        public string MotivoCancelacion { get; private set; }

        // 🔥 NAVEGACIONES AGREGADAS para queries más eficientes
        /// <summary>
        /// Navegación hacia la entidad Mesa reservada
        /// Facilita acceso a información de la mesa sin queries adicionales
        /// </summary>
        public virtual Mesa? Mesa { get; set; }

        /// <summary>
        /// Navegación hacia la entidad Cliente que realizó la reservación
        /// Útil para obtener información completa del cliente
        /// </summary>
        public virtual Cliente? Cliente { get; set; }

        /// <summary>
        /// Constructor privado para EF Core
        /// </summary>
        private Reservacion() { }

        /// <summary>
        /// Método de fábrica para crear una nueva reservación
        /// </summary>
        public static Reservacion Crear(Guid mesaId, Guid clienteId, DateTime fecha, TimeSpan duracionEstimada, int cantidadPersonas, string telefono, string email, string? observaciones = null)
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

            // Validar duración estimada
            if (duracionEstimada.TotalMinutes < 15)
            {
                throw new ArgumentException("La duración estimada debe ser de al menos 15 minutos", nameof(duracionEstimada));
            }

            // Crear la reservación
            var reservacion = new Reservacion
            {
                Id = Guid.NewGuid(),
                ClienteId = clienteId,
                MesaId = mesaId,
                Fecha = fecha.Date, // Guardamos solo la fecha sin la hora
                Hora = fecha.TimeOfDay,
                DuracionEstimada = duracionEstimada,
                CantidadPersonas = cantidadPersonas,
                Telefono = telefono,
                Email = email,
                Observaciones = observaciones ?? string.Empty,
                Estado = EstadoReservacion.Pendiente,
                FechaCreacion = DateTime.Now
            };

            // Registrar el evento de dominio
            reservacion.AddDomainEvent(new ReservacionCreada(
                reservacion.Id,
                clienteId,
                mesaId,
                fecha.Date,
                fecha.TimeOfDay,
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
            if (Estado == EstadoReservacion.Cancelada)
            {
                throw new InvalidOperationException($"La reservación ya está cancelada");
            }
            
            if (Estado == EstadoReservacion.Completada)
            {
                throw new InvalidOperationException($"La reservación completada no puede ser cancelada");
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
        public void MarcarNoAsistio()
        {
            if (Estado != EstadoReservacion.Confirmada && Estado != EstadoReservacion.Pendiente)
            {
                throw new InvalidOperationException($"No se puede marcar como no-show una reservación con estado {Estado}");
            }

            Estado = EstadoReservacion.NoShow;
            FechaActualizacion = DateTime.Now;

            ValidarInvariantes();
            AddDomainEvent(new ReservacionNoAsistio(Id, ClienteId, MesaId, Fecha, CantidadPersonas));
        }

        /// <summary>
        /// Cambia la mesa asignada a la reservación
        /// </summary>
        /// <param name="nuevaMesaId">ID de la nueva mesa</param>
        /// <exception cref="InvalidOperationException">Si la reservación no está en un estado que permita cambios</exception>
        public void CambiarMesa(Guid nuevaMesaId)
        {
            if (Estado != EstadoReservacion.Pendiente && Estado != EstadoReservacion.Confirmada)
            {
                throw new InvalidOperationException($"No se puede cambiar la mesa de una reservación con estado {Estado}");
            }

            if (nuevaMesaId == Guid.Empty)
            {
                throw new ArgumentException("El ID de la mesa no puede estar vacío", nameof(nuevaMesaId));
            }

            var mesaAnterior = MesaId;
            MesaId = nuevaMesaId;
            FechaActualizacion = DateTime.Now;

            ValidarInvariantes();
            AddDomainEvent(new MesaReservacionCambiada(Id, mesaAnterior, nuevaMesaId));
        }

        /// <summary>
        /// Actualiza las observaciones de la reservación
        /// </summary>
        /// <param name="nuevasObservaciones">Nuevas observaciones</param>
        /// <exception cref="InvalidOperationException">Si la reservación no está en un estado que permita cambios</exception>
        public void ActualizarObservaciones(string nuevasObservaciones)
        {
            if (Estado != EstadoReservacion.Pendiente && Estado != EstadoReservacion.Confirmada)
            {
                throw new InvalidOperationException($"No se pueden actualizar las observaciones de una reservación con estado {Estado}");
            }

            if (string.IsNullOrWhiteSpace(nuevasObservaciones))
            {
                throw new ArgumentException("Las observaciones no pueden estar vacías", nameof(nuevasObservaciones));
            }

            if (nuevasObservaciones.Length > 500)
            {
                throw new ArgumentException("Las observaciones no pueden exceder 500 caracteres", nameof(nuevasObservaciones));
            }

            var observacionesAnteriores = Observaciones;
            Observaciones = nuevasObservaciones;
            FechaActualizacion = DateTime.Now;

            ValidarInvariantes();
            AddDomainEvent(new ObservacionesReservacionActualizadas(Id, observacionesAnteriores, nuevasObservaciones));
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

            if (DuracionEstimada.TotalMinutes < 15)
            {
                throw new InvalidOperationException("La duración estimada debe ser de al menos 15 minutos");
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
