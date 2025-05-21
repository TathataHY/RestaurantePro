namespace RestaurantePro.Domain.Core.SharedKernel.ValueObjects
{
    /// <summary>
    /// Objeto de valor que representa un número telefónico válido.
    /// </summary>
    public class PhoneNumber : ValueObject
    {
        // Formato general que acepta varios formatos internacionales
        private static readonly Regex PhoneRegex = new Regex(
            @"^[\d\s\(\)\+\-\.]{8,20}$",
            RegexOptions.Compiled);
            
        // Formato específico para Chile (móvil): +56 9 xxxx xxxx
        private static readonly Regex ChileanMobileRegex = new Regex(
            @"^(\+?56)?[\s\-]?9[\s\-]?\d{4}[\s\-]?\d{4}$",
            RegexOptions.Compiled);
            
        // Formato específico para Chile (fijo): +56 x xxxx xxxx (donde x es 2, 3, etc.)
        private static readonly Regex ChileanFixedRegex = new Regex(
            @"^(\+?56)?[\s\-]?[2-9][\s\-]?\d{4}[\s\-]?\d{4}$",
            RegexOptions.Compiled);

        /// <summary>
        /// El número telefónico en formato normalizado
        /// </summary>
        public string Value { get; }
        
        /// <summary>
        /// Indica si el número es un teléfono móvil chileno
        /// </summary>
        public bool EsMovilChileno => ChileanMobileRegex.IsMatch(Value);
        
        /// <summary>
        /// Indica si el número es un teléfono fijo chileno
        /// </summary>
        public bool EsFijoChileno => ChileanFixedRegex.IsMatch(Value);
        
        /// <summary>
        /// Indica si el número es un teléfono chileno (fijo o móvil)
        /// </summary>
        public bool EsTelefonoChileno => EsMovilChileno || EsFijoChileno;

        private PhoneNumber(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Crea una nueva instancia de PhoneNumber
        /// </summary>
        /// <param name="phoneNumber">Número telefónico a validar</param>
        /// <returns>Objeto PhoneNumber validado</returns>
        /// <exception cref="ArgumentException">Si el formato no es válido</exception>
        public static PhoneNumber Create(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("El número telefónico no puede estar vacío", nameof(phoneNumber));

            phoneNumber = phoneNumber.Trim();

            if (!PhoneRegex.IsMatch(phoneNumber))
                throw new ArgumentException("El formato del número telefónico no es válido", nameof(phoneNumber));

            return new PhoneNumber(phoneNumber);
        }
        
        /// <summary>
        /// Crea un número de teléfono chileno con validación específica
        /// </summary>
        /// <param name="phoneNumber">Número telefónico a validar</param>
        /// <param name="esMovil">Indica si debe validarse como teléfono móvil</param>
        /// <returns>Objeto PhoneNumber validado como número chileno</returns>
        /// <exception cref="ArgumentException">Si el formato no corresponde a un número chileno válido</exception>
        public static PhoneNumber CreateChilean(string phoneNumber, bool? esMovil = null)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("El número telefónico no puede estar vacío", nameof(phoneNumber));

            phoneNumber = phoneNumber.Trim();
            
            bool validoMovil = ChileanMobileRegex.IsMatch(phoneNumber);
            bool validoFijo = ChileanFixedRegex.IsMatch(phoneNumber);
            
            // Si se especificó el tipo, validar según corresponda
            if (esMovil.HasValue)
            {
                if (esMovil.Value && !validoMovil)
                    throw new ArgumentException("El formato no corresponde a un número móvil chileno válido (+56 9 xxxx xxxx)", nameof(phoneNumber));
                    
                if (!esMovil.Value && !validoFijo)
                    throw new ArgumentException("El formato no corresponde a un número fijo chileno válido (+56 2 xxxx xxxx)", nameof(phoneNumber));
            }
            // Si no se especificó el tipo, cualquier formato chileno es válido
            else if (!validoMovil && !validoFijo)
            {
                throw new ArgumentException("El formato no corresponde a un número chileno válido", nameof(phoneNumber));
            }
            
            // Normalizar el formato para números chilenos
            // Primero eliminamos todos los caracteres no numéricos excepto el +
            string normalizado = new string(phoneNumber.Where(c => char.IsDigit(c) || c == '+').ToArray());
            
            // Si no tiene el prefijo internacional, lo agregamos
            if (!normalizado.StartsWith("+"))
            {
                if (normalizado.StartsWith("56"))
                    normalizado = "+" + normalizado;
                else
                    normalizado = "+56" + normalizado;
            }
            
            return new PhoneNumber(normalizado);
        }

        /// <summary>
        /// Intenta crear un PhoneNumber sin lanzar excepciones
        /// </summary>
        /// <param name="phoneNumber">Número telefónico a validar</param>
        /// <param name="result">PhoneNumber resultante si es válido</param>
        /// <returns>True si se creó correctamente, False en caso contrario</returns>
        public static bool TryCreate(string phoneNumber, out PhoneNumber result)
        {
            result = null;

            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            phoneNumber = phoneNumber.Trim();

            if (!PhoneRegex.IsMatch(phoneNumber))
                return false;

            result = new PhoneNumber(phoneNumber);
            return true;
        }
        
        /// <summary>
        /// Intenta crear un número de teléfono chileno sin lanzar excepciones
        /// </summary>
        /// <param name="phoneNumber">Número telefónico a validar</param>
        /// <param name="result">PhoneNumber resultante si es válido como número chileno</param>
        /// <param name="esMovil">Opcional, especifica si debe validarse como móvil</param>
        /// <returns>True si se creó correctamente como número chileno, False en caso contrario</returns>
        public static bool TryCreateChilean(string phoneNumber, out PhoneNumber result, bool? esMovil = null)
        {
            result = null;
            
            try
            {
                result = CreateChilean(phoneNumber, esMovil);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Conversión implícita de PhoneNumber a string
        /// </summary>
        public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber?.Value;

        /// <summary>
        /// Convierte el número telefónico a su representación en texto
        /// </summary>
        public override string ToString() => Value;
        
        /// <summary>
        /// Formatea el número telefónico chileno para mostrar
        /// </summary>
        /// <returns>Número telefónico con formato +56 9 1234 5678</returns>
        public string ToFormattedString()
        {
            if (!EsTelefonoChileno)
                return Value;
                
            string digitsOnly = new string(Value.Where(char.IsDigit).ToArray());
            
            if (digitsOnly.Length < 8)
                return Value;
                
            if (digitsOnly.StartsWith("56"))
                digitsOnly = digitsOnly.Substring(2);
                
            // Es móvil
            if (digitsOnly.StartsWith("9") && digitsOnly.Length >= 9)
            {
                return $"+56 9 {digitsOnly.Substring(1, 4)} {digitsOnly.Substring(5, 4)}";
            }
            // Es fijo
            else if (digitsOnly.Length >= 9)
            {
                char areaCode = digitsOnly[0];
                return $"+56 {areaCode} {digitsOnly.Substring(1, 4)} {digitsOnly.Substring(5, 4)}";
            }
            
            return Value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
} 