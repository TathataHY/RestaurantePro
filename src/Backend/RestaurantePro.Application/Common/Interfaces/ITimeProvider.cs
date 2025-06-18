namespace RestaurantePro.Application.Common.Interfaces;

public interface ITimeProvider
{
    long GetTimestamp();
    TimeSpan GetElapsedTime(long startingTimestamp);
} 