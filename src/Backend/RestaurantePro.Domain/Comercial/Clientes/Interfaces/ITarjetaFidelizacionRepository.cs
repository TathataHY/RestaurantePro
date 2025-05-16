namespace RestaurantePro.Domain.Comercial.Clientes.Interfaces
{
    /// <summary>
    /// Repositorio para la gestión de tarjetas de fidelización
    /// </summary>
    public interface ITarjetaFidelizacionRepository : IRepository<TarjetaFidelizacion>
    {
        /// <summary>
        /// Obtiene una tarjeta de fidelización por su ID
        /// </summary>
        /// <param name="id">ID de la tarjeta</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarjeta encontrada o null si no existe</returns>
        Task<TarjetaFidelizacion> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene una tarjeta de fidelización por su código
        /// </summary>
        /// <param name="codigo">Código de la tarjeta</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarjeta encontrada o null si no existe</returns>
        Task<TarjetaFidelizacion> ObtenerPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todas las tarjetas de un cliente
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de tarjetas del cliente</returns>
        Task<IEnumerable<TarjetaFidelizacion>> ObtenerPorClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene la tarjeta activa de un cliente (si existe)
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarjeta activa del cliente o null si no existe</returns>
        Task<TarjetaFidelizacion> ObtenerTarjetaActivaPorClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene las tarjetas filtradas por estado
        /// </summary>
        /// <param name="estado">Estado de las tarjetas a obtener</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de tarjetas que cumplen con el filtro</returns>
        Task<IEnumerable<TarjetaFidelizacion>> ObtenerPorEstadoAsync(EstadoTarjeta estado, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene las tarjetas filtradas por nivel de fidelización
        /// </summary>
        /// <param name="nivel">Nivel de fidelización de las tarjetas a obtener</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de tarjetas que cumplen con el filtro</returns>
        Task<IEnumerable<TarjetaFidelizacion>> ObtenerPorNivelAsync(NivelFidelizacion nivel, CancellationToken cancellationToken = default);

        /// <summary>
        /// Agrega una nueva tarjeta de fidelización
        /// </summary>
        /// <param name="tarjeta">Tarjeta a agregar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea asíncrona</returns>
        Task AgregarAsync(TarjetaFidelizacion tarjeta, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza una tarjeta existente
        /// </summary>
        /// <param name="tarjeta">Tarjeta a actualizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea asíncrona</returns>
        Task ActualizarAsync(TarjetaFidelizacion tarjeta, CancellationToken cancellationToken = default);

        /// <summary>
        /// Guarda los cambios en la unidad de trabajo
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea asíncrona</returns>
        Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Genera un código único para una nueva tarjeta
        /// </summary>
        /// <param name="prefijo">Prefijo opcional para el código</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Código único para la tarjeta</returns>
        Task<string> GenerarCodigoUnicoAsync(string prefijo = "TF", CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Elimina una tarjeta de fidelización
        /// </summary>
        /// <param name="id">ID de la tarjeta a eliminar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea asíncrona</returns>
        Task EliminarAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
