namespace RestaurantePro.Domain.Core.Productos.Policies
{
    /// <summary>
    /// Política que determina qué categorías deben ser visibles según reglas de negocio específicas
    /// </summary>
    public class VisibilidadCategoriasPolicy
    {
        private readonly IProductoCategoriaRepository _categoriaRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IDateTimeService _dateTimeService;

        /// <summary>
        /// Constructor de la política de visibilidad de categorías
        /// </summary>
        public VisibilidadCategoriasPolicy(
            IProductoCategoriaRepository categoriaRepository,
            IProductoRepository productoRepository,
            IDateTimeService dateTimeService)
        {
            _categoriaRepository = categoriaRepository;
            _productoRepository = productoRepository;
            _dateTimeService = dateTimeService;
        }

        /// <summary>
        /// Determina las categorías visibles según criterios de negocio
        /// </summary>
        /// <param name="ocultarCategoriasVacias">Indica si se deben ocultar categorías sin productos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de categorías visibles, ordenadas por su propiedad Orden</returns>
        public async Task<List<Entities.ProductoCategoria>> ObtenerCategoriasVisiblesAsync(
            bool ocultarCategoriasVacias = true,
            CancellationToken cancellationToken = default)
        {
            // Obtener todas las categorías activas
            var categoriasActivas = await _categoriaRepository.ObtenerActivasAsync(cancellationToken);
            
            if (!ocultarCategoriasVacias)
            {
                // Si no se requiere ocultar categorías vacías, devolver todas las activas ordenadas por Orden
                return categoriasActivas.OrderBy(c => c.Orden).ToList();
            }
            
            // Filtrar categorías sin productos
            var categoriasFiltradas = new List<Entities.ProductoCategoria>();
            foreach (var categoria in categoriasActivas)
            {
                var productosCategoria = await _productoRepository.ObtenerPorCategoriaAsync(
                    categoria.Id, true, cancellationToken);
                
                // Agregar la categoría solo si tiene al menos un producto activo
                if (productosCategoria.Any())
                {
                    categoriasFiltradas.Add(categoria);
                }
            }
            
            // Ordenar las categorías por su orden
            return categoriasFiltradas.OrderBy(c => c.Orden).ToList();
        }
        
        /// <summary>
        /// Determina si una categoría debe ser visible
        /// </summary>
        /// <param name="categoriaId">ID de la categoría a verificar</param>
        /// <param name="ocultarCategoriasVacias">Indica si se deben ocultar categorías sin productos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si la categoría debe ser visible, False en caso contrario</returns>
        public async Task<bool> EsCategoriaVisibleAsync(
            Guid categoriaId,
            bool ocultarCategoriasVacias = true,
            CancellationToken cancellationToken = default)
        {
            // Obtener la categoría
            var categoria = await _categoriaRepository.ObtenerPorIdAsync(categoriaId, cancellationToken);
            if (categoria == null || !categoria.EstaActivo)
            {
                return false;
            }
            
            // Si no se requiere ocultar categorías vacías, la categoría activa es visible
            if (!ocultarCategoriasVacias)
            {
                return true;
            }
            
            // Verificar si la categoría tiene productos activos
            var productosCategoria = await _productoRepository.ObtenerPorCategoriaAsync(
                categoria.Id, true, cancellationToken);
                
            return productosCategoria.Any();
        }
    }
} 