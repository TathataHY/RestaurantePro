namespace RestaurantePro.Mobile.Core.Services.Authorization;

/// <summary>
/// Representa el resultado de una validación de autorización
/// </summary>
public class AuthorizationResult
{
    /// <summary>
    /// Indica si la autorización fue exitosa
    /// </summary>
    public bool IsAllowed { get; private set; }
    
    /// <summary>
    /// Mensaje de error en caso de autorización denegada
    /// </summary>
    public string? ErrorMessage { get; private set; }
    
    /// <summary>
    /// Indica si la autorización fue denegada
    /// </summary>
    public bool IsDenied => !IsAllowed;

    private AuthorizationResult(bool isAllowed, string? errorMessage = null)
    {
        IsAllowed = isAllowed;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Crea un resultado de autorización exitosa
    /// </summary>
    /// <returns>Resultado exitoso</returns>
    public static AuthorizationResult Allowed()
    {
        return new AuthorizationResult(true);
    }

    /// <summary>
    /// Crea un resultado de autorización denegada
    /// </summary>
    /// <param name="errorMessage">Mensaje de error</param>
    /// <returns>Resultado denegado</returns>
    public static AuthorizationResult Denied(string errorMessage)
    {
        return new AuthorizationResult(false, errorMessage);
    }

    /// <summary>
    /// Conversión implícita a bool para facilitar el uso
    /// </summary>
    /// <param name="result">Resultado de autorización</param>
    public static implicit operator bool(AuthorizationResult result)
    {
        return result.IsAllowed;
    }
}
