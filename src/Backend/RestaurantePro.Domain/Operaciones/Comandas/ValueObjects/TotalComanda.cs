namespace RestaurantePro.Domain.Operaciones.Comandas.ValueObjects
{
    /// <summary>
    /// Value Object que representa el total de una comanda
    /// </summary>
    public class TotalComanda : ValueObject
    {
        // Constructor para EF Core
        private TotalComanda() { }

        /// <summary>
        /// Subtotal (sin impuestos)
        /// </summary>
        public decimal Subtotal { get; }

        /// <summary>
        /// Impuestos aplicados
        /// </summary>
        public decimal Impuestos { get; }

        /// <summary>
        /// Descuento por fidelización (si aplica)
        /// </summary>
        public decimal? Descuento { get; }

        /// <summary>
        /// Total (subtotal - descuento + impuestos)
        /// </summary>
        public decimal Total { get; }

        // Constructor privado
        private TotalComanda(decimal subtotal, decimal impuestos, decimal? descuento = null)
        {
            if (subtotal < 0)
                throw new ArgumentException("El subtotal no puede ser negativo", nameof(subtotal));

            if (impuestos < 0)
                throw new ArgumentException("Los impuestos no pueden ser negativos", nameof(impuestos));

            if (descuento < 0)
                throw new ArgumentException("El descuento no puede ser negativo", nameof(descuento));

            Subtotal = subtotal;
            Impuestos = impuestos;
            Descuento = descuento;

            // Calcular total considerando el descuento
            if (descuento.HasValue && descuento.Value > 0)
            {
                // El descuento se aplica sobre el subtotal y luego se calculan los impuestos
                Total = subtotal - descuento.Value + impuestos;
            }
            else
            {
                Total = subtotal + impuestos;
            }
        }

        /// <summary>
        /// Crea una nueva instancia de TotalComanda
        /// </summary>
        public static TotalComanda Crear(decimal subtotal, decimal impuestos, decimal? descuento = null)
        {
            return new TotalComanda(subtotal, impuestos, descuento);
        }

        /// <summary>
        /// Implementación requerida por la clase base ValueObject
        /// </summary>
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Subtotal;
            yield return Impuestos;
            
            if (Descuento.HasValue)
                yield return Descuento.Value;
                
            yield return Total;
        }

        public override string ToString()
        {
            if (Descuento.HasValue && Descuento.Value > 0)
                return $"Subtotal: {Subtotal:C}, Descuento: {Descuento:C}, Impuestos: {Impuestos:C}, Total: {Total:C}";
            else
                return $"Subtotal: {Subtotal:C}, Impuestos: {Impuestos:C}, Total: {Total:C}";
        }
    }
}
