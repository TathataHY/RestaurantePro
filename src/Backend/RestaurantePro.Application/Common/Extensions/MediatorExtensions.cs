using MediatR;

namespace RestaurantePro.Application.Common.Extensions;

/// <summary>
/// Extensiones útiles para MediatR
/// </summary>
public static class MediatorExtensions
{
    /// <summary>
    /// Envía un comando y devuelve el resultado, manejando automáticamente las excepciones
    /// </summary>
    public static async Task<TResponse> SendSafeAsync<TResponse>(
        this IMediator mediator, 
        IRequest<TResponse> request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await mediator.Send(request, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log de la excepción si es necesario
            throw new ApplicationException($"Error ejecutando {typeof(TResponse).Name}", ex);
        }
    }

    /// <summary>
    /// Envía múltiples comandos en paralelo
    /// </summary>
    public static async Task<TResponse[]> SendManyAsync<TResponse>(
        this IMediator mediator,
        IEnumerable<IRequest<TResponse>> requests,
        CancellationToken cancellationToken = default)
    {
        var tasks = requests.Select(request => mediator.Send(request, cancellationToken));
        return await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Ejecuta un comando con tiempo límite
    /// </summary>
    public static async Task<TResponse> SendWithTimeoutAsync<TResponse>(
        this IMediator mediator,
        IRequest<TResponse> request,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, 
            timeoutCts.Token);

        try
        {
            return await mediator.Send(request, combinedCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
        {
            throw new TimeoutException($"La operación {typeof(TResponse).Name} excedió el tiempo límite de {timeout.TotalSeconds} segundos");
        }
    }
} 