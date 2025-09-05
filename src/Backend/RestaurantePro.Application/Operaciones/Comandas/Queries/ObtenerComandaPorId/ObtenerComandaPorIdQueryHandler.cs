using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.DTOs;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;

namespace RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerComandaPorId;

/// <summary>
/// Handler para obtener una comanda específica por ID
/// </summary>
public class ObtenerComandaPorIdQueryHandler : IRequestHandler<ObtenerComandaPorIdQuery, Result<ComandaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ObtenerComandaPorIdQueryHandler> _logger;
    private readonly IMapper _mapper;

    public ObtenerComandaPorIdQueryHandler(
        IApplicationDbContext context,
        ILogger<ObtenerComandaPorIdQueryHandler> logger,
        IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<ComandaDto>> Handle(ObtenerComandaPorIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔍 Obteniendo comanda por ID: {ComandaId}, IncluirItems: {IncluirItems}", 
                request.ComandaId, request.IncluirItems);

            // Construir query base
            IQueryable<Domain.Operaciones.Comandas.Entities.Comanda> query = _context.Comandas
                .Include(c => c.Mesa)
                .Include(c => c.Mesero)
                .Include(c => c.Cliente);

            // Incluir items si se solicita
            if (request.IncluirItems)
            {
                query = query.Include(c => c.Items);
            }

            // Buscar la comanda
            var comanda = await query.FirstOrDefaultAsync(c => c.Id == request.ComandaId, cancellationToken);

            if (comanda == null)
            {
                _logger.LogWarning("⚠️ Comanda no encontrada: {ComandaId}", request.ComandaId);
                return Result.Failure<ComandaDto>($"No se encontró una comanda con el ID {request.ComandaId}");
            }

            // Mapear a DTO
            var comandaDto = _mapper.Map<ComandaDto>(comanda);

            // Enriquecer los items con el nombre del producto (si se incluyeron items)
            if (request.IncluirItems && comandaDto.Items != null && comandaDto.Items.Count > 0)
            {
                var productoIds = comanda.Items
                    .Select(i => i.ProductoId)
                    .Distinct()
                    .ToList();

                var productos = await _context.Productos
                    .Where(p => productoIds.Contains(p.Id))
                    .Select(p => new { p.Id, p.Nombre })
                    .ToListAsync(cancellationToken);

                var productosDict = productos.ToDictionary(p => p.Id, p => p.Nombre);

                foreach (var item in comandaDto.Items)
                {
                    if (productosDict.TryGetValue(item.ProductoId, out var nombre) && !string.IsNullOrWhiteSpace(nombre))
                    {
                        item.Nombre = nombre;
                    }
                    else if (string.IsNullOrWhiteSpace(item.Nombre))
                    {
                        item.Nombre = "Producto";
                    }
                }

                _logger.LogInformation("🧩 Items enriquecidos con nombres de producto: {Count}", comandaDto.Items.Count);
            }

            _logger.LogInformation("✅ Comanda obtenida exitosamente: {ComandaId}", request.ComandaId);
            return Result.Success(comandaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error obteniendo comanda por ID: {ComandaId}", request.ComandaId);
            return Result.Failure<ComandaDto>($"Error interno al obtener la comanda: {ex.Message}");
        }
    }
} 