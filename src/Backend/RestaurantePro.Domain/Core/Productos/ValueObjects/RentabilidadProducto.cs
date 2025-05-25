namespace RestaurantePro.Domain.Core.Productos.ValueObjects
{
    /// <summary>
    /// Value Object que representa la rentabilidad de un producto basado en su costo y precio de venta
    /// </summary>
    public class RentabilidadProducto : ValueObject
    {
        /// <summary>
        /// Costo total del producto basado en sus ingredientes
        /// </summary>
        public decimal CostoTotal { get; }

        /// <summary>
        /// Precio de venta del producto
        /// </summary>
        public decimal PrecioVenta { get; }

        /// <summary>
        /// Margen de ganancia (PrecioVenta - CostoTotal)
        /// </summary>
        public decimal MargenGanancia { get; }

        /// <summary>
        /// Rentabilidad en porcentaje ((MargenGanancia / PrecioVenta) * 100)
        /// </summary>
        public decimal Rentabilidad { get; }

        /// <summary>
        /// Nivel de rentabilidad categorizado
        /// </summary>
        public NivelRentabilidad Nivel { get; }

        /// <summary>
        /// Constructor privado
        /// </summary>
        private RentabilidadProducto(
            decimal costoTotal,
            decimal precioVenta,
            decimal margenGanancia,
            decimal rentabilidad,
            NivelRentabilidad nivel)
        {
            CostoTotal = costoTotal;
            PrecioVenta = precioVenta;
            MargenGanancia = margenGanancia;
            Rentabilidad = rentabilidad;
            Nivel = nivel;
        }

        /// <summary>
        /// Factory method para crear una nueva instancia de RentabilidadProducto
        /// </summary>
        /// <param name="costoTotal">Costo total del producto</param>
        /// <param name="precioVenta">Precio de venta del producto</param>
        /// <returns>Nueva instancia de RentabilidadProducto</returns>
        public static RentabilidadProducto Calcular(decimal costoTotal, decimal precioVenta)
        {
            if (precioVenta < 0)
                throw new ArgumentException("El precio de venta no puede ser negativo", nameof(precioVenta));

            if (costoTotal < 0)
                throw new ArgumentException("El costo total no puede ser negativo", nameof(costoTotal));

            // Calcular margen de ganancia
            decimal margenGanancia = precioVenta - costoTotal;

            // Calcular rentabilidad en porcentaje
            decimal rentabilidad = 0;
            if (precioVenta > 0)
            {
                rentabilidad = Math.Round((margenGanancia / precioVenta) * 100, 1);
            }
            else if (costoTotal == 0)
            {
                rentabilidad = 100; // Si no hay costo, asumimos 100% de rentabilidad
            }

            // Determinar nivel de rentabilidad
            var nivel = DeterminarNivelRentabilidad(rentabilidad);

            return new RentabilidadProducto(costoTotal, precioVenta, margenGanancia, rentabilidad, nivel);
        }

        /// <summary>
        /// Determina el nivel de rentabilidad basado en el porcentaje
        /// </summary>
        /// <param name="rentabilidadPorcentaje">Porcentaje de rentabilidad</param>
        /// <returns>Nivel de rentabilidad</returns>
        private static NivelRentabilidad DeterminarNivelRentabilidad(decimal rentabilidadPorcentaje)
        {
            if (rentabilidadPorcentaje < 0)
                return NivelRentabilidad.Negativa;

            if (rentabilidadPorcentaje < 20)
                return NivelRentabilidad.Baja;

            if (rentabilidadPorcentaje < 40)
                return NivelRentabilidad.Media;

            if (rentabilidadPorcentaje < 60)
                return NivelRentabilidad.Alta;

            return NivelRentabilidad.MuyAlta;
        }

        /// <inheritdoc/>
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return CostoTotal;
            yield return PrecioVenta;
            yield return MargenGanancia;
            yield return Rentabilidad;
            yield return Nivel;
        }
    }

    /// <summary>
    /// Enumeración para clasificar el nivel de rentabilidad de un producto
    /// </summary>
    public enum NivelRentabilidad
    {
        /// <summary>
        /// Rentabilidad negativa (pérdida)
        /// </summary>
        Negativa,

        /// <summary>
        /// Rentabilidad baja (0-20%)
        /// </summary>
        Baja,

        /// <summary>
        /// Rentabilidad media (20-40%)
        /// </summary>
        Media,

        /// <summary>
        /// Rentabilidad alta (40-60%)
        /// </summary>
        Alta,

        /// <summary>
        /// Rentabilidad muy alta (60%+)
        /// </summary>
        MuyAlta
    }
} 