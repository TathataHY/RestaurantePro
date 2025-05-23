

namespace RestaurantePro.Domain.Comercial.Pagos.Entities
{
    /// <summary>
    /// Representa un pago realizado por un cliente para una comanda.
    /// 
    /// Reglas de negocio:
    /// - Un pago tiene un monto que debe ser mayor que cero
    /// - Un pago está asociado a una comanda
    /// - Un pago tiene un método de pago
    /// - Un pago tiene un estado que puede cambiar según el flujo de negocio
    /// - Un pago puede ser reembolsado total o parcialmente
    /// - Un pago puede tener una referencia de transacción externa
    /// </summary>
    public class Pago : EntityBase, IAggregateRoot
    {
        /// <summary>
        /// Identificador de la comanda asociada a este pago
        /// </summary>
        public Guid ComandaId { get; private set; }

        /// <summary>
        /// Monto del pago
        /// </summary>
        public decimal Monto { get; private set; }

        /// <summary>
        /// Método de pago utilizado
        /// </summary>
        public MetodoPago MetodoPago { get; private set; }

        /// <summary>
        /// Estado actual del pago
        /// </summary>
        public EstadoPago Estado { get; private set; }

        /// <summary>
        /// Fecha y hora en que se realizó el pago
        /// </summary>
        public DateTime FechaPago { get; private set; }

        /// <summary>
        /// Monto reembolsado hasta el momento (si aplica)
        /// </summary>
        public decimal MontoReembolsado { get; private set; }

        /// <summary>
        /// Fecha y hora del último reembolso (si aplica)
        /// </summary>
        public DateTime? FechaReembolso { get; private set; }

        /// <summary>
        /// Motivo del reembolso (si aplica)
        /// </summary>
        public string? MotivoReembolso { get; private set; }

        /// <summary>
        /// Referencia externa de la transacción (número de autorización, código de seguimiento, etc.)
        /// </summary>
        public string? ReferenciaTransaccion { get; private set; }

        /// <summary>
        /// Observaciones adicionales sobre el pago
        /// </summary>
        public string? Observaciones { get; private set; }

        // Constructor privado para EF Core
        private Pago() { }

        /// <summary>
        /// Crea un nuevo pago
        /// </summary>
        /// <param name="comandaId">Identificador de la comanda</param>
        /// <param name="monto">Monto del pago</param>
        /// <param name="metodoPago">Método de pago</param>
        /// <param name="referenciaTransaccion">Referencia externa de la transacción (opcional)</param>
        /// <param name="observaciones">Observaciones adicionales (opcional)</param>
        /// <param name="dateTimeService">Servicio para obtener la fecha y hora actual</param>
        /// <returns>Instancia de Pago</returns>
        public static Pago Crear(
            Guid comandaId,
            decimal monto,
            MetodoPago metodoPago,
            string? referenciaTransaccion = null,
            string? observaciones = null,
            IDateTimeService? dateTimeService = null)
        {
            if (comandaId == Guid.Empty)
            {
                throw new ArgumentException("El identificador de comanda no puede estar vacío", nameof(comandaId));
            }

            if (monto <= 0)
            {
                throw new ArgumentException("El monto debe ser mayor que cero", nameof(monto));
            }

            var fechaActual = dateTimeService?.Now ?? DateTime.Now;

            var pago = new Pago
            {
                Id = Guid.NewGuid(),
                ComandaId = comandaId,
                Monto = monto,
                MetodoPago = metodoPago,
                Estado = EstadoPago.Pendiente,
                FechaPago = fechaActual,
                ReferenciaTransaccion = referenciaTransaccion,
                Observaciones = observaciones,
                MontoReembolsado = 0
            };

            pago.AddDomainEvent(new PagoRegistrado(pago.Id, comandaId, metodoPago, monto, fechaActual));

            return pago;
        }

        /// <summary>
        /// Actualiza el estado del pago
        /// </summary>
        /// <param name="nuevoEstado">Nuevo estado del pago</param>
        /// <param name="motivo">Motivo del cambio de estado (opcional)</param>
        /// <param name="dateTimeService">Servicio para obtener la fecha y hora actual</param>
        public void ActualizarEstado(EstadoPago nuevoEstado, string? motivo = null, IDateTimeService? dateTimeService = null)
        {
            // No permitir cambios si el pago ya fue reembolsado completamente
            if (Estado == EstadoPago.Reembolsado)
            {
                throw new InvalidOperationException("No se puede cambiar el estado de un pago que ya ha sido reembolsado completamente");
            }

            // Validar transiciones de estado
            ValidarTransicionEstado(Estado, nuevoEstado);

            var estadoAnterior = Estado;
            Estado = nuevoEstado;

            var fechaActual = dateTimeService?.Now ?? DateTime.Now;

            // Registrar evento de cambio de estado
            AddDomainEvent(new EstadoPagoActualizado(Id, estadoAnterior, nuevoEstado, fechaActual, motivo));

            // Actualizar observaciones si se proporcionó un motivo
            if (!string.IsNullOrEmpty(motivo))
            {
                Observaciones = string.IsNullOrEmpty(Observaciones)
                    ? motivo
                    : $"{Observaciones} | {motivo}";
            }
        }

