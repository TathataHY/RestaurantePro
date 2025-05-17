namespace RestaurantePro.Domain.Comercial.EventHandlers
{
    /// <summary>
    /// Manejador que verifica si un cliente debe actualizar su nivel de fidelización
    /// después de registrar una nueva visita
    /// </summary>
    public class VisitaRegistrada_ActualizarNivelFidelizacionHandler : IDomainEventHandler<VisitaRegistrada>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
        private readonly IClientesFrecuentesPolicy _clientesFrecuentesPolicy;
        
        // Umbrales de niveles que deben coincidir con los de la política
        private const int UMBRAL_PLATA = 10;
        private const int UMBRAL_ORO = 20;
        private const int UMBRAL_PLATINO = 30;
        
        public VisitaRegistrada_ActualizarNivelFidelizacionHandler(
            IClienteRepository clienteRepository,
            ITarjetaFidelizacionRepository tarjetaRepository,
            IClientesFrecuentesPolicy clientesFrecuentesPolicy)
        {
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _tarjetaRepository = tarjetaRepository ?? throw new ArgumentNullException(nameof(tarjetaRepository));
            _clientesFrecuentesPolicy = clientesFrecuentesPolicy ?? throw new ArgumentNullException(nameof(clientesFrecuentesPolicy));
        }
        
        /// <summary>
        /// Maneja el evento de visita registrada verificando si el cliente llegó a un umbral
        /// para actualizar su nivel de fidelización
        /// </summary>
        public async Task Handle(VisitaRegistrada evento, CancellationToken cancellationToken)
        {
            // Obtener la cantidad de visitas del evento
            int cantidadVisitas = evento.TotalVisitas;
            
            // Verificar si la cantidad de visitas corresponde a algún umbral exacto
            bool alcanzadoUmbral = cantidadVisitas == UMBRAL_PLATA || 
                                  cantidadVisitas == UMBRAL_ORO || 
                                  cantidadVisitas == UMBRAL_PLATINO;
            
            // Si no alcanzó un umbral exacto, no hacer nada
            if (!alcanzadoUmbral)
                return;
                
            // Si llegó aquí, es porque el cliente alcanzó un umbral exacto,
            // por lo que ejecutamos la política para actualizar su nivel
            await _clientesFrecuentesPolicy.EjecutarPolicyParaCliente(evento.ClienteId, cancellationToken);
        }
    }
} 