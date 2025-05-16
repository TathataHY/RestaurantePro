namespace RestaurantePro.Domain.Core.SharedKernel.ValueObjects
{
    /// <summary>
    /// Enumeración de unidades de medida comunes
    /// </summary>
    public enum UnidadMedida
    {
        // Unidades de peso
        Gramo,
        Kilogramo,
        Libra,
        Onza,
        
        // Unidades de volumen
        Mililitro,
        Litro,
        Galon,
        
        // Unidades de longitud
        Milimetro,
        Centimetro,
        Metro,
        
        // Unidades discretas
        Unidad,
        Docena,
        Porcion
    }
    
    /// <summary>
    /// Objeto de valor que representa una cantidad medible con su unidad de medida
    /// </summary>
    public class CantidadMedible : ValueObject
    {
        /// <summary>
        /// Valor de la cantidad
        /// </summary>
        public decimal Valor { get; }
        
        /// <summary>
        /// Unidad de medida
        /// </summary>
        public UnidadMedida Unidad { get; }
        
        /// <summary>
        /// Indica si la cantidad es cero
        /// </summary>
        public bool EsCero => Valor == 0;
        
        /// <summary>
        /// Indica si la cantidad es positiva (mayor que cero)
        /// </summary>
        public bool EsPositiva => Valor > 0;
        
        /// <summary>
        /// Indica si la cantidad es negativa (menor que cero)
        /// </summary>
        public bool EsNegativa => Valor < 0;

        private CantidadMedible(decimal valor, UnidadMedida unidad)
        {
            Valor = valor;
            Unidad = unidad;
        }

        /// <summary>
        /// Crea una nueva cantidad medible
        /// </summary>
        /// <param name="valor">Valor de la cantidad</param>
        /// <param name="unidad">Unidad de medida</param>
        /// <returns>Objeto CantidadMedible validado</returns>
        /// <exception cref="ArgumentException">Si el valor no es válido</exception>
        public static CantidadMedible Create(decimal valor, UnidadMedida unidad)
        {
            // Redondear a 3 decimales para evitar problemas de precisión
            valor = Math.Round(valor, 3);
            
            // Para unidades discretas, el valor debe ser entero
            if (EsUnidadDiscreta(unidad) && valor != Math.Truncate(valor))
                throw new ArgumentException($"La unidad '{unidad}' requiere un valor entero", nameof(valor));
                
            return new CantidadMedible(valor, unidad);
        }
        
        /// <summary>
        /// Crea una cantidad medible con valor cero
        /// </summary>
        /// <param name="unidad">Unidad de medida</param>
        /// <returns>Objeto CantidadMedible con valor cero</returns>
        public static CantidadMedible Zero(UnidadMedida unidad)
        {
            return new CantidadMedible(0, unidad);
        }
        
        /// <summary>
        /// Determina si la unidad es discreta (no continua)
        /// </summary>
        /// <param name="unidad">Unidad a verificar</param>
        /// <returns>True si la unidad es discreta</returns>
        private static bool EsUnidadDiscreta(UnidadMedida unidad)
        {
            return unidad == UnidadMedida.Unidad || 
                   unidad == UnidadMedida.Docena ||
                   unidad == UnidadMedida.Porcion;
        }
        
        /// <summary>
        /// Suma dos cantidades medibles (deben tener la misma unidad)
        /// </summary>
        /// <param name="other">Otra cantidad medible</param>
        /// <returns>Nueva cantidad medible con la suma</returns>
        /// <exception cref="InvalidOperationException">Si las unidades son diferentes</exception>
        public CantidadMedible Sumar(CantidadMedible other)
        {
            if (other == null)
                return this;
                
            if (Unidad != other.Unidad)
                throw new InvalidOperationException($"No se pueden sumar cantidades con diferentes unidades: {Unidad} y {other.Unidad}");
                
            return Create(Valor + other.Valor, Unidad);
        }
        
        /// <summary>
        /// Resta dos cantidades medibles (deben tener la misma unidad)
        /// </summary>
        /// <param name="other">Otra cantidad medible</param>
        /// <returns>Nueva cantidad medible con la resta</returns>
        /// <exception cref="InvalidOperationException">Si las unidades son diferentes</exception>
        public CantidadMedible Restar(CantidadMedible other)
        {
            if (other == null)
                return this;
                
            if (Unidad != other.Unidad)
                throw new InvalidOperationException($"No se pueden restar cantidades con diferentes unidades: {Unidad} y {other.Unidad}");
                
            return Create(Valor - other.Valor, Unidad);
        }
        
        /// <summary>
        /// Multiplica la cantidad por un factor
        /// </summary>
        /// <param name="factor">Factor de multiplicación</param>
        /// <returns>Nueva cantidad medible con el resultado</returns>
        public CantidadMedible Multiplicar(decimal factor)
        {
            return Create(Valor * factor, Unidad);
        }
        
        /// <summary>
        /// Divide la cantidad por un divisor
        /// </summary>
        /// <param name="divisor">Divisor (no puede ser cero)</param>
        /// <returns>Nueva cantidad medible con el resultado</returns>
        /// <exception cref="DivideByZeroException">Si el divisor es cero</exception>
        public CantidadMedible Dividir(decimal divisor)
        {
            if (divisor == 0)
                throw new DivideByZeroException("No se puede dividir por cero");
                
            return Create(Valor / divisor, Unidad);
        }
        
        /// <summary>
        /// Obtiene el valor absoluto de la cantidad
        /// </summary>
        /// <returns>Nueva cantidad medible con el valor absoluto</returns>
        public CantidadMedible ValorAbsoluto()
        {
            return EsNegativa ? Create(-Valor, Unidad) : this;
        }

        // Sobrecarga de operadores
        public static CantidadMedible operator +(CantidadMedible a, CantidadMedible b) => a.Sumar(b);
        public static CantidadMedible operator -(CantidadMedible a, CantidadMedible b) => a.Restar(b);
        public static CantidadMedible operator *(CantidadMedible a, decimal f) => a.Multiplicar(f);
        public static CantidadMedible operator /(CantidadMedible a, decimal d) => a.Dividir(d);
        public static bool operator >(CantidadMedible a, CantidadMedible b) =>
            a.Unidad == b.Unidad && a.Valor > b.Valor;
        public static bool operator <(CantidadMedible a, CantidadMedible b) =>
            a.Unidad == b.Unidad && a.Valor < b.Valor;
        public static bool operator >=(CantidadMedible a, CantidadMedible b) =>
            a.Unidad == b.Unidad && a.Valor >= b.Valor;
        public static bool operator <=(CantidadMedible a, CantidadMedible b) =>
            a.Unidad == b.Unidad && a.Valor <= b.Valor;

        /// <summary>
        /// Obtiene la representación en texto de la cantidad medible
        /// </summary>
        public override string ToString()
        {
            // Para unidades discretas, mostrar sin decimales
            string valorStr = EsUnidadDiscreta(Unidad) 
                ? Valor.ToString("0") 
                : Valor.ToString("0.###");
                
            string unidadAbreviada = ObtenerAbreviatura(Unidad);
            
            return $"{valorStr} {unidadAbreviada}";
        }
        
        /// <summary>
        /// Obtiene la abreviatura de una unidad de medida
        /// </summary>
        /// <param name="unidad">Unidad de medida</param>
        /// <returns>Abreviatura de la unidad</returns>
        private static string ObtenerAbreviatura(UnidadMedida unidad)
        {
            return unidad switch
            {
                UnidadMedida.Gramo => "g",
                UnidadMedida.Kilogramo => "kg",
                UnidadMedida.Libra => "lb",
                UnidadMedida.Onza => "oz",
                UnidadMedida.Mililitro => "ml",
                UnidadMedida.Litro => "l",
                UnidadMedida.Galon => "gal",
                UnidadMedida.Milimetro => "mm",
                UnidadMedida.Centimetro => "cm",
                UnidadMedida.Metro => "m",
                UnidadMedida.Unidad => "un",
                UnidadMedida.Docena => "doc",
                UnidadMedida.Porcion => "porc",
                _ => unidad.ToString()
            };
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Valor;
            yield return Unidad;
        }
    }
} 