using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
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
        private readonly Dictionary<Type, Action<ExceptionContext>> _exceptionHandlers;

        public ApiExceptionFilterAttribute(ILogger<ApiExceptionFilterAttribute> logger)
        {
            _logger = logger;
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
            if (_exceptionHandlers.ContainsKey(type))
            {
                _exceptionHandlers[type].Invoke(context);
                return;
            }

            if (!context.ModelState.IsValid)
            {
                HandleInvalidModelStateException(context);
                return;
            }

            HandleUnknownException(context);
        }

        private void HandleValidationException(ExceptionContext context)
        {
            var exception = (RestaurantePro.Application.Common.Exceptions.ValidationException)context.Exception;
            var errors = exception.Errors.SelectMany(kvp => kvp.Value).ToList();

            _logger.LogWarning("Error de validación: {Errors}", string.Join(", ", errors));

            var response = ApiResponse<object>.ErrorResponse(
                errors, "Error de validación", StatusCodes.Status400BadRequest);

            context.Result = new BadRequestObjectResult(response);
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

            var response = ApiResponse<object>.ErrorResponse(
                errors, "Datos de entrada inválidos", StatusCodes.Status400BadRequest);

            context.Result = new BadRequestObjectResult(response);
            context.ExceptionHandled = true;
        }

        private void HandleUnknownException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Error no manejado: {Message}", context.Exception.Message);

            var response = ApiResponse<object>.ErrorResponse(
                new List<string> { "Ocurrió un error interno del servidor" }, 
                "Error interno", 
                StatusCodes.Status500InternalServerError);

            context.Result = new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
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