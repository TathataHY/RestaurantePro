using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RestaurantePro.Domain.Core.SharedKernel.Services
{
    /// <summary>
    /// Implementación en memoria de la telemetría de caché
    /// </summary>
    public class InMemoryCacheTelemetry : ICacheTelemetry
    {
        private readonly ConcurrentDictionary<string, long> _hitsByKey = new ConcurrentDictionary<string, long>();
        private readonly ConcurrentDictionary<string, long> _missesByKey = new ConcurrentDictionary<string, long>();
        private readonly ConcurrentDictionary<string, long> _invalidationsByPattern = new ConcurrentDictionary<string, long>();
        private readonly ConcurrentDictionary<string, ConcurrentBag<TimeSpan>> _operationDurationsByType = new ConcurrentDictionary<string, ConcurrentBag<TimeSpan>>();
        private readonly ConcurrentBag<CacheErrorInfo> _errors = new ConcurrentBag<CacheErrorInfo>();
        
        private long _totalHits;
        private long _totalMisses;
        private long _totalErrors;
        private long _totalInvalidations;
        private long _totalKeysAffected;
        
        private readonly Stopwatch _uptime = Stopwatch.StartNew();
        
        /// <summary>
        /// Información de un error en la caché
        /// </summary>
        private class CacheErrorInfo
        {
            public string Key { get; }
            public string OperationType { get; }
            public Exception Exception { get; }
            public DateTime Timestamp { get; }
            
            public CacheErrorInfo(string key, string operationType, Exception exception)
            {
                Key = key;
                OperationType = operationType;
                Exception = exception;
                Timestamp = DateTime.Now;
            }
        }
        
        /// <inheritdoc />
        public void TrackCacheAccess(string key, bool hit, string operationType, long elapsedMilliseconds)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(operationType))
                return;
                
            if (hit)
            {
                _hitsByKey.AddOrUpdate(key, 1, (_, count) => count + 1);
                Interlocked.Increment(ref _totalHits);
            }
            else
            {
                _missesByKey.AddOrUpdate(key, 1, (_, count) => count + 1);
                Interlocked.Increment(ref _totalMisses);
            }
            
            var durations = _operationDurationsByType.GetOrAdd(operationType, _ => new ConcurrentBag<TimeSpan>());
            durations.Add(TimeSpan.FromMilliseconds(elapsedMilliseconds));
        }
        
        /// <inheritdoc />
        public void TrackCacheInvalidation(string pattern, int keysAffected, long elapsedMilliseconds)
        {
            if (string.IsNullOrEmpty(pattern))
                return;
                
            _invalidationsByPattern.AddOrUpdate(pattern, 1, (_, count) => count + 1);
            Interlocked.Increment(ref _totalInvalidations);
            Interlocked.Add(ref _totalKeysAffected, keysAffected);
            
            var durations = _operationDurationsByType.GetOrAdd("Invalidation", _ => new ConcurrentBag<TimeSpan>());
            durations.Add(TimeSpan.FromMilliseconds(elapsedMilliseconds));
        }
        
        /// <inheritdoc />
        public void TrackCacheError(string key, string operationType, Exception exception)
        {
            if (exception == null)
                return;
                
            _errors.Add(new CacheErrorInfo(key, operationType, exception));
            Interlocked.Increment(ref _totalErrors);
        }
        
        /// <inheritdoc />
        public Dictionary<string, object> GetStatistics()
        {
            var stats = new Dictionary<string, object>();
            
            // Estadísticas generales
            stats["Uptime"] = _uptime.Elapsed;
            stats["TotalHits"] = _totalHits;
            stats["TotalMisses"] = _totalMisses;
            stats["TotalErrors"] = _totalErrors;
            stats["TotalInvalidations"] = _totalInvalidations;
            stats["TotalKeysAffected"] = _totalKeysAffected;
            
            // Calcular tasa de aciertos
            long totalAccesses = _totalHits + _totalMisses;
            double hitRate = totalAccesses > 0 ? (double)_totalHits / totalAccesses : 0;
            stats["HitRate"] = hitRate;
            
            // Top 10 claves más accedidas
            stats["TopHitKeys"] = _hitsByKey
                .OrderByDescending(x => x.Value)
                .Take(10)
                .ToDictionary(x => x.Key, x => x.Value);
                
            // Top 10 claves con más fallos
            stats["TopMissKeys"] = _missesByKey
                .OrderByDescending(x => x.Value)
                .Take(10)
                .ToDictionary(x => x.Key, x => x.Value);
                
            // Top 10 patrones de invalidación
            stats["TopInvalidationPatterns"] = _invalidationsByPattern
                .OrderByDescending(x => x.Value)
                .Take(10)
                .ToDictionary(x => x.Key, x => x.Value);
                
            // Tiempos promedio por operación
            var avgDurations = new Dictionary<string, double>();
            foreach (var kv in _operationDurationsByType)
            {
                var durations = kv.Value.ToArray();
                if (durations.Length > 0)
                {
                    avgDurations[kv.Key] = durations.Average(d => d.TotalMilliseconds);
                }
            }
            stats["AverageDurationMs"] = avgDurations;
            
            // Últimos 10 errores
            stats["RecentErrors"] = _errors
                .OrderByDescending(e => e.Timestamp)
                .Take(10)
                .Select(e => new 
                {
                    e.Key,
                    e.OperationType,
                    Error = e.Exception.Message,
                    e.Timestamp
                })
                .ToList();
                
            return stats;
        }
    }
} 