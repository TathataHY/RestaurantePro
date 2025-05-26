namespace RestaurantePro.Domain.Core.SharedKernel.Results;

/// <summary>
/// Representa el resultado de una operación de dominio
/// </summary>
public class Result
{
    /// <summary>
    /// Indica si la operación fue exitosa
    /// </summary>
    public bool Succeeded { get; }
    
    /// <summary>
    /// Mensaje de error (si la operación falló)
    /// </summary>
    public string? Error { get; }
    
    /// <summary>
    /// Lista de errores (si la operación falló)
    /// </summary>
    public List<string>? Errors { get; }
    
    /// <summary>
    /// Verifica si hay errores
    /// </summary>
    public bool HasErrors => !string.IsNullOrEmpty(Error) || (Errors != null && Errors.Count > 0);
    
    /// <summary>
    /// Constructor protegido
    /// </summary>
    protected Result(bool succeeded, string? error)
    {
        Succeeded = succeeded;
        Error = error;
    }
    
    /// <summary>
    /// Constructor protegido para múltiples errores
    /// </summary>
    protected Result(bool succeeded, List<string>? errors)
    {
        Succeeded = succeeded;
        Errors = errors ?? new List<string>();
    }
    
    /// <summary>
    /// Crear un resultado exitoso
    /// </summary>
    public static Result Success()
    {
        return new Result(true, error: null);
    }
    
    /// <summary>
    /// Crear un resultado exitoso con un valor
    /// </summary>
    public static Result<T> Success<T>(T value)
    {
        return new Result<T>(value, true, error: null);
    }
    
    /// <summary>
    /// Crear un resultado fallido con un mensaje de error
    /// </summary>
    public static Result Failure(string error)
    {
        return new Result(false, error);
    }
    
    /// <summary>
    /// Crear un resultado fallido con un mensaje de error y un tipo de retorno
    /// </summary>
    public static Result<T> Failure<T>(string error)
    {
        return new Result<T>(default!, false, error);
    }
    
    /// <summary>
    /// Crear un resultado fallido con múltiples errores
    /// </summary>
    public static Result Failure(List<string> errors)
    {
        return new Result(false, errors);
    }
    
    /// <summary>
    /// Crear un resultado fallido con múltiples errores y un tipo de retorno
    /// </summary>
    public static Result<T> Failure<T>(List<string> errors)
    {
        return new Result<T>(default!, false, errors);
    }
}

/// <summary>
/// Representa el resultado de una operación de dominio con un valor de retorno
/// </summary>
public class Result<T> : Result
{
    /// <summary>
    /// Valor de retorno (si la operación fue exitosa)
    /// </summary>
    public T Value { get; }
    
    /// <summary>
    /// Constructor
    /// </summary>
    internal Result(T value, bool succeeded, string? error)
        : base(succeeded, error)
    {
        Value = value;
    }
    
    /// <summary>
    /// Constructor para múltiples errores
    /// </summary>
    internal Result(T value, bool succeeded, List<string>? errors)
        : base(succeeded, errors)
    {
        Value = value;
    }
} 