namespace RestaurantePro.Application.Core.Productos.Queries.ObtenerProductosPaginados;

public class ObtenerProductosPaginadosHandler : IRequestHandler<ObtenerProductosPaginadosQuery, Result<PaginatedList<ProductoDto>>>
{
    private readonly IProductoRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerProductosPaginadosHandler> _logger;

    public ObtenerProductosPaginadosHandler(
        IProductoRepository repository,
        IMapper mapper,
        ILogger<ObtenerProductosPaginadosHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PaginatedList<ProductoDto>>> Handle(ObtenerProductosPaginadosQuery request, CancellationToken cancellationToken)
    {
            _logger.LogInformation("📄 Obteniendo productos paginados - Página: {PageNumber}, Tamaño: {PageSize}, SoloActivos: {SoloActivos}, CategoriaId: {CategoriaId}", 
                request.PageNumber, request.PageSize, request.SoloActivos, request.CategoriaId);

        try
        {
            // Validar parámetros de paginación
            if (request.PageNumber < 1)
            {
                _logger.LogWarning("❌ Número de página inválido: {PageNumber}", request.PageNumber);
                return Result.Failure<PaginatedList<ProductoDto>>("El número de página debe ser mayor a 0");
            }

            if (request.PageSize < 1 || request.PageSize > 100)
            {
                _logger.LogWarning("❌ Tamaño de página inválido: {PageSize}", request.PageSize);
                return Result.Failure<PaginatedList<ProductoDto>>("El tamaño de página debe estar entre 1 y 100");
            }

            // Obtener productos según los filtros
            IEnumerable<Producto> productos;

            if (request.CategoriaId.HasValue)
            {
                productos = await _repository.ObtenerPorCategoriaAsync(request.CategoriaId.Value, request.SoloActivos, cancellationToken);
            }
            else
            {
                productos = await _repository.ObtenerTodosAsync(request.SoloActivos, cancellationToken);
            }

            // Aplicar filtro de texto si se proporciona (búsqueda mejorada por palabras)
            if (!string.IsNullOrEmpty(request.Filtro))
            {
                _logger.LogInformation("🔍 Búsqueda inteligente activada con texto: '{Filtro}'", request.Filtro);
                productos = AplicarBusquedaInteligente(productos, request.Filtro);
            }

            // Aplicar filtros avanzados
            productos = ApplyAdvancedFilters(productos, request);

            // Aplicar ordenamiento
            productos = ApplyOrdering(productos, request.OrderBy, request.OrderDirection);

            // Calcular paginación
            var totalCount = productos.Count();
            var productosArray = productos.ToArray();
            
            _logger.LogInformation("🔍 Paginación - Total: {Total}, Página: {PageNumber}, Tamaño: {PageSize}", 
                totalCount, request.PageNumber, request.PageSize);
            
            var itemsToSkip = (request.PageNumber - 1) * request.PageSize;
            var productosPagina = productosArray
                .Skip(itemsToSkip)
                .Take(request.PageSize)
                .ToList();
                
            _logger.LogInformation("📊 Paginación - Items a saltar: {ItemsToSkip}, Items devueltos: {ItemsReturned}", 
                itemsToSkip, productosPagina.Count);

            // Mapear a DTOs
            var productosDto = _mapper.Map<List<ProductoDto>>(productosPagina);

            // Crear lista paginada
            var resultado = new PaginatedList<ProductoDto>(
                productosDto,
                totalCount,
                request.PageNumber,
                request.PageSize);

            _logger.LogInformation("✅ Productos obtenidos: {Count} de {Total} - Página {PageNumber}/{TotalPages}", 
                resultado.Items.Count, resultado.TotalCount, resultado.PageNumber, resultado.TotalPages);
            
            // Log detallado de los productos para debugging
            _logger.LogInformation("🔍 Productos en la página actual:");
            foreach (var producto in resultado.Items)
            {
                _logger.LogInformation("  - {Nombre}: Activo={Activo}", producto.Nombre, producto.Activo);
            }

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener productos paginados");
            return Result.Failure<PaginatedList<ProductoDto>>($"Error interno al obtener productos: {ex.Message}");
        }
    }

    private static IEnumerable<Producto> ApplyAdvancedFilters(IEnumerable<Producto> productos, ObtenerProductosPaginadosQuery request)
    {
        // Filtro por rango de precios
        if (request.PrecioMinimo.HasValue)
        {
            productos = productos.Where(p => p.Precio?.Valor >= request.PrecioMinimo.Value);
        }

        if (request.PrecioMaximo.HasValue)
        {
            productos = productos.Where(p => p.Precio?.Valor <= request.PrecioMaximo.Value);
        }

        // Filtro por rango de fechas de creación
        if (request.FechaCreacionDesde.HasValue)
        {
            productos = productos.Where(p => p.FechaCreacion >= request.FechaCreacionDesde.Value);
        }

        if (request.FechaCreacionHasta.HasValue)
        {
            productos = productos.Where(p => p.FechaCreacion <= request.FechaCreacionHasta.Value);
        }

        // Filtro por rango de popularidad
        if (request.PopularidadMinima.HasValue)
        {
            productos = productos.Where(p => p.Popularidad >= request.PopularidadMinima.Value);
        }

        if (request.PopularidadMaxima.HasValue)
        {
            productos = productos.Where(p => p.Popularidad <= request.PopularidadMaxima.Value);
        }

        return productos;
    }

    private static IEnumerable<Producto> ApplyOrdering(IEnumerable<Producto> productos, string orderBy, string orderDirection)
    {
        var isDescending = orderDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return orderBy.ToLowerInvariant() switch
        {
            "nombre" => isDescending 
                ? productos.OrderByDescending(p => p.Nombre)
                : productos.OrderBy(p => p.Nombre),
            
            "precio" => isDescending 
                ? productos.OrderByDescending(p => p.Precio?.Valor ?? 0)
                : productos.OrderBy(p => p.Precio?.Valor ?? 0),
            
            "fechacreacion" => isDescending 
                ? productos.OrderByDescending(p => p.FechaCreacion)
                : productos.OrderBy(p => p.FechaCreacion),
                
            "popularidad" => isDescending 
                ? productos.OrderByDescending(p => p.Popularidad)
                : productos.OrderBy(p => p.Popularidad),
            
            _ => productos.OrderBy(p => p.Nombre) // Default por nombre
        };
    }

    /// <summary>
    /// Aplica búsqueda inteligente que divide el texto en palabras individuales
    /// y busca productos que contengan TODAS las palabras en nombre o descripción.
    /// Soporta búsquedas parciales: "lomo sal" encuentra "Lomo Saltado"
    /// </summary>
    private static IEnumerable<Producto> AplicarBusquedaInteligente(IEnumerable<Producto> productos, string textoBusqueda)
    {
        // Normalizar y dividir el texto de búsqueda en palabras
        var palabras = NormalizarTexto(textoBusqueda)
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(p => p.Length >= 2) // Ignorar palabras de 1 letra
            .ToList();

        if (!palabras.Any())
        {
            return productos; // Si no hay palabras válidas, retornar todo
        }

        // Filtrar productos que contengan TODAS las palabras en nombre o descripción
        return productos.Where(p =>
        {
            var nombreNormalizado = NormalizarTexto(p.Nombre ?? "");
            var descripcionNormalizada = NormalizarTexto(p.Descripcion ?? "");
            var textoCompleto = $"{nombreNormalizado} {descripcionNormalizada}";

            // El producto debe contener TODAS las palabras buscadas
            return palabras.All(palabra => textoCompleto.Contains(palabra));
        });
    }

    /// <summary>
    /// Normaliza texto para búsqueda: convierte a minúsculas, remueve acentos y espacios extras
    /// </summary>
    private static string NormalizarTexto(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return string.Empty;

        // Convertir a minúsculas
        texto = texto.ToLowerInvariant();

        // Remover acentos
        var textoNormalizado = new System.Text.StringBuilder();
        foreach (var c in texto.Normalize(System.Text.NormalizationForm.FormD))
        {
            var categoriaUnicode = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (categoriaUnicode != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                textoNormalizado.Append(c);
            }
        }

        // Normalizar espacios múltiples a uno solo y quitar espacios al inicio/final
        var resultado = System.Text.RegularExpressions.Regex.Replace(
            textoNormalizado.ToString(), 
            @"\s+", 
            " ").Trim();

        return resultado;
    }
} 