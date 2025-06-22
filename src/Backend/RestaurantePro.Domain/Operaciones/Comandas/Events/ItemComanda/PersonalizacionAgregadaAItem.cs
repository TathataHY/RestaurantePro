namespace RestaurantePro.Domain.Operaciones.Comandas.Events.ItemComanda
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se agrega una personalización a un ítem de comanda
    /// </summary>
    public class PersonalizacionAgregadaAItem : DomainEvent
    {
        /// <summary>
        /// ID del ítem de comanda
        /// </summary>
        public Guid ItemId { get; }

        /// <summary>
        /// ID de la comanda
        /// </summary>
        public Guid ComandaId { get; }

        /// <summary>
        /// ID del ingrediente personalizado
        /// </summary>
        public Guid IngredienteId { get; }

        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string NombreIngrediente { get; }

        /// <summary>
        /// Acción de personalización
        /// </summary>
        public AccionPersonalizacion Accion { get; }

        /// <summary>
        /// Cantidad de la personalización
        /// </summary>
        public decimal Cantidad { get; }

        /// <summary>
        /// Precio adicional de la personalización
        /// </summary>
        public decimal PrecioAdicional { get; }

        /// <summary>
        /// ID del ingrediente de sustitución (si aplica)
        /// </summary>
        public Guid? IngredienteSustitucionId { get; }

        /// <summary>
        /// Nombre del ingrediente de sustitución (si aplica)
        /// </summary>
        public string? NombreIngredienteSustitucion { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PersonalizacionAgregadaAItem(
            Guid itemId, 
            Guid comandaId, 
            Guid ingredienteId,
            string nombreIngrediente,
            AccionPersonalizacion accion,
            decimal cantidad,
            decimal precioAdicional,
            Guid? ingredienteSustitucionId = null,
            string? nombreIngredienteSustitucion = null)
        {
            ItemId = itemId;
            ComandaId = comandaId;
            IngredienteId = ingredienteId;
            NombreIngrediente = nombreIngrediente;
            Accion = accion;
            Cantidad = cantidad;
            PrecioAdicional = precioAdicional;
            IngredienteSustitucionId = ingredienteSustitucionId;
            NombreIngredienteSustitucion = nombreIngredienteSustitucion;
        }
    }
} 
