namespace RestaurantePro.Domain.Core.Productos.Specifications
{
    /// <summary>
    /// Especificación que determina si un producto puede ser recomendado basado en rentabilidad,
    /// popularidad y disponibilidad de ingredientes.
    /// </summary>
    public class ProductoRecomendableSpecification : Specification<Producto>
    {
        private readonly decimal _rentabilidadMinima;
        private readonly int _popularidadMinima;
        private readonly bool _verificarDisponibilidadIngredientes;
        private readonly IRecetaService? _recetaService;

        /// <summary>
        /// Crea una nueva instancia de la especificación con criterios personalizados
        /// </summary>
        /// <param name="rentabilidadMinima">Rentabilidad mínima requerida en porcentaje (0-100)</param>
        /// <param name="popularidadMinima">Popularidad mínima requerida (0-10)</param>
        /// <param name="verificarDisponibilidadIngredientes">Indica si debe verificarse la disponibilidad de ingredientes</param>
        /// <param name="recetaService">Servicio opcional para verificar disponibilidad de ingredientes</param>
        public ProductoRecomendableSpecification(
            decimal rentabilidadMinima = 30.0m,
            int popularidadMinima = 5,
            bool verificarDisponibilidadIngredientes = false,
            IRecetaService? recetaService = null)
        {
            if (rentabilidadMinima < 0 || rentabilidadMinima > 100)
                throw new ArgumentOutOfRangeException(nameof(rentabilidadMinima), "La rentabilidad debe estar entre 0 y 100");
            
            if (popularidadMinima < 0 || popularidadMinima > 10)
                throw new ArgumentOutOfRangeException(nameof(popularidadMinima), "La popularidad debe estar entre 0 y 10");
            
            _rentabilidadMinima = rentabilidadMinima;
            _popularidadMinima = popularidadMinima;
            _verificarDisponibilidadIngredientes = verificarDisponibilidadIngredientes;
            
            // Solo se requiere recetaService si verificarDisponibilidadIngredientes es true
            if (verificarDisponibilidadIngredientes && recetaService == null)
                throw new ArgumentNullException(nameof(recetaService), "Se requiere recetaService cuando verificarDisponibilidadIngredientes es true");
            
            _recetaService = recetaService;
        }

        /// <summary>
        /// Convierte la especificación a una expresión LINQ
        /// </summary>
        public override Expression<Func<Producto, bool>> ToExpression()
        {
            // Criterios básicos que siempre se verifican
            return producto => 
                producto.EstaActivo &&
                producto.Precio != null &&
                producto.Precio.Valor > 0 &&
                (!string.IsNullOrEmpty(producto.CategoriaNombre) && producto.CategoriaId != Guid.Empty) &&
                producto.Popularidad >= _popularidadMinima;
            
            // Nota: La verificación de rentabilidad y disponibilidad de ingredientes no se puede realizar
            // en una expresión LINQ porque requiere cálculos complejos y consultas adicionales.
            // Por eso, debemos implementar IsSatisfiedBy manualmente.
        }

        /// <summary>
        /// Verifica si la entidad satisface la especificación, incluyendo criterios complejos.
        /// </summary>
        /// <param name="producto">Producto a verificar</param>
        /// <returns>True si el producto cumple todos los criterios para ser recomendable</returns>
        public override bool IsSatisfiedBy(Producto producto)
        {
            // Primero verificamos los criterios básicos usando la expresión LINQ
            if (!base.IsSatisfiedBy(producto))
                return false;
            
            // Verificar rentabilidad si tenemos acceso a la información necesaria
            if (_recetaService != null)
            {
                try
                {
                    // Calcular rentabilidad de forma sincrónica
                    // Nota: En producción sería mejor tener un método sincrónico en IRecetaService,
                    // pero por ahora utilizamos .Result con precaución
                    var rentabilidad = _recetaService.CalcularRentabilidadProductoAsync(producto.Id).Result;
                    
                    // Verificar si la rentabilidad cumple el mínimo requerido
                    if (rentabilidad.Rentabilidad < _rentabilidadMinima)
                        return false;
                    
                    // Verificar disponibilidad de ingredientes si es necesario
                    if (_verificarDisponibilidadIngredientes)
                    {
                        var hayDisponibilidad = _recetaService.VerificarDisponibilidadIngredientesAsync(producto.Id, 1).Result;
                        if (!hayDisponibilidad)
                            return false;
                    }
                }
                catch (Exception)
                {
                    // Si ocurre algún error, asumimos que no se cumple el criterio
                    return false;
                }
            }
            
            // Si llegamos aquí, el producto cumple todos los criterios
            return true;
        }
    }
} 
