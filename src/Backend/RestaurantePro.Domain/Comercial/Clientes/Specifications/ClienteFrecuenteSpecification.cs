using System;
using System.Linq.Expressions;
using RestaurantePro.Domain.Core.SharedKernel.Specifications;

namespace RestaurantePro.Domain.Comercial.Clientes.Specifications
{
    /// <summary>
    /// Especificación que determina si un cliente es considerado "frecuente" basado en
    /// múltiples criterios como frecuencia de visitas, gasto promedio y antigüedad.
    /// 
    /// Esta especificación es útil para campañas de fidelización, ofertas personalizadas
    /// y para la política ClientesFrecuentesPolicy.
    /// </summary>
    public class ClienteFrecuenteSpecification : Specification<Cliente>
    {
        private readonly int _visitasMinimas;
        private readonly decimal _gastoPromedioMinimo;
        private readonly int _diasAntiguedadMinima;
        private readonly int _periodoDiasAnalisis;
        private readonly DateTime _fechaReferencia;
        
        /// <summary>
        /// Crea una nueva instancia de la especificación
        /// </summary>
        /// <param name="visitasMinimas">Número mínimo de visitas en el periodo de análisis (por defecto 3)</param>
        /// <param name="gastoPromedioMinimo">Gasto promedio mínimo por visita (por defecto 0)</param>
        /// <param name="diasAntiguedadMinima">Días de antigüedad mínima como cliente (por defecto 0)</param>
        /// <param name="periodoDiasAnalisis">Periodo en días para analizar visitas y gastos (por defecto 90 días)</param>
        /// <param name="fechaReferencia">Fecha de referencia para los cálculos (por defecto DateTime.Now)</param>
        public ClienteFrecuenteSpecification(
            int visitasMinimas = 3,
            decimal gastoPromedioMinimo = 0,
            int diasAntiguedadMinima = 0,
            int periodoDiasAnalisis = 90,
            DateTime? fechaReferencia = null)
        {
            _visitasMinimas = visitasMinimas;
            _gastoPromedioMinimo = gastoPromedioMinimo;
            _diasAntiguedadMinima = diasAntiguedadMinima;
            _periodoDiasAnalisis = periodoDiasAnalisis;
            _fechaReferencia = fechaReferencia ?? DateTime.Now;
        }

        /// <summary>
        /// Convierte la especificación a una expresión LINQ
        /// </summary>
        /// <returns>Expresión que representa esta especificación</returns>
        public override Expression<Func<Cliente, bool>> ToExpression()
        {
            return cliente => 
                cliente != null &&
                cliente.EstaActivo &&
                (_diasAntiguedadMinima <= 0 || (_fechaReferencia - cliente.FechaCreacion).TotalDays >= _diasAntiguedadMinima) &&
                ObtenerVisitasRecientesCliente(cliente, _fechaReferencia, _periodoDiasAnalisis) >= _visitasMinimas &&
                (_gastoPromedioMinimo <= 0 || ObtenerGastoPromedioCliente(cliente, _fechaReferencia, _periodoDiasAnalisis) >= _gastoPromedioMinimo);
        }
        
        /// <summary>
        /// Sobrescribe el método IsSatisfiedBy de la clase base para manejar el caso null explícitamente
        /// </summary>
        public override bool IsSatisfiedBy(Cliente entity)
        {
            if (entity == null)
                return false;
                
            var predicate = ToExpression().Compile();
            return predicate(entity);
        }
        
        /// <summary>
        /// Obtiene el número de visitas del cliente en el periodo especificado
        /// Nota: Esta es una implementación simulada. En un sistema real, se consultaría
        /// el historial de comandas del cliente desde un repositorio.
        /// </summary>
        private static int ObtenerVisitasRecientesCliente(Cliente cliente, DateTime fechaReferencia, int periodoDias)
        {
            if (cliente == null)
                return 0;
                
            // En una implementación real, se realizaría una consulta a la base de datos
            // para contar las comandas del cliente en el periodo especificado.
            
            // Para no depender de una propiedad CantidadVisitas que puede estar sin inicializar,
            // utilizamos una estimación basada en los puntos acumulados
            int visitasEstimadas = cliente.PuntosAcumulados / 100;
            
            // Nos aseguramos de un mínimo de 0 visitas
            return Math.Max(0, visitasEstimadas);
        }
        
        /// <summary>
        /// Obtiene el gasto promedio del cliente en el periodo especificado
        /// Nota: Esta es una implementación simulada. En un sistema real, se consultaría
        /// el historial de comandas del cliente desde un repositorio.
        /// </summary>
        private static decimal ObtenerGastoPromedioCliente(Cliente cliente, DateTime fechaReferencia, int periodoDias)
        {
            if (cliente == null)
                return 0;
                
            // En una implementación real, se realizaría una consulta a la base de datos
            // para calcular el promedio de gasto por comanda en el periodo especificado.
            
            // Simulación basada en los puntos acumulados y el segmento
            // Asumimos que 10 puntos equivalen aproximadamente a 1 unidad monetaria de gasto
            decimal gastoBase = cliente.PuntosAcumulados / 10.0m;
            
            // Aplicamos un multiplicador según el segmento del cliente
            // Protegemos contra posible null en Segmento
            var segmento = cliente.Segmento;
            
            switch (segmento)
            {
                case SegmentoCliente.Premium:
                    return gastoBase * 1.5m;
                case SegmentoCliente.TicketAlto:
                    return gastoBase * 1.3m;
                case SegmentoCliente.FrecuenciaAlta:
                    return gastoBase * 0.8m;
                default:
                    return gastoBase;
            }
        }
    }
} 