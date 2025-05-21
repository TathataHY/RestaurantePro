

namespace RestaurantePro.Domain.Inventario.Ingredientes.Specifications
{
    /// <summary>
    /// Especificación para identificar ingredientes de alta rotación que podrían
    /// necesitar una gestión de inventario más frecuente.
    /// </summary>
    public class IngredienteRotacionAltaSpecification : Specification<Ingrediente>
    {
        private readonly decimal _porcentajeUsoMinimo;
        private readonly int _diasAnalisis;
        private readonly DateTime _fechaReferencia;

        /// <summary>
        /// Crea una nueva instancia de la especificación de ingredientes con alta rotación
        /// </summary>
        /// <param name="porcentajeUsoMinimo">Porcentaje mínimo de uso respecto al stock total (por defecto 40%)</param>
        /// <param name="diasAnalisis">Periodo en días para analizar el consumo (por defecto 30 días)</param>
        /// <param name="fechaReferencia">Fecha de referencia para los cálculos (por defecto DateTime.Now)</param>
        public IngredienteRotacionAltaSpecification(
            decimal porcentajeUsoMinimo = 40,
            int diasAnalisis = 30,
            DateTime? fechaReferencia = null)
        {
            _porcentajeUsoMinimo = porcentajeUsoMinimo;
            _diasAnalisis = diasAnalisis;
            _fechaReferencia = fechaReferencia ?? DateTime.Now;
        }

        /// <summary>
        /// Convierte la especificación a una expresión LINQ
        /// </summary>
        public override Expression<Func<Ingrediente, bool>> ToExpression()
        {
            return ingrediente =>
                ingrediente.EstaActivo &&
                (ingrediente.Rotacion == RotacionIngrediente.Alta || ingrediente.Rotacion == RotacionIngrediente.Critica) &&
                (ingrediente.StockMinimo > 0) &&
                (
                    // Si el stock actual está por debajo del 60% del stock mínimo
                    ingrediente.Stock < (ingrediente.StockMinimo * 0.6m) ||
                    // O si la rotación es crítica y el stock está por debajo del 80% del mínimo
                    (ingrediente.Rotacion == RotacionIngrediente.Critica && ingrediente.Stock < (ingrediente.StockMinimo * 0.8m))
                );
        }
    }
} 