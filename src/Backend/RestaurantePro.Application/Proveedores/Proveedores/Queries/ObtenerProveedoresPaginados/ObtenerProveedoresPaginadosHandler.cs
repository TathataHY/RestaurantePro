namespace RestaurantePro.Application.Proveedores.Proveedores.Queries.ObtenerProveedoresPaginados;

/// <summary>
/// Handler para ObtenerProveedoresPaginadosQuery
/// Procesa consultas complejas con filtros, búsqueda, paginación y ordenamiento
/// </summary>
public class ObtenerProveedoresPaginadosHandler : IRequestHandler<ObtenerProveedoresPaginadosQuery, Result<PaginatedList<ProveedorSummaryDto>>>
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
    public async Task<Result<PaginatedList<ProveedorSummaryDto>>> Handle(
        ObtenerProveedoresPaginadosQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔍 Consultando proveedores paginados - Página: {PageNumber}, Tamaño: {PageSize}", 
            request.PageNumber, request.PageSize);

        try
        {
            // 1. Aplicar filtros de búsqueda
            var proveedores = await AplicarFiltros(request, cancellationToken);
            
            if (!proveedores.Any())
            {
                _logger.LogInformation("📭 No se encontraron proveedores con los criterios especificados");
                return Result.Success(
                    new PaginatedList<ProveedorSummaryDto>(
                        new List<ProveedorSummaryDto>(), 
                        0, 
                        request.PageNumber, 
                        request.PageSize));
            }

            // 2. Aplicar ordenamiento
            var proveedoresOrdenados = AplicarOrdenamiento(proveedores, request);

            // 3. Aplicar paginación
            var resultadoPaginado = AplicarPaginacion(proveedoresOrdenados, request);

            // 4. Mapear a DTOs
            var proveedoresDto = _mapper.Map<List<ProveedorSummaryDto>>(resultadoPaginado.Items);

            // 5. Crear resultado paginado
            var resultado = new PaginatedList<ProveedorSummaryDto>(
                proveedoresDto,
                resultadoPaginado.TotalCount,
                request.PageNumber,
                request.PageSize);

            // 6. Log de resultados
            _logger.LogInformation("✅ Consulta completada - {TotalCount} total, {PageCount} páginas, mostrando {ItemCount} elementos", 
                resultado.TotalCount, resultado.TotalPages, resultado.Items.Count);

            // 7. Log detallado de filtros aplicados
            LogFiltrosAplicados(request, resultado.TotalCount);

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al consultar proveedores paginados");
            return Result.Failure<PaginatedList<ProveedorSummaryDto>>(
                $"Error interno al consultar proveedores: {ex.Message}");
        }
    }

    /// <summary>
    /// Aplica todos los filtros especificados en la consulta
    /// </summary>
    private async Task<IEnumerable<Domain.Proveedores.Entities.Proveedor>> AplicarFiltros(
        ObtenerProveedoresPaginadosQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("🔧 Aplicando filtros de búsqueda");

        // Empezar con consulta base según estado activo
        IEnumerable<Domain.Proveedores.Entities.Proveedor> proveedores;
        
        if (request.SoloActivos)
        {
            proveedores = await _proveedorRepository.ObtenerActivosAsync(
                request.IncluirContactos, 
                false, 
                cancellationToken);
        }
        else
        {
            proveedores = await _proveedorRepository.ObtenerTodosAsync(
                request.IncluirContactos, 
                false, 
                cancellationToken);
        }

        // Aplicar filtro de término de búsqueda
        if (!string.IsNullOrWhiteSpace(request.TerminoBusqueda))
        {
            proveedores = await FiltrarPorTerminoBusqueda(request.TerminoBusqueda, cancellationToken);
        }

        // Aplicar filtro de ciudad
        if (!string.IsNullOrWhiteSpace(request.Ciudad))
        {
            proveedores = proveedores.Where(p => 
                p.Ciudad.Contains(request.Ciudad, StringComparison.OrdinalIgnoreCase));
        }

        // Aplicar filtro de país
        if (!string.IsNullOrWhiteSpace(request.Pais))
        {
            proveedores = proveedores.Where(p => 
                p.Pais.Contains(request.Pais, StringComparison.OrdinalIgnoreCase));
        }

        // Aplicar filtros de días de crédito
        if (request.DiasCredito_Min.HasValue)
        {
            proveedores = proveedores.Where(p => p.DiasCredito >= request.DiasCredito_Min.Value);
        }

        if (request.DiasCredito_Max.HasValue)
        {
            proveedores = proveedores.Where(p => p.DiasCredito <= request.DiasCredito_Max.Value);
        }

        return proveedores;
    }

    /// <summary>
    /// Filtra proveedores por término de búsqueda usando repositorio
    /// </summary>
    private async Task<IEnumerable<Domain.Proveedores.Entities.Proveedor>> FiltrarPorTerminoBusqueda(
        string termino, 
        CancellationToken cancellationToken)
    {
        try
        {
            var resultados = await _proveedorRepository.BuscarAsync(termino, cancellationToken);
            _logger.LogDebug("🔍 Búsqueda por término '{Termino}' encontró {Cantidad} resultados", 
                termino, resultados.Count());
            return resultados;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Error en búsqueda por término: {Termino}", termino);
            return Enumerable.Empty<Domain.Proveedores.Entities.Proveedor>();
        }
    }

    /// <summary>
    /// Aplica ordenamiento según los criterios especificados
    /// </summary>
    private IEnumerable<Domain.Proveedores.Entities.Proveedor> AplicarOrdenamiento(
        IEnumerable<Domain.Proveedores.Entities.Proveedor> proveedores,
        ObtenerProveedoresPaginadosQuery request)
    {
        _logger.LogDebug("📊 Ordenando por {Campo} {Direccion}", request.CampoOrden, request.DireccionOrden);

        var esAscendente = request.DireccionOrden.Equals("asc", StringComparison.OrdinalIgnoreCase);

        return request.CampoOrden.ToLower() switch
        {
            "nombre" => esAscendente 
                ? proveedores.OrderBy(p => p.Nombre) 
                : proveedores.OrderByDescending(p => p.Nombre),
                
            "ciudad" => esAscendente 
                ? proveedores.OrderBy(p => p.Ciudad) 
                : proveedores.OrderByDescending(p => p.Ciudad),
                
            "pais" => esAscendente 
                ? proveedores.OrderBy(p => p.Pais) 
                : proveedores.OrderByDescending(p => p.Pais),
                
            "rfc" => esAscendente 
                ? proveedores.OrderBy(p => p.RFC) 
                : proveedores.OrderByDescending(p => p.RFC),
                
            "diascredito" => esAscendente 
                ? proveedores.OrderBy(p => p.DiasCredito) 
                : proveedores.OrderByDescending(p => p.DiasCredito),
                
            "fecharegistro" => esAscendente 
                ? proveedores.OrderBy(p => p.FechaRegistro) 
                : proveedores.OrderByDescending(p => p.FechaRegistro),
                
            "email" => esAscendente 
                ? proveedores.OrderBy(p => p.Email.Value) 
                : proveedores.OrderByDescending(p => p.Email.Value),
                
            "nombrecontacto" => esAscendente 
                ? proveedores.OrderBy(p => p.NombreContacto) 
                : proveedores.OrderByDescending(p => p.NombreContacto),
                
            _ => proveedores.OrderBy(p => p.Nombre) // Default
        };
    }

    /// <summary>
    /// Aplica paginación a los resultados
    /// </summary>
    private (IEnumerable<Domain.Proveedores.Entities.Proveedor> Items, int TotalCount) AplicarPaginacion(
        IEnumerable<Domain.Proveedores.Entities.Proveedor> proveedores,
        ObtenerProveedoresPaginadosQuery request)
    {
        var totalCount = proveedores.Count();
        var skip = (request.PageNumber - 1) * request.PageSize;
        var items = proveedores.Skip(skip).Take(request.PageSize);

        _logger.LogDebug("📄 Paginación aplicada - Total: {Total}, Saltando: {Skip}, Tomando: {Take}", 
            totalCount, skip, request.PageSize);

        return (items, totalCount);
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