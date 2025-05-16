namespace RestaurantePro.Domain.Core.SharedKernel.ValueObjects
{
    /// <summary>
    /// Objeto de valor que representa un RUT chileno válido
    /// </summary>
    public class Rut : ValueObject
    {
        private static readonly Regex RutRegex = new Regex(
            @"^(\d{1,8})-(\d|k|K)$", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        /// <summary>
        /// Número del RUT (sin dígito verificador)
        /// </summary>
        public int Numero { get; }
        
        /// <summary>
        /// Dígito verificador
        /// </summary>
        public char DigitoVerificador { get; }
        
        /// <summary>
        /// Formato sin puntos: 12345678-9
        /// </summary>
        public string FormatoSinPuntos => $"{Numero}-{DigitoVerificador}";
        
        /// <summary>
        /// Formato con puntos: 12.345.678-9
        /// </summary>
        public string FormatoConPuntos
        {
            get
            {
                string numeroStr = Numero.ToString("#,###", new System.Globalization.CultureInfo("es-CL"));
                return $"{numeroStr.Replace(",", ".")}-{DigitoVerificador}";
            }
        }

        private Rut(int numero, char digitoVerificador)
        {
            Numero = numero;
            DigitoVerificador = char.ToUpper(digitoVerificador);
        }

        /// <summary>
        /// Crea un objeto RUT a partir de un número y dígito verificador
        /// </summary>
        /// <param name="numero">Número del RUT</param>
        /// <param name="digitoVerificador">Dígito verificador (puede ser número o 'K')</param>
        /// <returns>Objeto RUT validado</returns>
        /// <exception cref="ArgumentException">Si el número o dígito verificador no son válidos</exception>
        public static Rut Create(int numero, char digitoVerificador)
        {
            if (numero <= 0)
                throw new ArgumentException("El número del RUT debe ser positivo", nameof(numero));
                
            digitoVerificador = char.ToUpper(digitoVerificador);
            
            if (!EsDigitoVerificadorValido(digitoVerificador))
                throw new ArgumentException("El dígito verificador debe ser un número del 0 al 9 o 'K'", nameof(digitoVerificador));
                
            // Validar que el dígito verificador sea correcto para el número
            char digitoEsperado = CalcularDigitoVerificador(numero);
            if (digitoVerificador != digitoEsperado)
                throw new ArgumentException($"El dígito verificador no es válido. Se esperaba '{digitoEsperado}'", nameof(digitoVerificador));
                
            return new Rut(numero, digitoVerificador);
        }
        
        /// <summary>
        /// Crea un objeto RUT a partir de un valor string con formato "12345678-9" o "12.345.678-9"
        /// </summary>
        /// <param name="rutString">RUT en formato string</param>
        /// <returns>Objeto RUT validado</returns>
        /// <exception cref="ArgumentException">Si el formato o dígito verificador no son válidos</exception>
        public static Rut Parse(string rutString)
        {
            if (string.IsNullOrWhiteSpace(rutString))
                throw new ArgumentException("El RUT no puede estar vacío", nameof(rutString));
                
            // Eliminar puntos y espacios
            rutString = rutString.Replace(".", "").Replace(" ", "").Trim();
            
            var match = RutRegex.Match(rutString);
            if (!match.Success)
                throw new ArgumentException("El formato del RUT no es válido. Use 12345678-9", nameof(rutString));
                
            if (!int.TryParse(match.Groups[1].Value, out int numero))
                throw new ArgumentException("El número del RUT no es válido", nameof(rutString));
                
            char dv = match.Groups[2].Value[0];
            
            return Create(numero, dv);
        }
        
        /// <summary>
        /// Intenta crear un objeto RUT a partir de un string
        /// </summary>
        /// <param name="rutString">RUT en formato string</param>
        /// <param name="result">Objeto RUT resultante si es válido</param>
        /// <returns>True si se pudo crear, False en caso contrario</returns>
        public static bool TryParse(string rutString, out Rut result)
        {
            result = null;
            
            if (string.IsNullOrWhiteSpace(rutString))
                return false;
                
            try
            {
                result = Parse(rutString);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Calcula el dígito verificador para un número de RUT
        /// </summary>
        /// <param name="numero">Número de RUT</param>
        /// <returns>Dígito verificador calculado (0-9 o K)</returns>
        public static char CalcularDigitoVerificador(int numero)
        {
            int suma = 0;
            int multiplicador = 2;
            
            // Calcular la suma ponderada
            int tempNumero = numero;
            while (tempNumero > 0)
            {
                int digito = tempNumero % 10;
                suma += digito * multiplicador;
                multiplicador = multiplicador == 7 ? 2 : multiplicador + 1;
                tempNumero /= 10;
            }
            
            // Calcular el dígito verificador
            int resto = suma % 11;
            int resultado = 11 - resto;
            
            if (resultado == 11)
                return '0';
            else if (resultado == 10)
                return 'K';
            else
                return (char)('0' + resultado);
        }
        
        private static bool EsDigitoVerificadorValido(char dv)
        {
            return (dv >= '0' && dv <= '9') || dv == 'K';
        }

        /// <summary>
        /// Devuelve el RUT en formato con puntos
        /// </summary>
        public override string ToString()
        {
            return FormatoConPuntos;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Numero;
            yield return DigitoVerificador;
        }
    }
} 