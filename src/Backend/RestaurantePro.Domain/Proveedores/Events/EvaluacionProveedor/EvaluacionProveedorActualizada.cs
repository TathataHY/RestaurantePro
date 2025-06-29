namespace RestaurantePro.Domain.Proveedores.Events.EvaluacionProveedor
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se actualiza una evaluación de proveedor
    /// </summary>
    public class EvaluacionProveedorActualizada : DomainEvent
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
        /// Promedio ponderado anterior
        /// </summary>
        public decimal PromedioPonderadoAnterior { get; }
        
        /// <summary>
        /// Nuevo promedio ponderado
        /// </summary>
        public decimal PromedioPonderadoNuevo { get; }
        
        /// <summary>
        /// Constructor para el evento EvaluacionProveedorActualizada
        /// </summary>
        /// <param name="evaluacionId">ID de la evaluación</param>
        /// <param name="proveedorId">ID del proveedor evaluado</param>
        /// <param name="evaluadorId">ID del evaluador</param>
        /// <param name="promedioPonderadoAnterior">Promedio ponderado anterior</param>
        /// <param name="promedioPonderadoNuevo">Nuevo promedio ponderado</param>
        public EvaluacionProveedorActualizada(
            Guid evaluacionId,
            Guid proveedorId,
            Guid evaluadorId,
            decimal promedioPonderadoAnterior,
            decimal promedioPonderadoNuevo)
        {
            EvaluacionId = evaluacionId;
            ProveedorId = proveedorId;
            EvaluadorId = evaluadorId;
            PromedioPonderadoAnterior = promedioPonderadoAnterior;
            PromedioPonderadoNuevo = promedioPonderadoNuevo;
        }
    }
} 