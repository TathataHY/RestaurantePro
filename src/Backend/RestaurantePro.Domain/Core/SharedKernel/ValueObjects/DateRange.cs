namespace RestaurantePro.Domain.Core.SharedKernel.ValueObjects
{
    /// <summary>
    /// Objeto de valor que representa un rango de fechas (inicio y fin)
    /// </summary>
    public class DateRange : ValueObject
    {
        /// <summary>
        /// Fecha de inicio del rango
        /// </summary>
        public DateTime Start { get; }
        
        /// <summary>
        /// Fecha de fin del rango
        /// </summary>
        public DateTime End { get; }
        
        /// <summary>
        /// Duración del rango en días
        /// </summary>
        public int DurationInDays => (int)(End - Start).TotalDays;
        
        /// <summary>
        /// Duración del rango como TimeSpan
        /// </summary>
        public TimeSpan Duration => End - Start;

        private DateRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Crea un nuevo rango de fechas
        /// </summary>
        /// <param name="start">Fecha de inicio</param>
        /// <param name="end">Fecha de fin</param>
        /// <returns>Objeto DateRange validado</returns>
        /// <exception cref="ArgumentException">Si el fin es anterior al inicio</exception>
        public static DateRange Create(DateTime start, DateTime end)
        {
            if (end < start)
                throw new ArgumentException("La fecha de fin no puede ser anterior a la fecha de inicio", nameof(end));
                
            return new DateRange(start, end);
        }
        
        /// <summary>
        /// Crea un nuevo rango a partir de una fecha de inicio y una duración
        /// </summary>
        /// <param name="start">Fecha de inicio</param>
        /// <param name="duration">Duración del rango</param>
        /// <returns>Objeto DateRange validado</returns>
        public static DateRange CreateFromDuration(DateTime start, TimeSpan duration)
        {
            if (duration.TotalMilliseconds < 0)
                throw new ArgumentException("La duración no puede ser negativa", nameof(duration));
                
            return new DateRange(start, start.Add(duration));
        }
        
        /// <summary>
        /// Verifica si una fecha está dentro del rango (inclusivo)
        /// </summary>
        /// <param name="date">Fecha a verificar</param>
        /// <returns>True si la fecha está en el rango, False en caso contrario</returns>
        public bool Contains(DateTime date)
        {
            return date >= Start && date <= End;
        }
        
        /// <summary>
        /// Verifica si este rango se traslapa con otro
        /// </summary>
        /// <param name="other">Otro rango de fechas</param>
        /// <returns>True si los rangos se traslapan, False en caso contrario</returns>
        public bool Overlaps(DateRange other)
        {
            return Start <= other.End && End >= other.Start;
        }
        
        /// <summary>
        /// Obtiene la intersección entre este rango y otro
        /// </summary>
        /// <param name="other">Otro rango de fechas</param>
        /// <returns>Un nuevo DateRange que representa la intersección, o null si no hay intersección</returns>
        public DateRange Intersection(DateRange other)
        {
            if (!Overlaps(other))
                return null;
                
            var start = Start > other.Start ? Start : other.Start;
            var end = End < other.End ? End : other.End;
            
            return new DateRange(start, end);
        }

        /// <summary>
        /// Devuelve una representación en texto del rango de fechas
        /// </summary>
        public override string ToString()
        {
            return $"{Start:dd/MM/yyyy} - {End:dd/MM/yyyy}";
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Start;
            yield return End;
        }
    }
} 