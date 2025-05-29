namespace RestaurantePro.Domain.Core.SharedKernel.Guards;

/// <summary>
/// Clase estática que proporciona métodos de validación (Guard Clauses) comunes 
/// para proteger el dominio contra estados inválidos.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Valida que un objeto no sea nulo
    /// </summary>
    /// <typeparam name="T">Tipo del objeto</typeparam>
    /// <param name="value">Valor a validar</param>
    /// <param name="parameterName">Nombre del parámetro</param>
    /// <param name="message">Mensaje personalizado (opcional)</param>
    /// <exception cref="ArgumentNullException">Si el valor es nulo</exception>
    public static void AgainstNull<T>(T value, string parameterName, string? message = null) where T : class
    {
        if (value == null)
        {
            throw new ArgumentNullException(parameterName, message ?? $"El parámetro {parameterName} no puede ser nulo");
        }
    }

    /// <summary>
    /// Valida que un Guid no sea Empty
    /// </summary>
    /// <param name="value">Valor a validar</param>
    /// <param name="parameterName">Nombre del parámetro</param>
    /// <param name="message">Mensaje personalizado (opcional)</param>
    /// <exception cref="ArgumentException">Si el Guid es Empty</exception>
    public static void AgainstEmpty(Guid value, string parameterName, string? message = null)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(message ?? $"El parámetro {parameterName} no puede ser un Guid vacío", parameterName);
        }
    }

    /// <summary>
    /// Valida que una cadena no sea nula o vacía
    /// </summary>
    /// <param name="value">Valor a validar</param>
    /// <param name="parameterName">Nombre del parámetro</param>
    /// <param name="message">Mensaje personalizado (opcional)</param>
    /// <exception cref="ArgumentException">Si la cadena es nula o vacía</exception>
    public static void AgainstNullOrEmpty(string value, string parameterName, string? message = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException(message ?? $"El parámetro {parameterName} no puede ser nulo o vacío", parameterName);
        }
    }

    /// <summary>
    /// Valida que una cadena no sea nula, vacía o solo espacios en blanco
    /// </summary>
    /// <param name="value">Valor a validar</param>
    /// <param name="parameterName">Nombre del parámetro</param>
    /// <param name="message">Mensaje personalizado (opcional)</param>
    /// <exception cref="ArgumentException">Si la cadena es nula, vacía o solo espacios</exception>
    public static void AgainstNullOrWhiteSpace(string value, string parameterName, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(message ?? $"El parámetro {parameterName} no puede ser nulo, vacío o solo espacios", parameterName);
        }
    }

    /// <summary>
    /// Valida que un valor numérico sea positivo (mayor que cero)
    /// </summary>
    /// <param name="value">Valor a validar</param>
    /// <param name="parameterName">Nombre del parámetro</param>
    /// <param name="message">Mensaje personalizado (opcional)</param>
    /// <exception cref="ArgumentException">Si el valor no es positivo</exception>
    public static void AgainstNegativeOrZero(decimal value, string parameterName, string? message = null)
    {
        if (value <= 0)
        {
            throw new ArgumentException(message ?? $"El parámetro {parameterName} debe ser mayor a cero", parameterName);
        }
    }

    /// <summary>
    /// Valida que un valor numérico entero sea positivo (mayor que cero)
    /// </summary>
    /// <param name="value">Valor a validar</param>
    /// <param name="parameterName">Nombre del parámetro</param>
    /// <param name="message">Mensaje personalizado (opcional)</param>
    /// <exception cref="ArgumentException">Si el valor no es positivo</exception>
    public static void AgainstNegativeOrZero(int value, string parameterName, string? message = null)
    {
        if (value <= 0)
        {
            throw new ArgumentException(message ?? $"El parámetro {parameterName} debe ser mayor a cero", parameterName);
        }
    }

    /// <summary>
    /// Valida que un valor numérico no sea negativo (mayor o igual que cero)
    /// </summary>
    /// <param name="value">Valor a validar</param>
    /// <param name="parameterName">Nombre del parámetro</param>
    /// <param name="message">Mensaje personalizado (opcional)</param>
    /// <exception cref="ArgumentException">Si el valor es negativo</exception>
    public static void AgainstNegative(decimal value, string parameterName, string? message = null)
    {
        if (value < 0)
        {
            throw new ArgumentException(message ?? $"El parámetro {parameterName} no puede ser negativo", parameterName);
        }
    }

    /// <summary>
    /// Valida que un valor esté dentro de un rango específico
    /// </summary>
    /// <typeparam name="T">Tipo comparable</typeparam>
    /// <param name="value">Valor a validar</param>
    /// <param name="min">Valor mínimo (inclusivo)</param>
    /// <param name="max">Valor máximo (inclusivo)</param>
    /// <param name="parameterName">Nombre del parámetro</param>
    /// <param name="message">Mensaje personalizado (opcional)</param>
    /// <exception cref="ArgumentOutOfRangeException">Si el valor está fuera del rango</exception>
    public static void AgainstOutOfRange<T>(T value, T min, T max, string parameterName, string? message = null) 
        where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, 
                message ?? $"El parámetro {parameterName} debe estar entre {min} y {max}");
        }
    }

    /// <summary>
    /// Valida que una colección no sea nula o vacía
    /// </summary>
    /// <typeparam name="T">Tipo de elementos de la colección</typeparam>
    /// <param name="collection">Colección a validar</param>
    /// <param name="parameterName">Nombre del parámetro</param>
    /// <param name="message">Mensaje personalizado (opcional)</param>
    /// <exception cref="ArgumentException">Si la colección es nula o vacía</exception>
    public static void AgainstNullOrEmpty<T>(IEnumerable<T> collection, string parameterName, string? message = null)
    {
        if (collection == null || !collection.Any())
        {
            throw new ArgumentException(message ?? $"El parámetro {parameterName} no puede ser nulo o vacío", parameterName);
        }
    }

    /// <summary>
    /// Valida que una fecha no sea la fecha mínima de DateTime
    /// </summary>
    /// <param name="value">Fecha a validar</param>
    /// <param name="parameterName">Nombre del parámetro</param>
    /// <param name="message">Mensaje personalizado (opcional)</param>
    /// <exception cref="ArgumentException">Si la fecha es DateTime.MinValue</exception>
    public static void AgainstMinValue(DateTime value, string parameterName, string? message = null)
    {
        if (value == DateTime.MinValue)
        {
            throw new ArgumentException(message ?? $"El parámetro {parameterName} no puede ser la fecha mínima", parameterName);
        }
    }

    /// <summary>
    /// Valida que una fecha sea futura (mayor a DateTime.Now)
    /// </summary>
    /// <param name="value">Fecha a validar</param>
    /// <param name="parameterName">Nombre del parámetro</param>
    /// <param name="message">Mensaje personalizado (opcional)</param>
    /// <exception cref="ArgumentException">Si la fecha no es futura</exception>
    public static void AgainstPastDate(DateTime value, string parameterName, string? message = null)
    {
        if (value <= DateTime.Now)
        {
            throw new ArgumentException(message ?? $"El parámetro {parameterName} debe ser una fecha futura", parameterName);
        }
    }

    /// <summary>
    /// Valida que una fecha sea pasada (menor a DateTime.Now)
    /// </summary>
    /// <param name="value">Fecha a validar</param>
    /// <param name="parameterName">Nombre del parámetro</param>
    /// <param name="message">Mensaje personalizado (opcional)</param>
    /// <exception cref="ArgumentException">Si la fecha no es pasada</exception>
    public static void AgainstFutureDate(DateTime value, string parameterName, string? message = null)
    {
        if (value >= DateTime.Now)
        {
            throw new ArgumentException(message ?? $"El parámetro {parameterName} debe ser una fecha pasada", parameterName);
        }
    }

    /// <summary>
    /// Valida que una cadena tenga una longitud específica
    /// </summary>
    /// <param name="value">Cadena a validar</param>
    /// <param name="maxLength">Longitud máxima permitida</param>
    /// <param name="parameterName">Nombre del parámetro</param>
    /// <param name="message">Mensaje personalizado (opcional)</param>
    /// <exception cref="ArgumentException">Si la cadena excede la longitud máxima</exception>
    public static void AgainstTooLong(string value, int maxLength, string parameterName, string? message = null)
    {
        if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
        {
            throw new ArgumentException(
                message ?? $"El parámetro {parameterName} no puede exceder {maxLength} caracteres", 
                parameterName);
        }
    }

    /// <summary>
    /// Valida que una cadena tenga al menos una longitud mínima
    /// </summary>
    /// <param name="value">Cadena a validar</param>
    /// <param name="minLength">Longitud mínima requerida</param>
    /// <param name="parameterName">Nombre del parámetro</param>
    /// <param name="message">Mensaje personalizado (opcional)</param>
    /// <exception cref="ArgumentException">Si la cadena es menor que la longitud mínima</exception>
    public static void AgainstTooShort(string value, int minLength, string parameterName, string? message = null)
    {
        if (string.IsNullOrEmpty(value) || value.Length < minLength)
        {
            throw new ArgumentException(
                message ?? $"El parámetro {parameterName} debe tener al menos {minLength} caracteres", 
                parameterName);
        }
    }

    /// <summary>
    /// Valida que una condición sea verdadera
    /// </summary>
    /// <param name="condition">Condición a evaluar</param>
    /// <param name="message">Mensaje de error si la condición es falsa</param>
    /// <exception cref="ArgumentException">Si la condición es falsa</exception>
    public static void Against(bool condition, string message)
    {
        if (condition)
        {
            throw new ArgumentException(message);
        }
    }

    /// <summary>
    /// Valida que una condición sea verdadera
    /// </summary>
    /// <param name="condition">Condición a evaluar</param>
    /// <param name="message">Mensaje de error si la condición es falsa</param>
    /// <param name="parameterName">Nombre del parámetro relacionado</param>
    /// <exception cref="ArgumentException">Si la condición es falsa</exception>
    public static void Against(bool condition, string message, string parameterName)
    {
        if (condition)
        {
            throw new ArgumentException(message, parameterName);
        }
    }

    /// <summary>
    /// Valida que una condición sea verdadera, si no lanza la excepción personalizada
    /// </summary>
    /// <param name="condition">Condición a evaluar</param>
    /// <param name="exceptionFactory">Factory para crear la excepción personalizada</param>
    /// <exception cref="Exception">La excepción creada por el factory</exception>
    public static void Against(bool condition, Func<Exception> exceptionFactory)
    {
        if (condition)
        {
            throw exceptionFactory();
        }
    }
} 