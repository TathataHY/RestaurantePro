namespace RestaurantePro.Application.Common.Exceptions;

/// <summary>
/// Excepción lanzada cuando hay un conflicto con el estado actual del sistema
/// Por ejemplo: intentar eliminar un producto que tiene comandas asociadas
/// </summary>
public class ConflictException : Exception
{
    /// <summary>
    /// Tipo de conflicto específico
    /// </summary>
    public ConflictType ConflictType { get; }

    /// <summary>
    /// Entidad que está en conflicto
    /// </summary>
    public string? EntityName { get; }

    /// <summary>
    /// ID de la entidad en conflicto
    /// </summary>
    public object? EntityId { get; }

    /// <summary>
    /// Detalles adicionales sobre el conflicto
    /// </summary>
    public object? ConflictDetails { get; }

    public ConflictException() 
        : base("Existe un conflicto con el estado actual del sistema")
    {
        ConflictType = ConflictType.StateConflict;
    }

    public ConflictException(string message) 
        : base(message)
    {
        ConflictType = ConflictType.StateConflict;
    }

    public ConflictException(string entity, string reason)
        : base($"No se puede procesar {entity}: {reason}")
    {
        EntityName = entity;
        ConflictType = ConflictType.StateConflict;
    }

    public ConflictException(string message, Exception innerException) 
        : base(message, innerException)
    {
        ConflictType = ConflictType.StateConflict;
    }

    public ConflictException(string message, ConflictType conflictType, string? entityName = null, object? entityId = null, object? details = null)
        : base(message)
    {
        ConflictType = conflictType;
        EntityName = entityName;
        EntityId = entityId;
        ConflictDetails = details;
    }

    /// <summary>
    /// Crea una ConflictException para conflictos de concurrencia
    /// </summary>
    public static ConflictException ForConcurrency(string entityName, object entityId, string operation)
    {
        return new ConflictException(
            $"Conflicto de concurrencia: La entidad {entityName} con ID {entityId} fue modificada por otro proceso durante la operación '{operation}'.",
            ConflictType.ConcurrencyConflict,
            entityName,
            entityId,
            new { Operation = operation, Timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Crea una ConflictException para entidades duplicadas
    /// </summary>
    public static ConflictException ForDuplicate(string entityName, string field, object value)
    {
        return new ConflictException(
            $"Ya existe un {entityName} con {field}: {value}",
            ConflictType.DuplicateEntity,
            entityName,
            value,
            new { Field = field, Value = value });
    }

    /// <summary>
    /// Crea una ConflictException para dependencias
    /// </summary>
    public static ConflictException ForDependency(string entityName, object entityId, string dependentEntity, int dependentCount)
    {
        return new ConflictException(
            $"No se puede eliminar {entityName} con ID {entityId} porque tiene {dependentCount} {dependentEntity}(s) asociado(s).",
            ConflictType.DependencyConflict,
            entityName,
            entityId,
            new { DependentEntity = dependentEntity, DependentCount = dependentCount });
    }

    /// <summary>
    /// Crea una ConflictException para estado inválido
    /// </summary>
    public static ConflictException ForInvalidState(string entityName, object entityId, string currentState, string requiredState, string operation)
    {
        return new ConflictException(
            $"No se puede realizar la operación '{operation}' en {entityName} con ID {entityId}. Estado actual: {currentState}, estado requerido: {requiredState}.",
            ConflictType.StateConflict,
            entityName,
            entityId,
            new { CurrentState = currentState, RequiredState = requiredState, Operation = operation });
    }

    /// <summary>
    /// Crea una ConflictException para recursos agotados
    /// </summary>
    public static ConflictException ForResourceExhausted(string resourceName, string operation, object? details = null)
    {
        return new ConflictException(
            $"Recurso '{resourceName}' agotado durante la operación '{operation}'.",
            ConflictType.ResourceConflict,
            resourceName,
            null,
            details ?? new { Operation = operation, Timestamp = DateTime.UtcNow });
    }
}

/// <summary>
/// Tipos específicos de conflicto para categorización
/// </summary>
public enum ConflictType
{
    /// <summary>
    /// Conflicto de estado general
    /// </summary>
    StateConflict,

    /// <summary>
    /// Conflicto de concurrencia (optimistic locking)
    /// </summary>
    ConcurrencyConflict,

    /// <summary>
    /// Entidad duplicada
    /// </summary>
    DuplicateEntity,

    /// <summary>
    /// Conflicto de dependencias (foreign key)
    /// </summary>
    DependencyConflict,

    /// <summary>
    /// Conflicto de recursos (stock, mesas, etc.)
    /// </summary>
    ResourceConflict,

    /// <summary>
    /// Conflicto de reglas de negocio
    /// </summary>
    BusinessRuleConflict
} 