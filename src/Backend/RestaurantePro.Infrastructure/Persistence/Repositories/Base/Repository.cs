using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Base;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
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
public class Repository<T> : IRepository<T> where T : class
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
    /// Obtiene una entidad por su ID
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
    public virtual async Task AgregarAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task AgregarRangoAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    /// <summary>
    /// Actualiza una entidad existente
    /// </summary>
    public virtual async Task ActualizarAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            // Estrategia 1: Detección forzada de cambios
            _dbContext.ChangeTracker.DetectChanges();
            
            var entry = _dbContext.Entry(entity);
            
            // Estrategia 2: Si la entidad no está siendo rastreada, la adjuntamos
            if (entry.State == EntityState.Detached)
            {
                _dbSet.Update(entity);
            }
            else
            {
                // Estrategia 3: Marcar explícitamente como modificada
                entry.State = EntityState.Modified;
            }
            
            // Estrategia 4: Guardar cambios con verificación
            var rowsAffected = await _dbContext.SaveChangesAsync(cancellationToken);
            
            // Estrategia 5: Verificación post-guardado
            if (rowsAffected == 0)
            {
                _logger.LogWarning("No se detectaron cambios al guardar entidad {EntityType}. Reintentando con estrategia alternativa", typeof(T).Name);
                
                // Estrategia 6: Reintento con enfoque más agresivo
                _dbSet.Update(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            
            _logger.LogInformation("Entidad {EntityType} actualizada exitosamente. Filas afectadas: {RowsAffected}", typeof(T).Name, rowsAffected);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar entidad {EntityType}", typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Elimina una entidad
    /// </summary>
    public virtual Task EliminarAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity is EntityBase entityBase)
        {
            entityBase.MarkAsDeleted();
            _dbSet.Update(entity);
        }
        else
        {
            _dbSet.Remove(entity);
        }
        return Task.CompletedTask;
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

    public Task<IEnumerable<T>> BuscarAsync(Func<T, bool> predicado, CancellationToken cancellationToken = default)
    {
        var result = _dbSet.Where(predicado).ToList();
        return Task.FromResult<IEnumerable<T>>(result);
    }

    public Task<bool> ExisteAsync(Func<T, bool> predicado, CancellationToken cancellationToken = default)
    {
        var result = _dbSet.Any(predicado);
        return Task.FromResult(result);
    }

    public Task<int> ContarAsync(Func<T, bool> predicado, CancellationToken cancellationToken = default)
    {
        var result = _dbSet.Count(predicado);
        return Task.FromResult(result);
    }

    public Task<T?> PrimeroODefaultAsync(Func<T, bool> predicado, CancellationToken cancellationToken = default)
    {
        var result = _dbSet.FirstOrDefault(predicado);
        return Task.FromResult(result);
    }

    public async Task<IEnumerable<T>> ObtenerPorSpecAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(specification.ToExpression()).ToListAsync(cancellationToken);
    }

    public async Task<int> ContarPorSpecAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(specification.ToExpression(), cancellationToken);
    }

    public async Task<T?> PrimeroODefaultPorSpecAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(specification.ToExpression(), cancellationToken);
    }

    /// <summary>
    /// Recarga una entidad desde la base de datos para evitar problemas de concurrencia
    /// </summary>
    public virtual async Task RecargarEntidadAsync(T entity, CancellationToken cancellationToken = default)
    {
        var entry = _dbContext.Entry(entity);
        if (entry.State != EntityState.Detached)
        {
            await entry.ReloadAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Marca una entidad como modificada sin guardar cambios
    /// </summary>
    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }
} 