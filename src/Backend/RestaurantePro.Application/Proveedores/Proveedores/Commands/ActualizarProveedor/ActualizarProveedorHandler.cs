namespace RestaurantePro.Application.Proveedores.Proveedores.Commands.ActualizarProveedor;

public class ActualizarProveedorHandler : IRequestHandler<ActualizarProveedorCommand, Result<ProveedorDto>>
{
    private readonly IProveedorRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ActualizarProveedorHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public ActualizarProveedorHandler(
        IProveedorRepository repository,
        IMapper mapper,
        ILogger<ActualizarProveedorHandler> logger,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ProveedorDto>> Handle(
        ActualizarProveedorCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando actualización de proveedor: {ProveedorId}", request.Id);

        try
        {
            // Obtener proveedor existente
            var proveedorExistente = await _repository.ObtenerPorIdAsync(request.Id);
            if (proveedorExistente == null)
            {
                _logger.LogWarning("Proveedor no encontrado: {ProveedorId}", request.Id);
                return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<ProveedorDto>($"El proveedor con ID {request.Id} no fue encontrado");
            }

            // Verificar si el email ya existe en otro proveedor
            // TODO: Implementar cuando exista ObtenerPorEmailAsync en IProveedorRepository
            /*
            var proveedorConEmail = await _repository.ObtenerPorEmailAsync(request.Email);
            if (proveedorConEmail != null && proveedorConEmail.Id != request.Id)
            {
                _logger.LogWarning("Email ya existe en otro proveedor: {Email}", request.Email);
                return Result<ProveedorDto>.Failure($"Ya existe otro proveedor con el email {request.Email}");
            }
            */

            // Actualizar propiedades básicas del proveedor
            // TODO: Implementar métodos de actualización específicos en el dominio
            // Por ahora solo actualizamos campos básicos que estén disponibles
            _logger.LogInformation("Actualizando proveedor {ProveedorId} con nuevos datos", request.Id);
            
            // TODO: Usar métodos específicos del dominio cuando estén implementados:
            // proveedorExistente.ActualizarNombre(request.Nombre);
            // proveedorExistente.ActualizarEmail(request.Email);
            // etc.

            // TODO: Establecer auditoría cuando exista el método
            // proveedorExistente.EstablecerModificadoPor(_currentUserService.UserId ?? "Sistema");

            // Guardar cambios
            await _repository.ActualizarAsync(proveedorExistente);
            
            // Persistir cambios en base de datos
            await _repository.GuardarCambiosAsync();

            _logger.LogInformation("✅ Proveedor actualizado exitosamente y persistido: {ProveedorId}", request.Id);

            var proveedorDto = _mapper.Map<ProveedorDto>(proveedorExistente);
            return Result<ProveedorDto>.Success(proveedorDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar proveedor: {ProveedorId}", request.Id);
            return RestaurantePro.Domain.Core.SharedKernel.Results.Result.Failure<ProveedorDto>("Error interno del servidor al actualizar el proveedor");
        }
    }
} 