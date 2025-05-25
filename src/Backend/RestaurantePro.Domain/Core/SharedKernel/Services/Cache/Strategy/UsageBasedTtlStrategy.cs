namespace RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Strategy
{
    /// <summary>
    /// Implementación de estrategia de TTL dinámico basado en patrones de uso
    /// </summary>
    public class UsageBasedTtlStrategy : IDynamicTtlStrategy
    {
        private readonly ConcurrentDictionary<string, KeyUsageStats> _keyStats = new ConcurrentDictionary<string, KeyUsageStats>();
        private readonly ConcurrentDictionary<string, DateTime> _lastInvalidation = new ConcurrentDictionary<string, DateTime>();
        
        // Factores de peso para el cálculo
        private const double FREQUENCY_WEIGHT = 0.4;    // Peso para frecuencia de acceso
        private const double HIT_RATE_WEIGHT = 0.3;     // Peso para tasa de aciertos
        private const double RECENCY_WEIGHT = 0.2;      // Peso para lo reciente que es
        private const double INVALIDATION_WEIGHT = 0.1; // Peso para invalidaciones
        
        // Límites de TTL
        private const int MIN_TTL_MINUTES = 5;          // Mínimo TTL 
        private const int MAX_TTL_MINUTES = 120;        // Máximo TTL
        
        /// <summary>
        /// Estadísticas de uso de una clave
        /// </summary>
        private class KeyUsageStats
        {
            public int TotalAccesses { get; set; }
            public int TotalHits { get; set; }
            public DateTime LastAccess { get; set; }
            public DateTime FirstAccess { get; set; }
            
            public KeyUsageStats()
            {
                FirstAccess = DateTime.Now;
                LastAccess = DateTime.Now;
            }
            
            public double GetHitRate()
            {
                return TotalAccesses > 0 ? (double)TotalHits / TotalAccesses : 0;
            }
            
            public double GetAccessFrequency()
            {
                var timeSpan = (DateTime.Now - FirstAccess).TotalMinutes;
                return timeSpan > 0 ? TotalAccesses / timeSpan : 0;
            }
            
            public double GetRecency()
            {
                // Valor entre 0 y 1, donde 1 significa muy reciente
                var minutesAgo = (DateTime.Now - LastAccess).TotalMinutes;
                return Math.Exp(-0.01 * minutesAgo); // Función exponencial de decaimiento
            }
        }
        
        /// <inheritdoc />
        public int CalculateTtl(string key, int defaultTtlMinutes)
        {
            if (!_keyStats.TryGetValue(key, out var stats))
            {
                return defaultTtlMinutes;
            }
            
            // Calcular factores
            double frequencyScore = NormalizeFrequency(stats.GetAccessFrequency());
            double hitRateScore = stats.GetHitRate();
            double recencyScore = stats.GetRecency();
            double invalidationScore = CalculateInvalidationScore(key);
            
            // Calcular puntuación compuesta (valores más altos favorecen TTL más largos)
            double compositeScore = 
                (frequencyScore * FREQUENCY_WEIGHT) +
                (hitRateScore * HIT_RATE_WEIGHT) +
                (recencyScore * RECENCY_WEIGHT) +
                (invalidationScore * INVALIDATION_WEIGHT);
            
            // Mapear puntuación al rango de TTL
            int calculatedTtl = MapScoreToTtl(compositeScore, defaultTtlMinutes);
            
            return calculatedTtl;
        }
        
        /// <inheritdoc />
        public void RegisterAccess(string key, bool isHit, string operationType)
        {
            var stats = _keyStats.GetOrAdd(key, _ => new KeyUsageStats());
            
            stats.TotalAccesses++;
            if (isHit)
            {
                stats.TotalHits++;
            }
            stats.LastAccess = DateTime.Now;
        }
        
        /// <inheritdoc />
        public void RegisterInvalidation(string pattern, int affectedKeys)
        {
            _lastInvalidation[pattern] = DateTime.Now;
            
            // Para todas las claves que coinciden con este patrón, registramos la invalidación
            var matchingKeys = _keyStats.Keys.Where(k => k.Contains(pattern)).ToList();
            foreach (var key in matchingKeys)
            {
                // Actualizamos la estadística para reflejar que ha sido invalidada
                if (_keyStats.TryGetValue(key, out var stats))
                {
                    // Reducimos artificialmente la tasa de aciertos para claves que se invalidan frecuentemente
                    stats.TotalHits = Math.Max(0, stats.TotalHits - 1);
                }
            }
        }
        
        /// <summary>
        /// Normaliza la frecuencia de acceso a un valor entre 0 y 1
        /// </summary>
        private double NormalizeFrequency(double frequency)
        {
            // Asumimos que 10 accesos por minuto es muy frecuente (1.0)
            const double MAX_EXPECTED_FREQUENCY = 10;
            
            return Math.Min(1.0, frequency / MAX_EXPECTED_FREQUENCY);
        }
        
        /// <summary>
        /// Calcula un puntaje de invalidación basado en la frecuencia de invalidaciones
        /// </summary>
        private double CalculateInvalidationScore(string key)
        {
            // Buscar patrones que podrían afectar a esta clave
            var relevantPatterns = _lastInvalidation.Keys
                .Where(pattern => key.Contains(pattern))
                .ToList();
            
            if (!relevantPatterns.Any())
            {
                return 1.0; // Sin invalidaciones, puntuación máxima
            }
            
            // Obtener la invalidación más reciente
            var mostRecentInvalidation = relevantPatterns
                .Select(p => _lastInvalidation[p])
                .Max();
            
            // Calcular tiempo desde la última invalidación en minutos
            var minutesSinceInvalidation = (DateTime.Now - mostRecentInvalidation).TotalMinutes;
            
            // Si fue invalidada hace poco, la puntuación es baja
            // Si fue hace mucho tiempo, la puntuación es alta
            return Math.Min(1.0, minutesSinceInvalidation / 60); // Normalizado a 1 hora
        }
        
        /// <summary>
        /// Mapea una puntuación (0-1) a un valor TTL entre MIN_TTL_MINUTES y MAX_TTL_MINUTES
        /// </summary>
        private int MapScoreToTtl(double score, int defaultTtl)
        {
            // Ajustar el rango según el valor por defecto
            int adjustedMin = Math.Min(MIN_TTL_MINUTES, defaultTtl / 2);
            int adjustedMax = Math.Max(MAX_TTL_MINUTES, defaultTtl * 2);
            
            // Usar una función sigmoide para mapear el score al rango de TTL
            // Esto da una transición más suave entre valores
            double normalizedScore = 1.0 / (1.0 + Math.Exp(-10 * (score - 0.5)));
            
            // Calcular el TTL dentro del rango ajustado
            int ttl = (int)(adjustedMin + normalizedScore * (adjustedMax - adjustedMin));
            
            return ttl;
        }
    }
} 