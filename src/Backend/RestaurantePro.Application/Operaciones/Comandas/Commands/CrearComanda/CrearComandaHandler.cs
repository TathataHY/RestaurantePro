using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Invalidation;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.CrearComanda;

/// <summary>
/// Handler para crear una nueva comanda
/// Implementa la lógica de negocio para iniciar el flujo operativo de pedidos
/// </summary>
public class CrearComandaHandler : IRequestHandler<CrearComandaCommand, Result<ComandaDto>>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CrearComandaHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IMesaRepository _mesaRepository;

    public CrearComandaHandler(
        IComandaRepository comandaRepository,
        IProductoRepository productoRepository,
        IMapper mapper,
        ILogger<CrearComandaHandler> logger,
        ICacheService cacheService,
        IMesaRepository mesaRepository)
    {
        _comandaRepository = comandaRepository;
        _productoRepository = productoRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
        _mesaRepository = mesaRepository;
    }

    public async Task<Result<ComandaDto>> Handle(
        CrearComandaCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🍽️ Iniciando creación de comanda - Mesero: {MeseroId}, Mesa: {MesaId}", 
            request.MeseroId, 
            request.MesaId);

        try
        {
            // 1. Validar datos básicos
            await ValidarDatosBasicos(request, cancellationToken);

            // 2. Crear la comanda usando el factory method del dominio
            var comanda = Comanda.Crear(
                request.MeseroId,
                request.ClienteId,
                request.MesaId,
                request.Observaciones,
                null,
                RestaurantePro.Domain.Operaciones.Comandas.Enums.TipoComanda.Mesa);

            _logger.LogInformation("✅ Comanda creada: {ComandaId}", comanda.Id);

            // 3. Agregar productos iniciales si se proporcionaron
            if (request.ProductosIniciales.Any())
            {
                await AgregarProductosIniciales(comanda, request.ProductosIniciales, cancellationToken);
            }

            // 4. Persistir la comanda
            await _comandaRepository.AgregarAsync(comanda, cancellationToken);
            await _comandaRepository.GuardarCambiosAsync(cancellationToken);

            // 4.1 Marcar mesa como Ocupada si aplica
            if (request.MesaId.HasValue && request.MesaId.Value != Guid.Empty)
            {
                try
                {
                    var mesa = await _mesaRepository.ObtenerPorIdAsync(request.MesaId.Value, cancellationToken);
                    if (mesa != null)
                    {
                        mesa.MarcarComoOcupada();
                        await _mesaRepository.ActualizarAsync(mesa, cancellationToken);
                        await _mesaRepository.GuardarCambiosAsync(cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo marcar la mesa {MesaId} como Ocupada al crear comanda {ComandaId}", request.MesaId, comanda.Id);
                }
            }

            // 5. Invalidar caches para que aparezca en listados inmediatamente y refrescar estado de mesas
            _cacheService.InvalidatePattern("ObtenerComandasPaginadasQuery_");
            _cacheService.InvalidatePattern("ObtenerComandasPorMesaQuery_");
            _cacheService.InvalidateForEntity("ObtenerComandaPorIdQuery_", comanda.Id);
            _cacheService.InvalidatePattern("ObtenerMesasDisponiblesQuery_");
            _cacheService.InvalidatePattern("ObtenerEstadoMesasQuery_");

            // 6. Mapear a DTO y retornar
            var comandaDto = _mapper.Map<ComandaDto>(comanda);

            _logger.LogInformation("🎉 Comanda creada exitosamente: {ComandaId} - Total productos: {CantidadProductos}", 
                comanda.Id, 
                comanda.Items.Count);

            return Result.Success(comandaDto);
        }
        catch (BusinessRuleViolationException ex)
        {
            _logger.LogWarning("📋 Violación de regla de negocio al crear comanda: {Message}", ex.Message);
            return Result.Failure<ComandaDto>(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("⚠️ Argumento inválido al crear comanda: {Message}", ex.Message);
            return Result.Failure<ComandaDto>(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error inesperado al crear comanda");
            return Result.Failure<ComandaDto>("Error interno del servidor al crear la comanda");
        }
    }

    /// <summary>
    /// Agrega los productos iniciales a la comanda
    /// Valida precios y disponibilidad de productos
    /// </summary>
    private async Task AgregarProductosIniciales(
        Comanda comanda, 
        List<AgregarProductoDto> productosIniciales, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("➕ Agregando {Cantidad} productos iniciales a la comanda {ComandaId}", 
            productosIniciales.Count, 
            comanda.Id);

        foreach (var productoDto in productosIniciales)
        {
            try
            {
                // Validar que el producto existe y está activo
                var producto = await _productoRepository.ObtenerPorIdAsync(productoDto.ProductoId, cancellationToken);
                if (producto == null)
                {
                    _logger.LogWarning("⚠️ Producto no encontrado: {ProductoId}", productoDto.ProductoId);
                    throw new InvalidOperationException($"El producto {productoDto.ProductoId} no existe");
                }
                
                if (!producto.EstaActivo)
                {
                    _logger.LogWarning("⚠️ Producto inactivo: {ProductoId}", productoDto.ProductoId);
                    throw new InvalidOperationException($"El producto {productoDto.ProductoId} no está disponible");
                }

                // Obtener el precio del producto desde el repositorio
                var precioUnitario = producto.Precio.Valor;

                // Agregar el producto a la comanda usando el método del dominio
                comanda.AgregarProducto(
                    productoDto.ProductoId,
                    productoDto.Cantidad,
                    precioUnitario,
                    productoDto.Observaciones);

                // TODO: Agregar personalizaciones si las hay
                // await AgregarPersonalizaciones(comanda, itemId, productoDto.Personalizaciones, cancellationToken);

                _logger.LogInformation("✅ Producto agregado: {ProductoId} x{Cantidad} = ${Total:F2}", 
                    productoDto.ProductoId, 
                    productoDto.Cantidad, 
                    productoDto.Cantidad * precioUnitario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al agregar producto {ProductoId} a la comanda", productoDto.ProductoId);
                throw; // Re-lanzar para que la transacción se revierta
            }
        }

        _logger.LogInformation("🎯 Productos iniciales agregados exitosamente - Total items: {TotalItems}", 
            comanda.Items.Count);
    }

    /// <summary>
    /// Valida los datos básicos necesarios para crear una comanda
    /// </summary>
    private async Task ValidarDatosBasicos(CrearComandaCommand request, CancellationToken cancellationToken)
    {
        // Nota: Por ahora implementamos validaciones básicas sin repositorios adicionales
        // En una implementación completa se verificarían contra repositorios específicos
        
        // Validar ID del mesero
        if (request.MeseroId == Guid.Empty)
        {
            throw new ArgumentException("El ID del mesero es requerido para crear una comanda");
        }
        
        // TODO: Verificar que el mesero existe y está activo cuando tengamos IUsuarioRepository disponible
        // var mesero = await _usuarioRepository.ObtenerPorIdAsync(request.MeseroId, cancellationToken);
        // if (mesero == null || !mesero.EstaActivo || !mesero.TieneRol("Mesero"))
        // {
        //     throw new InvalidOperationException($"El mesero {request.MeseroId} no está disponible");
        // }
        
        // Validar mesa si se especifica
        if (request.MesaId.HasValue)
        {
            if (request.MesaId.Value == Guid.Empty)
            {
                throw new ArgumentException("El ID de la mesa no puede ser un GUID vacío");
            }
            
            // TODO: Verificar que la mesa existe y está disponible cuando tengamos IMesaRepository
            // var mesa = await _mesaRepository.ObtenerPorIdAsync(request.MesaId.Value, cancellationToken);
            // if (mesa == null || !mesa.EstaDisponible)
            // {
            //     throw new InvalidOperationException($"La mesa {request.MesaId.Value} no está disponible");
            // }
        }
        
        // Validar cliente si se especifica
        if (request.ClienteId.HasValue)
        {
            if (request.ClienteId.Value == Guid.Empty)
            {
                throw new ArgumentException("El ID del cliente no puede ser un GUID vacío");
            }
            
            // TODO: Verificar que el cliente existe y está activo cuando tengamos IClienteRepository
            // var cliente = await _clienteRepository.ObtenerPorIdAsync(request.ClienteId.Value, cancellationToken);
            // if (cliente == null || !cliente.EstaActivo)
            // {
            //     throw new InvalidOperationException($"El cliente {request.ClienteId.Value} no está activo");
            // }
        }
        
        // Validar productos iniciales si se proporcionan
        if (request.ProductosIniciales.Any())
        {
            var productosInvalidos = request.ProductosIniciales
                .Where(p => p.ProductoId == Guid.Empty || p.Cantidad <= 0)
                .ToList();
                
            if (productosInvalidos.Any())
            {
                var idsInvalidos = productosInvalidos.Select(p => p.ProductoId).ToList();
                throw new ArgumentException($"Productos con datos inválidos: {string.Join(", ", idsInvalidos)}");
            }
        }
        
        _logger.LogDebug("✅ Validaciones básicas completadas para comanda - Mesero: {MeseroId}, Mesa: {MesaId}, Cliente: {ClienteId}", 
            request.MeseroId, request.MesaId, request.ClienteId);
        
        await Task.CompletedTask; // Para mantener la signatura async
    }
} 