        /// <summary>
        /// Procesa un reembolso total o parcial del pago
        /// </summary>
        /// <param name="montoReembolso">Monto a reembolsar</param>
        /// <param name="motivo">Motivo del reembolso</param>
        /// <param name="referenciaTransaccion">Referencia de la transacción de reembolso (opcional)</param>
        /// <param name="dateTimeService">Servicio para obtener la fecha y hora actual</param>
        public void ProcesarReembolso(
            decimal montoReembolso,
            string motivo,
            string? referenciaTransaccion = null,
            IDateTimeService? dateTimeService = null)
        {
            // Validaciones
            if (montoReembolso <= 0)
            {
                throw new ArgumentException("El monto del reembolso debe ser mayor que cero", nameof(montoReembolso));
            }

            if (string.IsNullOrEmpty(motivo))
            {
                throw new ArgumentException("El motivo del reembolso es obligatorio", nameof(motivo));
            }

            decimal nuevoMontoReembolsado = MontoReembolsado + montoReembolso;

            if (nuevoMontoReembolsado > Monto)
            {
                throw new InvalidOperationException($"El monto total reembolsado ({nuevoMontoReembolsado}) no puede superar el monto original del pago ({Monto})");
            }

            // Actualizar datos del reembolso
            MontoReembolsado = nuevoMontoReembolsado;
            MotivoReembolso = motivo;
            
            var fechaActual = dateTimeService?.Now ?? DateTime.Now;
            FechaReembolso = fechaActual;

            // Determinar si es un reembolso total
            bool esReembolsoTotal = nuevoMontoReembolsado >= Monto;

            // Actualizar estado según corresponda
            if (esReembolsoTotal)
            {
                ActualizarEstado(EstadoPago.Reembolsado, motivo, dateTimeService);
            }
            else if (Estado != EstadoPago.EnReembolso)
            {
                ActualizarEstado(EstadoPago.EnReembolso, motivo, dateTimeService);
            }

            // Si se proporciona una referencia de transacción, actualizarla
            if (!string.IsNullOrEmpty(referenciaTransaccion))
            {
                ReferenciaTransaccion = referenciaTransaccion;
            }

            // Emitir evento de dominio
            AddDomainEvent(new PagoReembolsado(
                Id,
                montoReembolso,
                esReembolsoTotal,
                motivo,
                fechaActual,
                referenciaTransaccion));
        }

        /// <summary>
        /// Actualiza la referencia de transacción externa
        /// </summary>
        /// <param name="referenciaTransaccion">Nueva referencia de transacción</param>
        public void ActualizarReferenciaTransaccion(string referenciaTransaccion)
        {
            if (string.IsNullOrEmpty(referenciaTransaccion))
            {
                throw new ArgumentException("La referencia de transacción no puede estar vacía", nameof(referenciaTransaccion));
            }

            ReferenciaTransaccion = referenciaTransaccion;
        }

        /// <summary>
        /// Valida si una transición de estado es válida
        /// </summary>
        /// <param name="estadoActual">Estado actual del pago</param>
        /// <param name="nuevoEstado">Nuevo estado propuesto</param>
        private void ValidarTransicionEstado(EstadoPago estadoActual, EstadoPago nuevoEstado)
        {
            bool esTransicionValida = false;

            switch (estadoActual)
            {
                case EstadoPago.Pendiente:
                    // De Pendiente se puede pasar a cualquier estado excepto EnReembolso o Reembolsado
                    esTransicionValida = nuevoEstado != EstadoPago.EnReembolso && nuevoEstado != EstadoPago.Reembolsado;
                    break;

                case EstadoPago.Completado:
                    // De Completado solo se puede pasar a EnReembolso o Reembolsado
                    esTransicionValida = nuevoEstado == EstadoPago.EnReembolso || nuevoEstado == EstadoPago.Reembolsado;
                    break;

                case EstadoPago.ParcialmentePagado:
                    // De ParcialmentePagado se puede pasar a Completado, Cancelado o EnReembolso
                    esTransicionValida = nuevoEstado == EstadoPago.Completado || 
                                        nuevoEstado == EstadoPago.Cancelado || 
                                        nuevoEstado == EstadoPago.EnReembolso;
                    break;

                case EstadoPago.EnReembolso:
                    // De EnReembolso solo se puede pasar a Reembolsado
                    esTransicionValida = nuevoEstado == EstadoPago.Reembolsado;
                    break;

                case EstadoPago.Rechazado:
                case EstadoPago.Cancelado:
                    // De Rechazado o Cancelado no se puede cambiar a ningún otro estado
                    esTransicionValida = false;
                    break;

                case EstadoPago.EnVerificacion:
                    // De EnVerificacion se puede pasar a Completado, Rechazado o Cancelado
                    esTransicionValida = nuevoEstado == EstadoPago.Completado || 
                                        nuevoEstado == EstadoPago.Rechazado || 
                                        nuevoEstado == EstadoPago.Cancelado;
                    break;
            }

            if (!esTransicionValida)
            {
                throw new InvalidOperationException($"No se permite la transición de estado de {estadoActual} a {nuevoEstado}");
            }
        }
    }
} 