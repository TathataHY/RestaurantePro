using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Common.Exceptions;

namespace RestaurantePro.Api.Filters
{
    /// <summary>
    /// Filtro que maneja excepciones de los controladores y las convierte en respuestas HTTP adecuadas
    /// </summary>
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger<ApiExceptionFilterAttribute> _logger;
        private readonly IDictionary<Type, Action<ExceptionContext>> _exceptionHandlers;

        public ApiExceptionFilterAttribute(ILogger<ApiExceptionFilterAttribute> logger)
        {
            _logger = logger;
            _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
            {
                { typeof(AppValidationException), HandleValidationException },
                { typeof(NotFoundException), HandleNotFoundException },
                { typeof(UnauthorizedAccessException), HandleUnauthorizedAccessException },
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
            Type type = context.Exception.GetType();
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
            var exception = (AppValidationException)context.Exception;
            var details = ApiResponse<object>.ErrorResponse(
                exception.Errors.SelectMany(kvp => kvp.Value).ToList(), 
                "Error de validación", 
                StatusCodes.Status400BadRequest);

            context.Result = new BadRequestObjectResult(details);
            context.ExceptionHandled = true;
        }

        private void HandleNotFoundException(ExceptionContext context)
        {
            var exception = (NotFoundException)context.Exception;
            var details = ApiResponse<object>.ErrorResponse(
                exception.Message, 
                "Recurso no encontrado", 
                StatusCodes.Status404NotFound);

            context.Result = new NotFoundObjectResult(details);
            context.ExceptionHandled = true;
        }

        private void HandleUnauthorizedAccessException(ExceptionContext context)
        {
            var details = ApiResponse<object>.ErrorResponse(
                "No tiene autorización para realizar esta acción", 
                "Acceso no autorizado", 
                StatusCodes.Status401Unauthorized);

            context.Result = new ObjectResult(details)
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            context.ExceptionHandled = true;
        }

        private void HandleForbiddenAccessException(ExceptionContext context)
        {
            var details = ApiResponse<object>.ErrorResponse(
                "No tiene permisos para realizar esta acción", 
                "Acceso prohibido", 
                StatusCodes.Status403Forbidden);

            context.Result = new ObjectResult(details)
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            context.ExceptionHandled = true;
        }

        private void HandleInvalidModelStateException(ExceptionContext context)
        {
            var errors = new List<string>();
            foreach (var key in context.ModelState.Keys)
            {
                foreach (var error in context.ModelState[key].Errors)
                {
                    errors.Add($"{key}: {error.ErrorMessage}");
                }
            }

            var details = ApiResponse<object>.ErrorResponse(
                errors, 
                "Datos de entrada inválidos", 
                StatusCodes.Status400BadRequest);

            context.Result = new BadRequestObjectResult(details);
            context.ExceptionHandled = true;
        }

        private void HandleUnknownException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Error no manejado: {Exception}", context.Exception.Message);

            var details = ApiResponse<object>.ErrorResponse(
                "Ha ocurrido un error inesperado. Por favor, inténtelo de nuevo más tarde.", 
                "Error del servidor", 
                StatusCodes.Status500InternalServerError);

            context.Result = new ObjectResult(details)
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