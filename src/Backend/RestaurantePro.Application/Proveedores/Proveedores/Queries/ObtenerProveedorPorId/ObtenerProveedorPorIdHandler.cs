using RestaurantePro.Domain.Proveedores.Interfaces;
using RestaurantePro.Application.Proveedores.Proveedores.DTOs;

namespace RestaurantePro.Application.Proveedores.Proveedores.Queries.ObtenerProveedorPorId;

/// <summary>
/// Handler para ObtenerProveedorPorIdQuery
/// Procesa la consulta de un proveedor específico
/// </summary>
public class ObtenerProveedorPorIdHandler : IRequestHandler<ObtenerProveedorPorIdQuery, Result<ProveedorDto>>
{
    private readonly IProveedorRepository _proveedorRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerProveedorPorIdHandler> _logger;

    public ObtenerProveedorPorIdHandler(
        IProveedorRepository proveedorRepository,
        IMapper mapper,
        ILogger<ObtenerProveedorPorIdHandler> logger)
    {
        _proveedorRepository = proveedorRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Procesa la consulta para obtener un proveedor por ID
    /// </summary>
    public async Task<Result<ProveedorDto>> Handle(ObtenerProveedorPorIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔍 Buscando proveedor por ID: {ProveedorId}", request.ProveedorId);
        
        try
        {
            // 1. Validar que el ID no sea vacío
            if (request.ProveedorId == Guid.Empty)
            {
                _logger.LogWarning("❌ ID de proveedor inválido: {ProveedorId}", request.ProveedorId);
                return Result.Failure<ProveedorDto>("El ID del proveedor no puede estar vacío");
            }

            // 2. Buscar el proveedor en el repositorio
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(
                request.ProveedorId, 
                request.IncluirContactos, 
                request.IncluirCategorias, 
                cancellationToken);

            // 3. Verificar si se encontró el proveedor
            if (proveedor == null)
            {
                _logger.LogWarning("🚫 Proveedor no encontrado: {ProveedorId}", request.ProveedorId);
                return Result.Failure<ProveedorDto>($"No se encontró el proveedor con ID {request.ProveedorId}");
            }

            // 4. Verificar si el proveedor está activo (opcional según reglas de negocio)
            if (!proveedor.Activo)
            {
                _logger.LogInformation("⚠️ Consultando proveedor inactivo: {ProveedorId} - {Nombre}", 
                    proveedor.Id, proveedor.Nombre);
            }

            // 5. Mapear a DTO
            var proveedorDto = _mapper.Map<ProveedorDto>(proveedor);

            // 6. Log de auditoría
            _logger.LogInformation("✅ Proveedor encontrado: {ProveedorId} - {Nombre} (Usuario: {UsuarioId})", 
                proveedor.Id, proveedor.Nombre, request.UsuarioId);

            // 7. Log de información adicional si se incluyeron relaciones
            if (request.IncluirContactos && proveedorDto.TieneContactos)
            {
                _logger.LogDebug("📞 Incluidos {CantidadContactos} contactos para proveedor {ProveedorId}", 
                    proveedorDto.TotalContactos, proveedor.Id);
            }

            return Result.Success(proveedorDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al obtener proveedor por ID: {ProveedorId}", request.ProveedorId);
            return Result.Failure<ProveedorDto>($"Error interno al buscar el proveedor: {ex.Message}");
        }
    }
} 