namespace RestaurantePro.Application.Proveedores.Proveedores.Queries.ObtenerProveedoresPaginados;

/// <summary>
/// Handler para ObtenerProveedoresPaginadosQuery
/// Procesa consultas complejas con filtros, búsqueda, paginación y ordenamiento
/// </summary>
public class ObtenerProveedoresPaginadosHandler : IRequestHandler<ObtenerProveedoresPaginadosQuery, Result<PaginatedList<ProveedorDto>>>
{
    private readonly IProveedorRepository _proveedorRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerProveedoresPaginadosHandler> _logger;

    public ObtenerProveedoresPaginadosHandler(
        IProveedorRepository proveedorRepository,
        IMapper mapper,
        ILogger<ObtenerProveedoresPaginadosHandler> logger)
    {
        _proveedorRepository = proveedorRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Procesa la consulta paginada de proveedores
    /// </summary>
    public async Task<Result<PaginatedList<ProveedorDto>>> Handle(
        ObtenerProveedoresPaginadosQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔍 Obteniendo proveedores paginados - Página: {PageNumber}, Tamaño: {PageSize}", 
            request.PageNumber, request.PageSize);

        try
        {
            // Validar parámetros de entrada
            if (request.PageNumber <= 0)
            {
                return Result.Failure<PaginatedList<ProveedorDto>>("La página debe ser mayor a 0");
            }

            if (request.PageSize <= 0)
            {
                return Result.Failure<PaginatedList<ProveedorDto>>("El tamaño de página debe ser mayor a 0");
            }

            if (request.PageSize > 100)
            {
                return Result.Failure<PaginatedList<ProveedorDto>>("El tamaño de página no puede superar los 100 elementos");
            }

            // Validar campos de ordenamiento
            var camposOrdenValidos = new[] { "nombre", "fechacreacion", "categoria", "activo", "ciudad", "pais", "rfc", "diascredito", "fecharegistro", "email", "nombrecontacto" };
            if (!string.IsNullOrEmpty(request.CampoOrden) && !camposOrdenValidos.Contains(request.CampoOrden.ToLower()))
            {
                return Result.Failure<PaginatedList<ProveedorDto>>("Campo de ordenamiento no válido");
            }

            // Validar lógica de activos/inactivos
            if (!request.SoloActivos)
            {
                return Result.Failure<PaginatedList<ProveedorDto>>("Debe incluir al menos proveedores activos o inactivos");
            }

            _logger.LogDebug("🔧 Aplicando filtros de búsqueda");

            // Detectar si el término de búsqueda es una categoría válida
            CategoriaProveedor? categoria = null;
            if (!string.IsNullOrEmpty(request.TerminoBusqueda) && 
                Enum.TryParse<CategoriaProveedor>(request.TerminoBusqueda, out var categoriaDetectada))
            {
                categoria = categoriaDetectada;
            }

            // Obtener proveedores usando el repositorio
            var proveedores = await _proveedorRepository.ObtenerProveedoresPaginadosAsync(
                request.PageNumber,
                request.PageSize,
                request.TerminoBusqueda,
                categoria,
                request.SoloActivos,
                false, // incluirInactivos
                request.CampoOrden,
                request.DireccionOrden.Equals("asc", StringComparison.OrdinalIgnoreCase),
                cancellationToken);

            // Obtener total de elementos
            var totalCount = await _proveedorRepository.ContarProveedoresAsync(
                request.TerminoBusqueda,
                categoria,
                request.SoloActivos,
                false, // incluirInactivos
                cancellationToken);

            // Mapear a DTOs
            var proveedoresDto = _mapper.Map<IEnumerable<ProveedorDto>>(proveedores);

            // Crear resultado paginado
            var resultado = new PaginatedList<ProveedorDto>(
                proveedoresDto.ToList(),
                totalCount,
                request.PageNumber,
                request.PageSize);

            // Log de resultados
            _logger.LogInformation("✅ Consulta completada - {TotalCount} total, {PageCount} páginas, mostrando {ItemCount} elementos", 
                resultado.TotalCount, resultado.TotalPages, resultado.Items.Count);

            LogFiltrosAplicados(request, resultado.TotalCount);

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al consultar proveedores paginados");
            
            // Determinar tipo de error más específico
            if (ex.Message.Contains("mapeo") || ex.Message.Contains("mapear"))
            {
                return Result.Failure<PaginatedList<ProveedorDto>>("Error al mapear proveedores");
            }
            
            return Result.Failure<PaginatedList<ProveedorDto>>("Error al obtener proveedores");
        }
    }

    /// <summary>
    /// Registra información detallada sobre los filtros aplicados
    /// </summary>
    private void LogFiltrosAplicados(ObtenerProveedoresPaginadosQuery request, int totalResultados)
    {
        var filtros = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.TerminoBusqueda))
            filtros.Add($"Búsqueda: '{request.TerminoBusqueda}'");

        if (!string.IsNullOrWhiteSpace(request.Ciudad))
            filtros.Add($"Ciudad: '{request.Ciudad}'");

        if (!string.IsNullOrWhiteSpace(request.Pais))
            filtros.Add($"País: '{request.Pais}'");

        if (request.DiasCredito_Min.HasValue)
            filtros.Add($"Crédito min: {request.DiasCredito_Min} días");

        if (request.DiasCredito_Max.HasValue)
            filtros.Add($"Crédito max: {request.DiasCredito_Max} días");

        filtros.Add($"Solo activos: {request.SoloActivos}");
        filtros.Add($"Orden: {request.CampoOrden} {request.DireccionOrden}");

        _logger.LogInformation("📋 Filtros aplicados: {Filtros} → {TotalResultados} resultados", 
            string.Join(" | ", filtros), totalResultados);
    }
} 