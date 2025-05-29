namespace RestaurantePro.Application.Common.Exceptions;

/// <summary>
/// Excepción lanzada cuando un usuario intenta acceder a un recurso sin permisos
/// </summary>
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException() 
        : base("No tienes permisos para acceder a este recurso")
    {
    }

    public ForbiddenAccessException(string message) 
        : base(message)
    {
    }

    public ForbiddenAccessException(string resource, string action)
        : base($"No tienes permisos para {action} en {resource}")
    {
    }
} 