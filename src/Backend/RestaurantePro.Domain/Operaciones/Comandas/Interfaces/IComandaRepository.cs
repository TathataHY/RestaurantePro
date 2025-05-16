
namespace RestaurantePro.Domain.Operaciones.Comandas.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de comandas
    /// </summary>
    public interface IComandaRepository
    {
        /// <summary>
        /// Obtiene una comanda por su ID
        /// </summary>
        Task<Comanda> ObtenerPorIdAsync(Guid id);

        /// <summary>
        /// Obtiene comandas por estado
        /// </summary>
        Task<IEnumerable<Comanda>> ObtenerPorEstadoAsync(EstadoComanda estado);

        /// <summary>
        /// Obtiene comandas por mesa
        /// </summary>
        Task<IEnumerable<Comanda>> ObtenerPorMesaAsync(Guid mesaId);

        /// <summary>
        /// Obtiene comandas por mesero
        /// </summary>
        Task<IEnumerable<Comanda>> ObtenerPorMeseroAsync(Guid meseroId);

        /// <summary>
        /// Obtiene comandas por cliente
        /// </summary>
        Task<IEnumerable<Comanda>> ObtenerPorClienteAsync(Guid clienteId);

        /// <summary>
        /// Obtiene comandas creadas en un rango de fechas
        /// </summary>
        Task<IEnumerable<Comanda>> ObtenerPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Agrega una nueva comanda
        /// </summary>
        Task AgregarAsync(Comanda comanda);

        /// <summary>
        /// Actualiza una comanda existente
        /// </summary>
        Task ActualizarAsync(Comanda comanda);

        /// <summary>
        /// Elimina una comanda (solo para propósitos administrativos)
        /// </summary>
        Task EliminarAsync(Guid id);
    }
}
