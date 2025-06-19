using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Base
{
    public interface IRecurringJob
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
} 