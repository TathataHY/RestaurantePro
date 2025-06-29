using RestaurantePro.Application.Proveedores.Proveedores.DTOs;
using RestaurantePro.Domain.Proveedores.Interfaces;

namespace RestaurantePro.Application.Proveedores.ContactosProveedor.Queries.ObtenerContactoPorId;

/// <summary>
/// Handler para obtener un contacto específico por ID
/// </summary>
public class ObtenerContactoPorIdQueryHandler : IRequestHandler<ObtenerContactoPorIdQuery, Result<ContactoProveedorDto>>
{
    private readonly IContactoProveedorRepository _contactoRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerContactoPorIdQueryHandler> _logger;

    public ObtenerContactoPorIdQueryHandler(
        IContactoProveedorRepository contactoRepository,
        IMapper mapper,
        ILogger<ObtenerContactoPorIdQueryHandler> logger)
    {
        _contactoRepository = contactoRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ContactoProveedorDto>> Handle(ObtenerContactoPorIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obteniendo contacto por ID: {ContactoId}", request.Id);

        try
        {
            // Obtener el contacto por ID
            var contacto = await _contactoRepository.ObtenerPorIdAsync(request.Id, cancellationToken);

            if (contacto == null)
            {
                _logger.LogWarning("Contacto no encontrado: {ContactoId}", request.Id);
                return Result.Failure<ContactoProveedorDto>("El contacto especificado no existe");
            }

            // Verificar si está eliminado
            if (contacto.EstaEliminado)
            {
                _logger.LogWarning("Contacto eliminado: {ContactoId}", request.Id);
                return Result.Failure<ContactoProveedorDto>("El contacto especificado ha sido eliminado");
            }

            _logger.LogInformation("Contacto encontrado: {ContactoId} - {Nombre}", request.Id, contacto.Nombre);

            // Mapear a DTO
            var contactoDto = _mapper.Map<ContactoProveedorDto>(contacto);

            return Result.Success(contactoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener contacto por ID: {ContactoId}", request.Id);
            return Result.Failure<ContactoProveedorDto>("Error interno del servidor al obtener el contacto");
        }
    }
} 