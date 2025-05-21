namespace RestaurantePro.Domain.Core.SharedKernel.ValueObjects
{
    /// <summary>
    /// Objeto de valor que representa una dirección de correo electrónico válida.
    /// </summary>
    public class Email : ValueObject
    {
        // RFC 5322 más restrictiva para evitar errores comunes
        private static readonly Regex EmailRegex = new Regex(
            @"^(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Lista de dominios temporales o desechables prohibidos
        private static readonly HashSet<string> DominiosProhibidos = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "tempmail.com",
            "guerrillamail.com",
            "10minutemail.com",
            "mailinator.com",
            "throwawaymail.com",
            "yopmail.com",
            "fakeinbox.com"
            // Añadir más dominios según sea necesario
        };
        
        // Límites de longitud
        private const int MaxLongitudTotal = 254; // RFC 5321 SMTP limits
        private const int MaxLongitudUsuario = 64; // RFC 5321 SMTP limits
        private const int MaxLongitudDominio = 253; // RFC 1035 DNS limits

        /// <summary>
        /// La dirección de correo en formato texto
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Nombre de usuario (parte antes de @)
        /// </summary>
        public string Username => Value.Split('@')[0];

        /// <summary>
        /// Dominio (parte después de @)
        /// </summary>
        public string Domain => Value.Split('@')[1];

        /// <summary>
        /// Extensión del dominio (.com, .cl, etc.)
        /// </summary>
        public string Extension => Domain.Contains(".") ? Domain.Substring(Domain.LastIndexOf('.')) : string.Empty;

        private Email(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Crea una nueva instancia de Email
        /// </summary>
        /// <param name="email">Dirección de correo a validar</param>
        /// <returns>Objeto Email validado</returns>
        /// <exception cref="ArgumentException">Si el formato no es válido</exception>
        public static Email Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email no puede estar vacío", nameof(email));

            email = email.Trim();
            
            // Validar longitud total
            if (email.Length > MaxLongitudTotal)
                throw new ArgumentException($"El email no puede exceder {MaxLongitudTotal} caracteres", nameof(email));

            if (!EmailRegex.IsMatch(email))
                throw new ArgumentException("El formato del email no es válido", nameof(email));
            
            string[] parts = email.Split('@');
            
            // Validar longitud del usuario
            if (parts[0].Length > MaxLongitudUsuario)
                throw new ArgumentException($"El nombre de usuario no puede exceder {MaxLongitudUsuario} caracteres", nameof(email));
                
            // Validar longitud del dominio
            if (parts[1].Length > MaxLongitudDominio)
                throw new ArgumentException($"El dominio no puede exceder {MaxLongitudDominio} caracteres", nameof(email));
                
            // Validar dominios prohibidos
            if (DominiosProhibidos.Contains(parts[1]))
                throw new ArgumentException($"El dominio {parts[1]} no está permitido para registros", nameof(email));
                
            // Validación de dominios de nivel superior
            string tld = parts[1].Substring(parts[1].LastIndexOf('.') + 1);
            if (tld.Length < 2)
                throw new ArgumentException("El dominio de nivel superior no es válido", nameof(email));
                
            // Validar que el dominio tenga al menos un punto (ej: gmail.com)
            if (!parts[1].Contains("."))
                throw new ArgumentException("El dominio debe contener al menos un punto", nameof(email));

            return new Email(email);
        }

        /// <summary>
        /// Intenta crear un Email sin lanzar excepciones
        /// </summary>
        /// <param name="email">Dirección de correo a validar</param>
        /// <param name="result">Email resultante si es válido</param>
        /// <returns>True si se creó correctamente, False en caso contrario</returns>
        public static bool TryCreate(string email, out Email result)
        {
            result = null;

            try
            {
                result = Create(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Conversión implícita de Email a string
        /// </summary>
        public static implicit operator string(Email email) => email?.Value;

        /// <summary>
        /// Convierte el email a su representación en texto
        /// </summary>
        public override string ToString() => Value;

        /// <summary>
        /// Compara dos emails ignorando mayúsculas/minúsculas
        /// </summary>
        public bool EqualsIgnoreCase(Email other)
        {
            if (other is null) return false;
            return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Verifica si el email tiene un dominio específico
        /// </summary>
        /// <param name="dominio">Dominio a verificar (ej: "gmail.com")</param>
        public bool TieneDominio(string dominio)
        {
            if (string.IsNullOrWhiteSpace(dominio)) return false;
            return string.Equals(Domain, dominio, StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Verifica si el email tiene una extensión de dominio específica
        /// </summary>
        /// <param name="extension">Extensión a verificar (ej: ".cl", ".com")</param>
        public bool TieneExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension)) return false;
            if (!extension.StartsWith(".")) extension = "." + extension;
            return Domain.EndsWith(extension, StringComparison.OrdinalIgnoreCase);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
} 