namespace RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerComandaPorId;

/// <summary>
/// Handler para obtener una comanda por su ID
/// Incluye mapeo completo a DTO con cálculos de tiempo y estado
/// </summary>
public class ObtenerComandaPorIdHandler : IRequestHandler<ObtenerComandaPorIdQuery, Result<ComandaDto>>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerComandaPorIdHandler> _logger;

    public ObtenerComandaPorIdHandler(
        IComandaRepository comandaRepository,
        IMapper mapper,
        ILogger<ObtenerComandaPorIdHandler> logger)
    {
        _comandaRepository = comandaRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ComandaDto>> Handle(
        ObtenerComandaPorIdQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔍 Buscando comanda por ID: {ComandaId} - Incluir items: {IncluirItems}", 
            request.ComandaId, 
            request.IncluirItems);

        try
        {
            // 1. Buscar la comanda en el repositorio
            var comanda = await _comandaRepository.ObtenerPorIdAsync(
                request.ComandaId, 
                request.IncluirItems, 
                cancellationToken);

            if (comanda == null)
            {
                _logger.LogWarning("⚠️ Comanda no encontrada: {ComandaId}", request.ComandaId);
                return Result.Failure<ComandaDto>($"No se encontró una comanda con el ID {request.ComandaId}");
            }

            // 2. Mapear a DTO usando AutoMapper
            var comandaDto = _mapper.Map<ComandaDto>(comanda);

            // 3. TODO: Enriquecer con datos adicionales si es necesario
            // - Nombre del mesero desde el contexto Core/Usuarios
            // - Número de mesa desde el contexto Operaciones/Mesas  
            // - Nombre del cliente desde el contexto Comercial/Clientes
            // - Nombres de productos desde el contexto Core/Productos
            await EnriquecerConDatosAdicionales(comandaDto, comanda, cancellationToken);

            _logger.LogInformation("✅ Comanda encontrada exitosamente: {ComandaId} - Estado: {Estado}, Items: {CantidadItems}", 
                comanda.Id, 
                comanda.Estado, 
                comanda.Items.Count);

            return Result.Success(comandaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al buscar comanda por ID: {ComandaId}", request.ComandaId);
            return Result.Failure<ComandaDto>("Error interno del servidor al buscar la comanda");
        }
    }

    /// <summary>
    /// Enriquece el DTO con datos adicionales de otros contextos
    /// </summary>
    private async Task EnriquecerConDatosAdicionales(
        ComandaDto comandaDto, 
        Comanda comanda, 
        CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Implementar cuando tengamos los servicios de otros contextos

            // 1. Obtener nombre del mesero
            // if (comanda.MeseroId != Guid.Empty)
            // {
            //     var usuario = await _usuarioRepository.ObtenerPorIdAsync(comanda.MeseroId, cancellationToken);
            //     comandaDto.NombreMesero = usuario?.NombreCompleto;
            // }

            // 2. Obtener número de mesa
            // if (comanda.MesaId != Guid.Empty)
            // {
            //     var mesa = await _mesaRepository.ObtenerPorIdAsync(comanda.MesaId, cancellationToken);
            //     comandaDto.NumeroMesa = mesa?.Numero?.ToString();
            // }

            // 3. Obtener nombre del cliente
            // if (comanda.ClienteId.HasValue)
            // {
            //     var cliente = await _clienteRepository.ObtenerPorIdAsync(comanda.ClienteId.Value, cancellationToken);
            //     comandaDto.NombreCliente = cliente?.Nombre?.NombreCompleto;
            // }

            // 4. Obtener nombres de productos para los items
            // foreach (var item in comandaDto.Items)
            // {
            //     var producto = await _productoRepository.ObtenerPorIdAsync(item.ProductoId, cancellationToken);
            //     item.NombreProducto = producto?.Nombre?.Valor ?? "Producto no encontrado";
            // }

            _logger.LogDebug("📋 Datos adicionales procesados para comanda: {ComandaId}", comanda.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Error al enriquecer datos adicionales para comanda: {ComandaId}", comanda.Id);
            // No fallar la operación por errores en el enriquecimiento
        }
    }
} 