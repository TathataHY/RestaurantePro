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

            var usuarioDto = new UsuarioDto
            {
                Id = usuario.Id,
                NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email,
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