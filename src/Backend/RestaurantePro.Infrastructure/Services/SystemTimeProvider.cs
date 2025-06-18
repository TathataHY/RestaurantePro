using System.Diagnostics;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Infrastructure.Services;

public class SystemTimeProvider : ITimeProvider
{
    public long GetTimestamp() => Stopwatch.GetTimestamp();

    public TimeSpan GetElapsedTime(long startingTimestamp) => Stopwatch.GetElapsedTime(startingTimestamp);
} 