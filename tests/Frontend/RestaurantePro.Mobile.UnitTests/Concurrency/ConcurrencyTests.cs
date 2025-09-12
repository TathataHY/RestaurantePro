using FluentAssertions;
using System.Collections.Concurrent;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Concurrency;

/// <summary>
/// Pruebas unitarias para manejo de concurrencia
/// </summary>
public class ConcurrencyTests
{
    #region Thread Safety Tests

    [Fact]
    public async Task ConcurrentAccess_ToSharedResource_ShouldBeThreadSafe()
    {
        // Arrange
        var sharedCounter = new SharedCounter();
        var tasks = new List<Task>();
        var iterations = 1000;
        var threadCount = 10;

        // Act
        for (int i = 0; i < threadCount; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                for (int j = 0; j < iterations; j++)
                {
                    await sharedCounter.IncrementAsync();
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        sharedCounter.Value.Should().Be(iterations * threadCount);
    }

    [Fact]
    public async Task ConcurrentAccess_ToDictionary_ShouldBeThreadSafe()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        var tasks = new List<Task>();
        var keyCount = 100;
        var threadCount = 10;

        // Act
        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            tasks.Add(Task.Run(async () =>
            {
                for (int j = 0; j < keyCount; j++)
                {
                    var key = $"key_{threadId}_{j}";
                    dictionary.TryAdd(key, threadId);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        dictionary.Count.Should().Be(keyCount * threadCount);
    }

    #endregion

    #region Async Lock Tests

    [Fact]
    public async Task AsyncLock_ShouldPreventConcurrentAccess()
    {
        // Arrange
        var asyncLock = new AsyncLock();
        var sharedResource = 0;
        var tasks = new List<Task>();
        var iterations = 100;

        // Act
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                for (int j = 0; j < iterations; j++)
                {
                    using (await asyncLock.LockAsync())
                    {
                        var current = sharedResource;
                        await Task.Delay(1); // Simular trabajo
                        sharedResource = current + 1;
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        sharedResource.Should().Be(5 * iterations);
    }

    [Fact]
    public async Task AsyncLock_WithTimeout_ShouldThrowWhenTimeout()
    {
        // Arrange
        var asyncLock = new AsyncLock();
        var timeout = TimeSpan.FromMilliseconds(100);

        // Act
        using (await asyncLock.LockAsync())
        {
            var task = Task.Run(async () =>
            {
                using (await asyncLock.LockAsync(timeout))
                {
                    // This should not execute
                }
            });

            // Assert
            await Assert.ThrowsAsync<TimeoutException>(() => task);
        }
    }

    #endregion

    #region Semaphore Tests

    [Fact]
    public async Task Semaphore_ShouldLimitConcurrentAccess()
    {
        // Arrange
        var semaphore = new SemaphoreSlim(2, 2); // Máximo 2 concurrentes
        var concurrentCount = 0;
        var maxConcurrent = 0;
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync();
                try
                {
                    Interlocked.Increment(ref concurrentCount);
                    maxConcurrent = Math.Max(maxConcurrent, concurrentCount);
                    await Task.Delay(100); // Simular trabajo
                }
                finally
                {
                    Interlocked.Decrement(ref concurrentCount);
                    semaphore.Release();
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        maxConcurrent.Should().BeLessOrEqualTo(2);
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task CancellationToken_ShouldCancelOperation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var operation = new LongRunningOperation();

        // Act
        var task = operation.ExecuteAsync(cts.Token);
        cts.CancelAfter(100);

        // Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => task);
    }

    [Fact]
    public async Task CancellationToken_WithMultipleOperations_ShouldCancelAll()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var operations = new List<LongRunningOperation>();
        var tasks = new List<Task>();

        for (int i = 0; i < 5; i++)
        {
            operations.Add(new LongRunningOperation());
        }

        // Act
        foreach (var operation in operations)
        {
            tasks.Add(operation.ExecuteAsync(cts.Token));
        }

        cts.CancelAfter(100);

        // Assert
        var exceptions = await Assert.ThrowsAsync<AggregateException>(() => Task.WhenAll(tasks));
        exceptions.InnerExceptions.Should().AllBeOfType<OperationCanceledException>();
    }

    #endregion

    #region Producer Consumer Tests

    [Fact]
    public async Task ProducerConsumer_ShouldProcessAllItems()
    {
        // Arrange
        var queue = new BlockingCollection<int>();
        var producer = new Producer(queue);
        var consumer = new Consumer(queue);
        var itemCount = 1000;

        // Act
        var producerTask = Task.Run(() => producer.ProduceAsync(itemCount));
        var consumerTask = Task.Run(() => consumer.ConsumeAsync());

        await producerTask;
        queue.CompleteAdding();
        await consumerTask;

        // Assert
        consumer.ProcessedItems.Should().Be(itemCount);
    }

    #endregion

    #region Race Condition Tests

    [Fact]
    public async Task RaceCondition_WithProperSynchronization_ShouldBePrevented()
    {
        // Arrange
        var bankAccount = new BankAccount();
        var tasks = new List<Task>();
        var transactionCount = 1000;

        // Act
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                for (int j = 0; j < transactionCount; j++)
                {
                    await bankAccount.DepositAsync(1);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        bankAccount.Balance.Should().Be(10 * transactionCount);
    }

    #endregion

    #region Deadlock Prevention Tests

    [Fact]
    public async Task DeadlockPrevention_WithOrderedLocks_ShouldNotDeadlock()
    {
        // Arrange
        var lock1 = new AsyncLock();
        var lock2 = new AsyncLock();
        var tasks = new List<Task>();

        // Act
        tasks.Add(Task.Run(async () =>
        {
            using (await lock1.LockAsync())
            {
                await Task.Delay(10);
                using (await lock2.LockAsync())
                {
                    // Critical section
                }
            }
        }));

        tasks.Add(Task.Run(async () =>
        {
            using (await lock1.LockAsync()) // Same order as above
            {
                await Task.Delay(10);
                using (await lock2.LockAsync())
                {
                    // Critical section
                }
            }
        }));

        // Assert - Should complete without deadlock
        await Task.WhenAll(tasks);
    }

    #endregion
}

#region Helper Classes

/// <summary>
/// Contador compartido thread-safe
/// </summary>
public class SharedCounter
{
    private int _value;
    private readonly object _lock = new();

    public int Value => _value;

    public async Task IncrementAsync()
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                _value++;
            }
        });
    }
}

/// <summary>
/// Lock asíncrono simple
/// </summary>
public class AsyncLock
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<IDisposable> LockAsync(TimeSpan? timeout = null)
    {
        if (timeout.HasValue)
        {
            if (!await _semaphore.WaitAsync(timeout.Value))
                throw new TimeoutException("Lock acquisition timeout");
        }
        else
        {
            await _semaphore.WaitAsync();
        }

        return new LockReleaser(_semaphore);
    }

    private class LockReleaser : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        public LockReleaser(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public void Dispose()
        {
            _semaphore.Release();
        }
    }
}

/// <summary>
/// Operación de larga duración
/// </summary>
public class LongRunningOperation
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        for (int i = 0; i < 1000; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(10, cancellationToken);
        }
    }
}

/// <summary>
/// Productor de elementos
/// </summary>
public class Producer
{
    private readonly BlockingCollection<int> _queue;

