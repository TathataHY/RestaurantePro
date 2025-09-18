using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace RestaurantePro.Infrastructure.Persistence.Converters
{
    /// <summary>
    /// Convertidor que automáticamente convierte fechas entre hora de Chile y UTC
    /// para que la aplicación siempre trabaje con hora de Chile
    /// </summary>
    public class ChileDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        private static readonly TimeZoneInfo _chileTimeZone = GetChileTimeZone();

        public ChileDateTimeConverter() : base(
            // Convertir de hora de Chile (aplicación) a UTC (base de datos)
            chileTime => ConvertirAUtc(chileTime),
            // Convertir de UTC (base de datos) a hora de Chile (aplicación)
            utcTime => ConvertirAChile(utcTime))
        {
        }

        private static TimeZoneInfo GetChileTimeZone()
        {
            try
            {
                // Intentar obtener la zona horaria de Chile
                return TimeZoneInfo.FindSystemTimeZoneById("America/Santiago");
            }
            catch
            {
                try
                {
                    // En Windows, el ID puede ser diferente
                    return TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time");
                }
                catch
                {
                    // Fallback: crear manualmente la zona horaria de Chile
                    return TimeZoneInfo.CreateCustomTimeZone(
                        "Chile Standard Time",
                        TimeSpan.FromHours(-3),
                        "Chile Standard Time",
                        "Chile Standard Time");
                }
            }
        }

        /// <summary>
        /// Convierte una fecha a UTC manejando correctamente el DateTimeKind
        /// </summary>
        private static DateTime ConvertirAUtc(DateTime dateTime)
        {
            // Si ya es UTC, devolverlo tal como está
            if (dateTime.Kind == DateTimeKind.Utc)
                return dateTime;
            
            // Si es Local, convertir usando la zona horaria local
            if (dateTime.Kind == DateTimeKind.Local)
                return TimeZoneInfo.ConvertTimeToUtc(dateTime);
            
            // Si es Unspecified, asumir que es hora de Chile
            if (dateTime.Kind == DateTimeKind.Unspecified)
            {
                // Especificar que es hora de Chile y convertir a UTC
                var chileDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
                return TimeZoneInfo.ConvertTimeToUtc(chileDateTime, _chileTimeZone);
            }
            
            return dateTime;
        }

        /// <summary>
        /// Convierte una fecha UTC a hora de Chile
        /// </summary>
        private static DateTime ConvertirAChile(DateTime utcDateTime)
        {
            // Si no es UTC, especificar que lo es
            if (utcDateTime.Kind != DateTimeKind.Utc)
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            
            // Convertir de UTC a hora de Chile
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _chileTimeZone);
        }
    }

    /// <summary>
    /// Convertidor para DateTime nullable
    /// </summary>
    public class ChileDateTimeNullableConverter : ValueConverter<DateTime?, DateTime?>
    {
        private static readonly TimeZoneInfo _chileTimeZone = GetChileTimeZone();

        public ChileDateTimeNullableConverter() : base(
            // Convertir de hora de Chile (aplicación) a UTC (base de datos)
            chileTime => chileTime.HasValue ? ConvertirAUtc(chileTime.Value) : null,
            // Convertir de UTC (base de datos) a hora de Chile (aplicación)
            utcTime => utcTime.HasValue ? ConvertirAChile(utcTime.Value) : null)
        {
        }

        private static TimeZoneInfo GetChileTimeZone()
        {
            try
            {
                // Intentar obtener la zona horaria de Chile
                return TimeZoneInfo.FindSystemTimeZoneById("America/Santiago");
            }
            catch
            {
                try
                {
                    // En Windows, el ID puede ser diferente
                    return TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time");
                }
                catch
                {
                    // Fallback: crear manualmente la zona horaria de Chile
                    return TimeZoneInfo.CreateCustomTimeZone(
                        "Chile Standard Time",
                        TimeSpan.FromHours(-3),
                        "Chile Standard Time",
                        "Chile Standard Time");
                }
            }
        }

        /// <summary>
        /// Convierte una fecha a UTC manejando correctamente el DateTimeKind
        /// </summary>
        private static DateTime ConvertirAUtc(DateTime dateTime)
        {
            // Si ya es UTC, devolverlo tal como está
            if (dateTime.Kind == DateTimeKind.Utc)
                return dateTime;
            
            // Si es Local, convertir usando la zona horaria local
            if (dateTime.Kind == DateTimeKind.Local)
                return TimeZoneInfo.ConvertTimeToUtc(dateTime);
            
            // Si es Unspecified, asumir que es hora de Chile
            if (dateTime.Kind == DateTimeKind.Unspecified)
            {
                // Especificar que es hora de Chile y convertir a UTC
                var chileDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
                return TimeZoneInfo.ConvertTimeToUtc(chileDateTime, _chileTimeZone);
            }
            
            return dateTime;
        }

        /// <summary>
        /// Convierte una fecha UTC a hora de Chile
        /// </summary>
        private static DateTime ConvertirAChile(DateTime utcDateTime)
        {
            // Si no es UTC, especificar que lo es
            if (utcDateTime.Kind != DateTimeKind.Utc)
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            
            // Convertir de UTC a hora de Chile
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _chileTimeZone);
        }
    }
}
