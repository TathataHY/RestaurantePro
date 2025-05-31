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
        _logger.LogInformation("🚀 Iniciando creación de proveedor: {Nombre}", request.Nombre);

        try
        {
            // 1. Validar que el proveedor no exista (RFC único)
            var proveedorExistentePorRfc = await ValidarRfcUnico(request.RFC, cancellationToken);
            if (!proveedorExistentePorRfc.Succeeded)
            {
                _logger.LogWarning("❌ RFC ya existe: {RFC}", request.RFC);
                return Result.Failure<ProveedorDto>(proveedorExistentePorRfc.Error ?? "Error al validar RFC");
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
            await _proveedorRepository.AgregarAsync(proveedor);
            _logger.LogInformation("💾 Proveedor guardado en repositorio: {Id}", proveedor.Id);

            // 5. Mapear a DTO
            var proveedorDto = _mapper.Map<ProveedorDto>(proveedor);
            
            _logger.LogInformation("✅ Proveedor creado exitosamente: {Id} - {Nombre}", proveedor.Id, proveedor.Nombre);
            
            return Result.Success(proveedorDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error inesperado al crear proveedor: {Nombre}", request.Nombre);
            return Result.Failure<ProveedorDto>($"Error interno al crear el proveedor: {ex.Message}");
        }
    }

    /// <summary>
    /// Valida que el RFC sea único en el sistema
    /// </summary>
    private async Task<Result> ValidarRfcUnico(string rfc, CancellationToken cancellationToken)
    {
        try
        {
            var proveedoresConRfc = await _proveedorRepository.ObtenerPorRFCAsync(rfc, cancellationToken);
            
            if (proveedoresConRfc.Any())
            {
                var proveedor = proveedoresConRfc.First();
                return Result.Failure($"Ya existe un proveedor con el RFC {rfc}: {proveedor.Nombre}");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar RFC único: {RFC}", rfc);
            return Result.Failure("Error al validar el RFC en el sistema");
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
            if (!request.Pais.Equals("México", StringComparison.OrdinalIgnoreCase))
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