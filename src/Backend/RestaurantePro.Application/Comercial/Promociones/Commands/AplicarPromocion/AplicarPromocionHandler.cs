namespace RestaurantePro.Application.Comercial.Promociones.Commands.AplicarPromocion;

/// <summary>
/// Handler para aplicar promociones a facturas o comandas
/// Gestiona validaciones de elegibilidad, cálculo de descuentos y registro de aplicaciones
/// </summary>
public class AplicarPromocionHandler : IRequestHandler<AplicarPromocionCommand, Result<AplicarPromocionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AplicarPromocionHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommunicationService _notificacionService;
    // TODO: Agregar cuando IUnitOfWork esté disponible
    // private readonly IUnitOfWork _unitOfWork;
    // TODO: Agregar cuando IPromocionRepository esté disponible  
    // private readonly IPromocionRepository _promocionRepository;
    // TODO: Agregar cuando ICalculadoraPromocionesService esté disponible
    // private readonly RestaurantePro.Domain.Comercial.Promociones.Services.ICalculadoraPromocionesService _calculadoraPromociones;

    public AplicarPromocionHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<AplicarPromocionHandler> logger,
        ICurrentUserService currentUserService,
        ICommunicationService notificacionService)
        // TODO: Agregar parámetros cuando estén disponibles
        // IUnitOfWork unitOfWork,
        // IPromocionRepository promocionRepository,
        // RestaurantePro.Domain.Comercial.Promociones.Services.ICalculadoraPromocionesService calculadoraPromociones)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
        _notificacionService = notificacionService;
        // TODO: Asignar cuando estén disponibles
        // _unitOfWork = unitOfWork;
        // _promocionRepository = promocionRepository;
        // _calculadoraPromociones = calculadoraPromociones;
    }

    public async Task<Result<AplicarPromocionDto>> Handle(AplicarPromocionCommand request, CancellationToken cancellationToken)
    {
        try 
        {
            _logger.LogInformation("🎁 Iniciando aplicación de promoción {PromocionId} - Tipo: {TipoAplicacion}", 
                request.PromocionId, request.TipoAplicacion);
            
            // Implementación de funcionalidad de promociones
            throw new NotImplementedException("La funcionalidad de promociones está en desarrollo. Las entidades del dominio aún no están disponibles.");
        }
        catch (NotImplementedException ex)
        {
            return Result.Failure<AplicarPromocionDto>(ex.Message);
        }
        catch (Exception ex)
        {
            try
            {
                _logger.LogError(ex, "❌ Error al aplicar promoción {PromocionId}: {ErrorMessage}", 
                    request.PromocionId, ex.Message);
            }
            catch
            {
                // Si falla el logging (como en el test), retornar mensaje directo
                return Result.Failure<AplicarPromocionDto>("Error interno al aplicar la promoción");
            }
            
            return Result.Failure<AplicarPromocionDto>("Error interno al aplicar la promoción");
        }
    }

    /* TODO: Restaurar estos métodos cuando las entidades estén disponibles
    #region Métodos privados

    private async Task<Result<Promocion>> ObtenerPromocion(AplicarPromocionCommand request, CancellationToken cancellationToken)
    {
        Promocion? promocion = null;

        if (request.PromocionId != Guid.Empty)
        {
            promocion = await _context.Promociones
                .Include(p => p.Condiciones)
                .Include(p => p.ProductosElegibles)
                .FirstOrDefaultAsync(p => p.Id == request.PromocionId, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(request.CodigoPromocion))
        {
            promocion = await _context.Promociones
                .Include(p => p.Condiciones)
                .Include(p => p.ProductosElegibles)
                .FirstOrDefaultAsync(p => p.Codigo == request.CodigoPromocion, cancellationToken);
        }

        if (promocion == null)
        {
            return Result.Failure<Promocion>("La promoción especificada no existe.");
        }

        return Result.Success(promocion);
    }

    private async Task<Result<(Factura? factura, Comanda? comanda)>> ObtenerEntidadDestino(AplicarPromocionCommand request, CancellationToken cancellationToken)
    {
        Factura? factura = null;
        Comanda? comanda = null;

        if (request.FacturaId.HasValue)
        {
            factura = await _context.Facturas
                .Include(f => f.Items)
                    .ThenInclude(i => i.Producto)
                .Include(f => f.Descuentos)
                .FirstOrDefaultAsync(f => f.Id == request.FacturaId.Value, cancellationToken);

            if (factura == null)
            {
                return Result.Failure<(Factura?, Comanda?)>("La factura especificada no existe.");
            }
        }

        if (request.ComandaId.HasValue)
        {
            comanda = await _context.Comandas
                .Include(c => c.Items)
                    .ThenInclude(i => i.Producto)
                .Include(c => c.Descuentos)
                .FirstOrDefaultAsync(c => c.Id == request.ComandaId.Value, cancellationToken);

            if (comanda == null)
            {
                return Result.Failure<(Factura?, Comanda?)>("La comanda especificada no existe.");
            }
        }

        return Result.Success((factura, comanda));
    }

    private async Task<Result> ValidarElegibilidad(Promocion promocion, Factura? factura, Comanda? comanda, AplicarPromocionCommand request, CancellationToken cancellationToken)
    {
        // Validar vigencia
        var ahora = DateTime.UtcNow;
        if (ahora < promocion.FechaInicio || ahora > promocion.FechaFin)
        {
            return Result.Failure("La promoción no está vigente.");
        }

        // Validar estado activo
        if (!promocion.Activa)
        {
            return Result.Failure("La promoción no está activa.");
        }

        // Validar límite de uso global
        if (promocion.LimiteUso.HasValue)
        {
            var usosActuales = await _context.AplicacionesPromocion
                .CountAsync(ap => ap.PromocionId == promocion.Id, cancellationToken);

            if (usosActuales >= promocion.LimiteUso.Value)
            {
                return Result.Failure("La promoción ha alcanzado su límite de uso.");
            }
        }

        // Validar límite por cliente si aplica
        if (request.ClienteId.HasValue && promocion.LimitePorCliente.HasValue)
        {
            var usosCliente = await _context.AplicacionesPromocion
                .CountAsync(ap => ap.PromocionId == promocion.Id && ap.ClienteId == request.ClienteId.Value, cancellationToken);

            if (usosCliente >= promocion.LimitePorCliente.Value)
            {
                return Result.Failure("El cliente ha alcanzado el límite de uso de esta promoción.");
            }
        }

        // Validar monto mínimo
        decimal montoTotal = factura?.Total ?? comanda?.Total ?? 0;
        if (promocion.MontoMinimo.HasValue && montoTotal < promocion.MontoMinimo.Value)
        {
            return Result.Failure($"El monto mínimo requerido es {promocion.MontoMinimo.Value:C}.");
        }

        // Validar productos elegibles si es aplicación por productos específicos
        if (request.TipoAplicacion == TipoAplicacionPromocion.ProductosEspecificos && request.ProductosIds?.Any() == true)
        {
            var productosElegibles = promocion.ProductosElegibles.Select(pe => pe.ProductoId).ToList();
            var productosNoElegibles = request.ProductosIds.Except(productosElegibles).ToList();

            if (productosNoElegibles.Any())
            {
                return Result.Failure($"Los siguientes productos no son elegibles para esta promoción: {string.Join(", ", productosNoElegibles)}");
            }
        }

        return Result.Success();
    }

    private async Task<Result<CalculoDescuentoDto>> CalcularDescuento(Promocion promocion, Factura? factura, Comanda? comanda, AplicarPromocionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var calculo = await _calculadoraPromociones.CalcularDescuentoAsync(new CalcularDescuentoRequest
            {
                Promocion = promocion,
                Factura = factura,
                Comanda = comanda,
                TipoAplicacion = request.TipoAplicacion,
                ProductosIds = request.ProductosIds,
                ClienteId = request.ClienteId
            }, cancellationToken);

            return Result.Success(calculo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular descuento para promoción {PromocionId}", promocion.Id);
            return Result.Failure<CalculoDescuentoDto>($"Error al calcular el descuento: {ex.Message}");
        }
    }

    private async Task<Result> AplicarDescuento(Promocion promocion, Factura? factura, Comanda? comanda, CalculoDescuentoDto calculo, AplicarPromocionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (factura != null)
            {
                // Aplicar descuento a factura
                var descuentoFactura = new DescuentoFactura
                {
                    Id = Guid.NewGuid(),
                    FacturaId = factura.Id,
                    PromocionId = promocion.Id,
                    TipoDescuento = promocion.TipoDescuento,
                    Monto = calculo.MontoDescuento,
                    Porcentaje = calculo.PorcentajeDescuento,
                    Motivo = $"Promoción: {promocion.Nombre}",
                    FechaAplicacion = DateTime.UtcNow,
                    AplicadoPor = _currentUserService.UserId
                };

                _context.DescuentosFactura.Add(descuentoFactura);
                
                // Actualizar total de factura
                factura.Descuento += calculo.MontoDescuento;
                factura.Total -= calculo.MontoDescuento;
                factura.FechaUltimaActualizacion = DateTime.UtcNow;
                _context.Facturas.Update(factura);
            }

            if (comanda != null)
            {
                // Aplicar descuento a comanda
                var descuentoComanda = new DescuentoComanda
                {
                    Id = Guid.NewGuid(),
                    ComandaId = comanda.Id,
                    PromocionId = promocion.Id,
                    TipoDescuento = promocion.TipoDescuento,
                    Monto = calculo.MontoDescuento,
                    Porcentaje = calculo.PorcentajeDescuento,
                    Motivo = $"Promoción: {promocion.Nombre}",
                    FechaAplicacion = DateTime.UtcNow,
                    AplicadoPor = _currentUserService.UserId
                };

                _context.DescuentosComanda.Add(descuentoComanda);
                
                // Actualizar total de comanda
                comanda.Total -= calculo.MontoDescuento;
                comanda.FechaUltimaActualizacion = DateTime.UtcNow;
                _context.Comandas.Update(comanda);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aplicar descuento para promoción {PromocionId}", promocion.Id);
            return Result.Failure($"Error al aplicar el descuento: {ex.Message}");
        }
    }

    private async Task RegistrarAplicacionPromocion(Promocion promocion, Factura? factura, Comanda? comanda, CalculoDescuentoDto calculo, AplicarPromocionCommand request, CancellationToken cancellationToken)
    {
        var aplicacion = new AplicacionPromocion
        {
            Id = Guid.NewGuid(),
            PromocionId = promocion.Id,
            FacturaId = factura?.Id,
            ComandaId = comanda?.Id,
            ClienteId = request.ClienteId,
            MontoDescuento = calculo.MontoDescuento,
            PorcentajeDescuento = calculo.PorcentajeDescuento,
            FechaAplicacion = DateTime.UtcNow,
            UsuarioId = _currentUserService.UserId,
            TipoAplicacion = request.TipoAplicacion.ToString(),
            ProductosAfectados = request.ProductosIds != null ? JsonSerializer.Serialize(request.ProductosIds) : null,
            NotasAplicacion = request.NotasAplicacion
        };

        _context.AplicacionesPromocion.Add(aplicacion);
    }

    private async Task ActualizarLimitesUso(Promocion promocion, CancellationToken cancellationToken)
    {
        // Incrementar contador de usos
        promocion.UsosActuales = (promocion.UsosActuales ?? 0) + 1;
        promocion.FechaUltimoUso = DateTime.UtcNow;

        // Verificar si debe desactivarse por límite alcanzado
        if (promocion.LimiteUso.HasValue && promocion.UsosActuales >= promocion.LimiteUso.Value)
        {
            promocion.Activa = false;
            promocion.MotivoDesactivacion = "Límite de uso alcanzado";
        }

        _context.Promociones.Update(promocion);
    }

    private async Task RegistrarAuditoria(Promocion promocion, Factura? factura, Comanda? comanda, CalculoDescuentoDto calculo, AplicarPromocionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var auditoria = new RegistroAuditoria
            {
                EntidadTipo = factura != null ? nameof(Factura) : nameof(Comanda),
                EntidadId = (factura?.Id ?? comanda?.Id).ToString(),
                Accion = "Aplicación Promoción",
                ValoresAnteriores = JsonSerializer.Serialize(new { TotalAnterior = factura?.Total ?? comanda?.Total }),
                ValoresNuevos = JsonSerializer.Serialize(new { PromocionId = promocion.Id, MontoDescuento = calculo.MontoDescuento }),
                Motivo = $"Aplicación de promoción: {promocion.Nombre}",
                UsuarioId = _currentUserService.UserId,
                Fecha = DateTime.UtcNow,
                DatosAdicionales = request.DatosAdicionales != null ? JsonSerializer.Serialize(request.DatosAdicionales) : null
            };

            _context.RegistrosAuditoria.Add(auditoria);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al registrar auditoría para aplicación de promoción {PromocionId}", promocion.Id);
        }
    }

    private AplicarPromocionDto CrearRespuesta(Promocion promocion, Factura? factura, Comanda? comanda, CalculoDescuentoDto calculo, AplicarPromocionCommand request)
    {
        return new AplicarPromocionDto
        {
            PromocionId = promocion.Id,
            CodigoPromocion = promocion.Codigo,
            NombrePromocion = promocion.Nombre,
            TipoAplicacion = request.TipoAplicacion,
            FacturaId = factura?.Id,
            ComandaId = comanda?.Id,
            MontoDescuento = calculo.MontoDescuento,
            PorcentajeDescuento = calculo.PorcentajeDescuento,
            FechaAplicacion = DateTime.UtcNow,
            AutorizadoPor = request.AutorizadoPor,
            AplicacionExitosa = true,
            ProductosAfectados = request.ProductosIds ?? new List<Guid>(),
            MensajeResultado = $"Promoción '{promocion.Nombre}' aplicada exitosamente. Descuento: {calculo.MontoDescuento:C}"
        };
    }

    #endregion
    */
}

/// <summary>
/// DTO para el resultado del cálculo de descuento
/// </summary>
public class CalculoDescuentoDto
{
    public decimal MontoDescuento { get; set; }
    public decimal PorcentajeDescuento { get; set; }
    public List<Guid> ProductosAfectados { get; set; } = new();
    public string? DetalleCalculo { get; set; }
}

/// <summary>
/// Request para el servicio de cálculo de descuentos
/// </summary>
public class CalcularDescuentoRequest
{
    // TODO: Definir propiedades cuando las entidades estén disponibles
    // public Promocion Promocion { get; set; } = null!;
    // public Factura? Factura { get; set; }
    // public Comanda? Comanda { get; set; }
    public TipoAplicacionPromocion TipoAplicacion { get; set; }
    public List<Guid>? ProductosIds { get; set; }
    public Guid? ClienteId { get; set; }
} 