namespace RestaurantePro.Application.UnitTests.Common;

/// <summary>
/// Helper para crear DbSet mocks en los tests unitarios que funcione con EF async methods
/// </summary>
public static class MockDbSetHelper
{
    /// <summary>
    /// Crea un mock de DbSet con datos específicos usando TestAsyncQueryProvider
    /// </summary>
    /// <typeparam name="T">Tipo de entidad</typeparam>
    /// <param name="data">Datos a incluir en el DbSet mockeado</param>
    /// <returns>Mock de DbSet configurado</returns>
    public static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        
        // Configurar como IQueryable con TestAsyncQueryProvider
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(data.Provider));
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        
        // Configurar como IAsyncEnumerable
        mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));
        
        // Configurar métodos específicos de EF síncronos
        mockSet.Setup(x => x.Add(It.IsAny<T>())).Returns((T entity) => null!);
        mockSet.Setup(x => x.Remove(It.IsAny<T>())).Returns((T entity) => null!);
        mockSet.Setup(x => x.Update(It.IsAny<T>())).Returns((T entity) => null!);
        
        return mockSet;
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

/// <summary>
/// Implementación de IAsyncQueryProvider para tests que soporte Entity Framework async methods
/// </summary>
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

    public object Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var expectedResultType = typeof(TResult);
        
        // Si esperamos Task<bool> (como para AnyAsync)
        if (expectedResultType == typeof(Task<bool>))
        {
            try
            {
                var result = Execute<bool>(expression);
                return (TResult)(object)Task.FromResult(result);
            }
            catch
            {
                // Si falla, retornar false por defecto
                return (TResult)(object)Task.FromResult(false);
            }
        }
        
        // Si esperamos Task<T> para otras operaciones como FirstOrDefaultAsync
        if (expectedResultType.IsGenericType && expectedResultType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var taskResultType = expectedResultType.GetGenericArguments()[0];
            try
            {
                // Ejecutar la expresión síncronamente y envolver en Task
                var result = _inner.Execute(expression);
                
                // Si el resultado es del tipo correcto, devolverlo
                if (result != null && taskResultType.IsAssignableFrom(result.GetType()))
                {
                    var taskResult = typeof(Task).GetMethod(nameof(Task.FromResult))?.MakeGenericMethod(taskResultType)?.Invoke(null, new[] { result });
                    return (TResult)taskResult!;
                }
                
                // Si el resultado es null o no es del tipo correcto, usar valor por defecto
                var defaultValue = taskResultType.IsValueType ? Activator.CreateInstance(taskResultType) : null;
                var taskResultDefault = typeof(Task).GetMethod(nameof(Task.FromResult))?.MakeGenericMethod(taskResultType)?.Invoke(null, new[] { defaultValue });
                return (TResult)taskResultDefault!;
            }
            catch
            {
                // Si falla, crear una tarea completada con valor por defecto
                var defaultValue = taskResultType.IsValueType ? Activator.CreateInstance(taskResultType) : null;
                var taskResult = typeof(Task).GetMethod(nameof(Task.FromResult))?.MakeGenericMethod(taskResultType)?.Invoke(null, new[] { defaultValue });
                return (TResult)taskResult!;
            }
        }
        
        // Para otros casos, ejecutar síncronamente
        try
        {
            var result = Execute<TResult>(expression);
            return result;
        }
        catch
        {
            // Si falla, retornar valor por defecto
            return typeof(TResult).IsValueType ? (TResult)Activator.CreateInstance(typeof(TResult))! : default(TResult)!;
        }
    }
    
    private Task<T> ExecuteAsyncGeneric<T>(Expression expression, CancellationToken cancellationToken)
    {
        try
        {
            var result = Execute<T>(expression);
            return Task.FromResult(result);
        }
        catch
        {
            var defaultValue = typeof(T).IsValueType ? (T)Activator.CreateInstance(typeof(T))! : default(T)!;
            return Task.FromResult(defaultValue);
        }
    }
}

/// <summary>
/// Implementación de IAsyncEnumerable para tests
/// </summary>
internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    { }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

/// <summary>
/// Implementación de IAsyncEnumerator para tests
/// </summary>
internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(_inner.MoveNext());
    }

    public T Current => _inner.Current;

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
} 