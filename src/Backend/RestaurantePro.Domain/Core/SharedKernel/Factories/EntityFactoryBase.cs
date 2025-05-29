namespace RestaurantePro.Domain.Core.SharedKernel.Factories;

/// <summary>
/// Clase base abstracta para factories de entidades que proporciona funcionalidad común.
/// </summary>
/// <typeparam name="TEntity">Tipo de entidad que crea el factory</typeparam>
/// <typeparam name="TId">Tipo del identificador de la entidad</typeparam>
public abstract class EntityFactoryBase<TEntity, TId> : IEntityFactory<TEntity, TId>
    where TEntity : class
    where TId : notnull
{
    /// <summary>
    /// Notification manager para agregar validaciones y errores
    /// </summary>
    protected readonly INotificationManager NotificationManager;

    /// <summary>
    /// Logger para registrar eventos del factory
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// Constructor base del factory
    /// </summary>
    /// <param name="notificationManager">Manager de notificaciones</param>
    /// <param name="logger">Logger para eventos</param>
    protected EntityFactoryBase(INotificationManager notificationManager, ILogger logger)
    {
        Guard.AgainstNull(notificationManager, nameof(notificationManager));
        Guard.AgainstNull(logger, nameof(logger));
        
        NotificationManager = notificationManager;
        Logger = logger;
    }

    /// <summary>
    /// Crea una nueva entidad validando primero los parámetros
    /// </summary>
    /// <param name="parameters">Parámetros para crear la entidad</param>
    /// <returns>Resultado con la entidad creada o errores</returns>
    public virtual Result<TEntity> Crear(object parameters)
    {
        try
        {
            // Validar parámetros primero
            var validationResult = ValidarParametros(parameters);
            if (!validationResult.Succeeded)
            {
                return Result.Failure<TEntity>(validationResult.Errors);
            }

            // Crear la entidad
            var entidad = CrearEntidadInterno(parameters);
            if (entidad == null)
            {
                var error = $"Error interno: no se pudo crear la entidad {typeof(TEntity).Name}";
                Logger?.LogError(error);
                return Result.Failure<TEntity>(error);
            }

            // Log de éxito
            Logger?.LogInformation("Entidad {EntityType} creada exitosamente", typeof(TEntity).Name);
            return Result.Success(entidad);
        }
        catch (DomainException ex)
        {
            Logger?.LogWarning(ex, "Error de dominio al crear {EntityType}: {Message}", typeof(TEntity).Name, ex.Message);
            return ex.ToResult<TEntity>();
        }
        catch (Exception ex)
        {
            var message = $"Error inesperado al crear {typeof(TEntity).Name}: {ex.Message}";
            Logger?.LogError(ex, message);
            return Result.Failure<TEntity>(message);
        }
    }

    /// <summary>
    /// Reconstruye una entidad desde datos persistidos
    /// </summary>
    /// <param name="id">Identificador de la entidad</param>
    /// <param name="data">Datos para reconstruir</param>
    /// <returns>Resultado con la entidad reconstruida o errores</returns>
    public virtual Result<TEntity> Reconstruir(TId id, object data)
    {
        try
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var entidad = ReconstruirEntidadInterno(id, data);
            if (entidad == null)
            {
                var error = $"Error interno: no se pudo reconstruir la entidad {typeof(TEntity).Name} con ID {id}";
                Logger?.LogError(error);
                return Result.Failure<TEntity>(error);
            }

            Logger?.LogDebug("Entidad {EntityType} con ID {EntityId} reconstruida exitosamente", typeof(TEntity).Name, id);
            return Result.Success(entidad);
        }
        catch (DomainException ex)
        {
            Logger?.LogWarning(ex, "Error de dominio al reconstruir {EntityType} con ID {EntityId}: {Message}", 
                typeof(TEntity).Name, id, ex.Message);
            return ex.ToResult<TEntity>();
        }
        catch (Exception ex)
        {
            var message = $"Error inesperado al reconstruir {typeof(TEntity).Name} con ID {id}: {ex.Message}";
            Logger?.LogError(ex, message);
            return Result.Failure<TEntity>(message);
        }
    }

    /// <summary>
    /// Valida los parámetros de entrada
    /// </summary>
    /// <param name="parameters">Parámetros a validar</param>
    /// <returns>Resultado de la validación</returns>
    public virtual Result ValidarParametros(object parameters)
    {
        try
        {
            Guard.AgainstNull(parameters, nameof(parameters));
            
            // Limpiar notificaciones previas del contexto del factory
            NotificationManager.ClearErrors();
            
            // Validación específica del factory derivado
            ValidarParametrosEspecificos(parameters);
            
            // Retornar resultado basado en notificaciones
            if (NotificationManager.HasErrors)
            {
                var errores = NotificationManager.GetErrors()
                    .Select(n => n.Message)
                    .ToArray();
                return Result.Failure(errores.ToList());
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            var message = $"Error al validar parámetros para {typeof(TEntity).Name}: {ex.Message}";
            Logger?.LogError(ex, message);
            return Result.Failure(message);
        }
    }

    /// <summary>
    /// Método abstracto para crear la entidad específica - debe ser implementado por clases derivadas
    /// </summary>
    /// <param name="parameters">Parámetros validados</param>
    /// <returns>Entidad creada</returns>
    protected abstract TEntity? CrearEntidadInterno(object parameters);

    /// <summary>
    /// Método abstracto para reconstruir la entidad específica - debe ser implementado por clases derivadas
    /// </summary>
    /// <param name="id">ID de la entidad</param>
    /// <param name="data">Datos para reconstruir</param>
    /// <returns>Entidad reconstruida</returns>
    protected abstract TEntity? ReconstruirEntidadInterno(TId id, object data);

    /// <summary>
    /// Método virtual para validaciones específicas del factory derivado
    /// </summary>
    /// <param name="parameters">Parámetros a validar</param>
    protected virtual void ValidarParametrosEspecificos(object parameters)
    {
        // Implementación por defecto vacía - las clases derivadas pueden sobrescribir
    }

    /// <summary>
    /// Método de ayuda para agregar error de validación
    /// </summary>
    /// <param name="campo">Campo que falló la validación</param>
    /// <param name="mensaje">Mensaje de error</param>
    protected void AgregarError(string campo, string mensaje)
    {
        NotificationManager.AddError(mensaje, null, campo);
    }

    /// <summary>
    /// Método de ayuda para validar que una propiedad de string no sea nula o vacía
    /// </summary>
    /// <param name="valor">Valor a validar</param>
    /// <param name="nombreCampo">Nombre del campo</param>
    /// <param name="maxLength">Longitud máxima opcional</param>
    protected void ValidarStringRequerido(string? valor, string nombreCampo, int? maxLength = null)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            AgregarError(nombreCampo, $"{nombreCampo} es requerido");
            return;
        }

        if (maxLength.HasValue && valor.Length > maxLength.Value)
        {
            AgregarError(nombreCampo, $"{nombreCampo} no puede exceder {maxLength} caracteres");
        }
    }

    /// <summary>
    /// Método de ayuda para validar que un valor numérico sea positivo
    /// </summary>
    /// <param name="valor">Valor a validar</param>
    /// <param name="nombreCampo">Nombre del campo</param>
    protected void ValidarPositivo(decimal valor, string nombreCampo)
    {
        if (valor <= 0)
        {
            AgregarError(nombreCampo, $"{nombreCampo} debe ser mayor que cero");
        }
    }

    /// <summary>
    /// Método de ayuda para validar que un valor no sea negativo
    /// </summary>
    /// <param name="valor">Valor a validar</param>
    /// <param name="nombreCampo">Nombre del campo</param>
    protected void ValidarNoNegativo(decimal valor, string nombreCampo)
    {
        if (valor < 0)
        {
            AgregarError(nombreCampo, $"{nombreCampo} no puede ser negativo");
        }
    }

    /// <summary>
    /// Método de ayuda para validar que un Guid no sea empty
    /// </summary>
    /// <param name="valor">Valor a validar</param>
    /// <param name="nombreCampo">Nombre del campo</param>
    protected void ValidarGuidRequerido(Guid valor, string nombreCampo)
    {
        if (valor == Guid.Empty)
        {
            AgregarError(nombreCampo, $"{nombreCampo} es requerido");
        }
    }
} 