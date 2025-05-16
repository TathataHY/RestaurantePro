namespace RestaurantePro.Domain.Inventario.Ingredientes.Events
{
    /// <summary>
    /// Evento que se dispara cuando se asocia un proveedor principal a un ingrediente
    /// </summary>
    public class ProveedorPrincipalAsociadoEvent : IDomainEvent
    {
        /// <summary>
        /// Momento en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;
        
        /// <summary>
        /// ID del ingrediente
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string IngredienteNombre { get; }
        
        /// <summary>
        /// ID del proveedor asociado
        /// </summary>
        public Guid ProveedorId { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="ingredienteNombre">Nombre del ingrediente</param>
        /// <param name="proveedorId">ID del proveedor</param>
        public ProveedorPrincipalAsociadoEvent(Guid ingredienteId, string ingredienteNombre, Guid proveedorId)
        {
            IngredienteId = ingredienteId;
            IngredienteNombre = ingredienteNombre;
            ProveedorId = proveedorId;
        }
    }
} 