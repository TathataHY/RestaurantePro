namespace RestaurantePro.Domain.Core.SharedKernel.Factories;

/// <summary>
/// Interfaz base para factories de entidades en el dominio.
/// Define el contrato básico que deben implementar todos los factories.
/// </summary>
/// <typeparam name="TEntity">Tipo de entidad que crea el factory</typeparam>
/// <typeparam name="TId">Tipo del identificador de la entidad</typeparam>
public interface IEntityFactory<TEntity, in TId> 
    where TEntity : class
    where TId : notnull
{
    /// <summary>
    /// Crea una nueva entidad con parámetros específicos del factory
    /// </summary>
    /// <param name="parameters">Parámetros necesarios para crear la entidad</param>
    /// <returns>Resultado que contiene la entidad creada o errores de validación</returns>
    Result<TEntity> Crear(object parameters);

    /// <summary>
    /// Reconstruye una entidad desde datos persistidos (ej: desde base de datos)
    /// </summary>
    /// <param name="id">Identificador de la entidad</param>
    /// <param name="data">Datos para reconstruir la entidad</param>
    /// <returns>Resultado que contiene la entidad reconstruida o errores</returns>
    Result<TEntity> Reconstruir(TId id, object data);

    /// <summary>
    /// Valida que los parámetros proporcionados son válidos para crear una entidad
    /// </summary>
    /// <param name="parameters">Parámetros a validar</param>
    /// <returns>Resultado de la validación</returns>
    Result ValidarParametros(object parameters);
} 