using FluentValidation.Results;

namespace RestaurantePro.Application.Common.Exceptions;

/// <summary>
/// Excepción lanzada cuando fallan las validaciones de negocio
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Diccionario de errores de validación agrupados por campo
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException() 
        : base("Uno o más errores de validación han ocurrido.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }

    public ValidationException(string field, string error)
        : this()
    {
        Errors = new Dictionary<string, string[]>
        {
            { field, new[] { error } }
        };
    }

    public ValidationException(string field, string[] errors)
        : this()
    {
        Errors = new Dictionary<string, string[]>
        {
            { field, errors }
        };
    }
} 