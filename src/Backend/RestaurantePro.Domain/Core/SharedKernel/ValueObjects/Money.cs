namespace RestaurantePro.Domain.Core.SharedKernel.ValueObjects
{
    /// <summary>
    /// Objeto de valor que representa un valor monetario con su moneda
    /// </summary>
    public class Money : ValueObject
    {
        /// <summary>
        /// Valor del dinero con precisión decimal
        /// </summary>
        public decimal Amount { get; }
        
        /// <summary>
        /// Código ISO de la moneda (MXN, USD, EUR, etc.)
        /// </summary>
        public string Currency { get; }
        
        /// <summary>
        /// Indica si el valor es cero
        /// </summary>
        public bool IsZero => Amount == 0;
        
        /// <summary>
        /// Indica si el valor es positivo (mayor que cero)
        /// </summary>
        public bool IsPositive => Amount > 0;
        
        /// <summary>
        /// Indica si el valor es negativo (menor que cero)
        /// </summary>
        public bool IsNegative => Amount < 0;

        private Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        /// <summary>
        /// Crea un nuevo valor monetario
        /// </summary>
        /// <param name="amount">Cantidad</param>
        /// <param name="currency">Código de moneda (por defecto MXN)</param>
        /// <returns>Objeto Money validado</returns>
        public static Money Create(decimal amount, string currency = "MXN")
        {
            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("La moneda no puede estar vacía", nameof(currency));
                
            // Normalizar a exactamente 2 decimales
            amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
                
            return new Money(amount, currency.Trim().ToUpperInvariant());
        }
        
        /// <summary>
        /// Crea un nuevo valor monetario de cero
        /// </summary>
        /// <param name="currency">Código de moneda (por defecto MXN)</param>
        /// <returns>Objeto Money con valor cero</returns>
        public static Money Zero(string currency = "MXN")
        {
            return Create(0, currency);
        }
        
        /// <summary>
        /// Suma dos valores monetarios (deben tener la misma moneda)
        /// </summary>
        /// <param name="other">Otro valor monetario</param>
        /// <returns>Un nuevo objeto Money con la suma</returns>
        /// <exception cref="InvalidOperationException">Si las monedas son diferentes</exception>
        public Money Add(Money other)
        {
            if (other == null) 
                return this;
                
            if (Currency != other.Currency)
                throw new InvalidOperationException($"No se pueden sumar montos en diferentes monedas: {Currency} y {other.Currency}");
                
            return Create(Amount + other.Amount, Currency);
        }
        
        /// <summary>
        /// Resta dos valores monetarios (deben tener la misma moneda)
        /// </summary>
        /// <param name="other">Otro valor monetario</param>
        /// <returns>Un nuevo objeto Money con la resta</returns>
        /// <exception cref="InvalidOperationException">Si las monedas son diferentes</exception>
        public Money Subtract(Money other)
        {
            if (other == null) 
                return this;
                
            if (Currency != other.Currency)
                throw new InvalidOperationException($"No se pueden restar montos en diferentes monedas: {Currency} y {other.Currency}");
                
            return Create(Amount - other.Amount, Currency);
        }
        
        /// <summary>
        /// Multiplica el valor monetario por un factor
        /// </summary>
        /// <param name="factor">Factor de multiplicación</param>
        /// <returns>Un nuevo objeto Money con el resultado</returns>
        public Money Multiply(decimal factor)
        {
            return Create(Amount * factor, Currency);
        }
        
        /// <summary>
        /// Divide el valor monetario por un divisor
        /// </summary>
        /// <param name="divisor">Divisor (no puede ser cero)</param>
        /// <returns>Un nuevo objeto Money con el resultado</returns>
        /// <exception cref="DivideByZeroException">Si el divisor es cero</exception>
        public Money Divide(decimal divisor)
        {
            if (divisor == 0)
                throw new DivideByZeroException("No se puede dividir por cero");
                
            return Create(Amount / divisor, Currency);
        }
        
        /// <summary>
        /// Obtiene el valor absoluto del monto
        /// </summary>
        /// <returns>Un nuevo objeto Money con el valor absoluto</returns>
        public Money Abs()
        {
            return IsNegative ? Create(-Amount, Currency) : this;
        }

        // Sobrecarga de operadores 
        public static Money operator +(Money left, Money right) => left.Add(right);
        public static Money operator -(Money left, Money right) => left.Subtract(right);
        public static Money operator *(Money left, decimal right) => left.Multiply(right);
        public static Money operator /(Money left, decimal right) => left.Divide(right);
        public static bool operator >(Money left, Money right) => 
            left.Currency == right.Currency && left.Amount > right.Amount;
        public static bool operator <(Money left, Money right) => 
            left.Currency == right.Currency && left.Amount < right.Amount;
        public static bool operator >=(Money left, Money right) => 
            left.Currency == right.Currency && left.Amount >= right.Amount;
        public static bool operator <=(Money left, Money right) => 
            left.Currency == right.Currency && left.Amount <= right.Amount;

        /// <summary>
        /// Representa el valor monetario como una cadena de texto
        /// </summary>
        public override string ToString()
        {
            return $"{Amount:N2} {Currency}";
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }
}
