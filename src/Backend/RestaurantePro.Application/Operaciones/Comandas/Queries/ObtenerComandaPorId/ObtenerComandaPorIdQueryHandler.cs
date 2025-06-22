using Microsoft.EntityFrameworkCore;
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
                request.Id, request.IncluirItems);

            // Construir query base
            var query = _context.Comandas
                .Include(c => c.Mesa)
                .Include(c => c.Mesero)
                .Include(c => c.Cliente)
                .AsNoTracking();

            // Incluir items si se solicita
            if (request.IncluirItems)
            {
                query = query.Include(c => c.Items);
            }

            // Buscar la comanda
            var comanda = await query.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (comanda == null)
            {
                _logger.LogWarning("⚠️ Comanda no encontrada con ID: {ComandaId}", request.Id);
                return Result.Failure<ComandaDto>($"Comanda con ID {request.Id} no encontrada");
            }

            // Mapear a DTO
            var comandaDto = _mapper.Map<ComandaDto>(comanda);

            _logger.LogInformation("✅ Comanda obtenida exitosamente - ID: {ComandaId}, Número: {NumeroComanda}, Estado: {Estado}",
                comanda.Id, comanda.NumeroComanda, comanda.Estado);

            return Result.Success(comandaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error obteniendo comanda por ID {ComandaId}: {ErrorMessage}", 
                request.Id, ex.Message);
            return Result.Failure<ComandaDto>($"Error obteniendo comanda: {ex.Message}");
        }
    }
} 