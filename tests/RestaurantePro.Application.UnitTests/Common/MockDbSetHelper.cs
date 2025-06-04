using System.Linq.Expressions;
using System.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using MockQueryable;

namespace RestaurantePro.Application.UnitTests.Common;

/// <summary>
/// Helper para crear DbSet mocks en los tests unitarios que funcione con EF async methods
/// Usa MockQueryable.Moq para manejo correcto de operaciones asíncronas
/// </summary>
public static class MockDbSetHelper
{
    /// <summary>
    /// Crea un mock de DbSet con datos específicos
    /// </summary>
    /// <typeparam name="T">Tipo de entidad</typeparam>
    /// <param name="data">Datos a incluir en el DbSet mockeado</param>
    /// <returns>Mock de DbSet configurado</returns>
    public static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
    {
        // Usar MockQueryable.Moq que maneja correctamente EF async operations
        return data.BuildMockDbSet();
    }

    /// <summary>
    /// Crea un mock de DbSet vacío
    /// </summary>
    /// <typeparam name="T">Tipo de entidad</typeparam>
    /// <returns>Mock de DbSet vacío</returns>
    public static Mock<DbSet<T>> CreateEmptyMockDbSet<T>() where T : class
    {
        return CreateMockDbSet(new List<T>().AsQueryable());
    }
}

// Clases helper para mockear Entity Framework async operations
internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object? Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        // ✅ IMPORTANTE: Verificar si el token está cancelado antes de ejecutar
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        var resultType = typeof(TResult);
        
        // Manejar Task<T>
        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var innerType = resultType.GetGenericArguments()[0];
            
            // Para FirstOrDefaultAsync<T>
            if (innerType == typeof(TEntity) || innerType.IsAssignableFrom(typeof(TEntity)))
            {
                var queryable = new TestAsyncEnumerable<TEntity>(expression);
                var result = queryable.FirstOrDefault();
                return (TResult)(object)Task.FromResult(result);
            }
            
            // Para AnyAsync - devuelve Task<bool>
            if (innerType == typeof(bool))
            {
                var queryable = new TestAsyncEnumerable<TEntity>(expression);
                var result = queryable.Any();
                return (TResult)(object)Task.FromResult(result);
            }
            
            // Para consultas que devuelven listas
            if (innerType.IsGenericType && innerType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var queryable = new TestAsyncEnumerable<TEntity>(expression);
                var result = queryable.ToList();
                return (TResult)(object)Task.FromResult(result);
            }
            
            // Fallback para otros tipos
            var syncResult = Execute(expression);
            return (TResult)(object)Task.FromResult(syncResult);
        }
        
        // Manejar ValueTask<T>
        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(ValueTask<>))
        {
            var innerType = resultType.GetGenericArguments()[0];
            var syncResult = Execute(expression);
            return (TResult)(object)ValueTask.FromResult(syncResult);
        }
        
        // Para otros tipos, ejecutar síncronamente
        return Execute<TResult>(expression);
    }
}

internal class TestAsyncEnumerable<T> : IAsyncEnumerable<T>, IQueryable<T>
{
    private readonly IQueryable<T> _queryable;

    public TestAsyncEnumerable(IEnumerable<T> enumerable)
    {
        _queryable = enumerable.AsQueryable();
    }

    public TestAsyncEnumerable(Expression expression)
    {
        _queryable = new EnumerableQuery<T>(expression);
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        // ✅ IMPORTANTE: Verificar si el token está cancelado antes de crear el enumerador
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(cancellationToken);
        }
        
        return new TestAsyncEnumerator<T>(_queryable.GetEnumerator(), cancellationToken);
    }

    public Type ElementType => _queryable.ElementType;
    public Expression Expression => _queryable.Expression;
    public IQueryProvider Provider => new TestAsyncQueryProvider<T>(_queryable.Provider);

    public IEnumerator<T> GetEnumerator()
    {
        return _queryable.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _queryable.GetEnumerator();
    }
}

/// <summary>
/// Implementación de IAsyncEnumerator para tests
/// </summary>
internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;
    private readonly CancellationToken _cancellationToken;

    public TestAsyncEnumerator(IEnumerator<T> inner, CancellationToken cancellationToken = default)
    {
        _inner = inner;
        _cancellationToken = cancellationToken;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        // ✅ IMPORTANTE: Verificar si el token está cancelado antes de cada movimiento
        if (_cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(_cancellationToken);
        }
        
        return ValueTask.FromResult(_inner.MoveNext());
    }

    public T Current => _inner.Current;

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
} 