using System;
using System.Globalization;

namespace RestaurantePro.Domain.Core.SharedKernel.ValueObjects
{
    /// <summary>
    /// ValueObject para representar dinero de forma consistente en todo el sistema
    /// </summary>
    public class Money
    {
        public decimal Amount { get; }
        public string Currency { get; }
        
        private Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }
        
        public static Money FromDecimal(decimal amount, string currency = "MXN")
        {
            // Validación de importe
            if (amount < 0)
                throw new ArgumentException("El importe no puede ser negativo", nameof(amount));
                
            // Validación de moneda
            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("La moneda es requerida", nameof(currency));
                
            if (currency.Length != 3)
                throw new ArgumentException("El código de moneda debe tener 3 caracteres", nameof(currency));
            
            return new Money(
                Math.Round(amount, 2, MidpointRounding.AwayFromZero), 
                currency.ToUpperInvariant());
        }
        
        public static Money Zero(string currency = "MXN")
        {
            return FromDecimal(0, currency);
        }
        
        // Operaciones básicas
        public Money Add(Money other)
        {
            EnsureSameCurrency(other);
            return FromDecimal(Amount + other.Amount, Currency);
        }
        
        public Money Subtract(Money other)
        {
            EnsureSameCurrency(other);
            return FromDecimal(Amount - other.Amount, Currency);
        }
        
        public Money Multiply(decimal factor)
        {
            return FromDecimal(Amount * factor, Currency);
        }
        
        // Validación de misma moneda
        private void EnsureSameCurrency(Money other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
                
            if (Currency != other.Currency)
                throw new InvalidOperationException($"No se pueden realizar operaciones entre monedas diferentes: {Currency} y {other.Currency}");
        }
        
        // Representación como cadena
        public override string ToString()
        {
            return Amount.ToString("C", CultureInfo.CurrentCulture) + " " + Currency;
        }
        
        // Igualdad y hash
        public override bool Equals(object obj)
        {
            if (obj is not Money other)
                return false;
                
            return Amount == other.Amount && Currency == other.Currency;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(Amount, Currency);
        }
        
        // Operadores
        public static Money operator +(Money left, Money right)
        {
            return left.Add(right);
        }
        
        public static Money operator -(Money left, Money right)
        {
            return left.Subtract(right);
        }
        
        public static Money operator *(Money left, decimal right)
        {
            return left.Multiply(right);
        }
        
        public static bool operator ==(Money left, Money right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
                
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
                
            return left.Equals(right);
        }
        
        public static bool operator !=(Money left, Money right)
        {
            return !(left == right);
        }
    }
}
