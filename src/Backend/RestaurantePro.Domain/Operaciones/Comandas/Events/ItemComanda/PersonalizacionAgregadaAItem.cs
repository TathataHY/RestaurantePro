using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Operaciones.Comandas.ValueObjects;

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
        /// Datos de la personalización agregada
        /// </summary>
        public PersonalizacionItemDto Personalizacion { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PersonalizacionAgregadaAItem(Guid itemId, Guid comandaId, PersonalizacionItemDto personalizacion)
        {
            ItemId = itemId;
            ComandaId = comandaId;
            Personalizacion = personalizacion;
        }
    }

    /// <summary>
    /// DTO para transferir datos de personalización en eventos
    /// </summary>
    public class PersonalizacionItemDto
    {
        public Guid IngredienteId { get; }
        public string NombreIngrediente { get; }
        public AccionPersonalizacion Accion { get; }
        public decimal Cantidad { get; }
        public decimal PrecioAdicional { get; }
        public Guid? IngredienteSustitucionId { get; }
        public string? NombreIngredienteSustitucion { get; }

        public PersonalizacionItemDto(
            Guid ingredienteId,
            string nombreIngrediente,
            AccionPersonalizacion accion,
            decimal cantidad,
            decimal precioAdicional,
            Guid? ingredienteSustitucionId = null,
            string? nombreIngredienteSustitucion = null)
        {
            IngredienteId = ingredienteId;
            NombreIngrediente = nombreIngrediente;
            Accion = accion;
            Cantidad = cantidad;
            PrecioAdicional = precioAdicional;
            IngredienteSustitucionId = ingredienteSustitucionId;
            NombreIngredienteSustitucion = nombreIngredienteSustitucion;
        }

        /// <summary>
        /// Crea un DTO a partir de un objeto PersonalizacionItem
        /// </summary>
        public static PersonalizacionItemDto FromPersonalizacionItem(PersonalizacionItem personalizacion)
        {
            return new PersonalizacionItemDto(
                personalizacion.IngredienteId,
                personalizacion.NombreIngrediente,
                personalizacion.Accion,
                personalizacion.Cantidad,
                personalizacion.PrecioAdicional,
                personalizacion.IngredienteSustitucionId,
                personalizacion.NombreIngredienteSustitucion
            );
        }
    }
} 