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
        private readonly IIngredienteRepository? _ingredienteRepository;

        /// <summary>
        /// Crea una nueva instancia de la especificación con criterios personalizados
        /// </summary>
        /// <param name="rentabilidadMinima">Rentabilidad mínima requerida en porcentaje (0-100)</param>
        /// <param name="popularidadMinima">Popularidad mínima requerida (0-10)</param>
        /// <param name="verificarDisponibilidadIngredientes">Indica si debe verificarse la disponibilidad de ingredientes</param>
        /// <param name="ingredienteRepository">Repositorio para verificar disponibilidad y costos de ingredientes</param>
        public ProductoRecomendableSpecification(
            decimal rentabilidadMinima = 30.0m,
            int popularidadMinima = 5,
            bool verificarDisponibilidadIngredientes = false,
            IIngredienteRepository? ingredienteRepository = null)
        {
            if (rentabilidadMinima < 0 || rentabilidadMinima > 100)
                throw new ArgumentOutOfRangeException(nameof(rentabilidadMinima), "La rentabilidad debe estar entre 0 y 100");
            
            if (popularidadMinima < 0 || popularidadMinima > 10)
                throw new ArgumentOutOfRangeException(nameof(popularidadMinima), "La popularidad debe estar entre 0 y 10");
            
            _rentabilidadMinima = rentabilidadMinima;
            _popularidadMinima = popularidadMinima;
            _verificarDisponibilidadIngredientes = verificarDisponibilidadIngredientes;
            
            if (verificarDisponibilidadIngredientes && ingredienteRepository == null)
                throw new ArgumentNullException(nameof(ingredienteRepository), "Se requiere ingredienteRepository cuando verificarDisponibilidadIngredientes es true");

            _ingredienteRepository = ingredienteRepository;
        }

        /// <summary>
        /// Convierte la especificación a una expresión LINQ
        /// </summary>
        public override Expression<Func<Producto, bool>> ToExpression()
        {
            return producto => 
                producto.EstaActivo &&
                producto.Precio != null &&
                producto.Precio.Valor > 0 &&
                (!string.IsNullOrEmpty(producto.CategoriaNombre) && producto.CategoriaId != Guid.Empty) &&
                producto.Popularidad >= _popularidadMinima;
        }

        /// <summary>
        /// Verifica si la entidad satisface la especificación, incluyendo criterios complejos.
        /// </summary>
        public override bool IsSatisfiedBy(Producto producto)
        {
            if (!base.IsSatisfiedBy(producto))
                return false;

            if (_ingredienteRepository == null) return true; // No se pueden hacer más validaciones

            var receta = producto.Recetas.FirstOrDefault();
            if (receta == null) return true; // Si no hay receta, no se pueden hacer más validaciones
            
            decimal costoTotal = 0;

            foreach(var ingredienteReceta in receta.Ingredientes)
            {
                var ingrediente = _ingredienteRepository.ObtenerPorIdAsync(ingredienteReceta.IngredienteId).Result;
                if(ingrediente == null) return false; // Ingrediente no encontrado

                if(_verificarDisponibilidadIngredientes)
                {
                    if(ingrediente.Stock < ingredienteReceta.Cantidad) return false; // No hay stock
                }
                
                costoTotal += ingrediente.CostoPromedio * ingredienteReceta.Cantidad;
            }

            if(producto.Precio.Valor <= 0) return false;
            
            var rentabilidad = ( (producto.Precio.Valor - costoTotal) / producto.Precio.Valor ) * 100;

            if (rentabilidad < _rentabilidadMinima)
                return false;
            
            return true;
        }
    }
} 
