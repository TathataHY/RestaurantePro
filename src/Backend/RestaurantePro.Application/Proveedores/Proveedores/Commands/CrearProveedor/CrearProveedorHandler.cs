namespace RestaurantePro.Application.Proveedores.Proveedores.Commands.CrearProveedor;

/// <summary>
/// Handler para CrearProveedorCommand
/// Orquesta la creación de un nuevo proveedor
/// </summary>
public class CrearProveedorHandler : IRequestHandler<CrearProveedorCommand, Result<ProveedorDto>>
{
    private readonly IProveedorRepository _proveedorRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CrearProveedorHandler> _logger;

    public CrearProveedorHandler(
        IProveedorRepository proveedorRepository,
        IMapper mapper,
        ILogger<CrearProveedorHandler> logger)
    {
        _proveedorRepository = proveedorRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Procesa el comando de crear proveedor
    /// </summary>
    public async Task<Result<ProveedorDto>> Handle(CrearProveedorCommand request, CancellationToken cancellationToken)
    {
        // Validación explícita de request null o datos incompletos
        if (request == null)
        {
            _logger.LogWarning("❌ Request de creación de proveedor es null");
            return Result.Failure<ProveedorDto>("Faltan campos obligatorios o hay datos inválidos. Verifique nombre, email y teléfono.");
        }

        // Validación de campos obligatorios antes de cualquier procesamiento
        if (string.IsNullOrWhiteSpace(request.Nombre) || 
            string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.Telefono))
        {
            _logger.LogWarning("❌ Datos obligatorios faltantes: Nombre={Nombre}, Email={Email}, Telefono={Telefono}", 
                request.Nombre, request.Email, request.Telefono);
            return Result.Failure<ProveedorDto>("Faltan campos obligatorios o hay datos inválidos. Verifique nombre, email y teléfono.");
        }

        _logger.LogInformation("🚀 Iniciando creación de proveedor: {Nombre}", request.Nombre);

        try
        {
            // 1. Validar que el proveedor no exista (RUT único)
            var proveedorExistentePorRut = await ValidarRutUnico(request.RUT, cancellationToken);
            if (!proveedorExistentePorRut.Succeeded)
            {
                _logger.LogWarning("❌ RUT ya existe: {RUT}", request.RUT);
                return Result.Failure<ProveedorDto>(proveedorExistentePorRut.Error ?? "Error al validar RUT");
            }

            // 2. Validar que el email no esté en uso
            var emailValido = await ValidarEmailUnico(request.Email, cancellationToken);
            if (!emailValido.Succeeded)
            {
                _logger.LogWarning("❌ Email ya está en uso: {Email}", request.Email);
                return Result.Failure<ProveedorDto>(emailValido.Error ?? "Error al validar email");
            }

            // 3. Crear el proveedor usando factory del dominio
            var resultadoProveedor = await CrearProveedorConDominio(request);
            if (!resultadoProveedor.Succeeded)
            {
                _logger.LogError("❌ Error al crear proveedor en dominio: {Error}", resultadoProveedor.Error);
                return Result.Failure<ProveedorDto>(resultadoProveedor.Error ?? "Error al crear proveedor en dominio");
            }

            var proveedor = resultadoProveedor.Value;

            // 4. Guardar en repositorio
            await _proveedorRepository.AgregarAsync(proveedor, cancellationToken);
            _logger.LogInformation("💾 Proveedor agregado al repositorio: {Id}", proveedor.Id);

            // 5. Persistir cambios en base de datos
            await _proveedorRepository.GuardarCambiosAsync(cancellationToken);
            _logger.LogInformation("✅ Proveedor persistido en BD: {Id}", proveedor.Id);

            // 6. Mapear a DTO
            var proveedorDto = _mapper.Map<ProveedorDto>(proveedor);
            
            _logger.LogInformation("✅ Proveedor creado exitosamente: {Id} - {Nombre}", proveedor.Id, proveedor.Nombre);
            
            return Result.Success(proveedorDto);
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogError(ex, "Error de validación al crear proveedor: {Errores}", string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)));
            return Result.Failure<ProveedorDto>($"Validación fallida: {string.Join("; ", ex.Errors.Select(e => e.ErrorMessage))}");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("⚠️ Datos obligatorios faltantes o inválidos: {Error}", ex.Message);
            return Result.Failure<ProveedorDto>("Faltan campos obligatorios o hay datos inválidos. Verifique nombre, email y teléfono.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error inesperado al crear proveedor");
            var errorMsg = ex.Message + (ex.InnerException != null ? $" | Inner: {ex.InnerException.Message}" : "");
            return Result.Failure<ProveedorDto>(errorMsg);
        }
    }

    /// <summary>
    /// Valida que el RUT sea único en el sistema
    /// </summary>
    private async Task<Result> ValidarRutUnico(string rut, CancellationToken cancellationToken)
    {
        try
        {
            var proveedorConRut = await _proveedorRepository.ObtenerPorRUTAsync(rut, cancellationToken);
            
            if (proveedorConRut != null)
            {
                return Result.Failure($"Ya existe un proveedor con el RUT {rut}: {proveedorConRut.Nombre}");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar RUT único: {RUT}", rut);
            return Result.Failure("Error al validar el RUT en el sistema");
        }
    }

    /// <summary>
    /// Valida que el email sea único en el sistema
    /// </summary>
    private async Task<Result> ValidarEmailUnico(string email, CancellationToken cancellationToken)
    {
        try
        {
            var proveedores = await _proveedorRepository.BuscarAsync(email, cancellationToken);
            var proveedorConEmail = proveedores.FirstOrDefault(p => 
                p.Email.Value.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (proveedorConEmail != null)
            {
                return Result.Failure($"Ya existe un proveedor con el email {email}: {proveedorConEmail.Nombre}");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar email único: {Email}", email);
            return Result.Failure("Error al validar el email en el sistema");
        }
    }

    /// <summary>
    /// Crea el proveedor usando la lógica del dominio
    /// </summary>
    private Task<Result<Proveedor>> CrearProveedorConDominio(CrearProveedorCommand request)
    {
        try
        {
            _logger.LogDebug("🔧 Creando proveedor con factory del dominio");

            // Usar el factory method del dominio para crear el proveedor
            var proveedor = Proveedor.Crear(
                nombre: request.Nombre,
                nombreContacto: request.NombreContacto,
                email: request.Email,
                telefono: request.Telefono,
                direccion: request.Direccion,
                ciudad: request.Ciudad,
                codigoPostal: request.CodigoPostal,
                pais: request.Pais,
                rfc: request.RFC,
                informacionBancaria: request.InformacionBancaria,
                diasCredito: request.DiasCredito
            );

            // Verificar que la creación fue exitosa
            if (proveedor == null)
            {
                return Task.FromResult(Result.Failure<Proveedor>("No se pudo crear el proveedor. Verifique los datos proporcionados."));
            }

            // Registrar evento si es un proveedor con crédito
            if (request.DiasCredito > 0)
            {
                _logger.LogInformation("💳 Proveedor creado con crédito: {DiasCredito} días", request.DiasCredito);
            }

            // Registrar evento si es un proveedor internacional
            if (!request.Pais.Equals("Chile", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("🌍 Proveedor internacional creado: {Pais}", request.Pais);
            }

            return Task.FromResult(Result.Success(proveedor));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("⚠️ Datos inválidos para crear proveedor: {Error}", ex.Message);
            return Task.FromResult(Result.Failure<Proveedor>($"Datos inválidos: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error inesperado en factory del dominio");
            return Task.FromResult(Result.Failure<Proveedor>($"Error al crear proveedor: {ex.Message}"));
        }
    }
} 