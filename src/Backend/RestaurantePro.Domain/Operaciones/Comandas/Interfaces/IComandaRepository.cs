namespace RestaurantePro.Domain.Operaciones.Comandas.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de comandas
    /// </summary>
    public interface IComandaRepository : IRepository<Comanda>
    {
        /// <summary>
        /// Obtiene una comanda por su ID
        /// </summary>
        Task<Comanda> ObtenerPorIdAsync(Guid id);
        
        /// <summary>
        /// Obtiene una comanda por su ID
        /// </summary>
        Task<Comanda> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>
        /// Obtiene comandas por estado
        /// </summary>
        Task<IEnumerable<Comanda>> ObtenerPorEstadoAsync(EstadoComanda estado);
        
        /// <summary>
        /// Obtiene comandas por estado
        /// </summary>
        Task<IEnumerable<Comanda>> ObtenerPorEstadoAsync(EstadoComanda estado, CancellationToken cancellationToken);

        /// <summary>
        /// Obtiene comandas por mesa
        /// </summary>
        Task<IEnumerable<Comanda>> ObtenerPorMesaAsync(Guid mesaId);
        
        /// <summary>
        /// Obtiene comandas por mesa
        /// </summary>
        Task<IEnumerable<Comanda>> ObtenerPorMesaAsync(Guid mesaId, CancellationToken cancellationToken);

        /// <summary>
        /// Obtiene comandas por mesero
        /// </summary>
        Task<IEnumerable<Comanda>> ObtenerPorMeseroAsync(Guid meseroId);
        
        /// <summary>
        /// Obtiene comandas por mesero
        /// </summary>
        Task<IEnumerable<Comanda>> ObtenerPorMeseroAsync(Guid meseroId, CancellationToken cancellationToken);

        /// <summary>
        /// Obtiene comandas por cliente
        /// </summary>
        Task<IEnumerable<Comanda>> ObtenerPorClienteAsync(Guid clienteId);
        
        /// <summary>
        /// Obtiene comandas por cliente
        /// </summary>
        Task<IEnumerable<Comanda>> ObtenerPorClienteAsync(Guid clienteId, CancellationToken cancellationToken);
        
        /// <summary>
        /// Obtiene comandas abiertas (en proceso) para un cliente
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de comandas abiertas del cliente</returns>
        Task<IEnumerable<Comanda>> ObtenerComandasAbiertas(Guid clienteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene comandas creadas en un rango de fechas
        /// </summary>
        Task<IEnumerable<Comanda>> ObtenerPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);
        
        /// <summary>
        /// Obtiene comandas creadas en un rango de fechas
        /// </summary>
        Task<IEnumerable<Comanda>> ObtenerPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken);

        /// <summary>
        /// Agrega una nueva comanda
        /// </summary>
        Task AgregarAsync(Comanda comanda);
        
        /// <summary>
        /// Agrega una nueva comanda
        /// </summary>
        Task AgregarAsync(Comanda comanda, CancellationToken cancellationToken);

        /// <summary>
        /// Actualiza una comanda existente
        /// </summary>
        Task ActualizarAsync(Comanda comanda);
        
        /// <summary>
        /// Actualiza una comanda existente
        /// </summary>
        Task ActualizarAsync(Comanda comanda, CancellationToken cancellationToken);

        /// <summary>
        /// Elimina una comanda (solo para propósitos administrativos)
        /// </summary>
        Task EliminarAsync(Guid id);
        
        /// <summary>
        /// Elimina una comanda (solo para propósitos administrativos)
        /// </summary>
        Task EliminarAsync(Guid id, CancellationToken cancellationToken);
        
        /// <summary>
        /// Guarda los cambios en la base de datos
        /// </summary>
        Task GuardarCambiosAsync();
        
        /// <summary>
        /// Guarda los cambios en la base de datos
        /// </summary>
        Task GuardarCambiosAsync(CancellationToken cancellationToken);
    }
}
