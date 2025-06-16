namespace RestaurantePro.Domain.Core.Productos.ValueObjects
{
    /// <summary>
    /// Value Object que representa un ingrediente dentro de una receta
    /// </summary>
    public class IngredienteReceta : ValueObject
    {
        /// <summary>
        /// ID del ingrediente
        /// </summary>
        public Guid IngredienteId { get; }

        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string Nombre { get; }

        /// <summary>
        /// Cantidad requerida
        /// </summary>
        public decimal Cantidad { get; }

        /// <summary>
        /// Unidad de medida
        /// </summary>
        public Inventario.Ingredientes.Enums.UnidadMedida UnidadMedida { get; }

        /// <summary>
        /// Indica si el ingrediente es opcional
        /// </summary>
        public bool EsOpcional { get; }

        /// <summary>
        /// Constructor para EF Core
        /// </summary>
        private IngredienteReceta() { }

        /// <summary>
        /// Constructor privado
        /// </summary>
        private IngredienteReceta(
            Guid ingredienteId,
            string nombre,
            decimal cantidad,
            Inventario.Ingredientes.Enums.UnidadMedida unidadMedida,
            bool esOpcional)
        {
            IngredienteId = ingredienteId;
            Nombre = nombre;
            Cantidad = cantidad;
            UnidadMedida = unidadMedida;
            EsOpcional = esOpcional;
        }

        /// <summary>
        /// Factory method para crear un nuevo ingrediente de receta
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="nombre">Nombre del ingrediente</param>
        /// <param name="cantidad">Cantidad requerida</param>
        /// <param name="unidadMedida">Unidad de medida</param>
        /// <param name="esOpcional">Indica si el ingrediente es opcional</param>
        /// <returns>Nuevo IngredienteReceta</returns>
        public static IngredienteReceta Crear(
            Guid ingredienteId,
            string nombre,
            decimal cantidad,
            Inventario.Ingredientes.Enums.UnidadMedida unidadMedida,
            bool esOpcional = false)
        {
            if (ingredienteId == Guid.Empty)
                throw new ArgumentException("El ID del ingrediente no puede estar vacío", nameof(ingredienteId));

            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del ingrediente no puede estar vacío", nameof(nombre));

            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero", nameof(cantidad));

            return new IngredienteReceta(ingredienteId, nombre, cantidad, unidadMedida, esOpcional);
        }

        /// <summary>
        /// Crea una nueva instancia con una cantidad diferente
        /// </summary>
        /// <param name="nuevaCantidad">Nueva cantidad</param>
        /// <returns>Nuevo IngredienteReceta con la cantidad actualizada</returns>
        public IngredienteReceta ConCantidad(decimal nuevaCantidad)
        {
            if (nuevaCantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero", nameof(nuevaCantidad));

            return new IngredienteReceta(IngredienteId, Nombre, nuevaCantidad, UnidadMedida, EsOpcional);
        }

        /// <summary>
        /// Crea una nueva instancia cambiando si es opcional
        /// </summary>
        /// <param name="esOpcional">Nuevo valor para esOpcional</param>
        /// <returns>Nuevo IngredienteReceta con el valor actualizado</returns>
        public IngredienteReceta ConEsOpcional(bool esOpcional)
        {
            return new IngredienteReceta(IngredienteId, Nombre, Cantidad, UnidadMedida, esOpcional);
        }

        /// <inheritdoc/>
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return IngredienteId;
            yield return Nombre;
            yield return Cantidad;
            yield return UnidadMedida;
            yield return EsOpcional;
        }
    }
} 