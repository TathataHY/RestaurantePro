using RestaurantePro.Application.Proveedores.Proveedores.DTOs;
using RestaurantePro.Domain.Proveedores.Interfaces;

namespace RestaurantePro.Application.Proveedores.ContactosProveedor.Queries.ObtenerContactosPorProveedor;

/// <summary>
/// Handler para obtener contactos por proveedor específico
/// </summary>
public class ObtenerContactosPorProveedorQueryHandler : IRequestHandler<ObtenerContactosPorProveedorQuery, Result<List<ContactoProveedorDto>>>
{
    private readonly IContactoProveedorRepository _contactoRepository;
    private readonly IProveedorRepository _proveedorRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerContactosPorProveedorQueryHandler> _logger;

    public ObtenerContactosPorProveedorQueryHandler(
        IContactoProveedorRepository contactoRepository,
        IProveedorRepository proveedorRepository,
        IMapper mapper,
        ILogger<ObtenerContactosPorProveedorQueryHandler> logger)
    {
        _contactoRepository = contactoRepository;
        _proveedorRepository = proveedorRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<ContactoProveedorDto>>> Handle(ObtenerContactosPorProveedorQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obteniendo contactos para proveedor: {ProveedorId}", request.ProveedorId);

        try
        {
            // Verificar que el proveedor existe
            var proveedor = await _proveedorRepository.ObtenerPorIdAsync(request.ProveedorId, cancellationToken: cancellationToken);
            if (proveedor == null)
            {
                _logger.LogWarning("Proveedor no encontrado: {ProveedorId}", request.ProveedorId);
                return Result.Failure<List<ContactoProveedorDto>>("El proveedor especificado no existe");
            }

            // Obtener contactos del proveedor
            var contactos = await _contactoRepository.ObtenerPorProveedorAsync(request.ProveedorId, cancellationToken);

            // Aplicar filtros
            var contactosFiltrados = contactos.AsQueryable();

            // Filtrar solo activos
            if (request.SoloActivos)
            {
                contactosFiltrados = contactosFiltrados.Where(c => !c.EstaEliminado);
            }

            var contactosList = contactosFiltrados.ToList();

            _logger.LogInformation("Se encontraron {Cantidad} contactos para el proveedor {ProveedorId}", 
                contactosList.Count, request.ProveedorId);

            // Mapear a DTOs
            var contactosDto = _mapper.Map<List<ContactoProveedorDto>>(contactosList);

            return Result.Success(contactosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener contactos para proveedor: {ProveedorId}", request.ProveedorId);
            return Result.Failure<List<ContactoProveedorDto>>("Error interno del servidor al obtener los contactos");
        }
    }
} 