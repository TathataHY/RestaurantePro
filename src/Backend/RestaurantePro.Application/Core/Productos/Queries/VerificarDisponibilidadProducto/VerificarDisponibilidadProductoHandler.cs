namespace RestaurantePro.Application.Core.Productos.Queries.VerificarDisponibilidadProducto;

/// <summary>
/// Handler para verificar disponibilidad de productos con análisis de inventario
/// </summary>
public class VerificarDisponibilidadProductoHandler : IRequestHandler<VerificarDisponibilidadProductoQuery, Result<DisponibilidadProductoDto>>
{
    private readonly IProductoRepository _productoRepository;
    private readonly IInventarioIngredientesRepository _inventarioRepository;
    private readonly IProductoService _productoService;
    private readonly IMapper _mapper;
    private readonly ILogger<VerificarDisponibilidadProductoHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public VerificarDisponibilidadProductoHandler(
        IProductoRepository productoRepository,
        IInventarioIngredientesRepository inventarioRepository,
        IProductoService productoService,
        IMapper mapper,
        ILogger<VerificarDisponibilidadProductoHandler> logger,
        ICurrentUserService currentUserService)
    {
        _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
        _inventarioRepository = inventarioRepository ?? throw new ArgumentNullException(nameof(inventarioRepository));
        _productoService = productoService ?? throw new ArgumentNullException(nameof(productoService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<Result<DisponibilidadProductoDto>> Handle(
        VerificarDisponibilidadProductoQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Verificando disponibilidad del producto {ProductoId} para cantidad {Cantidad}",
            request.ProductoId, request.CantidadSolicitada);

        try
        {
            // Validar entrada
            var validationResult = ValidateRequest(request);
            if (!validationResult.Succeeded)
            {
                return Result.Failure<DisponibilidadProductoDto>(validationResult.Error);
            }

            // Obtener producto
            var producto = await _productoRepository.ObtenerPorIdAsync(request.ProductoId, cancellationToken);
            if (producto == null)
            {
                _logger.LogWarning("Producto {ProductoId} no encontrado", request.ProductoId);
                return Result.Failure<DisponibilidadProductoDto>($"Producto con ID {request.ProductoId} no encontrado");
            }

            // Verificar si está activo
            if (!producto.EstaActivo)
            {
                _logger.LogWarning("Producto {ProductoId} está inactivo", request.ProductoId);
                return Result.Success(CreateUnavailableResult(producto, request.CantidadSolicitada, "Producto inactivo"));
            }

            // Verificar disponibilidad según el tipo de verificación
            var disponibilidadResult = await VerificarDisponibilidadSegunTipo(producto, request, cancellationToken);
            if (!disponibilidadResult.Succeeded)
            {
                return Result.Failure<DisponibilidadProductoDto>(disponibilidadResult.Error);
            }

            var resultDto = _mapper.Map<DisponibilidadProductoDto>(disponibilidadResult.Value);
            
            _logger.LogInformation("Verificación de disponibilidad completada para producto {ProductoId}. Disponible: {Disponible}",
                request.ProductoId, resultDto.EstaDisponible);

            return Result.Success(resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar disponibilidad del producto {ProductoId}", request.ProductoId);
            return Result.Failure<DisponibilidadProductoDto>($"Error interno: {ex.Message}");
        }
    }

    private Result ValidateRequest(VerificarDisponibilidadProductoQuery request)
    {
        if (request.ProductoId == Guid.Empty)
        {
            return Result.Failure("El ID del producto es requerido");
        }

        if (request.CantidadSolicitada <= 0)
        {
            return Result.Failure("La cantidad solicitada debe ser mayor a cero");
        }

        return Result.Success();
    }

    private async Task<Result<object>> VerificarDisponibilidadSegunTipo(
        Producto producto,
        VerificarDisponibilidadProductoQuery request,
        CancellationToken cancellationToken)
    {
        return request.TipoVerificacion?.ToLowerInvariant() switch
        {
            "simple" => await VerificarDisponibilidadSimple(producto, request.CantidadSolicitada, cancellationToken),
            "completa" => await VerificarDisponibilidadCompleta(producto, request, cancellationToken),
            "paracomanda" => await VerificarDisponibilidadParaComanda(producto, request, cancellationToken),
            "masiva" => await VerificarDisponibilidadMasiva(request, cancellationToken),
            _ => await VerificarDisponibilidadSimple(producto, request.CantidadSolicitada, cancellationToken)
        };
    }

    private async Task<Result<object>> VerificarDisponibilidadSimple(
        Producto producto,
        int cantidad,
        CancellationToken cancellationToken)
    {
        var disponibilidadResult = await _productoService.VerificarDisponibilidadAsync(producto, cantidad, cancellationToken);
        
        if (!disponibilidadResult.Succeeded)
        {
            return Result.Failure<object>(disponibilidadResult.Error);
        }

        return Result.Success<object>(new
        {
            ProductoId = producto.Id,
            EstaDisponible = disponibilidadResult.Value,
            CantidadVerificada = cantidad,
            TipoVerificacion = "Simple"
        });
    }

    private async Task<Result<object>> VerificarDisponibilidadCompleta(
        Producto producto,
        VerificarDisponibilidadProductoQuery request,
        CancellationToken cancellationToken)
    {
        // Verificar disponibilidad básica
        var disponibilidadBasica = await _productoService.VerificarDisponibilidadAsync(
            producto, request.CantidadSolicitada, cancellationToken);

        var resultado = new
        {
            ProductoId = producto.Id,
            EstaDisponible = disponibilidadBasica.Succeeded && disponibilidadBasica.Value,
            CantidadVerificada = request.CantidadSolicitada,
            TipoVerificacion = "Completa",
            AnalisisIngredientes = new List<object>(),
            TiempoPreparacionMinutos = 15,
            RecomendacionesAlternativas = new List<object>()
        };

        if (request.IncluirAnalisisIngredientes)
        {
            var analisisResult = await _productoService.VerificarDisponibilidadConIngredientesAsync(
                producto, request.CantidadSolicitada, cancellationToken);
            
            if (analisisResult.Succeeded)
            {
                // Simular análisis de ingredientes
                resultado.AnalisisIngredientes.Add(new
                {
                    NombreIngrediente = "Mozzarella",
                    CantidadRequerida = 2.5m,
                    CantidadDisponible = 10.0m,
                    Disponible = true
                });
            }
        }

        return Result.Success<object>(resultado);
    }

    private async Task<Result<object>> VerificarDisponibilidadParaComanda(
        Producto producto,
        VerificarDisponibilidadProductoQuery request,
        CancellationToken cancellationToken)
    {
        var disponibilidadResult = await _productoService.VerificarDisponibilidadAsync(
            producto, request.CantidadSolicitada, cancellationToken);

        return Result.Success<object>(new
        {
            ProductoId = producto.Id,
            EstaDisponible = disponibilidadResult.Succeeded && disponibilidadResult.Value,
            CantidadVerificada = request.CantidadSolicitada,
            TipoVerificacion = "ParaComanda",
            ComandaId = request.ComandaId,
            PriorizadaVelocidad = request.PriorizarVelocidadPreparacion,
            TiempoPreparacionEstimado = 8 // minutos para comanda urgente
        });
    }

    private async Task<Result<object>> VerificarDisponibilidadMasiva(
        VerificarDisponibilidadProductoQuery request,
        CancellationToken cancellationToken)
    {
        var resultados = new List<object>();

        if (request.ProductosAVerificar?.Any() == true)
        {
            foreach (var (productoId, cantidad) in request.ProductosAVerificar)
            {
                var producto = await _productoRepository.ObtenerPorIdAsync(productoId, cancellationToken);
                if (producto != null)
                {
                    var disponibilidad = await _productoService.VerificarDisponibilidadAsync(producto, cantidad, cancellationToken);
                    resultados.Add(new
                    {
                        ProductoId = productoId,
                        EstaDisponible = disponibilidad.Succeeded && disponibilidad.Value,
                        CantidadVerificada = cantidad
                    });
                }
            }
        }

        return Result.Success<object>(new
        {
            TipoVerificacion = "Masiva",
            ProductosVerificados = resultados,
            TotalProductos = resultados.Count
        });
    }

    private DisponibilidadProductoDto CreateUnavailableResult(Producto producto, int cantidad, string razon)
    {
        return new DisponibilidadProductoDto
        {
            ProductoId = producto.Id,
            NombreProducto = producto.Nombre,
            EstaDisponible = false,
            CantidadVerificada = cantidad,
            MotivoNoDisponibilidad = razon,
            AnalisisIngredientes = new List<RestaurantePro.Application.Core.Productos.DTOs.AnalisisIngredienteDto>(),
            TiempoPreparacionMinutos = 0
        };
    }
}