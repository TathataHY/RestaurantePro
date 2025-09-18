using System;

namespace RestaurantePro.Domain.Core.Base.Services
{
    /// <summary>
    /// Servicio de fecha y hora que siempre retorna la hora de Chile
    /// </summary>
    public class ChileDateTimeService : IDateTimeService
    {
        private static readonly TimeZoneInfo _chileTimeZone = GetChileTimeZone();

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
                    // Fallback: crear manualmente la zona horaria de Chile (UTC-3/-4 según DST)
                    return TimeZoneInfo.CreateCustomTimeZone(
                        "Chile Standard Time",
                        TimeSpan.FromHours(-3),
                        "Chile Standard Time",
                        "Chile Standard Time");
                }
            }
        }

        /// <summary>
        /// Obtiene la fecha y hora actual en la zona horaria de Chile
        /// </summary>
        public DateTime Now 
        { 
            get 
            {
                var utcNow = DateTime.UtcNow;
                var chileNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, _chileTimeZone);
                
                // 🔧 IMPORTANTE: Especificar que es hora local (no UTC ni Unspecified)
                chileNow = DateTime.SpecifyKind(chileNow, DateTimeKind.Local);
                
                // Log temporal para debug
                Console.WriteLine($"🌍 [ChileDateTimeService] UTC: {utcNow:yyyy-MM-dd HH:mm:ss} (Kind: {utcNow.Kind})");
                Console.WriteLine($"🌍 [ChileDateTimeService] Chile: {chileNow:yyyy-MM-dd HH:mm:ss} (Kind: {chileNow.Kind})");
                Console.WriteLine($"🌍 [ChileDateTimeService] TimeZone: {_chileTimeZone.DisplayName}");
                
                return chileNow;
            }
        }

        /// <summary>
        /// Obtiene la fecha y hora actual en UTC
        /// </summary>
        public DateTime UtcNow => DateTime.UtcNow;

        /// <summary>
        /// Obtiene solo la fecha actual en la zona horaria de Chile
        /// </summary>
        public DateTime Today => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _chileTimeZone).Date;
    }
}
