using System;

namespace RestaurantePro.Domain.Comercial.Clientes.DomainEvents
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se crea un cliente
    /// </summary>
    public class ClienteCreadoEvent
    {
        public int ClienteId { get; }
        public string NombreCompleto { get; }
        public DateTime FechaCreacion { get; }

        public ClienteCreadoEvent(int clienteId, string nombreCompleto)
        {
            ClienteId = clienteId;
            NombreCompleto = nombreCompleto;
            FechaCreacion = DateTime.Now;
        }
    }

    /// <summary>
    /// Evento de dominio que se dispara cuando se agregan puntos a un cliente
    /// </summary>
    public class PuntosAgregadosEvent
    {
        public int ClienteId { get; }
        public int PuntosAgregados { get; }
        public int PuntosTotales { get; }
        public DateTime Fecha { get; }

        public PuntosAgregadosEvent(int clienteId, int puntosAgregados, int puntosTotales)
        {
            ClienteId = clienteId;
            PuntosAgregados = puntosAgregados;
            PuntosTotales = puntosTotales;
            Fecha = DateTime.Now;
        }
    }

    /// <summary>
    /// Evento de dominio que se dispara cuando se desactiva un cliente
    /// </summary>
    public class ClienteDesactivadoEvent
    {
        public int ClienteId { get; }
        public string NombreCompleto { get; }
        public DateTime Fecha { get; }

        public ClienteDesactivadoEvent(int clienteId, string nombreCompleto)
        {
            ClienteId = clienteId;
            NombreCompleto = nombreCompleto;
            Fecha = DateTime.Now;
        }
    }
}
