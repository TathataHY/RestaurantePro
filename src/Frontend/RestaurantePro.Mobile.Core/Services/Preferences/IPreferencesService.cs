namespace RestaurantePro.Mobile.Core.Services.Preferences;

/// <summary>
/// Servicio para manejar preferencias y configuraciones de la aplicación
/// </summary>
public interface IPreferencesService
{
    /// <summary>
    /// Obtiene un valor de preferencia
    /// </summary>
    /// <typeparam name="T">Tipo del valor</typeparam>
    /// <param name="key">Clave de la preferencia</param>
    /// <param name="defaultValue">Valor por defecto</param>
    /// <returns>Valor de la preferencia</returns>
    T Get<T>(string key, T defaultValue = default);

    /// <summary>
    /// Establece un valor de preferencia
    /// </summary>
    /// <typeparam name="T">Tipo del valor</typeparam>
    /// <param name="key">Clave de la preferencia</param>
    /// <param name="value">Valor a establecer</param>
    Task SetAsync<T>(string key, T value);

    /// <summary>
    /// Elimina una preferencia
    /// </summary>
    /// <param name="key">Clave de la preferencia</param>
    void Remove(string key);

    /// <summary>
    /// Verifica si existe una preferencia
    /// </summary>
    /// <param name="key">Clave de la preferencia</param>
    /// <returns>True si existe</returns>
    bool ContainsKey(string key);

    /// <summary>
    /// Limpia todas las preferencias
    /// </summary>
    void Clear();
} 