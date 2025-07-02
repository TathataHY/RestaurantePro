using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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
        /// Inicia una nueva transacción con nivel de aislamiento específico
        /// </summary>
        /// <param name="isolationLevel">Nivel de aislamiento de la transacción</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Transacción iniciada</returns>
        Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);

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
        /// Guarda los cambios en la base de datos
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de entidades afectadas</returns>
        Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Guarda entidades y publica eventos de dominio
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de entidades afectadas</returns>
        Task<int> GuardarEntidadesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Ejecuta una acción dentro de una transacción
        /// </summary>
        /// <param name="accion">Acción a ejecutar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task EjecutarEnTransaccionAsync(Func<Task> accion, CancellationToken cancellationToken = default);

        /// <summary>
        /// Ejecuta una función dentro de una transacción y retorna un resultado
        /// </summary>
        /// <typeparam name="TResultado">Tipo del resultado</typeparam>
        /// <param name="funcion">Función a ejecutar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la función</returns>
        Task<TResultado> EjecutarEnTransaccionAsync<TResultado>(Func<Task<TResultado>> funcion, CancellationToken cancellationToken = default);

        /// <summary>
        /// Indica si hay una transacción activa
        /// </summary>
        bool TieneTransaccionActiva { get; }

        /// <summary>
        /// Obtiene el contexto de base de datos subyacente
        /// </summary>
        /// <returns>DbContext subyacente</returns>
        DbContext GetDbContext();

        // ============================================
        // ALIAS EN INGLÉS PARA COMPATIBILIDAD
        // ============================================

        /// <summary>
        /// Alias en inglés para IniciarTransaccionAsync
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Alias en inglés para ConfirmarTransaccionAsync
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Alias en inglés para RevertirTransaccionAsync
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Alias en inglés para GuardarCambiosAsync
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de entidades afectadas</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
