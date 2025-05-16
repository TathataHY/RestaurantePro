
namespace RestaurantePro.Domain.Comercial.Clientes.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se crea una tarjeta de fidelización
    /// </summary>
    public class TarjetaFidelizacionCreadaEvent : IDomainEvent
    {
        /// <summary>
        /// Identificador de la tarjeta creada
        /// </summary>
        public Guid TarjetaId { get; }

        /// <summary>
        /// Identificador del cliente
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Código de la tarjeta
        /// </summary>
        public string Codigo { get; }

        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;

        public TarjetaFidelizacionCreadaEvent(Guid tarjetaId, Guid clienteId, string codigo)
        {
            TarjetaId = tarjetaId;
            ClienteId = clienteId;
            Codigo = codigo;
        }
    }

    /// <summary>
    /// Evento de dominio que se dispara cuando se activa una tarjeta
    /// </summary>
    public class TarjetaFidelizacionActivadaEvent : IDomainEvent
    {
        /// <summary>
        /// Identificador de la tarjeta
        /// </summary>
        public Guid TarjetaId { get; }

        /// <summary>
        /// Identificador del cliente
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Fecha de activación
        /// </summary>
        public DateTime FechaActivacion { get; }

        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;

        public TarjetaFidelizacionActivadaEvent(Guid tarjetaId, Guid clienteId, DateTime fechaActivacion)
        {
            TarjetaId = tarjetaId;
            ClienteId = clienteId;
            FechaActivacion = fechaActivacion;
        }
    }

    /// <summary>
    /// Evento de dominio que se dispara cuando se suspende una tarjeta
    /// </summary>
    public class TarjetaFidelizacionSuspendidaEvent : IDomainEvent
    {
        /// <summary>
        /// Identificador de la tarjeta
        /// </summary>
        public Guid TarjetaId { get; }

        /// <summary>
        /// Identificador del cliente
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Motivo de la suspensión
        /// </summary>
        public string Motivo { get; }

        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;

        public TarjetaFidelizacionSuspendidaEvent(Guid tarjetaId, Guid clienteId, string motivo)
        {
            TarjetaId = tarjetaId;
            ClienteId = clienteId;
            Motivo = motivo;
        }
    }

    /// <summary>
    /// Evento de dominio que se dispara cuando se cancela una tarjeta
    /// </summary>
    public class TarjetaFidelizacionCanceladaEvent : IDomainEvent
    {
        /// <summary>
        /// Identificador de la tarjeta
        /// </summary>
        public Guid TarjetaId { get; }

        /// <summary>
        /// Identificador del cliente
        /// </summary>
        public Guid ClienteId { get; }

        /// <summary>
        /// Motivo de la cancelación
        /// </summary>
        public string Motivo { get; }

        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;

        public TarjetaFidelizacionCanceladaEvent(Guid tarjetaId, Guid clienteId, string motivo)
        {
            TarjetaId = tarjetaId;
            ClienteId = clienteId;
            Motivo = motivo;
        }
    }

    /// <summary>
    /// Evento de dominio que se dispara cuando se agregan puntos a una tarjeta
    /// </summary>
    public class PuntosAgregadosATarjetaEvent : IDomainEvent
    {
        /// <summary>
        /// Identificador de la tarjeta
        /// </summary>
        public Guid TarjetaId { get; }

        /// <summary>
        /// Puntos agregados
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

        public PuntosAgregadosATarjetaEvent(Guid tarjetaId, int puntosAgregados, int puntosTotales)
        {
            TarjetaId = tarjetaId;
            PuntosAgregados = puntosAgregados;
            PuntosTotales = puntosTotales;
        }
    }

    /// <summary>
    /// Evento de dominio que se dispara cuando se canjean puntos de una tarjeta
    /// </summary>
    public class PuntosCanjeadosEvent : IDomainEvent
    {
        /// <summary>
        /// Identificador de la tarjeta
        /// </summary>
        public Guid TarjetaId { get; }

        /// <summary>
        /// Puntos canjeados
        /// </summary>
        public int PuntosCanjeados { get; }

        /// <summary>
        /// Concepto del canje
        /// </summary>
        public string Concepto { get; }

        /// <summary>
        /// Puntos disponibles después del canje
        /// </summary>
        public int PuntosDisponibles { get; }

        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;

        public PuntosCanjeadosEvent(Guid tarjetaId, int puntosCanjeados, string concepto, int puntosDisponibles)
        {
            TarjetaId = tarjetaId;
            PuntosCanjeados = puntosCanjeados;
            Concepto = concepto;
            PuntosDisponibles = puntosDisponibles;
        }
    }

    /// <summary>
    /// Evento de dominio que se dispara cuando se actualiza el nivel de fidelización
    /// </summary>
    public class NivelFidelizacionActualizadoEvent : IDomainEvent
    {
        /// <summary>
        /// Identificador de la tarjeta
        /// </summary>
        public Guid TarjetaId { get; }

        /// <summary>
        /// Nivel de fidelización anterior
        /// </summary>
        public NivelFidelizacion NivelAnterior { get; }

        /// <summary>
        /// Nuevo nivel de fidelización
        /// </summary>
        public NivelFidelizacion NuevoNivel { get; }

        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;

        public NivelFidelizacionActualizadoEvent(Guid tarjetaId, NivelFidelizacion nivelAnterior, NivelFidelizacion nuevoNivel)
        {
            TarjetaId = tarjetaId;
            NivelAnterior = nivelAnterior;
            NuevoNivel = nuevoNivel;
        }
    }
} 