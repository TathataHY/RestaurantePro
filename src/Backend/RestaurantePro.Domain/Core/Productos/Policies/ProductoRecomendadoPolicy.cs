namespace RestaurantePro.Domain.Core.Productos.Policies
{
    /// <summary>
    /// Implementación de la política para recomendar productos basados en historial
    /// y tendencias del restaurante.
    /// </summary>
    public class ProductoRecomendadoPolicy : IProductoRecomendadoPolicy
    {
        private readonly IProductoRepository _productoRepository;
        private readonly IComandaRepository _comandaRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly ProductoRecomendableSpecification _productoRecomendableSpec;

        /// <summary>
        /// Constructor de la política de recomendación de productos
        /// </summary>
        public ProductoRecomendadoPolicy(
            IProductoRepository productoRepository,
            IComandaRepository comandaRepository,
            IDateTimeService dateTimeService)
        {
            _productoRepository = productoRepository;
            _comandaRepository = comandaRepository;
            _dateTimeService = dateTimeService;
            _productoRecomendableSpec = new ProductoRecomendableSpecification();
        }

        /// <inheritdoc />
        public async Task<ResultadoProductoRecomendadoPolicy> GenerarRecomendacionesParaCliente(
            Guid clienteId,
            int cantidadRecomendaciones = 5,
            CancellationToken cancellationToken = default)
        {
            var resultado = new ResultadoProductoRecomendadoPolicy
            {
                Criterios = "Recomendación basada en historial personal de consumo"
            };

            // 1. Obtener historial de comandas del cliente
            var comandasCliente = await _comandaRepository.ObtenerPorClienteAsync(clienteId, cancellationToken);
            if (comandasCliente == null || !comandasCliente.Any())
            {
                // Si no hay historial, usar recomendaciones populares
                return await GenerarRecomendacionesPopulares(cantidadRecomendaciones, 30, cancellationToken);
            }

            // 2. Obtener todos los productos activos
            var productosActivos = await _productoRepository.ObtenerTodosAsync(true, cancellationToken);
            
            // 3. Filtrar productos que pueden ser recomendados según la especificación
            var productosRecomendables = productosActivos
                .Where(p => _productoRecomendableSpec.IsSatisfiedBy(p))
                .ToList();
            
            // 4. Analizar productos que el cliente ha consumido
            var productosConsumidos = ObtenerProductosConsumidos(comandasCliente);
            
            // 5. Encontrar categorías preferidas
            var categoriasPreferidas = productosConsumidos
                .GroupBy(p => p.CategoriaId)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => g.Key)
                .ToList();
            
            // 6. Recomendar productos similares a los que ha consumido pero que no sean los mismos
            var productosRecomendados = new List<ProductoRecomendado>();
            
            // Primero incluir productos de las mismas categorías que no haya consumido
            foreach (var categoriaId in categoriasPreferidas)
            {
                var productosEnCategoria = productosRecomendables
                    .Where(p => p.CategoriaId == categoriaId && 
                               !productosConsumidos.Any(pc => pc.Id == p.Id))
                    .Take(2)
                    .ToList();
                
                foreach (var producto in productosEnCategoria)
                {
                    if (productosRecomendados.Count < cantidadRecomendaciones)
                    {
                        productosRecomendados.Add(new ProductoRecomendado
                        {
                            ProductoId = producto.Id,
                            Nombre = producto.Nombre,
                            Puntuacion = 90, // Alta puntuación para productos de categorías preferidas
                            RazonRecomendacion = $"Basado en tu preferencia por productos de {producto.CategoriaNombre}",
                            CategoriaId = producto.CategoriaId,
                            CategoriaNombre = producto.CategoriaNombre
                        });
                    }
                }
            }
            
            // Si no tenemos suficientes, complementar con productos populares
            if (productosRecomendados.Count < cantidadRecomendaciones)
            {
                // Obtener productos populares que no estén ya en las recomendaciones
                var populares = await ObtenerProductosPopulares(30, cancellationToken);
                
                foreach (var popular in populares.Where(p => 
                            !productosRecomendados.Any(r => r.ProductoId == p.Id) && 
                            !productosConsumidos.Any(c => c.Id == p.Id)))
                {
                    if (productosRecomendados.Count < cantidadRecomendaciones)
                    {
                        productosRecomendados.Add(new ProductoRecomendado
                        {
                            ProductoId = popular.Id,
                            Nombre = popular.Nombre,
                            Puntuacion = 70, // Puntuación media para productos populares
                            RazonRecomendacion = "Producto popular entre nuestros clientes",
                            CategoriaId = popular.CategoriaId,
                            CategoriaNombre = popular.CategoriaNombre
                        });
                    }
                }
            }
            
            // Si aún no tenemos suficientes, añadir productos aleatorios
            if (productosRecomendados.Count < cantidadRecomendaciones)
            {
                var productosRestantes = productosRecomendables
                    .Where(p => !productosRecomendados.Any(r => r.ProductoId == p.Id) && 
                               !productosConsumidos.Any(c => c.Id == p.Id))
                    .OrderBy(_ => Guid.NewGuid()) // Ordenamiento aleatorio
                    .Take(cantidadRecomendaciones - productosRecomendados.Count);
                
                foreach (var producto in productosRestantes)
                {
                    productosRecomendados.Add(new ProductoRecomendado
                    {
                        ProductoId = producto.Id,
                        Nombre = producto.Nombre,
                        Puntuacion = 50, // Puntuación baja para recomendaciones aleatorias
                        RazonRecomendacion = "Podrías disfrutar de este producto",
                        CategoriaId = producto.CategoriaId,
                        CategoriaNombre = producto.CategoriaNombre
                    });
                }
            }
            
            resultado.ProductosRecomendados.AddRange(productosRecomendados);
            return resultado;
        }

        /// <inheritdoc />
        public async Task<ResultadoProductoRecomendadoPolicy> GenerarRecomendacionesPopulares(
            int cantidadRecomendaciones = 5,
            int diasAnalisis = 30,
            CancellationToken cancellationToken = default)
        {
            var resultado = new ResultadoProductoRecomendadoPolicy
            {
                Criterios = "Recomendación basada en popularidad general"
            };
            
            // 1. Obtener comandas del período de análisis
            var fechaActual = _dateTimeService.Now;
            var fechaInicio = fechaActual.AddDays(-diasAnalisis);
            
            var comandasRecientes = await _comandaRepository.ObtenerPorRangoFechasAsync(
                fechaInicio, 
                fechaActual, 
                cancellationToken);
            
            // 2. Obtener productos activos
            var productosActivos = await _productoRepository.ObtenerTodosAsync(true, cancellationToken);
            
            // 3. Contar frecuencia de productos en comandas
            var conteoProductos = new Dictionary<Guid, int>();
            
            foreach (var comanda in comandasRecientes)
            {
                foreach (var item in comanda.Items)
                {
                    if (conteoProductos.ContainsKey(item.ProductoId))
                    {
                        conteoProductos[item.ProductoId]++;
                    }
                    else
                    {
                        conteoProductos[item.ProductoId] = 1;
                    }
                }
            }
            
            // 4. Ordenar productos por frecuencia y seleccionar los más populares
            var productosPopulares = conteoProductos
                .OrderByDescending(kv => kv.Value)
                .Take(cantidadRecomendaciones)
                .Select(kv => kv.Key)
                .ToList();
            
            // 5. Convertir a ProductoRecomendado
            foreach (var productoId in productosPopulares)
            {
                var producto = productosActivos.FirstOrDefault(p => p.Id == productoId);
                if (producto != null && _productoRecomendableSpec.IsSatisfiedBy(producto))
                {
                    resultado.ProductosRecomendados.Add(new ProductoRecomendado
                    {
                        ProductoId = producto.Id,
                        Nombre = producto.Nombre,
                        Puntuacion = CalcularPuntuacion(conteoProductos[productoId], conteoProductos.Values.Max()),
                        RazonRecomendacion = "Uno de nuestros productos más populares",
                        CategoriaId = producto.CategoriaId,
                        CategoriaNombre = producto.CategoriaNombre
                    });
                }
            }
            
            return resultado;
        }

        /// <inheritdoc />
        public async Task<ResultadoProductoRecomendadoPolicy> GenerarRecomendacionesComplementarias(
            Guid comandaId,
            int cantidadRecomendaciones = 3,
            CancellationToken cancellationToken = default)
        {
            var resultado = new ResultadoProductoRecomendadoPolicy
            {
                Criterios = "Recomendación de productos complementarios para tu comanda actual"
            };
            
            // 1. Obtener la comanda actual
            var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId, cancellationToken);
            if (comanda == null || !comanda.Items.Any())
            {
                // Si no hay comanda o está vacía, usar recomendaciones populares
                return await GenerarRecomendacionesPopulares(cantidadRecomendaciones, 30, cancellationToken);
            }
            
            // 2. Obtener todos los productos activos
            var productosActivos = await _productoRepository.ObtenerTodosAsync(true, cancellationToken);
            
            // 3. Identificar categorías ya presentes en la comanda
            var categoriasEnComanda = new HashSet<Guid>();
            
            foreach (var item in comanda.Items)
            {
                var producto = await _productoRepository.ObtenerPorIdAsync(item.ProductoId, cancellationToken);
                if (producto != null)
                {
                    categoriasEnComanda.Add(producto.CategoriaId);
                }
            }
            
            // 4. Recomendar productos de categorías complementarias
            var recomendaciones = new List<ProductoRecomendado>();
            
            // 4.1 Filtrar productos de categorías diferentes a las ya presentes
            var productosCandidatos = productosActivos
                .Where(p => _productoRecomendableSpec.IsSatisfiedBy(p) && 
                           !categoriasEnComanda.Contains(p.CategoriaId) &&
                           !comanda.Items.Any(i => i.ProductoId == p.Id))
                .ToList();
            
            // 4.2 Agrupar por categoría para asegurar diversidad
            var productosPorCategoria = productosCandidatos
                .GroupBy(p => p.CategoriaId)
                .ToDictionary(g => g.Key, g => g.ToList());
            
            // 4.3 Seleccionar un producto de cada categoría hasta completar
            foreach (var categoria in productosPorCategoria.Keys)
            {
                if (recomendaciones.Count < cantidadRecomendaciones)
                {
                    var producto = productosPorCategoria[categoria]
                        .OrderBy(_ => Guid.NewGuid()) // Ordenamiento aleatorio dentro de la categoría
                        .First();
                    
                    recomendaciones.Add(new ProductoRecomendado
                    {
                        ProductoId = producto.Id,
                        Nombre = producto.Nombre,
                        Puntuacion = 85, // Alta puntuación para complementos
                        RazonRecomendacion = $"Complemento perfecto: {producto.CategoriaNombre}",
                        CategoriaId = producto.CategoriaId,
                        CategoriaNombre = producto.CategoriaNombre
                    });
                }
            }
            
            // 5. Si no tenemos suficientes recomendaciones, completar con productos populares
            if (recomendaciones.Count < cantidadRecomendaciones)
            {
                var populares = await ObtenerProductosPopulares(30, cancellationToken);
                
                foreach (var popular in populares.Where(p => 
                    !categoriasEnComanda.Contains(p.CategoriaId) && 
                    !recomendaciones.Any(r => r.ProductoId == p.Id) &&
                    !comanda.Items.Any(i => i.ProductoId == p.Id)))
                {
                    if (recomendaciones.Count < cantidadRecomendaciones)
                    {
                        recomendaciones.Add(new ProductoRecomendado
                        {
                            ProductoId = popular.Id,
                            Nombre = popular.Nombre,
                            Puntuacion = 75,
                            RazonRecomendacion = "Complemento popular",
                            CategoriaId = popular.CategoriaId,
                            CategoriaNombre = popular.CategoriaNombre
                        });
                    }
                }
            }
            
            resultado.ProductosRecomendados.AddRange(recomendaciones);
            return resultado;
        }
        
        #region Métodos privados auxiliares
        
        /// <summary>
        /// Obtiene los productos más populares del período especificado
        /// </summary>
        private async Task<List<Producto>> ObtenerProductosPopulares(int diasAnalisis, CancellationToken cancellationToken)
        {
            // 1. Obtener comandas del período de análisis
            var fechaActual = _dateTimeService.Now;
            var fechaInicio = fechaActual.AddDays(-diasAnalisis);
            
            var comandasRecientes = await _comandaRepository.ObtenerPorRangoFechasAsync(
                fechaInicio, 
                fechaActual, 
                cancellationToken);
            
            // 2. Obtener productos activos
            var productosActivos = await _productoRepository.ObtenerTodosAsync(true, cancellationToken);
            
            // 3. Contar frecuencia de productos en comandas
            var conteoProductos = new Dictionary<Guid, int>();
            
            foreach (var comanda in comandasRecientes)
            {
                foreach (var item in comanda.Items)
                {
                    if (conteoProductos.ContainsKey(item.ProductoId))
                    {
                        conteoProductos[item.ProductoId]++;
                    }
                    else
                    {
                        conteoProductos[item.ProductoId] = 1;
                    }
                }
            }
            
            // 4. Ordenar productos por frecuencia y obtener los más populares
            var productosPopulares = productosActivos
                .Where(p => conteoProductos.ContainsKey(p.Id) && _productoRecomendableSpec.IsSatisfiedBy(p))
                .OrderByDescending(p => conteoProductos[p.Id])
                .ToList();
                
            return productosPopulares;
        }
        
        /// <summary>
        /// Extrae los productos consumidos de una lista de comandas
        /// </summary>
        private List<Producto> ObtenerProductosConsumidos(IEnumerable<Comanda> comandas)
        {
            var productosConsumidos = new List<Producto>();
            
            foreach (var comanda in comandas)
            {
                foreach (var item in comanda.Items)
                {
                    var producto = _productoRepository.ObtenerPorIdAsync(item.ProductoId).Result;
                    if (producto != null)
                    {
                        productosConsumidos.Add(producto);
                    }
                }
            }
            
            return productosConsumidos;
        }
        
        /// <summary>
        /// Calcula una puntuación normalizada basada en la frecuencia relativa
        /// </summary>
        private int CalcularPuntuacion(int frecuencia, int frecuenciaMaxima)
        {
            // Normalizar a escala 50-100
            return 50 + (int)((double)frecuencia / frecuenciaMaxima * 50);
        }
        
        #endregion
    }
} 