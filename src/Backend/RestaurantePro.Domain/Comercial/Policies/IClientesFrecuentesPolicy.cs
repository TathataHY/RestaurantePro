namespace RestaurantePro.Domain.Comercial.Policies
{
    /// <summary>
    /// Resultado de la ejecución de la política de clientes frecuentes
    /// </summary>
    public class ResultadoClientesFrecuentesPolicy
    {
        /// <summary>
        /// Lista de IDs de clientes que fueron actualizados
        /// </summary>
        public List<Guid> ClientesActualizados { get; } = new List<Guid>();
        
        /// <summary>
        /// Lista de IDs de tarjetas de fidelización creadas
        /// </summary>
        public List<Guid> TarjetasCreadas { get; } = new List<Guid>();
        
        /// <summary>
        /// Lista de IDs de clientes con segmentos actualizados
        /// </summary>
        public List<Guid> ClientesSegmentados { get; } = new List<Guid>();
        
        /// <summary>
        /// Conteo de clientes por segmento
        /// </summary>
        public Dictionary<SegmentoCliente, int> ConteoSegmentos { get; } = new Dictionary<SegmentoCliente, int>();
    }
    
    /// <summary>
    /// Interfaz para la política de clientes frecuentes.
    /// Esta política se encarga de analizar los patrones de consumo de los clientes
    /// y aplicar beneficios o actualizar niveles de fidelización automáticamente.
    /// </summary>
    public interface IClientesFrecuentesPolicy
    {
        /// <summary>
        /// Ejecuta la política para todos los clientes activos,
        /// analizando sus patrones de consumo y actualizando niveles de fidelización
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la ejecución de la política</returns>
        Task<ResultadoClientesFrecuentesPolicy> EjecutarPolicy(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Ejecuta la política para un cliente específico
        /// </summary>
        /// <param name="clienteId">ID del cliente a analizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la ejecución de la política</returns>
        Task<ResultadoClientesFrecuentesPolicy> EjecutarPolicyParaCliente(Guid clienteId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Ejecuta la segmentación de clientes basada en su comportamiento
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la ejecución de la política</returns>
        Task<ResultadoClientesFrecuentesPolicy> EjecutarSegmentacionClientes(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Alias de EjecutarPolicy que ejecuta la política para todos los clientes activos
        /// </summary>
        /// <param name="diasHistorial">Días de historial a considerar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la ejecución de la política</returns>
        Task<ResultadoClientesFrecuentesPolicy> EjecutarAsync(int diasHistorial = 90, CancellationToken cancellationToken = default);
    }
} 