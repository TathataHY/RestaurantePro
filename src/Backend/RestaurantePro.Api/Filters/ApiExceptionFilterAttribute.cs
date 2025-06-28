using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Common.Exceptions;

namespace RestaurantePro.Api.Filters
{
    /// <summary>
    /// Filtro para manejar excepciones específicas de la API
    /// </summary>
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger<ApiExceptionFilterAttribute> _logger;
        private readonly IHostEnvironment _environment;
        private readonly Dictionary<Type, Action<ExceptionContext>> _exceptionHandlers;

        public ApiExceptionFilterAttribute(ILogger<ApiExceptionFilterAttribute> logger, IHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
            _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
            {
                { typeof(RestaurantePro.Application.Common.Exceptions.ValidationException), HandleValidationException },
                { typeof(NotFoundException), HandleNotFoundException },
                { typeof(ConflictException), HandleConflictException },
                { typeof(ForbiddenAccessException), HandleForbiddenAccessException }
            };
        }

        public override void OnException(ExceptionContext context)
        {
            HandleException(context);
            base.OnException(context);
        }

        private void HandleException(ExceptionContext context)
        {
            var type = context.Exception.GetType();
            
            _logger.LogInformation("🔍 FILTRO - Excepción capturada: {ExceptionType} - {Message}", 
                type.Name, context.Exception.Message);
            _logger.LogInformation("🔍 FILTRO - Entorno: {Environment}", _environment.EnvironmentName);
            _logger.LogInformation("🔍 FILTRO - ¿Es desarrollo? {IsDevelopment}", _environment.IsDevelopment());
            
            if (_exceptionHandlers.ContainsKey(type))
            {
                _logger.LogInformation("🔍 FILTRO - Usando handler específico para {ExceptionType}", type.Name);
                _exceptionHandlers[type].Invoke(context);
                return;
            }

            // Siempre verificar el ModelState, incluso cuando no hay excepciones
            if (!context.ModelState.IsValid)
            {
                _logger.LogInformation("🔍 FILTRO - ModelState inválido, usando HandleInvalidModelStateException");
                HandleInvalidModelStateException(context);
                return;
            }

            // Manejar excepciones no mapeadas
            _logger.LogError(context.Exception, "Excepción no manejada: {Message}", context.Exception.Message);
            
            // En desarrollo, incluir detalles de la excepción
            List<string> errorMessages;
            if (_environment.IsDevelopment())
            {
                _logger.LogInformation("🔍 FILTRO - Entorno de desarrollo detectado, incluyendo detalles de excepción");
                errorMessages = new List<string>
                {
                    $"Error: {context.Exception.Message}",
                    $"Tipo: {context.Exception.GetType().Name}",
                    $"Stack Trace: {context.Exception.StackTrace}"
                };
            }
            else
            {
                _logger.LogInformation("🔍 FILTRO - Entorno de producción, usando mensaje genérico");
                errorMessages = new List<string> { "Error interno del servidor" };
            }

            var response = ApiResponse<object>.ErrorResponse(
                errorMessages,
                "Error interno del servidor",
                StatusCodes.Status500InternalServerError);

            context.Result = new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            context.ExceptionHandled = true;
            
            _logger.LogInformation("🔍 FILTRO - Respuesta configurada con {ErrorCount} errores", errorMessages.Count);
        }

