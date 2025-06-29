namespace RestaurantePro.Domain.Proveedores.Events.EvaluacionProveedor
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se crea una nueva evaluación de proveedor
    /// </summary>
    public class EvaluacionProveedorCreada : DomainEvent
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
        /// Promedio ponderado de la evaluación
        /// </summary>
        public decimal PromedioPonderado { get; }
        
        /// <summary>
        /// Constructor para el evento EvaluacionProveedorCreada
        /// </summary>
        /// <param name="evaluacionId">ID de la evaluación</param>
        /// <param name="proveedorId">ID del proveedor evaluado</param>
        /// <param name="evaluadorId">ID del evaluador</param>
        /// <param name="promedioPonderado">Promedio ponderado de la evaluación</param>
        public EvaluacionProveedorCreada(
            Guid evaluacionId,
            Guid proveedorId,
            Guid evaluadorId,
            decimal promedioPonderado)
        {
            EvaluacionId = evaluacionId;
            ProveedorId = proveedorId;
            EvaluadorId = evaluadorId;
            PromedioPonderado = promedioPonderado;
        }
    }
} 