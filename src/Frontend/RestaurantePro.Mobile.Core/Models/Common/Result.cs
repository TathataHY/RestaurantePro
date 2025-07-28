namespace RestaurantePro.Mobile.Core.Models.Common;

/// <summary>
/// Resultado de una operación que puede ser exitosa o fallida
/// </summary>
/// <typeparam name="T">Tipo de datos del resultado</typeparam>
public class Result<T>
{
    /// <summary>
    /// Indica si la operación fue exitosa
    /// </summary>
    public bool Succeeded { get; }

    /// <summary>
    /// Datos del resultado (solo si Succeeded es true)
    /// </summary>
    public T? Data { get; }

    /// <summary>
    /// Mensaje de error (solo si Succeeded es false)
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Lista de errores (solo si Succeeded es false)
    /// </summary>
    public List<string>? Errors { get; }

    /// <summary>
    /// Constructor privado para crear resultados
    /// </summary>
    private Result(bool succeeded, T? data = default, string? error = null, List<string>? errors = null)
    {
        Succeeded = succeeded;
        Data = data;
        Error = error;
        Errors = errors;
    }

    /// <summary>
    /// Crea un resultado exitoso
    /// </summary>
    /// <param name="data">Datos del resultado</param>
    /// <returns>Resultado exitoso</returns>
    public static Result<T> Success(T data)
    {
        return new Result<T>(true, data);
    }

    /// <summary>
    /// Crea un resultado fallido
    /// </summary>
    /// <param name="error">Mensaje de error</param>
    /// <returns>Resultado fallido</returns>
    public static Result<T> Failure(string error)
    {
        return new Result<T>(false, error: error);
    }

    /// <summary>
    /// Crea un resultado fallido con múltiples errores
    /// </summary>
    /// <param name="errors">Lista de errores</param>
    /// <returns>Resultado fallido</returns>
    public static Result<T> Failure(List<string> errors)
    {
        return new Result<T>(false, errors: errors);
    }

    /// <summary>
    /// Crea un resultado fallido con un error y múltiples errores
    /// </summary>
    /// <param name="error">Mensaje de error principal</param>
    /// <param name="errors">Lista de errores adicionales</param>
    /// <returns>Resultado fallido</returns>
    public static Result<T> Failure(string error, List<string> errors)
    {
        return new Result<T>(false, error: error, errors: errors);
    }

    /// <summary>
    /// Operador de conversión implícita de T a Result<T>
    /// </summary>
    /// <param name="data">Datos a convertir</param>
    public static implicit operator Result<T>(T data)
    {
        return Success(data);
    }

    /// <summary>
    /// Operador de conversión implícita de string a Result<T> (fallido)
    /// </summary>
    /// <param name="error">Error a convertir</param>
    public static implicit operator Result<T>(string error)
    {
        return Failure(error);
    }
}

/// <summary>
/// Resultado de una operación sin datos de retorno
/// </summary>
public class Result
{
    /// <summary>
    /// Indica si la operación fue exitosa
    /// </summary>
    public bool Succeeded { get; }

    /// <summary>
    /// Mensaje de error (solo si Succeeded es false)
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Lista de errores (solo si Succeeded es false)
    /// </summary>
    public List<string>? Errors { get; }

    /// <summary>
    /// Constructor privado para crear resultados
    /// </summary>
    private Result(bool succeeded, string? error = null, List<string>? errors = null)
    {
        Succeeded = succeeded;
        Error = error;
        Errors = errors;
    }

    /// <summary>
    /// Crea un resultado exitoso
    /// </summary>
    /// <returns>Resultado exitoso</returns>
    public static Result Success()
    {
        return new Result(true);
    }

    /// <summary>
    /// Crea un resultado fallido
    /// </summary>
    /// <param name="error">Mensaje de error</param>
    /// <returns>Resultado fallido</returns>
    public static Result Failure(string error)
    {
        return new Result(false, error: error);
    }

    /// <summary>
    /// Crea un resultado fallido con múltiples errores
    /// </summary>
    /// <param name="errors">Lista de errores</param>
    /// <returns>Resultado fallido</returns>
    public static Result Failure(List<string> errors)
    {
        return new Result(false, errors: errors);
    }

    /// <summary>
    /// Crea un resultado fallido con un error y múltiples errores
    /// </summary>
    /// <param name="error">Mensaje de error principal</param>
    /// <param name="errors">Lista de errores adicionales</param>
    /// <returns>Resultado fallido</returns>
    public static Result Failure(string error, List<string> errors)
    {
        return new Result(false, error: error, errors: errors);
    }

    /// <summary>
    /// Operador de conversión implícita de string a Result (fallido)
    /// </summary>
    /// <param name="error">Error a convertir</param>
    public static implicit operator Result(string error)
    {
        return Failure(error);
    }
} 