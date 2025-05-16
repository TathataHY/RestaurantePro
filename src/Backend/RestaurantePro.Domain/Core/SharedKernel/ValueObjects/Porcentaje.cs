namespace RestaurantePro.Domain.Core.SharedKernel.ValueObjects
{
    /// <summary>
    /// Objeto de valor que representa un porcentaje
    /// </summary>
    public class Porcentaje : ValueObject
    {
        /// <summary>
        /// Valor del porcentaje (entre 0 y 100)
        /// </summary>
        public decimal Value { get; }
        
        /// <summary>
        /// Valor del porcentaje como fracción (entre 0 y 1)
        /// </summary>
        public decimal Fraccion => Value / 100m;

        private Porcentaje(decimal value)
        {
            Value = value;
        }

        /// <summary>
        /// Crea un nuevo porcentaje a partir de un valor decimal (0-100)
        /// </summary>
        /// <param name="value">Valor entre 0 y 100</param>
        /// <returns>Objeto Porcentaje validado</returns>
        /// <exception cref="ArgumentOutOfRangeException">Si el valor está fuera del rango 0-100</exception>
        public static Porcentaje Create(decimal value)
        {
            if (value < 0 || value > 100)
                throw new ArgumentOutOfRangeException(nameof(value), "El porcentaje debe estar entre 0 y 100");
                
            return new Porcentaje(Math.Round(value, 2));
        }
        
        /// <summary>
        /// Crea un nuevo porcentaje a partir de una fracción (0-1)
        /// </summary>
        /// <param name="fraccion">Valor entre 0 y 1</param>
        /// <returns>Objeto Porcentaje validado</returns>
        /// <exception cref="ArgumentOutOfRangeException">Si el valor está fuera del rango 0-1</exception>
        public static Porcentaje CreateFromFraccion(decimal fraccion)
        {
            if (fraccion < 0 || fraccion > 1)
                throw new ArgumentOutOfRangeException(nameof(fraccion), "La fracción debe estar entre 0 y 1");
                
            return new Porcentaje(Math.Round(fraccion * 100, 2));
        }
        
        /// <summary>
        /// Crea un porcentaje con valor cero
        /// </summary>
        public static Porcentaje Zero => new Porcentaje(0);
        
        /// <summary>
        /// Crea un porcentaje con valor 100%
        /// </summary>
        public static Porcentaje Cien => new Porcentaje(100);
        
        /// <summary>
        /// Aplica este porcentaje a un valor
        /// </summary>
        /// <param name="valor">Valor al que aplicar el porcentaje</param>
        /// <returns>Resultado de aplicar el porcentaje al valor</returns>
        public decimal AplicarA(decimal valor)
        {
            return valor * Fraccion;
        }
        
        /// <summary>
        /// Aplica este porcentaje a un valor monetario
        /// </summary>
        /// <param name="money">Valor monetario al que aplicar el porcentaje</param>
        /// <returns>Resultado de aplicar el porcentaje al valor monetario</returns>
        public Money AplicarA(Money money)
        {
            return money.Multiply(Fraccion);
        }
        
        /// <summary>
        /// Incrementa un valor en este porcentaje
        /// </summary>
        /// <param name="valor">Valor a incrementar</param>
        /// <returns>Valor incrementado en el porcentaje</returns>
        public decimal IncrementarA(decimal valor)
        {
            return valor * (1 + Fraccion);
        }
        
        /// <summary>
        /// Decrementa un valor en este porcentaje
        /// </summary>
        /// <param name="valor">Valor a decrementar</param>
        /// <returns>Valor decrementado en el porcentaje</returns>
        public decimal DecrementarA(decimal valor)
        {
            return valor * (1 - Fraccion);
        }

        // Sobrecarga de operadores
        public static Porcentaje operator +(Porcentaje a, Porcentaje b) => 
            Create(Math.Min(100, a.Value + b.Value));
            
        public static Porcentaje operator -(Porcentaje a, Porcentaje b) => 
            Create(Math.Max(0, a.Value - b.Value));
            
        public static bool operator >(Porcentaje a, Porcentaje b) => a.Value > b.Value;
        
        public static bool operator <(Porcentaje a, Porcentaje b) => a.Value < b.Value;
        
        public static bool operator >=(Porcentaje a, Porcentaje b) => a.Value >= b.Value;
        
        public static bool operator <=(Porcentaje a, Porcentaje b) => a.Value <= b.Value;

        /// <summary>
        /// Representación del porcentaje como cadena
        /// </summary>
        public override string ToString()
        {
            return $"{Value:0.##}%";
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
} 