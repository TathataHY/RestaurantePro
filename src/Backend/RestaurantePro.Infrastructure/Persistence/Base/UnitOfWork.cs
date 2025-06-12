using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Persistence.Base;

/// <summary>
/// Implementación del patrón Unit of Work para centralizar las transacciones
/// </summary>
public class UnitOfWork : IDisposable
{
    private readonly DbContext _dbContext;
    private readonly ILogger<UnitOfWork> _logger;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    public UnitOfWork(DbContext dbContext, ILogger<UnitOfWork> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Se guardaron {Count} cambios en la base de datos", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar cambios en la base de datos");
            throw;
        }
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("Ya existe una transacción activa");
        }

        _currentTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        _logger.LogInformation("Transacción iniciada: {TransactionId}", _currentTransaction.TransactionId);
        return _currentTransaction;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No hay transacción activa para confirmar");
        }

        try
        {
            await _currentTransaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Transacción confirmada: {TransactionId}", _currentTransaction.TransactionId);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            return;
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            _logger.LogInformation("Transacción revertida: {TransactionId}", _currentTransaction.TransactionId);
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _currentTransaction?.Dispose();
                _dbContext.Dispose();
            }

            _disposed = true;
        }
    }
} 