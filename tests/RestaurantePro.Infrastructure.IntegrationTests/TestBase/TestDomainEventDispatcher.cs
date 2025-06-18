using RestaurantePro.Domain.Core.Base.Events;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.IntegrationTests.TestBase
{
    public class TestDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task Dispatch(DomainEvent evento, CancellationToken cancellationToken = default)
        {
            // No-op for testing
            return Task.CompletedTask;
        }

        public Task DispatchAll(IEnumerable<DomainEvent> eventos, CancellationToken cancellationToken = default)
        {
            // No-op for testing
            return Task.CompletedTask;
        }
    }
} 