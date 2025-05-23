namespace RestaurantePro.Domain.Core.Productos.Services
{
    /// <summary>
    /// Implementación del servicio para gestionar recetas de productos y sus ingredientes.
    /// </summary>
    public class RecetaService : IRecetaService
    {
        private readonly IRecetaRepository _recetaRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IIngredienteRepository _ingredienteRepository;

        /// <summary>
        /// Constructor del servicio de recetas
        /// </summary>
        /// <param name="recetaRepository">Repositorio de recetas</param>
        /// <param name="productoRepository">Repositorio de productos</param>
        /// <param name="ingredienteRepository">Repositorio de ingredientes</param>
        public RecetaService(
            IRecetaRepository recetaRepository,
            IProductoRepository productoRepository,
            IIngredienteRepository ingredienteRepository)
        {
            _recetaRepository = recetaRepository ?? throw new ArgumentNullException(nameof(recetaRepository));
            _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
            _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
        }

        /// <inheritdoc/>
        public async Task<Dictionary<Guid, decimal>> ObtenerIngredientesParaProductoAsync(Guid productoId, CancellationToken cancellationToken = default)
        {
            // Verificar que el producto exista
            var producto = await _productoRepository.ObtenerPorIdAsync(productoId, cancellationToken);
            if (producto == null)
            {
                throw new InvalidOperationException($"No se encontró el producto con ID {productoId}");
            }

            // Obtener la receta del producto
            var receta = await _recetaRepository.ObtenerPorProductoIdAsync(productoId, cancellationToken);
            if (receta == null)
            {
                // Si no hay receta, regresamos un diccionario vacío
                return new Dictionary<Guid, decimal>();
            }

            // Obtener los ingredientes requeridos de la receta
            return receta.ObtenerIngredientesRequeridos();
        }

        /// <inheritdoc/>
        public async Task<bool> VerificarDisponibilidadIngredientesAsync(Guid productoId, int cantidad, CancellationToken cancellationToken = default)
        {
            // Verificar que la cantidad sea positiva
            if (cantidad <= 0)
            {
                throw new ArgumentException("La cantidad debe ser mayor a cero", nameof(cantidad));
            }

            // Obtener los ingredientes del producto
            var ingredientesReceta = await ObtenerIngredientesParaProductoAsync(productoId, cancellationToken);
            if (!ingredientesReceta.Any())
            {
                // Si no hay ingredientes, consideramos que está disponible
                return true;
            }

            // Verificar stock de cada ingrediente
            foreach (var (ingredienteId, cantidadUnitaria) in ingredientesReceta)
            {
                // Obtener el ingrediente actual
                var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId, cancellationToken);
                if (ingrediente == null)
                {
                    // Si el ingrediente no existe, no hay stock suficiente
                    return false;
                }

                // Calcular cantidad total necesaria
                decimal cantidadTotal = cantidadUnitaria * cantidad;

                // Verificar si hay suficiente stock
                if (ingrediente.Stock < cantidadTotal)
                {
                    return false;
                }
            }

            // Si llegamos aquí, hay suficiente stock de todos los ingredientes
            return true;
        }

        /// <inheritdoc/>
        public async Task<Dictionary<Guid, decimal>> ObtenerIngredientesFaltantesAsync(Guid productoId, int cantidad, CancellationToken cancellationToken = default)
        {
            // Verificar que la cantidad sea positiva
            if (cantidad <= 0)
            {
                throw new ArgumentException("La cantidad debe ser mayor a cero", nameof(cantidad));
            }

            // Obtener los ingredientes del producto
            var ingredientesReceta = await ObtenerIngredientesParaProductoAsync(productoId, cancellationToken);
            if (!ingredientesReceta.Any())
            {
                // Si no hay ingredientes, regresamos un diccionario vacío
                return new Dictionary<Guid, decimal>();
            }

            // Diccionario para almacenar los ingredientes faltantes y sus cantidades
            var ingredientesFaltantes = new Dictionary<Guid, decimal>();

            // Verificar stock de cada ingrediente
            foreach (var (ingredienteId, cantidadUnitaria) in ingredientesReceta)
            {
                // Obtener el ingrediente actual
                var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId, cancellationToken);
                if (ingrediente == null)
                {
                    // Si el ingrediente no existe, registramos la cantidad total como faltante
                    ingredientesFaltantes.Add(ingredienteId, cantidadUnitaria * cantidad);
                    continue;
                }

                // Calcular cantidad total necesaria
                decimal cantidadTotal = cantidadUnitaria * cantidad;

                // Verificar si hay suficiente stock
                if (ingrediente.Stock < cantidadTotal)
                {
                    // Agregar al diccionario la cantidad faltante
                    ingredientesFaltantes.Add(ingredienteId, cantidadTotal - ingrediente.Stock);
                }
            }

            return ingredientesFaltantes;
        }
    }
} 