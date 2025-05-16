namespace RestaurantePro.Domain.Core.SharedKernel.ValueObjects
{
    /// <summary>
    /// Objeto de valor que representa un número telefónico válido.
    /// </summary>
    public class PhoneNumber : ValueObject
    {
        private static readonly Regex PhoneRegex = new Regex(
            @"^[\d\s\(\)\+\-\.]{8,20}$",
            RegexOptions.Compiled);

        /// <summary>
        /// El número telefónico en formato normalizado
        /// </summary>
        public string Value { get; }

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
        /// Conversión implícita de PhoneNumber a string
        /// </summary>
        public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber?.Value;

        /// <summary>
        /// Convierte el número telefónico a su representación en texto
        /// </summary>
        public override string ToString() => Value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
} 