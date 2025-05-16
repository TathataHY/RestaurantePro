
namespace RestaurantePro.Domain.Core.Productos.ValueObjects
{
    /// <summary>
    /// Value Object que representa el precio de un producto
    /// </summary>
    public class PrecioProducto : ValueObject
    {
        /// <summary>
        /// Valor del precio
        /// </summary>
        public decimal Valor { get; private set; }

        /// <summary>
        /// Constructor que inicializa un nuevo precio de producto
        /// </summary>
        /// <param name="valor">Valor del precio</param>
        /// <exception cref="ArgumentException">Se lanza si el valor es igual o menor a cero</exception>
        public PrecioProducto(decimal valor)
        {
            if (valor <= 0)
            {
                throw new ArgumentException("El precio debe ser mayor que cero", nameof(valor));
            }

            Valor = valor;
        }

        /// <summary>
        /// Devuelve las propiedades que definen la igualdad del objeto de valor
        /// </summary>
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Valor;
        }
    }
} 