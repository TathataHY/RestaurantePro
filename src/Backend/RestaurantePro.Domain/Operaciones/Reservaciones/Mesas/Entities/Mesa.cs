using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Events;

namespace RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities
{
    /// <summary>
    /// Representa una mesa en el restaurante
    /// </summary>
    public class Mesa : EntityBase, IAggregateRoot
    {
        /// <summary>
        /// Número asignado a la mesa (visible para los clientes)
        /// </summary>
        public int Numero { get; private set; }

        /// <summary>
        /// Capacidad máxima de personas que pueden ocupar la mesa
        /// </summary>
        public int Capacidad { get; private set; }

        /// <summary>
        /// Ubicación de la mesa en el restaurante (e.g., Terraza, Interior, etc.)
        /// </summary>
        public string Ubicacion { get; private set; }

        /// <summary>
        /// Estado actual de la mesa
        /// </summary>
        public EstadoMesa Estado { get; private set; }

        /// <summary>
        /// Constructor privado para EF Core
        /// </summary>
        private Mesa() { }

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

            // Aquí podríamos añadir un evento de dominio para registrar que la mesa está fuera de servicio
            // AddDomainEvent(new MesaFueraDeServicio(Id, motivo));
        }
    }
}
