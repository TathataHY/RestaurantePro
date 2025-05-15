using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no manejado: {Message}", ex.Message);
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = exception switch
            {
                ValidationException validationEx => 
                    ApiResponse<object>.ErrorResponse(
                        validationEx.Errors.Select(e => e.ErrorMessage).ToList(), 
                        "Error de validación", 
                        StatusCodes.Status400BadRequest),
                        
                NotFoundException notFoundEx => 
                    ApiResponse<object>.ErrorResponse(
                        new List<string> { notFoundEx.Message }, 
                        "Recurso no encontrado", 
                        StatusCodes.Status404NotFound),
                        
                UnauthorizedAccessException => 
                    ApiResponse<object>.ErrorResponse(
                        new List<string> { "No autorizado" }, 
                        "Acceso denegado", 
                        StatusCodes.Status401Unauthorized),
                        
                _ => ApiResponse<object>.ErrorResponse(
                        new List<string> { "Ocurrió un error interno" }, 
                        "Error del servidor", 
                        StatusCodes.Status500InternalServerError)
            };
            
            context.Response.StatusCode = response.StatusCode;
            await context.Response.WriteAsJsonAsync(response);
        }
    }
} 