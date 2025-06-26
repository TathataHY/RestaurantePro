using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Comercial.Promociones.DTOs;
using RestaurantePro.Domain.Comercial.Promociones.Entities;
using RestaurantePro.Domain.Comercial.Promociones.Enums;

namespace RestaurantePro.Application.Comercial.Promociones.Commands.CrearPromocion;

/// <summary>
/// Handler para crear una nueva promoción
/// </summary>
public class CrearPromocionHandler : IRequestHandler<CrearPromocionCommand, Result<PromocionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CrearPromocionHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public CrearPromocionHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<CrearPromocionHandler> logger,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PromocionDto>> Handle(CrearPromocionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🎁 Creando nueva promoción: {Codigo} - {Nombre}", request.Codigo, request.Nombre);

            // Validar que el código sea único
            var codigoExiste = await _context.Promociones
                .AnyAsync(p => p.Codigo == request.Codigo, cancellationToken);

            if (codigoExiste)
            {
                return Result.Failure<PromocionDto>("Ya existe una promoción con este código");
            }

            // Crear la promoción usando el factory method del dominio
            var promocion = Promocion.Crear(
                request.Codigo,
                request.Nombre,
                request.Descripcion,
                request.Tipo,
                request.ValorDescuento,
                request.FechaInicio,
                request.FechaFin,
                request.MontoMinimo,
                request.PuntosRequeridos,
                request.MaximoUsos,
                request.EsAcumulable);

            // Establecer días válidos si se especifican
            if (request.DiasValidos?.Any() == true)
            {
                promocion.EstablecerDiasValidos(request.DiasValidos.ToArray());
            }

            // Agregar productos aplicables
            if (request.ProductosAplicablesIds?.Any() == true)
            {
                foreach (var productoId in request.ProductosAplicablesIds)
                {
                    promocion.AgregarProductoAplicable(productoId);
                }
            }

            // Agregar categorías aplicables
            if (request.CategoriasAplicablesIds?.Any() == true)
            {
                foreach (var categoriaId in request.CategoriasAplicablesIds)
                {
                    promocion.AgregarCategoriaAplicable(categoriaId);
                }
            }

            // Guardar en la base de datos
            _context.Promociones.Add(promocion);
            await _context.SaveChangesAsync(cancellationToken);

            // Mapear a DTO
            var promocionDto = _mapper.Map<PromocionDto>(promocion);

            _logger.LogInformation("✅ Promoción creada exitosamente: {Id}", promocion.Id);

            return Result.Success(promocionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creando promoción: {Codigo}", request.Codigo);
            return Result.Failure<PromocionDto>($"Error creando promoción: {ex.Message}");
        }
    }
} 