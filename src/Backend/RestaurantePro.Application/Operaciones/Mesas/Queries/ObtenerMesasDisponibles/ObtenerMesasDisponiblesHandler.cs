namespace RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerMesasDisponibles;

/// <summary>
/// Handler para ObtenerMesasDisponiblesQuery
/// </summary>
public class ObtenerMesasDisponiblesHandler : IRequestHandler<ObtenerMesasDisponiblesQuery, Result<PaginatedList<MesaDto>>>
{
    private readonly IMesaRepository _mesaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerMesasDisponiblesHandler> _logger;
    private readonly ICurrentUserService _currentUser;

    public ObtenerMesasDisponiblesHandler(
        IMesaRepository mesaRepository,
        IMapper mapper,
        ILogger<ObtenerMesasDisponiblesHandler> logger,
        ICurrentUserService currentUser)
    {
        _mesaRepository = mesaRepository;
        _mapper = mapper;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedList<MesaDto>>> Handle(ObtenerMesasDisponiblesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔍 Obteniendo mesas disponibles - Página: {Pagina}, Tamaño: {TamanoPagina}, Capacidad: {Capacidad}, Zona: {Zona}", 
                request.Pagina, request.TamanoPagina, request.CapacidadMinima?.ToString() ?? "Todas", request.Zona ?? "Todas");

            // Obtener todas las mesas disponibles del repositorio
            var mesas = await _mesaRepository.ObtenerMesasDisponiblesAsync();

            if (mesas == null || !mesas.Any())
            {
                _logger.LogInformation("📋 No se encontraron mesas disponibles con los criterios especificados");
                return Result.Success(new PaginatedList<MesaDto>(new List<MesaDto>(), 0, request.Pagina, request.TamanoPagina));
            }

            // Filtrar según criterios adicionales
            var mesasFiltradas = mesas.AsEnumerable();

            // Filtrar por capacidad mínima si se especifica
            if (request.CapacidadMinima.HasValue)
            {
                mesasFiltradas = mesasFiltradas.Where(m => m.Capacidad >= request.CapacidadMinima.Value);
            }

            // Filtrar por zona si se especifica
            if (!string.IsNullOrEmpty(request.Zona))
            {
                mesasFiltradas = mesasFiltradas.Where(m => m.Ubicacion.Equals(request.Zona, StringComparison.OrdinalIgnoreCase));
            }

            // Verificar que realmente estén disponibles (doble verificación)
            mesasFiltradas = mesasFiltradas.Where(m => m.Estado == EstadoMesa.Disponible);

            if (!mesasFiltradas.Any())
            {
                _logger.LogInformation("⚠️ No hay mesas disponibles que cumplan los criterios especificados");
                return Result.Success(new PaginatedList<MesaDto>(new List<MesaDto>(), 0, request.Pagina, request.TamanoPagina));
            }

            // Ordenar si se especifica
            if (request.OrdenarPorNumero)
            {
                mesasFiltradas = mesasFiltradas.OrderBy(m => m.Numero);
            }

            // Convertir a lista para aplicar paginación
            var listaMesas = mesasFiltradas.ToList();

            // Aplicar paginación
            var mesasPaginadas = listaMesas
                .Skip((request.Pagina - 1) * request.TamanoPagina)
                .Take(request.TamanoPagina)
                .ToList();

            // Mapear a DTOs
            var mesasDto = _mapper.Map<List<MesaDto>>(mesasPaginadas);

            // Crear resultado paginado
            var resultado = new PaginatedList<MesaDto>(
                mesasDto,
                listaMesas.Count,
                request.Pagina,
                request.TamanoPagina);

            _logger.LogInformation("✅ Se encontraron {TotalMesas} mesas disponibles. Página {Pagina} de {TotalPaginas} ({MesasEnPagina} mesas en esta página)", 
                resultado.TotalCount, 
                resultado.PageNumber, 
                resultado.TotalPages, 
                resultado.Items.Count);

            // Log adicional con detalles de filtros aplicados
            if (request.CapacidadMinima.HasValue)
            {
                _logger.LogDebug("🔍 Filtro aplicado - Capacidad mínima: {CapacidadMinima}", request.CapacidadMinima.Value);
            }
            
            if (!string.IsNullOrEmpty(request.Zona))
            {
                _logger.LogDebug("🔍 Filtro aplicado - Zona: {Zona}", request.Zona);
            }

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener mesas disponibles");
            return Result.Failure<PaginatedList<MesaDto>>("Error interno del servidor al obtener las mesas disponibles");
        }
    }
}