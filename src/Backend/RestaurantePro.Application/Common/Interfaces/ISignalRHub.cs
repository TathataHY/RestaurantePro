namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Interfaz para el hub de SignalR
/// Permite que Infrastructure use el hub sin referenciar directamente a la API
/// </summary>
public interface ISignalRHub
{
    /// <summary>
    /// Envía un mensaje a un grupo específico
    /// </summary>
    /// <param name="groupName">Nombre del grupo</param>
    /// <param name="method">Nombre del método a invocar</param>
    /// <param name="args">Argumentos del método</param>
    Task SendToGroupAsync(string groupName, string method, params object[] args);

    /// <summary>
    /// Envía un mensaje a todos los clientes
    /// </summary>
    /// <param name="method">Nombre del método a invocar</param>
    /// <param name="args">Argumentos del método</param>
    Task SendToAllAsync(string method, params object[] args);

    /// <summary>
    /// Envía un mensaje a un usuario específico
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="method">Nombre del método a invocar</param>
    /// <param name="args">Argumentos del método</param>
    Task SendToUserAsync(string userId, string method, params object[] args);
} 