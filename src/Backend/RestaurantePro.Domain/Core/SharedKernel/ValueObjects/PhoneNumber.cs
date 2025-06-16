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
            
        // Códigos de área para cada región de Chile
        private static readonly Dictionary<RegionChile, List<string>> CodigosAreaPorRegion = new Dictionary<RegionChile, List<string>>
        {
            { RegionChile.Arica, new List<string> { "58" } },
            { RegionChile.Tarapaca, new List<string> { "57" } },
            { RegionChile.Antofagasta, new List<string> { "55" } },
            { RegionChile.Atacama, new List<string> { "52" } },
            { RegionChile.Coquimbo, new List<string> { "51", "53" } },
            { RegionChile.Valparaiso, new List<string> { "32", "33", "34", "35" } },
            { RegionChile.Metropolitana, new List<string> { "2" } },
            { RegionChile.OHiggins, new List<string> { "72", "73" } },
            { RegionChile.Maule, new List<string> { "71", "73", "75" } },
            { RegionChile.Nuble, new List<string> { "42" } },
            { RegionChile.Biobio, new List<string> { "41", "43" } },
            { RegionChile.Araucania, new List<string> { "45" } },
            { RegionChile.LosRios, new List<string> { "63" } },
            { RegionChile.LosLagos, new List<string> { "64", "65" } },
            { RegionChile.Aysen, new List<string> { "67" } },
            { RegionChile.Magallanes, new List<string> { "61" } }
        };

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
        
        /// <summary>
        /// Obtiene el código de área del número (para teléfonos fijos chilenos)
        /// </summary>
        public string CodigoArea
        {
            get
            {
                if (!EsFijoChileno) return string.Empty;
                
                string digitsOnly = new string(Value.Where(char.IsDigit).ToArray());
                if (digitsOnly.StartsWith("56")) digitsOnly = digitsOnly.Substring(2);
                
                if (digitsOnly.StartsWith("2")) return "2"; // Santiago (Región Metropolitana)
                
                // Códigos de área de dos dígitos
                if (digitsOnly.Length >= 2)
                {
                    string posibleCodigo = digitsOnly.Substring(0, 2);
                    foreach (var regionCodigos in CodigosAreaPorRegion)
                    {
                        if (regionCodigos.Value.Contains(posibleCodigo))
                            return posibleCodigo;
                    }
                }
                
                return string.Empty;
            }
        }
        
        /// <summary>
        /// Intenta determinar la región de Chile basada en el código de área
        /// </summary>
        public RegionChile? RegionTelefono
        {
            get
            {
                if (!EsFijoChileno || string.IsNullOrEmpty(CodigoArea)) 
                    return null;
                
                foreach (var kvp in CodigosAreaPorRegion)
                {
                    if (kvp.Value.Contains(CodigoArea))
                        return kvp.Key;
                }
                
                return null;
            }
        }

        /// <summary>
        /// Instancia estática que representa un número de teléfono vacío
        /// </summary>
        public static readonly PhoneNumber Empty = new PhoneNumber(string.Empty);

        private PhoneNumber() { }

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
        /// Crea un número de teléfono chileno fijo para una región específica
        /// </summary>
        /// <param name="phoneNumber">Número telefónico a validar</param>
        /// <param name="region">Región de Chile para validar el código de área</param>
        /// <returns>Objeto PhoneNumber validado como teléfono fijo de la región indicada</returns>
        /// <exception cref="ArgumentException">Si el número no corresponde a la región especificada</exception>
        public static PhoneNumber CreateChileanForRegion(string phoneNumber, RegionChile region)
        {
            var phone = CreateChilean(phoneNumber, false);
            
            // Verificar que el código de área corresponda a la región
            string codigoArea = phone.CodigoArea;
            if (string.IsNullOrEmpty(codigoArea) || !CodigosAreaPorRegion[region].Contains(codigoArea))
            {
                throw new ArgumentException($"El número telefónico no corresponde a un teléfono fijo de la región {region}", nameof(phoneNumber));
            }
            
            return phone;
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
        /// Determina si un número telefónico podría pertenecer a una región específica de Chile
        /// </summary>
        /// <param name="region">Región de Chile a verificar</param>
        /// <returns>True si el número corresponde a la región especificada</returns>
        public bool PerteneceARegion(RegionChile region)
        {
            if (!EsFijoChileno || string.IsNullOrEmpty(CodigoArea))
                return false;
                
            return CodigosAreaPorRegion.ContainsKey(region) && 
                   CodigosAreaPorRegion[region].Contains(CodigoArea);
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
            // Es fijo con código de área de 1 dígito (Santiago)
            else if (digitsOnly.StartsWith("2") && digitsOnly.Length >= 9)
            {
                return $"+56 2 {digitsOnly.Substring(1, 4)} {digitsOnly.Substring(5, 4)}";
            }
            // Es fijo con código de área de 2 dígitos (regiones)
            else if (digitsOnly.Length >= 10)
            {
                string areaCode = digitsOnly.Substring(0, 2);
                return $"+56 {areaCode} {digitsOnly.Substring(2, 4)} {digitsOnly.Substring(6, 4)}";
            }
            
            return Value;
        }
        
        /// <summary>
        /// Formatea el número para ser marcado desde Chile
        /// </summary>
        /// <returns>Número telefónico en formato para marcar (ej: 9 1234 5678)</returns>
        public string ToDialFormat()
        {
            if (!EsTelefonoChileno)
                return Value;
                
            string digitsOnly = new string(Value.Where(char.IsDigit).ToArray());
            
            if (digitsOnly.StartsWith("56"))
                digitsOnly = digitsOnly.Substring(2);
                
            // Es móvil
            if (digitsOnly.StartsWith("9") && digitsOnly.Length >= 9)
            {
                return $"9 {digitsOnly.Substring(1, 4)} {digitsOnly.Substring(5, 4)}";
            }
            // Es fijo
            else if (digitsOnly.Length >= 9)
            {
                if (digitsOnly.StartsWith("2")) // Santiago
                {
                    return $"2 {digitsOnly.Substring(1, 4)} {digitsOnly.Substring(5, 4)}";
                }
                else if (digitsOnly.Length >= 10) // Regiones
                {
                    string areaCode = digitsOnly.Substring(0, 2);
                    return $"{areaCode} {digitsOnly.Substring(2, 4)} {digitsOnly.Substring(6, 4)}";
                }
            }
            
            return Value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
} 