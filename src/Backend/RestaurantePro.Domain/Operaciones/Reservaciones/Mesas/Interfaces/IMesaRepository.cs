namespace RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de mesas
    /// </summary>
    public interface IMesaRepository : IRepository<Mesa>
    {
        /// <summary>
        /// Obtiene todas las mesas
        /// </summary>
        Task<IEnumerable<Mesa>> ObtenerTodasAsync();

        /// <summary>
        /// Obtiene una mesa por su ID
        /// </summary>
        Task<Mesa> ObtenerPorIdAsync(Guid id);

        /// <summary>
        /// Obtiene una mesa por su número
        /// </summary>
        Task<Mesa> ObtenerPorNumeroAsync(int numero);

        /// <summary>
        /// Verifica si existe una mesa con el número especificado
        /// </summary>
        Task<bool> ExisteNumeroAsync(int numero);

        /// <summary>
        /// Busca mesas por ubicación
        /// </summary>
        Task<IEnumerable<Mesa>> BuscarPorUbicacionAsync(string ubicacion);

        /// <summary>
        /// Agrega una nueva mesa
        /// </summary>
        Task AgregarAsync(Mesa mesa);

        /// <summary>
        /// Actualiza una mesa existente
        /// </summary>
        Task ActualizarAsync(Mesa mesa);

        /// <summary>
        /// Elimina una mesa
        /// </summary>
        Task EliminarAsync(Guid id);

        /// <summary>
        /// Obtiene el número de comensales actuales (suma de capacidades de mesas ocupadas)
        /// </summary>
        Task<int> ObtenerTotalComensalesActualesAsync();

        /// <summary>
        /// Obtiene la lista de mesas disponibles
        /// </summary>
        Task<IEnumerable<Mesa>> ObtenerMesasDisponiblesAsync();
        
        /// <summary>
        /// Guarda los cambios en la base de datos
        /// </summary>
        Task GuardarCambiosAsync();

        /// <summary>
        /// Busca la mejor mesa disponible para los criterios especificados
        /// </summary>
        Task<Mesa?> BuscarMejorMesaAsync(int numeroPersonas, string? areaPreferida = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Busca mesas con criterios específicos
        /// </summary>
        Task<IEnumerable<Mesa>> BuscarMesasAsync(int? capacidadMinima = null, string? area = null, bool? disponible = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene mesas por capacidad específica
        /// </summary>
        Task<IEnumerable<Mesa>> ObtenerMesasPorCapacidadAsync(int capacidad, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si una mesa específica está disponible
        /// </summary>
        Task<bool> VerificarDisponibilidadAsync(Guid mesaId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene las mesas que han estado en estado 'PendienteLimpieza' por más tiempo que el especificado.
        /// </summary>
        /// <param name="minutosAntiguedad">El número de minutos para considerar una mesa como 'sucia'.</param>
        /// <returns>Una colección de mesas sucias.</returns>
        Task<IEnumerable<Mesa>> ObtenerMesasSuciaPorAntiguedad(int minutosAntiguedad);
    }
}
