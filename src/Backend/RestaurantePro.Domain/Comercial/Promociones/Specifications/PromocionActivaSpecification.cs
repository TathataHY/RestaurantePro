namespace RestaurantePro.Domain.Comercial.Promociones.Specifications
{
    /// <summary>
    /// Especificación para filtrar promociones activas y vigentes
    /// </summary>
    public class PromocionActivaSpecification : Specification<Promocion>
    {
        private readonly DateTime _fechaActual;
        
        /// <summary>
        /// Constructor de la especificación
        /// </summary>
        /// <param name="fechaActual">Fecha actual para verificar vigencia (inyectable para pruebas)</param>
        public PromocionActivaSpecification(DateTime fechaActual)
        {
            _fechaActual = fechaActual;
        }
        
        /// <summary>
        /// Constructor que usa la fecha actual del sistema
        /// </summary>
        public PromocionActivaSpecification() : this(DateTime.Now)
        {
        }
        
        /// <summary>
        /// Retorna la expresión que verifica si una promoción está activa y vigente
        /// </summary>
        public override Expression<Func<Promocion, bool>> ToExpression()
        {
            return promocion => 
                promocion.Estado == EstadoPromocion.Activa && 
                promocion.FechaInicio <= _fechaActual && 
                promocion.FechaFin >= _fechaActual &&
                (!promocion.MaximoUsos.HasValue || promocion.VecesUsada < promocion.MaximoUsos.Value);
        }
    }
} 