        private void HandleValidationException(ExceptionContext context)
        {
            var exception = (RestaurantePro.Application.Common.Exceptions.ValidationException)context.Exception;
            var errors = exception.Errors.SelectMany(kvp => kvp.Value).ToList();

            _logger.LogWarning("Error de validación: {Errors}", string.Join(", ", errors));

            // Robustecer la detección de mensajes de "no encontrada" o "no existe"
            var hasNotFoundMessage = errors.Any(error =>
                error != null && (
                    error.ToLower().Contains("no encontrada") ||
                    error.ToLower().Contains("no existe") ||
                    error.ToLower().Contains("no se encontró") ||
                    error.ToLower().Contains("no se encontro") ||
                    error.ToLower().Contains("especificada no existe")
                )
            );

            if (hasNotFoundMessage)
            {
                var response = ApiResponse<object>.ErrorResponse(
                    errors, "Recurso no encontrado", StatusCodes.Status404NotFound);

                context.Result = new NotFoundObjectResult(response);
            }
            else
            {
                var response = ApiResponse<object>.ErrorResponse(
                    errors, "Error de validación", StatusCodes.Status400BadRequest);

                context.Result = new BadRequestObjectResult(response);
            }

            context.ExceptionHandled = true;
        }

        private void HandleNotFoundException(ExceptionContext context)
        {
            var exception = (NotFoundException)context.Exception;

            _logger.LogWarning("Recurso no encontrado: {Message}", exception.Message);

            var response = ApiResponse<object>.ErrorResponse(
                new List<string> { exception.Message }, 
                "Recurso no encontrado", 
                StatusCodes.Status404NotFound);

            context.Result = new NotFoundObjectResult(response);
            context.ExceptionHandled = true;
        }

        private void HandleConflictException(ExceptionContext context)
        {
            var exception = (ConflictException)context.Exception;

            _logger.LogWarning("Conflicto detectado: {Message}", exception.Message);

            var response = ApiResponse<object>.ErrorResponse(
                new List<string> { exception.Message }, 
                "Conflicto detectado", 
                StatusCodes.Status409Conflict);

            context.Result = new ConflictObjectResult(response);
            context.ExceptionHandled = true;
        }

        private void HandleForbiddenAccessException(ExceptionContext context)
        {
            var exception = (ForbiddenAccessException)context.Exception;

            _logger.LogWarning("Acceso denegado: {Message}", exception.Message);

            var response = ApiResponse<object>.ErrorResponse(
                new List<string> { exception.Message }, 
                "Acceso denegado", 
                StatusCodes.Status403Forbidden);

            context.Result = new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            context.ExceptionHandled = true;
        }

        private void HandleInvalidModelStateException(ExceptionContext context)
        {
            var errors = context.ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                .ToList();

            _logger.LogWarning("Estado del modelo inválido: {Errors}", string.Join(", ", errors));

            // Robustecer la detección de mensajes de "no encontrada" o "no existe"
            var hasNotFoundMessage = errors.Any(error =>
                error != null && (
                    error.ToLower().Contains("no encontrada") ||
                    error.ToLower().Contains("no existe")
                )
            );

            _logger.LogInformation("🔍 FILTRO - Errores encontrados: {Errors}", string.Join(", ", errors));
            _logger.LogInformation("🔍 FILTRO - ¿Contiene mensaje de no encontrada/no existe? {HasNotFound}", hasNotFoundMessage);

            if (hasNotFoundMessage)
            {
                _logger.LogInformation("🔍 FILTRO - Devolviendo 404 para mensaje de no encontrada/no existe");
                var response = ApiResponse<object>.ErrorResponse(
                    errors, "Recurso no encontrado", StatusCodes.Status404NotFound);

                context.Result = new NotFoundObjectResult(response);
            }
            else
            {
                _logger.LogInformation("🔍 FILTRO - Devolviendo 400 para errores de validación");
                var response = ApiResponse<object>.ErrorResponse(
                    errors, "Datos de entrada inválidos", StatusCodes.Status400BadRequest);

                context.Result = new BadRequestObjectResult(response);
            }

            context.ExceptionHandled = true;
        }
    }

    /// <summary>
    /// Excepción para acceso prohibido
    /// </summary>
    public class ForbiddenAccessException : Exception
    {
        public ForbiddenAccessException() : base() { }

        public ForbiddenAccessException(string message) : base(message) { }

        public ForbiddenAccessException(string message, Exception innerException)
            : base(message, innerException) { }
    }
} 