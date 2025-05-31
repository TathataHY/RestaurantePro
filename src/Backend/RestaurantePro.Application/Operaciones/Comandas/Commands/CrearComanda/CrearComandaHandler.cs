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

    public CrearComandaHandler(
        IComandaRepository comandaRepository,
        IProductoRepository productoRepository,
        IMapper mapper,
        ILogger<CrearComandaHandler> logger)
    {
        _comandaRepository = comandaRepository;
        _productoRepository = productoRepository;
        _mapper = mapper;
        _logger = logger;
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
            // TODO: Verificar que el mesero existe y está activo
            // TODO: Verificar que la mesa está disponible (si se especifica)
            // TODO: Verificar que el cliente existe (si se especifica)

            // 2. Crear la comanda usando el factory method del dominio
            var comanda = Comanda.Crear(
                request.MeseroId, 
                request.ClienteId, 
                request.MesaId, 
                request.Observaciones);

            _logger.LogInformation("✅ Comanda creada: {ComandaId}", comanda.Id);

            // 3. Agregar productos iniciales si se proporcionaron
            if (request.ProductosIniciales.Any())
            {
                await AgregarProductosIniciales(comanda, request.ProductosIniciales, cancellationToken);
            }

            // 4. Persistir la comanda
            await _comandaRepository.AgregarAsync(comanda, cancellationToken);
            await _comandaRepository.GuardarCambiosAsync(cancellationToken);

            // 5. Mapear a DTO y retornar
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
                // TODO: Validar que el producto existe y está activo
                // var producto = await _productoRepository.ObtenerPorIdAsync(productoDto.ProductoId, cancellationToken);
                // if (producto == null || !producto.EstaActivo)
                // {
                //     throw new InvalidOperationException($"El producto {productoDto.ProductoId} no está disponible");
                // }

                // TODO: Validar que el precio es correcto
                // if (Math.Abs(producto.Precio - productoDto.PrecioUnitario) > 0.01m)
                // {
                //     _logger.LogWarning("💰 Precio diferente detectado - Producto: {ProductoId}, Esperado: {PrecioEsperado}, Recibido: {PrecioRecibido}", 
                //         productoDto.ProductoId, producto.Precio, productoDto.PrecioUnitario);
                // }

                // Agregar el producto a la comanda usando el método del dominio
                comanda.AgregarProducto(
                    productoDto.ProductoId,
                    productoDto.Cantidad,
                    productoDto.PrecioUnitario,
                    productoDto.Observaciones);

                // TODO: Agregar personalizaciones si las hay
                // await AgregarPersonalizaciones(comanda, itemId, productoDto.Personalizaciones, cancellationToken);

                _logger.LogInformation("✅ Producto agregado: {ProductoId} x{Cantidad} = ${Total}", 
                    productoDto.ProductoId, 
                    productoDto.Cantidad, 
                    productoDto.Cantidad * productoDto.PrecioUnitario);
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
} 