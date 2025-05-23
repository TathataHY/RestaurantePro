namespace RestaurantePro.Domain.Comercial.Promociones.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se crea una nueva promoción
    /// </summary>
    public class PromocionCreada : DomainEvent
    {
        /// <summary>
        /// Identificador único de la promoción
        /// </summary>
        public Guid PromocionId { get; }
        
        /// <summary>
        /// Código de la promoción
        /// </summary>
        public string Codigo { get; }
        
        /// <summary>
        /// Nombre de la promoción
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Tipo de promoción
        /// </summary>
        public TipoPromocion Tipo { get; }
        
        /// <summary>
        /// Valor del descuento
        /// </summary>
        public decimal ValorDescuento { get; }
        
        /// <summary>
        /// Fecha de inicio de la promoción
        /// </summary>
        public DateTime FechaInicio { get; }
        
        /// <summary>
        /// Fecha de fin de la promoción
        /// </summary>
        public DateTime FechaFin { get; }
        
        /// <summary>
        /// Constructor para el evento PromocionCreada
        /// </summary>
        /// <param name="promocionId">Identificador de la promoción</param>
        /// <param name="codigo">Código de la promoción</param>
        /// <param name="nombre">Nombre de la promoción</param>
        /// <param name="tipo">Tipo de la promoción</param>
        /// <param name="valorDescuento">Valor del descuento</param>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        public PromocionCreada(
            Guid promocionId, 
            string codigo, 
            string nombre, 
            TipoPromocion tipo, 
            decimal valorDescuento, 
            DateTime fechaInicio, 
            DateTime fechaFin)
        {
            PromocionId = promocionId;
            Codigo = codigo;
            Nombre = nombre;
            Tipo = tipo;
            ValorDescuento = valorDescuento;
            FechaInicio = fechaInicio;
            FechaFin = fechaFin;
        }
    }
} 