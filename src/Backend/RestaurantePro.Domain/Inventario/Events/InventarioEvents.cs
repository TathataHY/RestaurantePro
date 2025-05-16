using RestaurantePro.Domain.Core.Base.Interfaces;

namespace RestaurantePro.Domain.Inventario.Events
{
    /// <summary>
    /// Evento de dominio que se lanza cuando se crea un nuevo ingrediente
    /// </summary>
    public class IngredienteCreadoEvent : IDomainEvent
    {
        /// <summary>
        /// Fecha y hora en que ocurriu00f3 el evento
        /// </summary>
        public DateTime OccurredOn { get; }
        
        /// <summary>
        /// Id del ingrediente creado
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente creado
        /// </summary>
        public string Nombre { get; }
        
        public IngredienteCreadoEvent(Guid ingredienteId, string nombre)
        {
            OccurredOn = DateTime.UtcNow;
            IngredienteId = ingredienteId;
            Nombre = nombre;
        }
    }
    
    /// <summary>
    /// Evento de dominio que se lanza cuando se actualiza el stock de un ingrediente
    /// </summary>
    public class StockActualizadoEvent : IDomainEvent
    {
        /// <summary>
        /// Fecha y hora en que ocurriu00f3 el evento
        /// </summary>
        public DateTime OccurredOn { get; }
        
        /// <summary>
        /// Id del ingrediente
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string IngredienteNombre { get; }
        
        /// <summary>
        /// Nuevo stock del ingrediente
        /// </summary>
        public decimal NuevoStock { get; }
        
        public StockActualizadoEvent(Guid ingredienteId, string ingredienteNombre, decimal nuevoStock)
        {
            OccurredOn = DateTime.UtcNow;
            IngredienteId = ingredienteId;
            IngredienteNombre = ingredienteNombre;
            NuevoStock = nuevoStock;
        }
    }
    
    /// <summary>
    /// Evento de dominio que se lanza cuando el stock de un ingrediente cae por debajo del mu00ednimo
    /// </summary>
    public class StockBajoMinimoEvent : IDomainEvent
    {
        /// <summary>
        /// Fecha y hora en que ocurriu00f3 el evento
        /// </summary>
        public DateTime OccurredOn { get; }
        
        /// <summary>
        /// Id del ingrediente
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string IngredienteNombre { get; }
        
        /// <summary>
        /// Stock actual del ingrediente
        /// </summary>
        public decimal StockActual { get; }
        
        /// <summary>
        /// Stock mu00ednimo del ingrediente
        /// </summary>
        public decimal StockMinimo { get; }
        
        public StockBajoMinimoEvent(Guid ingredienteId, string ingredienteNombre, decimal stockActual, decimal stockMinimo)
        {
            OccurredOn = DateTime.UtcNow;
            IngredienteId = ingredienteId;
            IngredienteNombre = ingredienteNombre;
            StockActual = stockActual;
            StockMinimo = stockMinimo;
        }
    }
    
    /// <summary>
    /// Evento de dominio que se lanza cuando se desactiva un ingrediente
    /// </summary>
    public class IngredienteDesactivadoEvent : IDomainEvent
    {
        /// <summary>
        /// Fecha y hora en que ocurriu00f3 el evento
        /// </summary>
        public DateTime OccurredOn { get; }
        
        /// <summary>
        /// Id del ingrediente desactivado
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente desactivado
        /// </summary>
        public string Nombre { get; }
        
        public IngredienteDesactivadoEvent(Guid ingredienteId, string nombre)
        {
            OccurredOn = DateTime.UtcNow;
            IngredienteId = ingredienteId;
            Nombre = nombre;
        }
    }
    
    /// <summary>
    /// Evento de dominio que se lanza cuando se activa un ingrediente
    /// </summary>
    public class IngredienteActivadoEvent : IDomainEvent
    {
        /// <summary>
        /// Fecha y hora en que ocurriu00f3 el evento
        /// </summary>
        public DateTime OccurredOn { get; }
        
        /// <summary>
        /// Id del ingrediente activado
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente activado
        /// </summary>
        public string Nombre { get; }
        
        public IngredienteActivadoEvent(Guid ingredienteId, string nombre)
        {
            OccurredOn = DateTime.UtcNow;
            IngredienteId = ingredienteId;
            Nombre = nombre;
        }
    }
}
