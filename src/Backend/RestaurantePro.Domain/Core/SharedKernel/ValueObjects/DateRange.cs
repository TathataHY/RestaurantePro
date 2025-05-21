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

    /// <summary>
    /// Objeto de valor que representa las estaciones del año en el hemisferio sur (Chile)
    /// Útil para trabajar con ingredientes de temporada y promociones estacionales
    /// </summary>
    public class TemporadaChile : ValueObject
    {
        /// <summary>
        /// Fecha de inicio de la temporada
        /// </summary>
        public DateOnly Inicio { get; }
        
        /// <summary>
        /// Fecha de fin de la temporada
        /// </summary>
        public DateOnly Fin { get; }
        
        /// <summary>
        /// Nombre de la temporada
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Estación correspondiente a la temporada
        /// </summary>
        public RestaurantePro.Domain.Inventario.Ingredientes.Enums.TemporadaIngrediente Estacion { get; }
        
        private TemporadaChile(DateOnly inicio, DateOnly fin, string nombre, RestaurantePro.Domain.Inventario.Ingredientes.Enums.TemporadaIngrediente estacion)
        {
            Inicio = inicio;
            Fin = fin;
            Nombre = nombre;
            Estacion = estacion;
        }
        
        /// <summary>
        /// Obtiene la temporada de verano en Chile (Diciembre a Febrero)
        /// </summary>
        /// <param name="año">Año para el cual obtener la temporada</param>
        public static TemporadaChile Verano(int año)
        {
            return new TemporadaChile(
                new DateOnly(año, 12, 21), 
                new DateOnly(año + 1, 3, 20),
                "Verano",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.TemporadaIngrediente.Verano
            );
        }
        
        /// <summary>
        /// Obtiene la temporada de otoño en Chile (Marzo a Mayo)
        /// </summary>
        /// <param name="año">Año para el cual obtener la temporada</param>
        public static TemporadaChile Otoño(int año)
        {
            return new TemporadaChile(
                new DateOnly(año, 3, 21), 
                new DateOnly(año, 6, 20),
                "Otoño",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.TemporadaIngrediente.Otoño
            );
        }
        
        /// <summary>
        /// Obtiene la temporada de invierno en Chile (Junio a Agosto)
        /// </summary>
        /// <param name="año">Año para el cual obtener la temporada</param>
        public static TemporadaChile Invierno(int año)
        {
            return new TemporadaChile(
                new DateOnly(año, 6, 21), 
                new DateOnly(año, 9, 20),
                "Invierno",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.TemporadaIngrediente.Invierno
            );
        }
        
        /// <summary>
        /// Obtiene la temporada de primavera en Chile (Septiembre a Noviembre)
        /// </summary>
        /// <param name="año">Año para el cual obtener la temporada</param>
        public static TemporadaChile Primavera(int año)
        {
            return new TemporadaChile(
                new DateOnly(año, 9, 21), 
                new DateOnly(año, 12, 20),
                "Primavera",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.TemporadaIngrediente.Primavera
            );
        }
        
        /// <summary>
        /// Obtiene la temporada actual según la fecha del sistema
        /// </summary>
        public static TemporadaChile ObtenerTemporadaActual()
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            int año = hoy.Year;
            
            // Verificar en qué estación estamos
            var fechaActualMesDia = new DateOnly(1, hoy.Month, hoy.Day);
            
            if (EstaEnRango(hoy, new DateOnly(año, 12, 21), new DateOnly(año + 1, 3, 20)))
                return Verano(año);
                
            if (EstaEnRango(hoy, new DateOnly(año, 3, 21), new DateOnly(año, 6, 20)))
                return Otoño(año);
                
            if (EstaEnRango(hoy, new DateOnly(año, 6, 21), new DateOnly(año, 9, 20)))
                return Invierno(año);
                
            if (EstaEnRango(hoy, new DateOnly(año, 9, 21), new DateOnly(año, 12, 20)))
                return Primavera(año);
                
            // Si por alguna razón no cae en ninguna (no debería suceder)
            if (hoy.Month == 12)
                return Verano(año);
                
            // Para el caso especial de inicio de año (enero a marzo), es verano del año anterior
            if (hoy.Month < 3 || (hoy.Month == 3 && hoy.Day <= 20))
                return Verano(año - 1);
                
            throw new InvalidOperationException("No se pudo determinar la temporada actual");
        }
        
        /// <summary>
        /// Verifica si una fecha está dentro del rango de la temporada
        /// </summary>
        /// <param name="fecha">Fecha a verificar</param>
        /// <returns>True si la fecha está dentro de la temporada, false en caso contrario</returns>
        public bool Contiene(DateOnly fecha)
        {
            if (Inicio.Year < Fin.Year)
            {
                // Para temporadas que cruzan el año (ej. Verano)
                if (fecha.Year == Inicio.Year)
                    return fecha >= Inicio;
                else if (fecha.Year == Fin.Year)
                    return fecha <= Fin;
                else
                    return false;
            }
            else
            {
                // Para temporadas dentro del mismo año
                return fecha >= Inicio && fecha <= Fin;
            }
        }
        
        /// <summary>
        /// Verifica si una fecha está dentro de un rango
        /// </summary>
        private static bool EstaEnRango(DateOnly fecha, DateOnly inicio, DateOnly fin)
        {
            if (inicio.Year < fin.Year)
            {
                // Para rangos que cruzan el año (ej. Verano)
                if (fecha.Year == inicio.Year || fecha.Year == inicio.Year - 1)
                    return fecha >= inicio || fecha.Year > inicio.Year;
                else if (fecha.Year == fin.Year || fecha.Year == fin.Year + 1)
                    return fecha <= fin || fecha.Year < fin.Year;
                else
                    return false;
            }
            else
            {
                // Para rangos dentro del mismo año
                return fecha >= inicio && fecha <= fin;
            }
        }
        
        public override string ToString()
        {
            return $"{Nombre} ({Inicio:dd/MM} - {Fin:dd/MM})";
        }
        
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Inicio;
            yield return Fin;
            yield return Estacion;
        }
    }
} 