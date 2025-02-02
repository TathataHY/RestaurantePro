namespace RestaurantePro.Core.Mediator
{
    public interface IMediator
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request);
        Task Publish<TNotification>(TNotification notification) where TNotification : INotification;
    }

    public interface IRequest<out TResponse> { }
    public interface INotification { }
}