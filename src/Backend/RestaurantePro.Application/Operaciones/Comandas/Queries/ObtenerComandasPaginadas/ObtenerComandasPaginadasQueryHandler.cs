using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.DTOs;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;

namespace RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerComandasPaginadas;

/// <summary>
/// Handler para obtener comandas con paginación y filtros opcionales
/// </summary>
public class ObtenerComandasPaginadasQueryHandler : IRequestHandler<ObtenerComandasPaginadasQuery, Result<PaginatedList<ComandaDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ObtenerComandasPaginadasQueryHandler> _logger;
    private readonly IMapper _mapper;

    public ObtenerComandasPaginadasQueryHandler(
        IApplicationDbContext context,
        ILogger<ObtenerComandasPaginadasQueryHandler> logger,
        IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<ComandaDto>>> Handle(ObtenerComandasPaginadasQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔍 Obteniendo comandas paginadas - Página: {PageNumber}, Tamaño: {PageSize}, Filtros: Estado={Estado}, Mesa={MesaId}, Mesero={MeseroId}",
                request.PageNumber, request.PageSize, request.Estado, request.MesaId, request.MeseroId);

            // Construir query base
            var query = _context.Comandas
                .Include(c => c.Items)
                .Include(c => c.Mesa)
                .Include(c => c.Mesero)
                .Include(c => c.Cliente)
                .AsNoTracking();

            // Aplicar filtros
            query = AplicarFiltros(query, request);

            // Aplicar ordenamiento
            query = AplicarOrdenamiento(query, request);

            // Obtener total de registros
            var totalCount = await query.CountAsync(cancellationToken);

            // Aplicar paginación
            var comandas = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Mapear a DTOs
            var comandasDto = _mapper.Map<List<ComandaDto>>(comandas);

            // Crear resultado paginado
            var resultado = new PaginatedList<ComandaDto>
            {
                Items = comandasDto,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            _logger.LogInformation("✅ Comandas obtenidas exitosamente - Total: {TotalCount}, Página: {PageNumber}, Items: {ItemsCount}",
                totalCount, request.PageNumber, comandasDto.Count);

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error obteniendo comandas paginadas: {ErrorMessage}", ex.Message);
            return Result.Failure<PaginatedList<ComandaDto>>($"Error obteniendo comandas: {ex.Message}");
        }
    }

    /// <summary>
    /// Aplica filtros a la consulta
    /// </summary>
    private IQueryable<Domain.Operaciones.Comandas.Entities.Comanda> AplicarFiltros(
        IQueryable<Domain.Operaciones.Comandas.Entities.Comanda> query, 
        ObtenerComandasPaginadasQuery request)
    {
        // Filtrar por estado
        if (!string.IsNullOrEmpty(request.Estado))
        {
            if (Enum.TryParse<EstadoComanda>(request.Estado, true, out var estado))
            {
                query = query.Where(c => c.Estado == estado);
            }
        }

        // Filtrar por mesa
        if (request.MesaId.HasValue)
        {
            query = query.Where(c => c.MesaId == request.MesaId.Value);
        }

        // Filtrar por mesero
        if (request.MeseroId.HasValue)
        {
            query = query.Where(c => c.MeseroId == request.MeseroId.Value);
        }

        // Filtrar por cliente
        if (request.ClienteId.HasValue)
        {
            query = query.Where(c => c.ClienteId == request.ClienteId.Value);
        }

        // Filtrar solo activas
        if (request.SoloActivas)
        {
            var estadosActivos = new[] { EstadoComanda.Creada, EstadoComanda.EnProceso, EstadoComanda.Lista, EstadoComanda.Entregada };
            query = query.Where(c => estadosActivos.Contains(c.Estado));
        }

        // Filtrar por fecha desde
        if (request.FechaDesde.HasValue)
        {
            query = query.Where(c => c.FechaCreacion >= request.FechaDesde.Value);
        }

        // Filtrar por fecha hasta
        if (request.FechaHasta.HasValue)
        {
            query = query.Where(c => c.FechaCreacion <= request.FechaHasta.Value);
        }

        return query;
    }

    /// <summary>
    /// Aplica ordenamiento a la consulta
    /// </summary>
    private IQueryable<Domain.Operaciones.Comandas.Entities.Comanda> AplicarOrdenamiento(
        IQueryable<Domain.Operaciones.Comandas.Entities.Comanda> query, 
        ObtenerComandasPaginadasQuery request)
    {
        var ordenarPor = request.OrdenarPor?.ToLower() ?? "fechacreacion";
        var direccion = request.DireccionOrdenamiento?.ToLower() ?? "desc";

        return ordenarPor switch
        {
            "fechacreacion" => direccion == "asc" 
                ? query.OrderBy(c => c.FechaCreacion)
                : query.OrderByDescending(c => c.FechaCreacion),
                
            "numerocomanda" => direccion == "asc"
                ? query.OrderBy(c => c.NumeroComanda)
                : query.OrderByDescending(c => c.NumeroComanda),
                
            "estado" => direccion == "asc"
                ? query.OrderBy(c => c.Estado)
                : query.OrderByDescending(c => c.Estado),
                
            "mesa" => direccion == "asc"
                ? query.OrderBy(c => c.Mesa.Numero)
                : query.OrderByDescending(c => c.Mesa.Numero),
                
            "mesero" => direccion == "asc"
                ? query.OrderBy(c => c.Mesero.NombreCompleto)
                : query.OrderByDescending(c => c.Mesero.NombreCompleto),
                
            "cliente" => direccion == "asc"
                ? query.OrderBy(c => c.Cliente.Nombre)
                : query.OrderByDescending(c => c.Cliente.Nombre),
                
            "total" => direccion == "asc"
                ? query.OrderBy(c => c.Items.Sum(i => i.PrecioUnitario * i.Cantidad))
                : query.OrderByDescending(c => c.Items.Sum(i => i.PrecioUnitario * i.Cantidad)),
                
            _ => query.OrderByDescending(c => c.FechaCreacion) // Ordenamiento por defecto
        };
    }
} 