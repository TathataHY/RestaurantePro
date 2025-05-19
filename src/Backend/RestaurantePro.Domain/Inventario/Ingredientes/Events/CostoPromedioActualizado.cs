namespace RestaurantePro.Domain.Inventario.Ingredientes.Events
{
    /// <summary>
    /// Evento que se genera cuando se actualiza el costo promedio de un ingrediente.
    /// </summary>
    public class CostoPromedioActualizado : DomainEvent
    {
        /// <summary>
        /// ID del ingrediente cuyo costo ha sido actualizado
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Nuevo costo promedio
        /// </summary>
        public decimal CostoPromedio { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="nombre">Nombre del ingrediente</param>
        /// <param name="costoPromedio">Nuevo costo promedio</param>
        public CostoPromedioActualizado(Guid ingredienteId, string nombre, decimal costoPromedio) : base()
        {
            IngredienteId = ingredienteId;
            Nombre = nombre;
            CostoPromedio = costoPromedio;
        }
    }
} 