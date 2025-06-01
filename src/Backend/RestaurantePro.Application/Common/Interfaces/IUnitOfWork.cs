namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Interfaz para el patrón Unit of Work
/// Coordina el trabajo de múltiples repositorios y controla las transacciones
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Guarda todos los cambios pendientes en la base de datos
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de entidades afectadas</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Inicia una nueva transacción
    /// </summary>
    /// <returns>La transacción iniciada</returns>
    Task<IDbContextTransaction> BeginTransactionAsync();

    /// <summary>
    /// Confirma la transacción actual
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// Revierte la transacción actual
    /// </summary>
    Task RollbackTransactionAsync();

    /// <summary>
    /// Ejecuta una operación dentro de una transacción
    /// </summary>
    /// <param name="operation">Operación a ejecutar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta una operación dentro de una transacción y retorna un resultado
    /// </summary>
    /// <typeparam name="T">Tipo del resultado</typeparam>
    /// <param name="operation">Operación a ejecutar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la operación</returns>
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default);
} 