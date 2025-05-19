namespace RestaurantePro.Domain.Inventario.Ingredientes.Specifications
{
    /// <summary>
    /// Especificación que verifica si un ingrediente está disponible para ser utilizado
    /// en la preparación de productos.
    /// 
    /// Un ingrediente está disponible si:
    /// 1. Está activo (no eliminado lógicamente)
    /// 2. Tiene stock mayor que cero
    /// 3. No está bloqueado por control de calidad (opcional, según configuración)
    /// </summary>
    public class IngredienteDisponibleSpecification : Core.SharedKernel.Specifications.SpecificationBase<Entities.Ingrediente>
    {
        private readonly bool _verificarControlCalidad;
        private readonly decimal _cantidadMinima;
        
        /// <summary>
        /// Crea una nueva instancia de la especificación
        /// </summary>
        /// <param name="verificarControlCalidad">Indica si debe verificarse que el ingrediente no esté bloqueado por control de calidad</param>
        /// <param name="cantidadMinima">Cantidad mínima de stock para considerar que el ingrediente está disponible</param>
        public IngredienteDisponibleSpecification(bool verificarControlCalidad = false, decimal cantidadMinima = 0.01m)
        {
            _verificarControlCalidad = verificarControlCalidad;
            _cantidadMinima = cantidadMinima;
        }
        
        /// <summary>
        /// Verifica si un ingrediente cumple con los criterios de disponibilidad
        /// </summary>
        /// <param name="ingrediente">Ingrediente a evaluar</param>
        /// <returns>True si el ingrediente está disponible, False en caso contrario</returns>
        public override bool IsSatisfiedBy(Entities.Ingrediente ingrediente)
        {
            // Verificación básica
            if (ingrediente == null)
                return false;
                
            // No debe estar eliminado lógicamente
            if (ingrediente.EstaEliminado)
                return false;
                
            // Debe tener stock mayor que cero o la cantidad mínima configurada
            if (ingrediente.Stock < _cantidadMinima)
                return false;
                
            // Verificación de control de calidad (si se solicitó)
            if (_verificarControlCalidad && ingrediente.BloqueadoControlCalidad)
                return false;
            
            return true;
        }
    }
} 