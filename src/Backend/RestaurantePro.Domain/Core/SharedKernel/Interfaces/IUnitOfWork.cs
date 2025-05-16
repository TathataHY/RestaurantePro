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
    }
}
