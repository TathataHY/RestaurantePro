using System;
using RestaurantePro.Domain.Core.Base.Interfaces;

namespace RestaurantePro.Domain.Comercial.Clientes.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se crea un cliente
    /// </summary>
    public class ClienteCreadoEvent : IDomainEvent
    {
        /// <summary>
        /// Identificador del cliente creado
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Nombre completo del cliente
        /// </summary>
        public string NombreCompleto { get; }

        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;

        public ClienteCreadoEvent(Guid clienteId, string nombreCompleto)
        {
            ClienteId = clienteId;
            NombreCompleto = nombreCompleto;
        }
    }

    /// <summary>
    /// Evento de dominio que se dispara cuando se agregan puntos a un cliente
    /// </summary>
    public class PuntosAgregadosEvent : IDomainEvent
    {
        /// <summary>
        /// Identificador del cliente
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Cantidad de puntos agregados
        /// </summary>
        public int PuntosAgregados { get; }

        /// <summary>
        /// Total de puntos acumulados
        /// </summary>
        public int PuntosTotales { get; }

        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;

        public PuntosAgregadosEvent(Guid clienteId, int puntosAgregados, int puntosTotales)
        {
            ClienteId = clienteId;
            PuntosAgregados = puntosAgregados;
            PuntosTotales = puntosTotales;
        }
    }

    /// <summary>
    /// Evento de dominio que se dispara cuando se desactiva un cliente
    /// </summary>
    public class ClienteDesactivadoEvent : IDomainEvent
    {
        /// <summary>
        /// Identificador del cliente
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Nombre completo del cliente
        /// </summary>
        public string NombreCompleto { get; }

        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;

        public ClienteDesactivadoEvent(Guid clienteId, string nombreCompleto)
        {
            ClienteId = clienteId;
            NombreCompleto = nombreCompleto;
        }
    }

    /// <summary>
    /// Evento de dominio que se dispara cuando se reactiva un cliente
    /// </summary>
    public class ClienteReactivadoEvent : IDomainEvent
    {
        /// <summary>
        /// Identificador del cliente
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Nombre completo del cliente
        /// </summary>
        public string NombreCompleto { get; }

        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;

        public ClienteReactivadoEvent(Guid clienteId, string nombreCompleto)
        {
            ClienteId = clienteId;
            NombreCompleto = nombreCompleto;
        }
    }

    /// <summary>
    /// Evento de dominio que se dispara cuando se actualiza la información de contacto
    /// </summary>
    public class InformacionContactoActualizadaEvent : IDomainEvent
    {
        /// <summary>
        /// Identificador del cliente
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Nuevo email del cliente
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Nuevo teléfono del cliente
        /// </summary>
        public string Telefono { get; }

        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;

        public InformacionContactoActualizadaEvent(Guid clienteId, string email, string telefono)
        {
            ClienteId = clienteId;
            Email = email;
            Telefono = telefono;
        }
    }
} 