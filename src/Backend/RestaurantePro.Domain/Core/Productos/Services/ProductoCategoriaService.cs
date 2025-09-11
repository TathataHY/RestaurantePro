namespace RestaurantePro.Domain.Core.Productos.Services
{
    /// <summary>
    /// Servicio de dominio para gestionar operaciones relacionadas con categorías de productos
    /// </summary>
    public class ProductoCategoriaService : IProductoCategoriaService
    {
        private readonly IProductoRepository _productoRepository;
        private readonly IProductoCategoriaRepository _categoriaRepository;

        /// <summary>
        /// Constructor del servicio de categorías de productos
        /// </summary>
        public ProductoCategoriaService(
            IProductoRepository productoRepository,
            IProductoCategoriaRepository categoriaRepository)
        {
            _productoRepository = productoRepository;
            _categoriaRepository = categoriaRepository;
        }

        /// <summary>
        /// Obtiene todos los productos de una categoría específica
        /// </summary>
        /// <param name="categoriaId">ID de la categoría</param>
        /// <param name="soloActivos">Indica si solo se deben obtener productos activos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de productos de la categoría</returns>
        public async Task<List<Entities.Producto>> ObtenerProductosPorCategoriaAsync(
            Guid categoriaId, 
            bool soloActivos = true, 
            CancellationToken cancellationToken = default)
        {
            // Verificar que la categoría existe
            var categoria = await _categoriaRepository.ObtenerPorIdAsync(categoriaId, cancellationToken);
            if (categoria == null)
            {
                throw new InvalidOperationException($"La categoría con ID {categoriaId} no existe");
            }

            // Obtener productos de la categoría
            return await _productoRepository.ObtenerPorCategoriaAsync(categoriaId, soloActivos, cancellationToken);
        }

        /// <summary>
        /// Actualiza la categoría de un conjunto de productos
        /// </summary>
        /// <param name="productosIds">IDs de los productos a actualizar</param>
        /// <param name="categoriaId">ID de la nueva categoría</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de productos actualizados</returns>
        public async Task<int> ActualizarCategoriaProductosAsync(
            List<Guid> productosIds, 
            Guid categoriaId, 
            CancellationToken cancellationToken = default)
        {
            // Verificar que la categoría existe
            var categoria = await _categoriaRepository.ObtenerPorIdAsync(categoriaId, cancellationToken);
            if (categoria == null)
            {
                throw new InvalidOperationException($"La categoría con ID {categoriaId} no existe");
            }

            int productosActualizados = 0;

            // Actualizar cada producto
            foreach (var productoId in productosIds)
            {
                var producto = await _productoRepository.ObtenerPorIdAsync(productoId, cancellationToken);
                if (producto != null)
                {
                    producto.ActualizarCategoria(categoriaId, categoria.Nombre);
                    await _productoRepository.ActualizarAsync(producto, cancellationToken);
                    productosActualizados++;
                }
            }

            return productosActualizados;
        }

        /// <summary>
        /// Reorganiza el orden de las categorías
        /// </summary>
        /// <param name="nuevosOrdenes">Diccionario con ID de categoría como clave y nuevo orden como valor</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de categorías actualizadas</returns>
        public async Task<int> ReorganizarCategoriasAsync(
            Dictionary<Guid, int> nuevosOrdenes, 
            CancellationToken cancellationToken = default)
        {
            int categoriasActualizadas = 0;

            // Obtener todas las categorías
            var categorias = await _categoriaRepository.ObtenerTodasAsync(cancellationToken);

            // Actualizar el orden de cada categoría
            foreach (var categoria in categorias)
            {
                if (nuevosOrdenes.TryGetValue(categoria.Id, out int nuevoOrden))
                {
                    // Solo actualizar si el orden ha cambiado
                    if (categoria.Orden != nuevoOrden)
                    {
                        categoria.Actualizar(categoria.Nombre, categoria.Descripcion, nuevoOrden, categoria.Color, categoria.Icono);
                        await _categoriaRepository.ActualizarAsync(categoria, cancellationToken);
                        categoriasActualizadas++;
                    }
                }
            }

            return categoriasActualizadas;
        }
    }
} 