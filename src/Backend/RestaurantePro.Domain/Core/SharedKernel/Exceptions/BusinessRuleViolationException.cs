using System.Text.Json.Serialization;

namespace RestaurantePro.Domain.Core.SharedKernel.Exceptions;

/// <summary>
/// Excepción que se lanza cuando se viola una regla de negocio del dominio.
/// Utilizada para errores relacionados con lógica de negocio específica.
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    /// <summary>
    /// Nombre de la regla de negocio que fue violada
    /// </summary>
    public string RuleName { get; private set; }
    
    /// <summary>
    /// Entidad o agregado donde ocurrió la violación
    /// </summary>
    public string EntityName { get; private set; }
    
    /// <summary>
    /// ID de la entidad donde ocurrió la violación (si aplica)
    /// </summary>
    public Guid? EntityId { get; private set; }

    /// <summary>
    /// Constructor para deserialización JSON.
    /// </summary>
    [JsonConstructor]
    public BusinessRuleViolationException(string message, string errorCode, string domainContext, Dictionary<string, object> additionalData, DateTime occurredAt, string ruleName, string entityName, Guid? entityId)
        : base(message, errorCode, domainContext, additionalData, occurredAt)
    {
        RuleName = ruleName;
        EntityName = entityName;
        EntityId = entityId;
    }

    /// <summary>
    /// Constructor para deserialización
    /// </summary>
    public BusinessRuleViolationException() 
        : base("Se violó una regla de negocio.", "BUSINESS_RULE_VIOLATION", "Desconocido")
    {
        RuleName = "Desconocida";
        EntityName = "Desconocida";
    }

    /// <summary>
    /// Constructor para violación de regla de negocio
    /// </summary>
    /// <param name="ruleName">Nombre de la regla violada</param>
    /// <param name="entityName">Nombre de la entidad</param>
    /// <param name="message">Mensaje descriptivo del error</param>
    /// <param name="domainContext">Contexto del dominio</param>
    /// <param name="entityId">ID de la entidad (opcional)</param>
    /// <param name="errorCode">Código de error específico (opcional)</param>
    public BusinessRuleViolationException(
        string ruleName,
        string entityName,
        string message,
        string domainContext,
        Guid? entityId = null,
        string? errorCode = null) 
        : base(message, errorCode ?? "BUSINESS_RULE_VIOLATION", domainContext, null)
    {
        RuleName = ruleName ?? throw new ArgumentNullException(nameof(ruleName));
        EntityName = entityName ?? throw new ArgumentNullException(nameof(entityName));
        EntityId = entityId;
        
        // Agregar datos contextuales
        WithData("RuleName", RuleName)
            .WithData("EntityName", EntityName);
            
        if (EntityId.HasValue)
        {
            WithData("EntityId", EntityId.Value);
        }
    }

    /// <summary>
    /// Constructor simplificado para reglas comunes
    /// </summary>
    /// <param name="ruleName">Nombre de la regla violada</param>
    /// <param name="entityName">Nombre de la entidad</param>
    /// <param name="domainContext">Contexto del dominio</param>
    /// <param name="entityId">ID de la entidad (opcional)</param>
    public BusinessRuleViolationException(
        string ruleName,
        string entityName,
        string domainContext,
        Guid? entityId = null) 
        : this(ruleName, entityName, $"Se violó la regla de negocio '{ruleName}' en {entityName}", domainContext, entityId)
    {
    }

    /// <summary>
    /// Crea una excepción para estado inválido de entidad
    /// </summary>
    /// <param name="entityName">Nombre de la entidad</param>
    /// <param name="currentState">Estado actual de la entidad</param>
    /// <param name="expectedState">Estado esperado de la entidad</param>
    /// <param name="domainContext">Contexto del dominio</param>
    /// <param name="entityId">ID de la entidad</param>
    /// <returns>Nueva instancia de BusinessRuleViolationException</returns>
    public static BusinessRuleViolationException ForInvalidState(
        string entityName,
        string currentState,
        string expectedState,
        string domainContext,
        Guid? entityId = null)
    {
        var message = $"{entityName} está en estado inválido '{currentState}' pero se esperaba '{expectedState}'";
        var excepcion = new BusinessRuleViolationException(
            "InvalidState", entityName, message, domainContext, entityId, "INVALID_STATE");
        
        excepcion.WithData("EntityType", entityName)
                 .WithData("CurrentState", currentState)
                 .WithData("ExpectedState", expectedState);
                 
        return excepcion;
    }

    /// <summary>
    /// Crea una excepción para entidad inactiva
    /// </summary>
    /// <param name="entityName">Nombre de la entidad</param>
    /// <param name="domainContext">Contexto del dominio</param>
    /// <param name="entityId">ID de la entidad</param>
    /// <returns>Nueva instancia de BusinessRuleViolationException</returns>
    public static BusinessRuleViolationException ForInactiveEntity(
        string entityName,
        string domainContext,
        Guid entityId)
    {
        var message = $"No se puede operar con {entityName} porque es una entidad inactiva";
        return new BusinessRuleViolationException(
            "EntityInactive", entityName, message, domainContext, entityId, "INACTIVE_ENTITY");
    }

    /// <summary>
    /// Crea una excepción para operación no permitida
    /// </summary>
    /// <param name="operation">Operación que se intentó realizar</param>
    /// <param name="entityName">Nombre de la entidad</param>
    /// <param name="reason">Razón por la cual no está permitida</param>
    /// <param name="domainContext">Contexto del dominio</param>
    /// <param name="entityId">ID de la entidad</param>
    /// <returns>Nueva instancia de BusinessRuleViolationException</returns>
    public static BusinessRuleViolationException ForOperationNotAllowed(
        string operation,
        string entityName,
        string reason,
        string domainContext,
        Guid? entityId = null)
    {
        var message = $"operación no permitida '{operation}' en {entityName}: {reason}";
        var excepcion = new BusinessRuleViolationException(
            "OperationNotAllowed", entityName, message, domainContext, entityId, "OPERATION_NOT_ALLOWED");
        
        excepcion.WithData("Operation", operation)
                 .WithData("Reason", reason);
                 
        return excepcion;
    }

    /// <summary>
    /// Crea una excepción para duplicados no permitidos
    /// </summary>
    /// <param name="entityName">Nombre de la entidad</param>
    /// <param name="duplicateField">Campo que está duplicado</param>
    /// <param name="duplicateValue">Valor duplicado</param>
    /// <param name="domainContext">Contexto del dominio</param>
    /// <returns>Nueva instancia de BusinessRuleViolationException</returns>
    public static BusinessRuleViolationException ForDuplicateNotAllowed(
        string entityName,
        string duplicateField,
        object duplicateValue,
        string domainContext)
    {
        var message = $"Ya existe un {entityName} con {duplicateField} = '{duplicateValue}'";
        var excepcion = new BusinessRuleViolationException("DuplicateNotAllowed", entityName, message, domainContext);
        
        excepcion.WithData("DuplicateField", duplicateField)
                 .WithData("DuplicateValue", duplicateValue);
                 
        return excepcion;
    }
} 