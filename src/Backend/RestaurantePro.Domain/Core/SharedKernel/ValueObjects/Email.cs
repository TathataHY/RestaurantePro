namespace RestaurantePro.Domain.Core.SharedKernel.ValueObjects
{
    /// <summary>
    /// Objeto de valor que representa una dirección de correo electrónico válida.
    /// </summary>
    public class Email : ValueObject
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

            if (!EmailRegex.IsMatch(email))
                throw new ArgumentException("El formato del email no es válido", nameof(email));

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

            if (string.IsNullOrWhiteSpace(email))
                return false;

            email = email.Trim();

            if (!EmailRegex.IsMatch(email))
                return false;

            result = new Email(email);
            return true;
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

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
} 