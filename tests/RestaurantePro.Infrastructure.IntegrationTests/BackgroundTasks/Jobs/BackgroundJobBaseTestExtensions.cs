using System.Reflection;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Base;

namespace RestaurantePro.Infrastructure.IntegrationTests.BackgroundTasks.Jobs;

public static class BackgroundJobBaseTestExtensions
{
    public static Task DoWork(this BackgroundJobBase job, CancellationToken token)
    {
        var method = job.GetType().GetMethod("ExecuteInternalAsync", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (method != null)
        {
            var task = (Task)method.Invoke(job, new object[] { token });
            return task ?? Task.CompletedTask;
        }

        throw new InvalidOperationException($"Could not find method ExecuteInternalAsync on type {job.GetType().Name}");
    }
} 