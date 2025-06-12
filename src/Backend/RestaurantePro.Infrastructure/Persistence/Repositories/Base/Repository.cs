using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Base;

/// <summary>
/// Implementación base de repositorio genérico
/// </summary>
/// <typeparam name="T">Tipo de entidad</typeparam>
public class Repository<T> where T : class
{
    protected readonly DbContext _dbContext;
    protected readonly DbSet<T> _dbSet;
    protected readonly ILogger<Repository<T>> _logger;

    public Repository(DbContext dbContext, ILogger<Repository<T>> logger)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<T>();
        _logger = logger;
    }

    /// <summary>
    /// Obtiene una entidad por su identificador
    /// </summary>
    public virtual async Task<T?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <summary>
    /// Obtiene todas las entidades
    /// </summary>
    public virtual async Task<IEnumerable<T>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Agrega una nueva entidad
    /// </summary>
    public virtual async Task<T> AgregarAsync(T entity, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet.AddAsync(entity, cancellationToken);
        return result.Entity;
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<T>> AgregarRangoAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
        return entities;
    }

    /// <summary>
    /// Actualiza una entidad existente
    /// </summary>
    public virtual Task ActualizarAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Elimina una entidad
    /// </summary>
    public virtual async Task EliminarAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Elimina una entidad por su identificador
    /// </summary>
    public virtual async Task EliminarPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await ObtenerPorIdAsync(id, cancellationToken);
        if (entity != null)
        {
            await EliminarAsync(entity, cancellationToken);
        }
    }

    /// <inheritdoc />
    public virtual async Task<(IEnumerable<T> Items, int Total)> ObtenerPaginadoAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalItems = await _dbSet.CountAsync(cancellationToken);
        var items = await _dbSet
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (Items: items, Total: totalItems);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<T>> BuscarAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<bool> ExisteAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<int> ContarAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<T?> PrimeroODefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Guarda los cambios en la base de datos
    /// </summary>
    public virtual async Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
} 