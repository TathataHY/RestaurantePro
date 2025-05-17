namespace RestaurantePro.Domain.Comercial.Clientes.Interfaces
{
    /// <summary>
    /// Repositorio para la gestión de clientes
    /// </summary>
    public interface IClienteRepository : IRepository<Cliente>
    {
        /// <summary>
        /// Obtiene un cliente por su ID
        /// </summary>
        /// <param name="id">ID del cliente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Cliente encontrado o null si no existe</returns>
        Task<Cliente> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un cliente por su email
        /// </summary>
        /// <param name="email">Email del cliente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Cliente encontrado o null si no existe</returns>
        Task<Cliente> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todos los clientes
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de clientes</returns>
        Task<IEnumerable<Cliente>> ObtenerTodosAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los clientes filtrados por estado (activo/inactivo)
        /// </summary>
        /// <param name="activo">Indica si se quieren obtener clientes activos o inactivos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de clientes que cumplen con el filtro</returns>
        Task<IEnumerable<Cliente>> ObtenerPorEstadoAsync(bool activo, CancellationToken cancellationToken = default);

        /// <summary>
        /// Agrega un nuevo cliente
        /// </summary>
        /// <param name="cliente">Cliente a agregar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea asíncrona</returns>
        Task AgregarAsync(Cliente cliente, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza un cliente existente
        /// </summary>
        /// <param name="cliente">Cliente a actualizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea asíncrona</returns>
        Task ActualizarAsync(Cliente cliente, CancellationToken cancellationToken = default);

        /// <summary>
        /// Guarda los cambios en la unidad de trabajo
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea asíncrona</returns>
        Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Elimina un cliente (normalmente una eliminación lógica)
        /// </summary>
        /// <param name="id">ID del cliente a eliminar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea asíncrona</returns>
        Task EliminarAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todos los clientes activos con su información de visitas
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de clientes activos con información de visitas</returns>
        Task<IEnumerable<Cliente>> ObtenerClientesActivosConVisitasAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene clientes por sus IDs
        /// </summary>
        /// <param name="ids">Lista de IDs de clientes a obtener</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de clientes que coinciden con los IDs proporcionados</returns>
        Task<IEnumerable<Cliente>> ObtenerClientesPorIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    }
}
