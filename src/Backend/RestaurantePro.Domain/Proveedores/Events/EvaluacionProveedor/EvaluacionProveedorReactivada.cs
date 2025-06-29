namespace RestaurantePro.Domain.Proveedores.Events.EvaluacionProveedor
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se reactiva una evaluación de proveedor
    /// </summary>
    public class EvaluacionProveedorReactivada : DomainEvent
    {
        /// <summary>
        /// ID de la evaluación
        /// </summary>
        public Guid EvaluacionId { get; }
        
        /// <summary>
        /// ID del proveedor evaluado
        /// </summary>
        public Guid ProveedorId { get; }
        
        /// <summary>
        /// ID del evaluador
        /// </summary>
        public Guid EvaluadorId { get; }
        
        /// <summary>
        /// Constructor para el evento EvaluacionProveedorReactivada
        /// </summary>
        /// <param name="evaluacionId">ID de la evaluación</param>
        /// <param name="proveedorId">ID del proveedor evaluado</param>
        /// <param name="evaluadorId">ID del evaluador</param>
        public EvaluacionProveedorReactivada(
            Guid evaluacionId,
            Guid proveedorId,
            Guid evaluadorId)
        {
            EvaluacionId = evaluacionId;
            ProveedorId = proveedorId;
            EvaluadorId = evaluadorId;
        }
    }
} 