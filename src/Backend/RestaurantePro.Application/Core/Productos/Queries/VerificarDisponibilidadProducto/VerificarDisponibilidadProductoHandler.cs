namespace RestaurantePro.Application.Core.Productos.Queries.VerificarDisponibilidadProducto;

/// <summary>
/// Handler para verificar disponibilidad de productos
/// </summary>
public class VerificarDisponibilidadProductoHandler : IRequestHandler<VerificarDisponibilidadProductoQuery, RestaurantePro.Domain.Core.SharedKernel.Results.Result<DisponibilidadProductoDto>>
{
    private readonly IInventarioIngredientesRepository _inventarioRepository;
    private readonly IProductoService _productoService;
    private readonly ILogger<VerificarDisponibilidadProductoHandler> _logger;

    public VerificarDisponibilidadProductoHandler(
        IInventarioIngredientesRepository inventarioRepository,
        IProductoService productoService,
        ILogger<VerificarDisponibilidadProductoHandler> logger)
    {
        _inventarioRepository = inventarioRepository ?? throw new ArgumentNullException(nameof(inventarioRepository));
        _productoService = productoService ?? throw new ArgumentNullException(nameof(productoService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Maneja la query de verificación de disponibilidad
    /// </summary>
    public async Task<RestaurantePro.Domain.Core.SharedKernel.Results.Result<DisponibilidadProductoDto>> Handle(VerificarDisponibilidadProductoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Verificando disponibilidad del producto {ProductoId}", request.ProductoId);

            var producto = await _productoService.ObtenerPorIdAsync(request.ProductoId);
            if (producto == null)
            {
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<DisponibilidadProductoDto>($"Producto con ID {request.ProductoId} no encontrado");
            }

            if (!producto.EstaActivo)
            {
                var resultadoInactivo = new DisponibilidadProductoDto
                {
                    ProductoId = request.ProductoId,
                    EstaDisponible = false,
                    CantidadVerificada = request.CantidadSolicitada,
                    CantidadDisponible = 0,
                    MotivoNoDisponible = "Producto inactivo",
                    FechaVerificacion = DateTime.UtcNow,
                    AnalisisIngredientes = new List<RestaurantePro.Application.Core.Productos.DTOs.AnalisisIngredienteDto>()
                };
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Success<DisponibilidadProductoDto>(resultadoInactivo);
            }

            // Verificar disponibilidad de ingredientes
            var receta = await _productoService.ObtenerRecetaPorProductoIdAsync(request.ProductoId);
            if (receta == null)
            {
                var resultadoSinReceta = new DisponibilidadProductoDto
                {
                    ProductoId = request.ProductoId,
                    EstaDisponible = true,
                    CantidadVerificada = request.CantidadSolicitada,
                    CantidadDisponible = int.MaxValue,
                    MotivoNoDisponible = null,
                    FechaVerificacion = DateTime.UtcNow,
                    AnalisisIngredientes = new List<RestaurantePro.Application.Core.Productos.DTOs.AnalisisIngredienteDto>()
                };
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Success<DisponibilidadProductoDto>(resultadoSinReceta);
            }

            var analisisIngredientes = new List<RestaurantePro.Application.Core.Productos.DTOs.AnalisisIngredienteDto>();
            bool hayIngredientesFaltantes = false;

            foreach (var ingredienteReceta in receta.Ingredientes)
            {
                var stockDisponible = await _inventarioRepository.ObtenerStockDisponibleAsync(ingredienteReceta.IngredienteId);
                var cantidadNecesaria = ingredienteReceta.Cantidad * request.CantidadSolicitada;
                
                var analisis = new RestaurantePro.Application.Core.Productos.DTOs.AnalisisIngredienteDto
                {
                    IngredienteId = ingredienteReceta.IngredienteId,
                    NombreIngrediente = "Ingrediente", // TODO: obtener nombre real del ingrediente
                    EstaDisponible = stockDisponible >= cantidadNecesaria,
                    CantidadNecesaria = cantidadNecesaria,
                    CantidadDisponible = stockDisponible,
                    UnidadMedida = "unidad", // TODO: obtener unidad real
                    PorcentajeDisponibilidad = stockDisponible > 0 ? Math.Min(100, (stockDisponible / cantidadNecesaria) * 100) : 0
                };

                analisisIngredientes.Add(analisis);

                if (!analisis.EstaDisponible)
                {
                    hayIngredientesFaltantes = true;
                }
            }

            var resultado = new DisponibilidadProductoDto
            {
                ProductoId = request.ProductoId,
                EstaDisponible = !hayIngredientesFaltantes,
                CantidadVerificada = request.CantidadSolicitada,
                CantidadDisponible = hayIngredientesFaltantes ? 0 : request.CantidadSolicitada,
                MotivoNoDisponible = hayIngredientesFaltantes ? "Ingredientes insuficientes" : null,
                FechaVerificacion = DateTime.UtcNow,
                TiempoPreparacionMinutos = receta.TiempoPreparacionMinutos,
                PrioridadPreparacion = 5, // Prioridad media por defecto
                AnalisisIngredientes = analisisIngredientes
            };

            return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Success<DisponibilidadProductoDto>(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar disponibilidad del producto {ProductoId}", request.ProductoId);
            return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<DisponibilidadProductoDto>($"Error interno: {ex.Message}");
        }
    }
}