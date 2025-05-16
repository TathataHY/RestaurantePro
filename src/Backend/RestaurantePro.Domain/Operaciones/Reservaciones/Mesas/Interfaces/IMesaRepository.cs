namespace RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de mesas
    /// </summary>
    public interface IMesaRepository
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
    }
}
