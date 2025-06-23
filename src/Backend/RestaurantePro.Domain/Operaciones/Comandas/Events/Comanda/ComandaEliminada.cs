namespace RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda
{
    /// <summary>
    /// Evento emitido cuando se elimina una comanda (soft delete)
    /// </summary>
    public class ComandaEliminada : DomainEvent
    {
        /// <summary>
        /// ID de la comanda eliminada
        /// </summary>
        public Guid ComandaId { get; }

        /// <summary>
        /// ID del usuario que realizó la eliminación
        /// </summary>
        public string UsuarioId { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ComandaEliminada(Guid comandaId, string usuarioId)
        {
            ComandaId = comandaId;
            UsuarioId = usuarioId;
        }
    }
} 