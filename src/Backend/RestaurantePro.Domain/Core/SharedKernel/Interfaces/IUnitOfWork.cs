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

    /// <summary>
    /// Interfaz genérica para repositorios de entidades
    /// </summary>
    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IReadOnlyList<T>> GetAsync(ISpecification<T> spec);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<int> CountAsync(ISpecification<T> spec);
    }

    /// <summary>
    /// Interfaz para especificaciones genéricas
    /// </summary>
    public interface ISpecification<T>
    {
        bool IsSatisfiedBy(T entity);
    }

    /// <summary>
    /// Interfaz para el servicio de correo
    /// </summary>
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
    }

    /// <summary>
    /// Interfaz para el servicio de notificaciones
    /// </summary>
    public interface INotificationService
    {
        Task NotifyAsync(string userId, string message, string type = "info");
    }
}
