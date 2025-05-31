namespace RestaurantePro.Domain.Inventario.Ingredientes.Events.Ingrediente
{
    /// <summary>
    /// Evento que se genera cuando se reserva stock de un ingrediente.
    /// La reserva no modifica el stock físico, pero indica que una cantidad está comprometida.
    /// </summary>
    public class StockReservado : DomainEvent
    {
        /// <summary>
        /// ID del ingrediente cuyo stock ha sido reservado
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Cantidad de stock reservada
        /// </summary>
        public decimal CantidadReservada { get; }
        
        /// <summary>
        /// Motivo de la reserva
        /// </summary>
        public string Motivo { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="nombre">Nombre del ingrediente</param>
        /// <param name="cantidadReservada">Cantidad reservada</param>
        /// <param name="motivo">Motivo de la reserva</param>
        public StockReservado(Guid ingredienteId, string nombre, decimal cantidadReservada, string motivo) : base()
        {
            IngredienteId = ingredienteId;
            Nombre = nombre;
            CantidadReservada = cantidadReservada;
            Motivo = motivo;
        }
    }
} 