    public Producer(BlockingCollection<int> queue)
    {
        _queue = queue;
    }

    public async Task ProduceAsync(int itemCount)
    {
        for (int i = 0; i < itemCount; i++)
        {
            _queue.Add(i);
            await Task.Delay(1); // Simular trabajo
        }
    }
}

/// <summary>
/// Consumidor de elementos
/// </summary>
public class Consumer
{
    private readonly BlockingCollection<int> _queue;
    private int _processedItems;

    public Consumer(BlockingCollection<int> queue)
    {
        _queue = queue;
    }

    public int ProcessedItems => _processedItems;

    public async Task ConsumeAsync()
    {
        foreach (var item in _queue.GetConsumingEnumerable())
        {
            await Task.Delay(1); // Simular procesamiento
            Interlocked.Increment(ref _processedItems);
        }
    }
}

/// <summary>
/// Cuenta bancaria thread-safe
/// </summary>
public class BankAccount
{
    private decimal _balance;
    private readonly object _lock = new();

    public decimal Balance => _balance;

    public async Task DepositAsync(decimal amount)
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                _balance += amount;
            }
        });
    }

    public async Task WithdrawAsync(decimal amount)
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                if (_balance >= amount)
                {
                    _balance -= amount;
                }
                else
                {
                    throw new InvalidOperationException("Insufficient funds");
                }
            }
        });
    }
}

#endregion
