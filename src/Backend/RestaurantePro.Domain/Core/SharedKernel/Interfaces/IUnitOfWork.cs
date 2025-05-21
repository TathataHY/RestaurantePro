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
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Confirma la transacción actual
        /// </summary>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Deshace la transacción actual
        /// </summary>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Guarda todos los cambios en la base de datos
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Guarda todos los cambios en la base de datos y publica los eventos de dominio
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de registros afectados</returns>
        Task<int> SaveEntitiesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Ejecuta una acción dentro de una transacción
        /// </summary>
        /// <param name="action">Acción a ejecutar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la ejecución de la acción</returns>
        Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Ejecuta una función dentro de una transacción y devuelve su resultado
        /// </summary>
        /// <typeparam name="TResult">Tipo del resultado de la función</typeparam>
        /// <param name="func">Función a ejecutar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la ejecución de la función</returns>
        Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> func, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Verifica si hay una transacción activa
        /// </summary>
        bool HasActiveTransaction { get; }
    }
}
