using RestaurantePro.Application.Proveedores.Proveedores.DTOs;
using RestaurantePro.Domain.Proveedores.Interfaces;

namespace RestaurantePro.Application.Proveedores.ContactosProveedor.Queries.ObtenerTodosContactos;

/// <summary>
/// Handler para obtener todos los contactos de proveedores
/// </summary>
public class ObtenerTodosContactosQueryHandler : IRequestHandler<ObtenerTodosContactosQuery, Result<List<ContactoProveedorDto>>>
{
    private readonly IContactoProveedorRepository _contactoRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerTodosContactosQueryHandler> _logger;

    public ObtenerTodosContactosQueryHandler(
        IContactoProveedorRepository contactoRepository,
        IMapper mapper,
        ILogger<ObtenerTodosContactosQueryHandler> logger)
    {
        _contactoRepository = contactoRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<ContactoProveedorDto>>> Handle(ObtenerTodosContactosQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obteniendo contactos de proveedores. Filtros: Termino={Termino}, SoloActivos={SoloActivos}, ProveedorId={ProveedorId}", 
            request.TerminoBusqueda, request.SoloActivos, request.ProveedorId);

        try
        {
            // Obtener todos los contactos
            var contactos = await _contactoRepository.ObtenerTodosAsync(cancellationToken);

            // Aplicar filtros
            var contactosFiltrados = contactos.AsQueryable();

            // Filtrar por término de búsqueda
            if (!string.IsNullOrWhiteSpace(request.TerminoBusqueda))
            {
                var termino = request.TerminoBusqueda.ToLower();
                contactosFiltrados = contactosFiltrados.Where(c => 
                    c.Nombre.ToLower().Contains(termino) ||
                    c.Cargo.ToLower().Contains(termino) ||
                    c.Email.Value.ToLower().Contains(termino));
            }

            // Filtrar por proveedor específico
            if (request.ProveedorId.HasValue)
            {
                contactosFiltrados = contactosFiltrados.Where(c => c.ProveedorId == request.ProveedorId.Value);
            }

            // Filtrar solo activos
            if (request.SoloActivos)
            {
                contactosFiltrados = contactosFiltrados.Where(c => !c.EstaEliminado);
            }

            // Aplicar ordenamiento
            contactosFiltrados = request.CampoOrden.ToLower() switch
            {
                "nombre" => request.DireccionOrden.ToLower() == "desc" 
                    ? contactosFiltrados.OrderByDescending(c => c.Nombre)
                    : contactosFiltrados.OrderBy(c => c.Nombre),
                "cargo" => request.DireccionOrden.ToLower() == "desc"
                    ? contactosFiltrados.OrderByDescending(c => c.Cargo)
                    : contactosFiltrados.OrderBy(c => c.Cargo),
                "email" => request.DireccionOrden.ToLower() == "desc"
                    ? contactosFiltrados.OrderByDescending(c => c.Email.Value)
                    : contactosFiltrados.OrderBy(c => c.Email.Value),
                "fechacreacion" => request.DireccionOrden.ToLower() == "desc"
                    ? contactosFiltrados.OrderByDescending(c => c.FechaCreacion)
                    : contactosFiltrados.OrderBy(c => c.FechaCreacion),
                _ => contactosFiltrados.OrderBy(c => c.Nombre)
            };

            var contactosList = contactosFiltrados.ToList();

            _logger.LogInformation("Se encontraron {Cantidad} contactos de proveedores", contactosList.Count);

            // Mapear a DTOs
            var contactosDto = _mapper.Map<List<ContactoProveedorDto>>(contactosList);

            return Result.Success(contactosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener contactos de proveedores");
            return Result.Failure<List<ContactoProveedorDto>>("Error interno del servidor al obtener los contactos");
        }
    }
} 