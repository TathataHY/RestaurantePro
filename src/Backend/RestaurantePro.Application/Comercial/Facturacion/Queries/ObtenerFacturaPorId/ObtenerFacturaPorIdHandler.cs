namespace RestaurantePro.Application.Comercial.Facturacion.Queries.ObtenerFacturaPorId;

public class ObtenerFacturaPorIdHandler : IRequestHandler<ObtenerFacturaPorIdQuery, Result<FacturaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerFacturaPorIdHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public ObtenerFacturaPorIdHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ObtenerFacturaPorIdHandler> logger,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<FacturaDto>> Handle(ObtenerFacturaPorIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Verificar explícitamente si la cancelación ha sido solicitada
            cancellationToken.ThrowIfCancellationRequested();
            
            _logger.LogInformation("Consultando factura por ID: {FacturaId}", request.FacturaId);

            // 1. Obtener la factura con las relaciones necesarias
            var factura = await _context.Facturas
                // TODO: Descomentar cuando las relaciones estén implementadas
                // .Include(f => f.Cliente)
                // .Include(f => f.Detalles)
                .FirstOrDefaultAsync(f => f.Id == request.FacturaId, cancellationToken);

            // Verificar nuevamente la cancelación después de la operación de base de datos
            cancellationToken.ThrowIfCancellationRequested();

            if (factura == null)
            {
                _logger.LogWarning("Factura {FacturaId} no encontrada", request.FacturaId);
                return Result.Failure<FacturaDto>("La factura especificada no existe.");
            }

            // 2. Crear el DTO usando el DTO existente
            var facturaDto = CrearFacturaDto(factura, request);

            _logger.LogInformation("Factura {FacturaId} consultada exitosamente", request.FacturaId);

            return Result.Success(facturaDto);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("🚫 Operación cancelada al consultar factura {FacturaId}", request.FacturaId);
            throw; // Re-throw para que se propague correctamente
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar factura {FacturaId}", request.FacturaId);
            return Result.Failure<FacturaDto>("Error interno al consultar la factura.");
        }
    }

    private FacturaDto CrearFacturaDto(Factura factura, ObtenerFacturaPorIdQuery request)
    {
        // Verificar si la factura está vencida
        var estado = factura.Estado;
        if (factura.FechaVencimiento < DateTime.Now && 
            factura.Estado != EstadoFactura.Anulada)
        {
            estado = EstadoFactura.Vencida;
        }
        
        // Calcular el total correcto a partir de los componentes
        decimal totalCalculado = factura.Subtotal - factura.TotalDescuentos + factura.TotalImpuestos;
        
        return new FacturaDto
        {
            Id = factura.Id,
            Numero = factura.NumeroFactura,
            ClienteId = factura.ClienteId,
            NombreCliente = factura.NombreCliente,
            FechaEmision = factura.FechaEmision,
            FechaVencimiento = factura.FechaVencimiento,
            Estado = estado,
            Tipo = factura.TipoFactura,
            Subtotal = factura.Subtotal,
            Impuestos = factura.TotalImpuestos,
            Descuentos = factura.TotalDescuentos,
            Total = totalCalculado, // Usar el total calculado en lugar del almacenado
            MontoPagado = factura.TotalPagado,
            FechaPago = factura.FechaPago,
            MetodoPago = "Efectivo", // TODO: Obtener de la entidad cuando esté disponible
            ReferenciaPago = "", // TODO: Obtener de la entidad cuando esté disponible
            FechaCreacion = factura.FechaCreacion,
            CreadoPor = "Sistema", // TODO: Obtener de la entidad cuando esté disponible
            // FechaModificacion = factura.FechaModificacion, // TODO: Descomentar cuando esté disponible
            // ModificadoPor = factura.ModificadoPor, // TODO: Descomentar cuando esté disponible
            Activo = true // TODO: Obtener de la entidad cuando esté disponible
        };
    }
} 