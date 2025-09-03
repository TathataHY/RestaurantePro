using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.DTOs;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using System.Linq.Expressions;

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

            // Construir query base con includes optimizados - solo lo necesario
            var query = _context.Comandas
                .Include(c => c.Mesa)      // Necesario para mostrar número de mesa
                .Include(c => c.Mesero)    // Necesario para mostrar nombre del mesero
                .Include(c => c.Cliente)   // Necesario para mostrar nombre del cliente
                // Removido: .Include(c => c.Items) - se carga bajo demanda si es necesario
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(request.Estado))
            {
                if (Enum.TryParse<EstadoComanda>(request.Estado, true, out var estado))
                {
                    query = query.Where(c => c.Estado == estado);
                }
            }
            if (request.MesaId != null)
            {
                query = query.Where(c => c.MesaId == request.MesaId);
            }
            if (request.ClienteId != null)
            {
                query = query.Where(c => c.ClienteId == request.ClienteId);
            }
            if (request.FechaDesde != null)
            {
                query = query.Where(c => c.FechaCreacion >= request.FechaDesde);
            }
            if (request.FechaHasta != null)
            {
                query = query.Where(c => c.FechaCreacion <= request.FechaHasta);
            }

            if (request.SoloActivas)
            {
                var estadosActivos = new[] { EstadoComanda.Creada, EstadoComanda.EnProceso, EstadoComanda.Lista, EstadoComanda.Entregada };
                query = query.Where(c => estadosActivos.Contains(c.Estado));
            }

            // Aplicar ordenamiento
            var direccion = request.DireccionOrdenamiento?.ToLower() ?? "desc";
            switch (request.OrdenarPor)
            {
                case "fecha":
                    query = direccion == "desc"
                        ? Queryable.OrderByDescending<Comanda, DateTime>(query, c => c.FechaCreacion)
                        : Queryable.OrderBy<Comanda, DateTime>(query, c => c.FechaCreacion);
                    break;
                case "estado":
                    query = direccion == "desc"
                        ? Queryable.OrderByDescending<Comanda, int>(query, c => (int)c.Estado)
                        : Queryable.OrderBy<Comanda, int>(query, c => (int)c.Estado);
                    break;
                case "mesa":
                    query = direccion == "desc"
                        ? Queryable.OrderByDescending<Comanda, Guid?>(query, c => c.MesaId)
                        : Queryable.OrderBy<Comanda, Guid?>(query, c => c.MesaId);
                    break;
                default:
                    // Orden por fecha por defecto
                    query = direccion == "desc"
                        ? Queryable.OrderByDescending<Comanda, DateTime>(query, c => c.FechaCreacion)
                        : Queryable.OrderBy<Comanda, DateTime>(query, c => c.FechaCreacion);
                    break;
            }

            // Obtener total de registros
            var totalCount = await query.CountAsync(cancellationToken);

            // Aplicar paginación
            var comandas = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Mapear a DTOs (sin items para mejor rendimiento)
            var comandasDto = _mapper.Map<List<ComandaDto>>(comandas);
            
            // Si se necesitan los items, cargarlos por separado (bajo demanda)
            if (request.IncluirItems)
            {
                var comandaIds = comandas.Select(c => c.Id).ToList();
                var items = await _context.ItemsComanda
                    .Where(i => comandaIds.Contains(i.ComandaId))
                    .ToListAsync(cancellationToken);
                
                // Agrupar items por comanda
                var itemsPorComanda = items.GroupBy(i => i.ComandaId).ToDictionary(g => g.Key, g => g.ToList());
                
                // Obtener IDs de productos únicos para cargar sus nombres
                var productoIds = items.Select(i => i.ProductoId).Distinct().ToList();
                var productos = await _context.Productos
                    .Where(p => productoIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id, p => p, cancellationToken);
                
                // Asignar items a cada comanda
                foreach (var comandaDto in comandasDto)
                {
                    if (itemsPorComanda.TryGetValue(comandaDto.Id, out var itemsComanda))
                    {
                        var itemsDto = _mapper.Map<List<ItemComandaDto>>(itemsComanda);
                        
                        // Mapear nombres de productos manualmente
                        foreach (var itemDto in itemsDto)
                        {
                            if (productos.TryGetValue(itemDto.ProductoId, out var producto))
                            {
                                itemDto.NombreProducto = producto.Nombre;
                                itemDto.DescripcionProducto = producto.Descripcion;
                            }
                            else
                            {
                                itemDto.NombreProducto = $"Producto {itemDto.ProductoId}";
                            }
                        }
                        
                        // Mapear ItemComandaDto a ComandaProductoDto para el frontend
                        comandaDto.Items = _mapper.Map<List<ComandaProductoDto>>(itemsDto);
                    }
                }
            }

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
} 