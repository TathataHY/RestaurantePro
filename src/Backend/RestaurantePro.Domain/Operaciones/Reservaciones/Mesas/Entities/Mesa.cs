namespace RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities
{
    /// <summary>
    /// Agregado que representa una mesa en el restaurante.
    /// 
    /// Invariantes:
    /// - El número de mesa debe ser mayor que cero
    /// - La capacidad debe ser mayor que cero
    /// - La ubicación no puede estar vacía
    /// - El estado debe ser un valor válido del enum EstadoMesa
    /// 
    /// Ciclo de vida:
    /// - Creación/Disponible → [Reservada | Ocupada | FueraDeServicio]
    /// - Desde cualquier estado puede volver a Disponible, excepto desde Ocupada a FueraDeServicio
    /// 
    /// Reglas de negocio:
    /// - Solo se pueden ocupar mesas que estén disponibles
    /// - Solo se pueden reservar mesas que estén disponibles
    /// - No se puede marcar como fuera de servicio una mesa ocupada
    /// - Cada cambio de estado genera eventos de dominio
    /// </summary>
    public class Mesa : EntityBase, IAggregateRoot
    {
        /// <summary>
        /// Número asignado a la mesa (visible para los clientes)
        /// </summary>
        public virtual int Numero { get; private set; }

        /// <summary>
        /// Capacidad máxima de personas que pueden ocupar la mesa
        /// </summary>
        public virtual int Capacidad { get; private set; }

        /// <summary>
        /// Ubicación de la mesa en el restaurante (e.g., Terraza, Interior, etc.)
        /// </summary>
        public virtual string Ubicacion { get; private set; }

        /// <summary>
        /// Estado actual de la mesa
        /// </summary>
        public virtual EstadoMesa Estado { get; private set; }

        /// <summary>
        /// Constructor privado para EF Core
        /// </summary>
        protected Mesa() { }

        /// <summary>
        /// Método de fábrica para crear una nueva mesa
        /// </summary>
        public static Mesa Crear(int numero, int capacidad, string ubicacion)
        {
            if (numero <= 0)
            {
                throw new ArgumentException("El número de mesa no puede ser negativo o cero", nameof(numero));
            }

            if (capacidad <= 0)
            {
                throw new ArgumentException("La capacidad debe ser mayor que cero", nameof(capacidad));
            }

            if (string.IsNullOrWhiteSpace(ubicacion))
            {
                throw new ArgumentException("La ubicación no puede estar vacía", nameof(ubicacion));
            }

            var mesa = new Mesa
            {
                Id = Guid.NewGuid(),
                Numero = numero,
                Capacidad = capacidad,
                Ubicacion = ubicacion,
                Estado = EstadoMesa.Disponible,
                FechaCreacion = DateTime.Now
            };

            mesa.AddDomainEvent(new MesaCreada(mesa.Id, numero, capacidad, ubicacion));

            mesa.ValidarInvariantes();
            return mesa;
        }

        /// <summary>
        /// Marca la mesa como ocupada
        /// </summary>
        public void MarcarComoOcupada()
        {
            if (Estado != EstadoMesa.Disponible)
            {
                throw new InvalidOperationException($"La mesa {Numero} no puede marcarse como ocupada porque su estado actual es {Estado}");
            }

            Estado = EstadoMesa.Ocupada;
            FechaActualizacion = DateTime.Now;

            ValidarInvariantes();
            AddDomainEvent(new MesaOcupada(Id));
        }

        /// <summary>
        /// Marca la mesa como reservada
        /// </summary>
        public void MarcarComoReservada()
        {
            if (Estado != EstadoMesa.Disponible)
            {
                throw new InvalidOperationException($"La mesa {Numero} no puede marcarse como reservada porque su estado actual es {Estado}");
            }

            Estado = EstadoMesa.Reservada;
            FechaActualizacion = DateTime.Now;

            ValidarInvariantes();
            AddDomainEvent(new MesaReservada(Id));
        }

        /// <summary>
        /// Marca la mesa como disponible
        /// </summary>
        public void MarcarComoDisponible()
        {
            if (Estado == EstadoMesa.Disponible)
            {
                return; // Ya está disponible, no hacemos nada
            }

            Estado = EstadoMesa.Disponible;
            FechaActualizacion = DateTime.Now;

            ValidarInvariantes();
            AddDomainEvent(new MesaDisponible(Id));
        }

        /// <summary>
        /// Marca la mesa como fuera de servicio
        /// </summary>
        public void MarcarComoFueraDeServicio(string motivo)
        {
            if (Estado == EstadoMesa.Ocupada)
            {
                throw new InvalidOperationException("No se puede marcar como fuera de servicio una mesa ocupada");
            }

            Estado = EstadoMesa.FueraDeServicio;
            FechaActualizacion = DateTime.Now;

            ValidarInvariantes();
            AddDomainEvent(new MesaFueraDeServicio(Id, motivo));
        }

        /// <summary>
        /// Valida las invariantes del agregado Mesa
        /// </summary>
        private void ValidarInvariantes()
        {
            if (Numero <= 0)
            {
                throw new InvalidOperationException("El número de mesa debe ser mayor que cero");
            }

            if (Capacidad <= 0)
            {
                throw new InvalidOperationException("La capacidad de la mesa debe ser mayor que cero");
            }

            if (string.IsNullOrWhiteSpace(Ubicacion))
            {
                throw new InvalidOperationException("La ubicación de la mesa no puede estar vacía");
            }

            if (!Enum.IsDefined(typeof(EstadoMesa), Estado))
            {
                throw new InvalidOperationException($"El estado {Estado} no es válido para una mesa");
            }
        }
    }
}
