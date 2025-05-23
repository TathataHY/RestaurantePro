namespace RestaurantePro.Domain.Comercial.Promociones.Specifications
{
    /// <summary>
    /// Especificación para filtrar promociones válidas en un día específico de la semana
    /// </summary>
    public class PromocionDiaSemanaSpecification : Specification<Promocion>
    {
        private readonly DayOfWeek _diaSemana;
        
        /// <summary>
        /// Constructor de la especificación
        /// </summary>
        /// <param name="diaSemana">Día de la semana a verificar (por defecto el día actual)</param>
        public PromocionDiaSemanaSpecification(DayOfWeek? diaSemana = null)
        {
            _diaSemana = diaSemana ?? DateTime.Now.DayOfWeek;
        }
        
        /// <summary>
        /// Retorna la expresión que verifica si una promoción es válida para un día específico
        /// </summary>
        public override Expression<Func<Promocion, bool>> ToExpression()
        {
            return promocion => 
                !promocion.DiasValidos.HasValue || 
                (promocion.DiasValidos.Value & (1 << (int)_diaSemana)) != 0;
        }
    }
} 