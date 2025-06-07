namespace RestaurantePro.Domain.Core.SharedKernel.Results;

/// <summary>
/// Extensiones adicionales para Result - agrega IsFailure y otras propiedades útiles
/// </summary>
public static class ResultFailureExtensions
{
    /// <summary>
    /// Verifica si el resultado ha fallado
    /// </summary>
    public static bool IsFailure(this Result result) => !result.Succeeded;
    
    /// <summary>
    /// Verifica si el resultado ha fallado (versión genérica)
    /// </summary>
    public static bool IsFailure<T>(this Result<T> result) => !result.Succeeded;
    
    /// <summary>
    /// Convierte un Result a Result<T>
    /// </summary>
    public static Result<T> ToResult<T>(this Result result, T defaultValue = default!)
    {
        if (result.Succeeded)
        {
            return Result<T>.Success(defaultValue);
        }
        else
        {
            return Result<T>.Failure(result.Error ?? "Error desconocido");
        }
    }
    
    /// <summary>
    /// Convierte un Result<T> a Result
    /// </summary>
    public static Result ToResult<T>(this Result<T> result)
    {
        if (result.Succeeded)
        {
            return Result.Success();
        }
        else
        {
            return Result.Failure(result.Error ?? "Error desconocido");
        }
    }
} 