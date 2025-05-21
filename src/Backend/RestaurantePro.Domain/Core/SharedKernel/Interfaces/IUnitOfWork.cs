namespace RestaurantePro.Domain.Core.SharedKernel.Interfaces
{
    /// <summary>
    /// Interfaz para implementar el patrón Unit of Work para transacciones
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Inicia una nueva transacción
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task IniciarTransaccionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Confirma la transacción actual
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task ConfirmarTransaccionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Deshace la transacción actual
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task RevertirTransaccionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Guarda todos los cambios en la base de datos
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de registros afectados</returns>
        Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Guarda todos los cambios en la base de datos y publica los eventos de dominio
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de registros afectados</returns>
        Task<int> GuardarEntidadesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Ejecuta una acción dentro de una transacción
        /// </summary>
        /// <param name="accion">Acción a ejecutar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la ejecución de la acción</returns>
        Task EjecutarEnTransaccionAsync(Func<Task> accion, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Ejecuta una función dentro de una transacción y devuelve su resultado
        /// </summary>
        /// <typeparam name="TResultado">Tipo del resultado de la función</typeparam>
        /// <param name="funcion">Función a ejecutar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la ejecución de la función</returns>
        Task<TResultado> EjecutarEnTransaccionAsync<TResultado>(Func<Task<TResultado>> funcion, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Verifica si hay una transacción activa
        /// </summary>
        bool TieneTransaccionActiva { get; }
    }
}
