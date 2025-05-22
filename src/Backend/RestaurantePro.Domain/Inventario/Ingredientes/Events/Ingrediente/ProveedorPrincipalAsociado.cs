namespace RestaurantePro.Domain.Inventario.Ingredientes.Events.Ingrediente
{
    /// <summary>
    /// Evento que se dispara cuando se asocia un proveedor principal a un ingrediente
    /// </summary>
    public class ProveedorPrincipalAsociado : DomainEvent
    {
        /// <summary>
        /// ID del ingrediente
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// ID del proveedor asociado
        /// </summary>
        public Guid ProveedorId { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="proveedorId">ID del proveedor</param>
        public ProveedorPrincipalAsociado(Guid ingredienteId, Guid proveedorId) : base()
        {
            IngredienteId = ingredienteId;
            ProveedorId = proveedorId;
        }
    }
} 


