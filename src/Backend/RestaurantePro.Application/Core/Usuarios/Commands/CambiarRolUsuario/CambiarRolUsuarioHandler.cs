namespace RestaurantePro.Application.Core.Usuarios.Commands.CambiarRolUsuario;

/// <summary>
/// Handler para cambiar el rol de un usuario
/// </summary>
public class CambiarRolUsuarioHandler : IRequestHandler<CambiarRolUsuarioCommand, Result<UsuarioDto>>
{
    private readonly IUsuarioService _usuarioService;
    private readonly ILogger<CambiarRolUsuarioHandler> _logger;

    public CambiarRolUsuarioHandler(
        IUsuarioService usuarioService,
        ILogger<CambiarRolUsuarioHandler> logger)
    {
        _usuarioService = usuarioService ?? throw new ArgumentNullException(nameof(usuarioService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<UsuarioDto>> Handle(
        CambiarRolUsuarioCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Cambiando rol de usuario {Id} a: {NuevoRol}", request.Id, request.NuevoRol);

            // Verificar que el usuario existe
            var usuario = await _usuarioService.ObtenerPorIdAsync(request.Id, cancellationToken);
            if (usuario == null)
            {
                _logger.LogWarning("Usuario no encontrado para cambiar rol: {Id}", request.Id);
                return Result.Failure<UsuarioDto>("Usuario no encontrado");
            }

            // Validar que el nuevo rol es válido
            if (!Enum.TryParse<RolUsuario>(request.NuevoRol, true, out var nuevoRol))
            {
                _logger.LogWarning("Rol inválido: {Rol}", request.NuevoRol);
                return Result.Failure<UsuarioDto>("Rol inválido");
            }

            // Cambiar el rol del usuario
            await _usuarioService.CambiarRolAsync(request.Id, nuevoRol, cancellationToken);

            // Obtener el usuario actualizado
            var usuarioActualizado = await _usuarioService.ObtenerPorIdAsync(request.Id, cancellationToken);
            if (usuarioActualizado == null)
            {
                return Result.Failure<UsuarioDto>("Error al obtener usuario actualizado");
            }

            var usuarioDto = new UsuarioDto
            {
                Id = usuarioActualizado.Id,
                NombreCompleto = usuarioActualizado.NombreCompleto,
                Email = usuarioActualizado.Email,
                NombreUsuario = usuarioActualizado.NombreUsuario,
                Estado = usuarioActualizado.Estado,
                TipoUsuario = usuarioActualizado.TipoUsuario,
                Rol = usuarioActualizado.Rol,
                NivelAcceso = usuarioActualizado.NivelAcceso,
                Permisos = usuarioActualizado.Permisos.ToList(),
                SupervisorId = usuarioActualizado.SupervisorId,
                Departamento = usuarioActualizado.Departamento,
                Posicion = usuarioActualizado.Posicion,
                Identificacion = usuarioActualizado.Identificacion,
                UltimoAcceso = usuarioActualizado.UltimoAcceso,
                MotivoBloqueo = usuarioActualizado.MotivoBloqueo,
                EsAdministrador = usuarioActualizado.EsAdministrador,
                Roles = usuarioActualizado.Roles.Select(r => r.ToString()).ToList()
            };

            _logger.LogInformation("Rol de usuario cambiado exitosamente: {Email} -> {Rol}", 
                usuario.Email, request.NuevoRol);

            return Result<UsuarioDto>.Success(usuarioDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar rol de usuario: {Id}", request.Id);
            return Result.Failure<UsuarioDto>("Error al cambiar rol de usuario");
        }
    }
} 