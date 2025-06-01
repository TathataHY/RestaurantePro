namespace RestaurantePro.Domain.Core.SharedKernel.Results;

/// <summary>
/// Extensiones para facilitar el trabajo con resultados
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Ejecuta una acción si el resultado es exitoso
    /// </summary>
    public static Result OnSuccess(this Result result, Action action)
    {
        if (result.Succeeded)
        {
            action();
        }
        
        return result;
    }
    
    /// <summary>
    /// Ejecuta una acción con el valor si el resultado es exitoso
    /// </summary>
    public static Result OnSuccess<T>(this Result<T> result, Action<T> action)
    {
        if (result.Succeeded)
        {
            action(result.Value);
        }
        
        return result;
    }
    
    /// <summary>
    /// Ejecuta una función si el resultado es exitoso y devuelve un nuevo resultado
    /// </summary>
    public static Result<TResult> OnSuccess<T, TResult>(this Result<T> result, Func<T, TResult> func)
    {
        if (result.Succeeded)
        {
            return Result.Success(func(result.Value));
        }
        
        return Result.Failure<TResult>(result.Error ?? "Error desconocido");
    }
    
    /// <summary>
    /// Ejecuta una función asíncrona si el resultado es exitoso y devuelve un nuevo resultado
    /// </summary>
    public static async Task<Result<TResult>> OnSuccessAsync<T, TResult>(this Result<T> result, Func<T, Task<TResult>> func)
    {
        if (result.Succeeded)
        {
            return Result.Success(await func(result.Value));
        }
        
        return Result.Failure<TResult>(result.Error ?? "Error desconocido");
    }
    
    /// <summary>
    /// Ejecuta una acción si el resultado es fallido
    /// </summary>
    public static Result OnFailure(this Result result, Action action)
    {
        if (!result.Succeeded)
        {
            action();
        }
        
        return result;
    }
    
    /// <summary>
    /// Ejecuta una acción con el mensaje de error si el resultado es fallido
    /// </summary>
    public static Result OnFailure(this Result result, Action<string?> action)
    {
        if (!result.Succeeded)
        {
            action(result.Error);
        }
        
        return result;
    }
    
    /// <summary>
    /// Combina múltiples resultados en uno solo
    /// </summary>
    public static Result Combine(params Result[] results)
    {
        var failedResults = results.Where(r => !r.Succeeded).ToList();
        
        if (!failedResults.Any())
            return Result.Success();
            
        var errors = failedResults
            .SelectMany(r => r.Errors ?? new List<string> { r.Error ?? "Error desconocido" })
            .Where(e => !string.IsNullOrEmpty(e))
            .ToList();
            
        return Result.Failure(errors);
    }

    /// <summary>
    /// Convierte un Result a Result<T> genérico
    /// </summary>
    public static Result<T> ToGeneric<T>(this Result result)
    {
        if (result.Succeeded)
            return Result.Success(default(T)!);
        
        return Result.Failure<T>(result.Error ?? "Error desconocido");
    }

    /// <summary>
    /// Convierte un Result<T> a Result<TTarget> con el valor proporcionado
    /// </summary>
    public static Result<TTarget> ToGeneric<T, TTarget>(this Result<T> result)
    {
        if (result.Succeeded)
            return Result.Success(default(TTarget)!);
        
        return Result.Failure<TTarget>(result.Error ?? "Error desconocido");
    }

    /// <summary>
    /// Convierte un Result<T> a Result<T> manteniendo el valor original
    /// </summary>
    public static Result<T> ToGeneric<T>(this Result<T> result)
    {
        return result;
    }
} 