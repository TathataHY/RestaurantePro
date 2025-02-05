using System;
using System.Threading.Tasks;

namespace RestaurantePro.Core.Interfaces.Services
{
    public interface IBackgroundJobService
    {
        string Enqueue<T>(Action<T> methodCall);
        string Schedule<T>(Action<T> methodCall, TimeSpan delay);
        Task<bool> DeleteAsync(string jobId);
    }
} 