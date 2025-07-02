using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Base;

/// <summary>
/// Implementación del patrón Unit of Work para centralizar las transacciones
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly RestauranteProDbContext _dbContext;
    private readonly ILogger<UnitOfWork> _logger;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    public UnitOfWork(RestauranteProDbContext dbContext, ILogger<UnitOfWork> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public bool TieneTransaccionActiva => _currentTransaction != null;

    public DbContext GetDbContext() => _dbContext;

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

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default) => 
        SaveChangesAsync(cancellationToken);

    public async Task<int> GuardarEntidadesAsync(CancellationToken cancellationToken = default)
    {
        // Aquí se podría implementar la publicación de eventos de dominio antes de guardar
        return await SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("Ya existe una transacción activa");
        }

        _currentTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        _logger.LogInformation("Transacción iniciada: {TransactionId}", _currentTransaction.TransactionId);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("Ya existe una transacción activa");
        }

        _currentTransaction = await _dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
        _logger.LogInformation("Transacción iniciada con IsolationLevel {IsolationLevel}: {TransactionId}", 
            isolationLevel, _currentTransaction.TransactionId);
        
        return _currentTransaction;
    }

    public Task IniciarTransaccionAsync(CancellationToken cancellationToken = default) => 
        BeginTransactionAsync(cancellationToken);

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

    public Task ConfirmarTransaccionAsync(CancellationToken cancellationToken = default) => 
        CommitTransactionAsync(cancellationToken);

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

    public Task RevertirTransaccionAsync(CancellationToken cancellationToken = default) => 
        RollbackTransactionAsync(cancellationToken);

    public async Task EjecutarEnTransaccionAsync(Func<Task> accion, CancellationToken cancellationToken = default)
    {
        var transaccionIniciada = false;
        
        try
        {
            if (_currentTransaction == null)
            {
                await BeginTransactionAsync(cancellationToken);
                transaccionIniciada = true;
            }

            await accion();
            
            // Guardar los cambios en la base de datos
            await SaveChangesAsync(cancellationToken);

            if (transaccionIniciada)
            {
                await CommitTransactionAsync(cancellationToken);
            }
        }
        catch
        {
            if (transaccionIniciada && _currentTransaction != null)
            {
                await RollbackTransactionAsync(cancellationToken);
            }
            throw;
        }
    }

    public async Task<TResultado> EjecutarEnTransaccionAsync<TResultado>(Func<Task<TResultado>> funcion, CancellationToken cancellationToken = default)
    {
        var transaccionIniciada = false;
        
        try
        {
            if (_currentTransaction == null)
            {
                await BeginTransactionAsync(cancellationToken);
                transaccionIniciada = true;
            }

            var resultado = await funcion();
            
            // Guardar los cambios en la base de datos
            await SaveChangesAsync(cancellationToken);

            if (transaccionIniciada)
            {
                await CommitTransactionAsync(cancellationToken);
            }

            return resultado;
        }
        catch
        {
            if (transaccionIniciada && _currentTransaction != null)
            {
                await RollbackTransactionAsync(cancellationToken);
            }
            throw;
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