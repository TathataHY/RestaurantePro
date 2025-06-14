using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using System;

namespace RestaurantePro.Infrastructure.Logging.Providers
{
    /// <summary>
    /// Implementación provisional de ApplicationInsightsProvider.
    /// Esta clase deberá implementarse completamente cuando se agreguen las dependencias de ApplicationInsights.
    /// </summary>
    public class ApplicationInsightsProvider : ILoggingProvider
    {
        /// <summary>
        /// Configura un logger para Application Insights
        /// </summary>
        public ILogger ConfigureLogger()
        {
            // Por ahora devolvemos un logger nulo
            return new DummyLogger();
        }
        
        /// <summary>
        /// Logger vacío que no hace nada
        /// </summary>
        private class DummyLogger : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => false;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, 
                Exception? exception, Func<TState, Exception?, string> formatter) { }
        }
    }
} 