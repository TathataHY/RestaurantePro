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
                return Result<ProveedorDto>.Failure($"El proveedor con ID {request.Id} no fue encontrado");
            }

            // Verificar si el email ya existe en otro proveedor
            var proveedorConEmail = await _repository.ObtenerPorEmailAsync(request.Email);
            if (proveedorConEmail != null && proveedorConEmail.Id != request.Id)
            {
                _logger.LogWarning("Email ya existe en otro proveedor: {Email}", request.Email);
                return Result<ProveedorDto>.Failure($"Ya existe otro proveedor con el email {request.Email}");
            }

            // Actualizar usando el builder del dominio
            var resultadoActualizacion = proveedorExistente
                .ActualizarNombre(request.Nombre)
                .ActualizarDescripcion(request.Descripcion)
                .ActualizarEmail(request.Email)
                .ActualizarTelefono(request.Telefono)
                .ActualizarDireccion(request.Direccion)
                .ActualizarPaginaWeb(request.PaginaWeb)
                .ActualizarTipo(request.Tipo)
                .ActualizarCondicionesPago(request.CondicionesPago)
                .ActualizarDiasEntrega(request.DiasEntrega)
                .ActualizarCalificacion(request.Calificacion)
                .ActualizarNotas(request.Notas);

            if (!resultadoActualizacion.Succeeded)
            {
                _logger.LogWarning("Error al actualizar proveedor: {Error}", resultadoActualizacion.ErrorMessage);
                return Result<ProveedorDto>.Failure(resultadoActualizacion.ErrorMessage);
            }

            // Establecer auditoría
            proveedorExistente.EstablecerModificadoPor(_currentUserService.UserId ?? "Sistema");

            // Guardar cambios
            await _repository.ActualizarAsync(proveedorExistente);

            _logger.LogInformation("Proveedor actualizado exitosamente: {ProveedorId}", request.Id);

            var proveedorDto = _mapper.Map<ProveedorDto>(proveedorExistente);
            return Result<ProveedorDto>.Success(proveedorDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar proveedor: {ProveedorId}", request.Id);
            return Result<ProveedorDto>.Failure("Error interno del servidor al actualizar el proveedor");
        }
    }
} 