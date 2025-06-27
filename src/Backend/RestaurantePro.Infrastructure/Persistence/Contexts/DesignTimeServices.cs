using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using RestaurantePro.Domain.Core.Base.Events;

namespace RestaurantePro.Infrastructure.Persistence.Contexts;

// Servicios mock para design time
public class MockLogger<T> : ILogger<T>
{
    public IDisposable BeginScope<TState>(TState state) => null!;
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}

public class MockCurrentUserService : ICurrentUserService
{
    public string? UserId => "design-time-user";
    public string? UserName => "design-time";
    public string? Email => "design-time@example.com";
    public bool IsAuthenticated => true;
    public IEnumerable<string> Roles => new List<string> { "Admin" };
    public string? Rol => "Admin";
    public bool IsInRole(string role) => role == "Admin";
}

public class MockDateTimeService : IDateTimeService
{
    public DateTime Now => DateTime.UtcNow;
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Today => DateTime.Today;
}

public class MockDomainEventDispatcher : IDomainEventDispatcher
{
    public Task Dispatch(DomainEvent evento, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task DispatchAll(IEnumerable<DomainEvent> eventos, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
} 