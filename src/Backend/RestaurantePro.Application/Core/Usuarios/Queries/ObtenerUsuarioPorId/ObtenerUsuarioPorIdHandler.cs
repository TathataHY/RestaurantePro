namespace RestaurantePro.Application.Core.Usuarios.Queries.ObtenerUsuarioPorId;

/// <summary>
/// Handler para obtener un usuario específico por su ID
/// </summary>
public class ObtenerUsuarioPorIdHandler : IRequestHandler<ObtenerUsuarioPorIdQuery, Result<UsuarioDto>>
{
    private readonly IUsuarioService _usuarioService;
    private readonly ILogger<ObtenerUsuarioPorIdHandler> _logger;

    public ObtenerUsuarioPorIdHandler(
        IUsuarioService usuarioService,
        ILogger<ObtenerUsuarioPorIdHandler> logger)
    {
        _usuarioService = usuarioService ?? throw new ArgumentNullException(nameof(usuarioService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<UsuarioDto>> Handle(
        ObtenerUsuarioPorIdQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Obteniendo usuario por ID: {Id}", request.Id);

            var usuario = await _usuarioService.ObtenerPorIdAsync(request.Id, cancellationToken);

            if (usuario == null)
            {
                _logger.LogWarning("Usuario no encontrado con ID: {Id}", request.Id);
                return Result.Failure<UsuarioDto>("Usuario no encontrado");
            }

            // 🔍 LOG: Verificar datos del usuario desde la base de datos
            _logger.LogInformation("🔍 [TELEFONO DEBUG] Usuario desde BD - ID: {Id}, Email: {Email}, Telefono: '{Telefono}'", 
                usuario.Id, usuario.Email, usuario.Telefono ?? "NULL");

            var usuarioDto = new UsuarioDto
            {
                Id = usuario.Id,
                NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                NombreUsuario = usuario.NombreUsuario,
                Estado = usuario.Estado,
                TipoUsuario = usuario.TipoUsuario,
                Rol = usuario.Rol,
                NivelAcceso = usuario.NivelAcceso,
                Permisos = usuario.Permisos.ToList(),
                SupervisorId = usuario.SupervisorId,
                Departamento = usuario.Departamento,
                Posicion = usuario.Posicion,
                Identificacion = usuario.Identificacion,
                UltimoAcceso = usuario.UltimoAcceso,
                MotivoBloqueo = usuario.MotivoBloqueo,
                EsAdministrador = usuario.EsAdministrador,
                Roles = usuario.Roles.Select(r => r.ToString()).ToList()
            };

            // 🔍 LOG: Verificar datos del DTO antes de retornar
            _logger.LogInformation("🔍 [TELEFONO DEBUG] DTO creado - ID: {Id}, Email: {Email}, Telefono: '{Telefono}'", 
                usuarioDto.Id, usuarioDto.Email, usuarioDto.Telefono ?? "NULL");

            _logger.LogInformation("Usuario obtenido exitosamente: {Email}", usuario.Email);

            return Result<UsuarioDto>.Success(usuarioDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario por ID: {Id}", request.Id);
            return Result.Failure<UsuarioDto>("Error al obtener usuario");
        }
    }
} 