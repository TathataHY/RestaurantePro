using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Common.Exceptions;

namespace RestaurantePro.Api.Middleware
{
    /// <summary>
    /// Middleware para manejar excepciones globalmente en la API
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                // TEMPORAL: Logging para debugging
                _logger.LogError(exception, "🔍 EXCEPCIÓN CAPTURADA EN MIDDLEWARE: {ExceptionType} - {Message}", 
                    exception.GetType().Name, exception.Message);
                
                await HandleExceptionAsync(context, exception);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            _logger.LogInformation("[DEBUG] ExceptionMiddleware - EnvironmentName: {EnvName}, IsDevelopment: {IsDev}", _environment.EnvironmentName, _environment.IsDevelopment());

            ApiResponse<object> response;
            int statusCode;

            if (exception is RestaurantePro.Application.Common.Exceptions.ValidationException validationEx)
            {
                response = ApiResponse<object>.ErrorResponse(
                    validationEx.Errors.SelectMany(kvp => kvp.Value).ToList(),
                    "Error de validación",
                    StatusCodes.Status400BadRequest);
                statusCode = StatusCodes.Status400BadRequest;
            }
            else if (exception is FluentValidation.ValidationException fvEx)
            {
                response = ApiResponse<object>.ErrorResponse(
                    fvEx.Errors.Select(e => e.ErrorMessage).ToList(),
                    "Error de validación",
                    StatusCodes.Status400BadRequest);
                statusCode = StatusCodes.Status400BadRequest;
            }
            else if (exception is ArgumentException argEx)
            {
                response = ApiResponse<object>.ErrorResponse(
                    new List<string> { argEx.Message },
                    "Error de validación",
                    StatusCodes.Status400BadRequest);
                statusCode = StatusCodes.Status400BadRequest;
            }
            else if (exception is System.Text.Json.JsonException jsonEx)
            {
                response = ApiResponse<object>.ErrorResponse(
                    new List<string> { "Error de formato JSON: " + jsonEx.Message },
                    "Error de deserialización",
                    StatusCodes.Status400BadRequest);
                statusCode = StatusCodes.Status400BadRequest;
            }
            else if (exception is Microsoft.AspNetCore.Http.BadHttpRequestException badRequestEx)
            {
                response = ApiResponse<object>.ErrorResponse(
                    new List<string> { "Error en la solicitud: " + badRequestEx.Message },
                    "Error de binding",
                    StatusCodes.Status400BadRequest);
                statusCode = StatusCodes.Status400BadRequest;
            }
            else if (exception is NotFoundException notFoundEx)
            {
                response = ApiResponse<object>.ErrorResponse(
                    new List<string> { notFoundEx.Message },
                    "Recurso no encontrado",
                    StatusCodes.Status404NotFound);
                statusCode = StatusCodes.Status404NotFound;
            }
            else if (exception is UnauthorizedAccessException)
            {
                response = ApiResponse<object>.ErrorResponse(
                    new List<string> { "No autorizado" },
                    "Acceso denegado",
                    StatusCodes.Status401Unauthorized);
                statusCode = StatusCodes.Status401Unauthorized;
            }
            else
            {
                // En desarrollo, incluir detalles de la excepción
                List<string> errorMessages;
                if (_environment.IsDevelopment())
                {
                    _logger.LogInformation("🔍 MIDDLEWARE - Entorno de desarrollo detectado, incluyendo detalles de excepción");
                    errorMessages = new List<string>
                    {
                        $"Error: {exception.Message}",
                        $"Tipo: {exception.GetType().Name}",
                        $"Stack Trace: {exception.StackTrace}"
                    };
                }
                else
                {
                    _logger.LogInformation("🔍 MIDDLEWARE - Entorno de producción, usando mensaje genérico");
                    errorMessages = new List<string> { "Ocurrió un error interno" };
                }
                response = ApiResponse<object>.ErrorResponse(
                    errorMessages,
                    "Error del servidor",
                    StatusCodes.Status500InternalServerError);
                statusCode = StatusCodes.Status500InternalServerError;
            }

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(response);
        }
    }
} 