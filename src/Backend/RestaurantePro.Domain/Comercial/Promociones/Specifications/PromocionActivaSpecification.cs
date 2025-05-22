
namespace RestaurantePro.Domain.Comercial.Promociones.Specifications
{
    /// <summary>
    /// Especificación que identifica promociones activas disponibles para ser aplicadas
    /// </summary>
    public class PromocionActivaSpecification : Specification<Promocion>
    {
        private readonly DateTime _fechaConsulta;

        /// <summary>
        /// Crea una nueva instancia de la especificación
        /// </summary>
        /// <param name="fechaConsulta">Fecha para verificar si la promoción está activa (por defecto DateTime.Now)</param>
        public PromocionActivaSpecification(DateTime? fechaConsulta = null)
        {
            _fechaConsulta = fechaConsulta ?? DateTime.Now;
        }

        /// <summary>
        /// Convierte la especificación a una expresión LINQ
        /// </summary>
        public override Expression<Func<Promocion, bool>> ToExpression()
        {
            return promocion =>
                promocion.Activa &&
                (!promocion.FechaInicio.HasValue || promocion.FechaInicio.Value <= _fechaConsulta) &&
                (!promocion.FechaFin.HasValue || promocion.FechaFin.Value >= _fechaConsulta) &&
                promocion.CantidadDisponible > 0;
        }
    }

    /// <summary>
    /// Especificación que identifica promociones aplicables en días específicos de la semana
    /// </summary>
    public class PromocionDiaSemanaSpecification : Specification<Promocion>
    {
        private readonly DayOfWeek _diaSemana;

        /// <summary>
        /// Crea una nueva instancia de la especificación
        /// </summary>
        /// <param name="diaSemana">Día de la semana a verificar</param>
        public PromocionDiaSemanaSpecification(DayOfWeek diaSemana)
        {
            _diaSemana = diaSemana;
        }

        /// <summary>
        /// Convierte la especificación a una expresión LINQ
        /// </summary>
        public override Expression<Func<Promocion, bool>> ToExpression()
        {
            return promocion =>
                !promocion.DiasValidos.HasValue || 
                (promocion.DiasValidos.Value & (1 << (int)_diaSemana)) != 0;
        }
    }

    /// <summary>
    /// Especificación compuesta que identifica promociones elegibles para ser aplicadas
    /// en un momento determinado, combinando estado activo y validez para el día de la semana
    /// </summary>
    public class PromocionElegibleSpecification : Specification<Promocion>
    {
        /// <summary>
        /// Crea una nueva instancia de la especificación
        /// </summary>
        /// <param name="fechaConsulta">Fecha para verificar si la promoción está activa</param>
        public PromocionElegibleSpecification(DateTime? fechaConsulta = null)
        {
            DateTime fecha = fechaConsulta ?? DateTime.Now;
            
            // Componer especificaciones usando los operadores And
            var promocionActiva = new PromocionActivaSpecification(fecha);
            var promocionDiaSemana = new PromocionDiaSemanaSpecification(fecha.DayOfWeek);
            
            // Esta especificación combina ambas condiciones
            _especificacionCombinada = promocionActiva.And(promocionDiaSemana);
        }

        private readonly Specification<Promocion> _especificacionCombinada;

        /// <summary>
        /// Convierte la especificación a una expresión LINQ
        /// </summary>
        public override Expression<Func<Promocion, bool>> ToExpression()
        {
            return _especificacionCombinada.ToExpression();
        }
    }
} 
