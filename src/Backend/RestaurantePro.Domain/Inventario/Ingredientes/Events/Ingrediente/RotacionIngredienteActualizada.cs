namespace RestaurantePro.Domain.Inventario.Ingredientes.Events.Ingrediente
{
    /// <summary>
    /// Evento que se genera cuando se actualiza el nivel de rotación de un ingrediente.
    /// </summary>
    public class RotacionIngredienteActualizada : DomainEvent
    {
        /// <summary>
        /// ID del ingrediente cuya rotación ha sido actualizada
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Nuevo nivel de rotación asignado
        /// </summary>
        public RotacionIngrediente Rotacion { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="nombre">Nombre del ingrediente</param>
        /// <param name="rotacion">Nuevo nivel de rotación</param>
        public RotacionIngredienteActualizada(Guid ingredienteId, string nombre, RotacionIngrediente rotacion) : base()
        {
            IngredienteId = ingredienteId;
            Nombre = nombre;
            Rotacion = rotacion;
        }
    }
} 
