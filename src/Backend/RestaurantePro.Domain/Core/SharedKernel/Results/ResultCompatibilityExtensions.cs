namespace RestaurantePro.Domain.Core.SharedKernel.Results;

/// <summary>
/// Extensiones de compatibilidad para Result - provee propiedades que usan los tests
/// </summary>
public static class ResultCompatibilityExtensions
{
    /// <summary>
    /// Propiedad de compatibilidad - sinónimo de Succeeded
    /// </summary>
    public static bool IsSuccess(this Result result) => result.Succeeded;

    /// <summary>
    /// Propiedad de compatibilidad - sinónimo de Error
    /// </summary>
    public static string? ErrorMessage(this Result result) => result.Error;

    /// <summary>
    /// Propiedad de compatibilidad para Result<T> - sinónimo de Succeeded
    /// </summary>
    public static bool IsSuccess<T>(this Result<T> result) => result.Succeeded;

    /// <summary>
    /// Propiedad de compatibilidad para Result<T> - sinónimo de Error
    /// </summary>
    public static string? ErrorMessage<T>(this Result<T> result) => result.Error;
} 