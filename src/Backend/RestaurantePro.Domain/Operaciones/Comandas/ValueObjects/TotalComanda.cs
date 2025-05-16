namespace RestaurantePro.Domain.Operaciones.Comandas.ValueObjects
{
    /// <summary>
    /// Value Object que representa el total de una comanda
    /// </summary>
    public class TotalComanda
    {
        /// <summary>
        /// Subtotal (sin impuestos)
        /// </summary>
        public decimal Subtotal { get; }

        /// <summary>
        /// Impuestos aplicados
        /// </summary>
        public decimal Impuestos { get; }

        /// <summary>
        /// Total (subtotal + impuestos)
        /// </summary>
        public decimal Total { get; }

        // Constructor privado
        private TotalComanda(decimal subtotal, decimal impuestos)
        {
            if (subtotal < 0)
                throw new ArgumentException("El subtotal no puede ser negativo", nameof(subtotal));

            if (impuestos < 0)
                throw new ArgumentException("Los impuestos no pueden ser negativos", nameof(impuestos));

            Subtotal = subtotal;
            Impuestos = impuestos;
            Total = subtotal + impuestos;
        }

        /// <summary>
        /// Crea una nueva instancia de TotalComanda
        /// </summary>
        public static TotalComanda Crear(decimal subtotal, decimal impuestos)
        {
            return new TotalComanda(subtotal, impuestos);
        }

        // Sobreescribimos Equals y GetHashCode para implementar valor semántico
        public override bool Equals(object obj)
        {
            if (obj is not TotalComanda other)
                return false;

            return Subtotal == other.Subtotal &&
                   Impuestos == other.Impuestos &&
                   Total == other.Total;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Subtotal, Impuestos, Total);
        }

        // Sobrecarga de operadores de igualdad
        public static bool operator ==(TotalComanda left, TotalComanda right)
        {
            if (left is null && right is null)
                return true;

            if (left is null || right is null)
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(TotalComanda left, TotalComanda right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"Subtotal: {Subtotal:C}, Impuestos: {Impuestos:C}, Total: {Total:C}";
        }
    }
}
