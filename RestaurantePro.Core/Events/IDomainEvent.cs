namespace RestaurantePro.Core.Events
{
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }
    }
}