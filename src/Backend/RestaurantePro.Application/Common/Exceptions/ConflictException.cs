namespace RestaurantePro.Application.Common.Exceptions;

/// <summary>
/// Excepción lanzada cuando hay un conflicto con el estado actual del sistema
/// Por ejemplo: intentar eliminar un producto que tiene comandas asociadas
/// </summary>
public class ConflictException : Exception
{
    public ConflictException() 
        : base("Existe un conflicto con el estado actual del sistema")
    {
    }

    public ConflictException(string message) 
        : base(message)
    {
    }

    public ConflictException(string entity, string reason)
        : base($"No se puede procesar {entity}: {reason}")
    {
    }

    public ConflictException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
} 